using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace XenoEngine.Systems
{
    //----------------------------------------------------------------------------------
    /// <summary>
    /// This is handle is the identifier for the task and it is used to access properties of the task
    /// </summary>
    //----------------------------------------------------------------------------------
    [Serializable]
    public struct TaskHandle
    {
        //Members
        private int m_value;
        //----------------------------------------------------------------------------------
        /// <summary>
        /// contructor
        /// </summary>
        /// <param name="nValue">identity id </param>
        //----------------------------------------------------------------------------------
        public TaskHandle(int nValue)
        {
            m_value = nValue;
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// over ride logical equals and not equals
        /// </summary>
        /// <param name="lhs">left hand argument</param>
        /// <param name="rhs">right hand argument</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
        public static bool operator ==(TaskHandle lhs, TaskHandle rhs)
        {
            return lhs.m_value == rhs.m_value;
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public static bool operator !=(TaskHandle lhs, TaskHandle rhs)
        {
            return lhs.m_value != rhs.m_value;
        }
    }
    //----------------------------------------------------------------------------------
    /// <summary>
    /// Class for queued tasks. 
    /// </summary>
    //----------------------------------------------------------------------------------
    class QueueEntry
    {
        TaskType m_taskType;
        float m_fTimeInQueue;

        public QueueEntry(TaskType taskType)
        {
            m_taskType = taskType;
            m_fTimeInQueue = 0.0f;
        }

        public TaskType TaskType { get { return m_taskType; } }
        public float TimeInQueue { get { return m_fTimeInQueue; } set { m_fTimeInQueue = value; } }
    }
    //----------------------------------------------------------------------------------
    /// <summary>
    /// This class is used to allow the thread to store persistent data and manage its self
    /// </summary>
    //----------------------------------------------------------------------------------
    class ThreadData
    {
        private float m_fTimeIdle;
        private int m_nNumberOfTasks;
        private int m_nThreadID;

        public ThreadData()
        {
            m_fTimeIdle = 0.0f;
            m_nNumberOfTasks = 0;
            m_nThreadID = 0;
        }

        public float IdleTime { get { return m_fTimeIdle; } set { m_fTimeIdle = value; } }
        public int NumberOfTasks { get { return m_nNumberOfTasks; } set { m_nNumberOfTasks = value; } }
        public int ThreadID { get { return m_nThreadID; } set { m_nThreadID = value; } }
    }
    //----------------------------------------------------------------------------------
    /// <summary>
    /// This is my take on the .NET ThreadPools 
    /// </summary>
    //----------------------------------------------------------------------------------
    public class TaskManager : IDisposable
    {
        #region Members
        private int m_nTaskCount;
        private int m_nMinThreads;
        private int m_nMaxThreads;

        //Two locks are used here in order to avoid locking when it is not
        //needed
        [NonSerialized] private ReaderWriterLockSlim m_queueLock;
        [NonSerialized] private ReaderWriterLockSlim m_dictionaryLock;
        [NonSerialized] private ReaderWriterLockSlim m_threadLock;
        //A Dictionary of threads the int will be used as an id so that the 
        //task can retrieve the thread.

        //NOTE: Probably don't actually need to store threads at all.
        //      but will store them for now for at least debugging purposes.
        private List<Thread> m_threads;
        private Queue<QueueEntry> m_taskQueue;
        private Queue<Thread> m_availableThreads;
        private Dictionary<TaskHandle, TaskType> m_tasks;

        #endregion
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        #region Singleton Access
        private static TaskManager m_instance;
        public static TaskManager Instance { get { return m_instance; } }
        public static void CreateStaticInstance(int nMinThreads, int nMaxThreads)
        {
            if (m_instance == null)
                m_instance = new TaskManager(nMinThreads, nMaxThreads, true);
        }
        #endregion
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        #region Constructor / Destructor
        public TaskManager(int nMinThreads, int nMaxThreads, bool bBackground)
        {
            m_queueLock = new ReaderWriterLockSlim();
            m_dictionaryLock = new ReaderWriterLockSlim();
            m_threadLock = new ReaderWriterLockSlim();

            m_threads = new List<Thread>(nMaxThreads);
            m_taskQueue = new Queue<QueueEntry>();
            m_availableThreads = new Queue<Thread>(nMaxThreads);
            m_tasks = new Dictionary<TaskHandle, TaskType>();

            m_nMinThreads = nMinThreads;
            m_nMaxThreads = nMaxThreads;
            m_nTaskCount = 0;

            //Create the initial threads up front.
            for (int nCounter = 0; nCounter < nMinThreads; ++nCounter)
            {
                Thread theThread = new Thread(new ParameterizedThreadStart(ThreadProcess));
                ThreadData threadData = new ThreadData();
                theThread.Name = "Thread " + (1 + nCounter).ToString();
                //Set it to back ground that they terminate along with the main thread.
                theThread.IsBackground = bBackground;
                threadData.ThreadID = nCounter;
                theThread.Start(threadData);
                m_threads.Add(theThread);
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        [OnSerialized]
        protected void OnDeserialized(StreamingContext context)
        {
            m_dictionaryLock = new ReaderWriterLockSlim();
            m_queueLock = new ReaderWriterLockSlim();
            m_threadLock = new ReaderWriterLockSlim();
        }
        #endregion
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <typeparam name="T">T is the stored data type</typeparam>
        /// <param name="userData">user data to be used in the task.</param>
        /// <param name="task">the thread for the task.</param>
        /// <param name="bContinuousExecution">if the task is to continuously execute and readd its self.</param>
        /// <returns></returns>
        //-------------------------------------------------------------------------------
        #region Create Task Methods
        public TaskHandle CreateTask<T>(object userData, ParameterizedThreadStart task, bool bContinuousExecution)
        {
            //create reset event object.
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            Task<T> theTask = new Task<T>(resetEvent, userData, task, bContinuousExecution);
            theTask.Name = "Task " + m_nTaskCount;
            //create new task handle to identify this task
            TaskHandle taskHandle = new TaskHandle(m_nTaskCount++);
            theTask.Handle = taskHandle;

            //lock and add task to dictionary.
            m_dictionaryLock.TryEnterWriteLock(5);
            {
                m_tasks.Add(taskHandle, theTask);
            }
            m_dictionaryLock.ExitWriteLock();

            return taskHandle;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// overloaded: Creates a task.
        /// </summary>
        /// <typeparam name="T">data type.</typeparam>
        /// <param name="taskCompletedCallBack"> callback upon complete.</param>
        /// <param name="userData">user data to be used in task.</param>
        /// <param name="task">thread</param>
        /// <param name="bContinuousExecution">if allow continuous execution.</param>
        /// <returns></returns>
        //-------------------------------------------------------------------------------
        public TaskHandle CreateTask<T>(TaskCompleted taskCompletedCallBack, object userData, ParameterizedThreadStart task, bool bContinuousExecution)
        {
            Task<T> theTask = new Task<T>(taskCompletedCallBack, userData, task, bContinuousExecution);
            theTask.Name = "Task " + m_nTaskCount;
            TaskHandle taskHandle = new TaskHandle(m_nTaskCount++);
            theTask.Handle = taskHandle;

            m_dictionaryLock.TryEnterWriteLock(5);
            {
                m_tasks.Add(taskHandle, theTask);
            }
            m_dictionaryLock.ExitWriteLock();

            return taskHandle;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// overloaded: Creates a task.
        /// </summary>
        /// <typeparam name="T">data type.</typeparam>
        /// <param name="aTaskCompletedCallBacks"> callback array upon complete.</param>
        /// <param name="userData">user data to be used in task.</param>
        /// <param name="task">thread</param>
        /// <param name="bContinuousExecution">if allow continuous execution.</param>
        /// <returns></returns>
        //-------------------------------------------------------------------------------
        public TaskHandle CreateTask<T>(TaskCompleted[] aTaskCompletedCallBacks, object userData, ParameterizedThreadStart task, bool bContinuousExecution)
        {
            Task<T> theTask = new Task<T>(aTaskCompletedCallBacks, userData, task, bContinuousExecution);
            theTask.Name = "Task " + m_nTaskCount;
            TaskHandle taskHandle = new TaskHandle(m_nTaskCount++);
            theTask.Handle = taskHandle;

            m_dictionaryLock.TryEnterWriteLock(5);
            {
                m_tasks.Add(taskHandle, theTask);
            }
            m_dictionaryLock.ExitWriteLock();

            return taskHandle;
        }
        #endregion
        //-------------------------------------------------------------------------------
        /// <summary>
        /// accessors
        /// </summary>
        /// <param name="taskHandle">the handle for the task to access.</param>
        /// <returns>Task</returns>
        //-------------------------------------------------------------------------------
        #region Get Functions
        public TaskType GetTaskByHandle(TaskHandle taskHandle)
        {
            TaskType taskType;

            m_dictionaryLock.TryEnterReadLock(5);
            {
                taskType = m_tasks[taskHandle];
            }
            m_dictionaryLock.ExitReadLock();

            return taskType;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public ManualResetEvent GetResetEvent(TaskHandle taskHandle)
        {
            ManualResetEvent resetEvent;

            m_dictionaryLock.TryEnterReadLock(5);
            {
                resetEvent = m_tasks[taskHandle].Event;
            }
            m_dictionaryLock.ExitReadLock();

            return resetEvent;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Get return data from task after completion.
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="taskHandle">handle for particular task.</param>
        /// <returns>data from task.</returns>
        //-------------------------------------------------------------------------------
        public T GetReturnData<T>(TaskHandle taskHandle)
        {
            TaskType task;

            m_dictionaryLock.TryEnterReadLock(5);
            task = m_tasks[taskHandle];
            m_dictionaryLock.ExitReadLock();

            Debug.Assert(task != null);

            Type type = task.GetType();

            PropertyInfo property = type.GetProperty("ReturnData");
            MethodInfo methodInfo = property.GetGetMethod();

            T returnData = (T)methodInfo.Invoke(task, null);

            DestroyTask(taskHandle);

            return returnData;
        }
        #endregion
        //-------------------------------------------------------------------------------
        /// <summary>
        /// start created task.
        /// </summary>
        /// <param name="taskHandle">task identifier.</param>
        //-------------------------------------------------------------------------------
        #region Execute / Destory
        public void ExecuteTask(TaskHandle taskHandle)
        {
            TaskType taskType = GetTaskByHandle(taskHandle);
            QueueEntry queueEntry = new QueueEntry(taskType);

            m_queueLock.TryEnterWriteLock(1000);
            m_taskQueue.Enqueue(queueEntry);
            m_queueLock.ExitWriteLock();
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Destroys created task.
        /// </summary>
        /// <param name="taskHandle">task identifier.</param>
        //-------------------------------------------------------------------------------
        public void DestroyTask(TaskHandle taskHandle)
        {
            TaskType taskType = GetTaskByHandle(taskHandle);

            Console.WriteLine("Removing " + taskType.Name);
            m_dictionaryLock.TryEnterWriteLock(5);
            m_tasks.Remove(taskHandle);
            m_dictionaryLock.ExitWriteLock();
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        #endregion
        //-------------------------------------------------------------------------------
        // This wraps the execution of a tasks delegate in a call to allow extra functionality.
        /// <summary>
        /// Thread process for running tasks.
        /// </summary>
        /// <param name="threadInfo">upon creation.</param>
        //-------------------------------------------------------------------------------
        #region ThreadProc
        private void ThreadProcess(object threadInfo)
        {
            ThreadData threadData = threadInfo as ThreadData;

            while (true)
            {
                int nTaskCount;
                QueueEntry queueEntry;

                m_queueLock.TryEnterUpgradeableReadLock(5);

                if (m_taskQueue.Count > 0)
                {
                    m_queueLock.EnterWriteLock();
                    queueEntry = m_taskQueue.Dequeue();
                    m_queueLock.ExitWriteLock();
                }
                else
                {
                    queueEntry = null;
                }

                m_queueLock.ExitUpgradeableReadLock();

                if (queueEntry != null)
                {
                    TaskType taskType = queueEntry.TaskType;
                    ParameterizedThreadStart taskDelegate = taskType.TaskOp;

                    Debug.Assert(taskDelegate != null);

                    threadData.NumberOfTasks++;

                    taskType.ExecutingThread = Thread.CurrentThread;

                    Console.WriteLine("processing " + taskType.Name + " on " + Thread.CurrentThread.Name);

                    taskDelegate.Invoke(taskType);

                    Console.WriteLine("finished processing " + taskType.Name + " on " + Thread.CurrentThread.Name);

                    if (taskType.Event != null && !taskType.ContinuousExecution)
                    {
                        taskType.Event.Set();
                    }
                    else
                    {
                        Type type = taskType.GetType();

                        PropertyInfo property = type.GetProperty("ReturnData");
                        MethodInfo methodInfo = property.GetGetMethod();


                        taskType.FireCompletedEvent(methodInfo.Invoke(taskType, null));

                        //This allows the task to be continuously executed.
                        if (taskType.ContinuousExecution)
                            ExecuteTask(taskType.Handle);
                        else
                            DestroyTask(taskType.Handle);
                    }
                }
            }
        }
        #endregion
    }
}