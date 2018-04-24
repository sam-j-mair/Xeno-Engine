using System;
using System.Threading;

namespace XenoEngine.Systems
{
    public delegate void TaskCompleted(object task);
    //Just a dummy type to ensure type safety on task list.
    public abstract class TaskType
    {
        private Thread                      m_executingThread;
        private ManualResetEvent            m_resetEvent;
        private ParameterizedThreadStart    m_taskOperation;
        private object                      m_UserData;
        private event TaskCompleted         m_eOnCompleted;
        private String                      m_szTaskName;
        //need a reference to this so that it can be removed from the tasks list.
        private TaskHandle                  m_taskHandle;
        private bool                        m_bContinuousExecution;

        internal Thread ExecutingThread { get { return m_executingThread; } set { m_executingThread = value; } }
        internal ManualResetEvent Event { get { return m_resetEvent; } set { m_resetEvent = value; } }
        internal ParameterizedThreadStart TaskOp { get { return m_taskOperation; } set { m_taskOperation = value; } }
        internal TaskHandle Handle { get { return m_taskHandle; } set { m_taskHandle = value; } }
        internal bool ContinuousExecution { get { return m_bContinuousExecution; } set { m_bContinuousExecution = value; } }

        public object UserData { get { return m_UserData; } set { m_UserData = value; } }
        public String Name { get { return m_szTaskName; } set { m_szTaskName = value; } }

        internal void SetCompleteEvent(TaskCompleted taskCompleted)
        {
            m_eOnCompleted += taskCompleted;
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        internal void SetCompleteEvent(TaskCompleted[] events)
        {
            foreach (TaskCompleted callback in events)
            {
                m_eOnCompleted += callback;
            }
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        internal void FireCompletedEvent(object returnData)
        {
            if(m_eOnCompleted != null)
                m_eOnCompleted(returnData);
        }
    }
}
