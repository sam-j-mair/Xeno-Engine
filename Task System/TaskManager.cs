using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace XenoEngine.Systems
{
    [Serializable]
    public struct TaskHandle
    {
        private int m_value;

        public TaskHandle(int nValue)
        {
            m_value = nValue;
        }

        public static bool operator ==(TaskHandle lhs, TaskHandle rhs)
        {
            return lhs.m_value == rhs.m_value;
        }

        public static bool operator !=(TaskHandle lhs, TaskHandle rhs)
        {
            return lhs.m_value != rhs.m_value;
        }

        public static TaskHandle operator +(TaskHandle lhs, TaskHandle rhs)
        {
            return new TaskHandle(lhs.m_value + rhs.m_value);
        }

        public static TaskHandle operator -(TaskHandle lhs, TaskHandle rhs)
        {
            return new TaskHandle(lhs.m_value - rhs.m_value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }

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

    //this class is used to allow the thread to store persistent data and manage its self
    class ThreadData
    {
        private float   m_fTimeIdle;
        private int     m_nNumberOfTasks;
        private int     m_nThreadID;

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

    //This is my take on the .NET ThreadPools
    public class TaskManager : IDisposable
    {
        #region Members
        private int                                 m_nTaskCount;
        private int                                 m_nMinThreads;
        private int                                 m_nMaxThreads;

        //Two locks are used here in order to avoid locking when it is not
        //needed
        [NonSerialized]private ReaderWriterLockSlim                m_queueLock;
        [NonSerialized]private ReaderWriterLockSlim                m_dictionaryLock;
        [NonSerialized]private ReaderWriterLockSlim                m_threadLock;
        //A Dictionary of threads the int will be used as an id so that the 
        //task can retrieve the thread.

        //NOTE: Probably don't actually need to store threads at all.
        //      but will store them for now for at least debugging purposes.
        private List<Thread>                        m_threads;
        private Queue<QueueEntry>                   m_taskQueue;
        private Queue<Thread>                       m_availableThreads;
        private Dictionary<TaskHandle, TaskType>    m_tasks;
        
        #endregion
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        #region Singleton Access
        private static TaskManager m_instance;
        public static TaskManager Instance { get { return m_instance; } }
        public static void CreateStaticInstance(int nMinThreads, int nMaxThreads)
        {
            if(m_instance == null)
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
        ~TaskManager()
        {

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
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        #region Create Task Methods
        public TaskHandle CreateTask<T>(object userData, ParameterizedThreadStart task, bool bContinuousExecution)
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            Task<T> theTask = new Task<T>(resetEvent, userData, task, bContinuousExecution);
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
        //-------------------------------------------------------------------------------
        #region Execute / Destory
        public void ExecuteTask(TaskHandle taskHandle)
        {
            TaskType taskType = GetTaskByHandle(taskHandle);
            QueueEntry queueEntry = new QueueEntry(taskType);
            
            m_queueLock.TryEnterWriteLock(1000);
            m_taskQueue.Enqueue(queueEntry);
            m_queueLock.ExitWriteLock();

            //CheckToWakeThreads(m_taskQueue.Count);
        }

//         private void CheckToWakeThreads(int nTaskCount)
//         {
//             int nthreadsToWake = 0;
// 
//             
//             nthreadsToWake = 1;
// 
//             m_threadLock.TryEnterUpgradeableReadLock();
// 
//             if(m_)
//             m_threadLock.EnterWriteLock();
// 
// 
//         }

        //-------------------------------------------------------------------------------
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

//                 else
//                 {
//                     Thread.Sleep(Timeout.Infinite);
//                 }
            }
        }
        #endregion
    }
}
