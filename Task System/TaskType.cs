using System;
using System.Threading;

namespace XenoEngine.Systems
{
    public delegate void TaskCompleted(object task);
    //Just a dummy type to ensure type safety on task list.
    public abstract class TaskType
    {
        private event TaskCompleted         m_eOnCompleted;
        //need a reference to this so that it can be removed from the tasks list.
        private bool                        m_bContinuousExecution;

        internal Thread ExecutingThread { get; set; }
        internal ManualResetEvent Event { get; set;  }
        internal ParameterizedThreadStart TaskOp { get; set; }
        internal TaskHandle Handle { get; set; }
        internal bool ContinuousExecution { get; set;  }

        public object UserData { get; set; }
        public String Name { get; set; }

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
