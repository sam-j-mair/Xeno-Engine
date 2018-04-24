using System.Threading;

namespace XenoEngine.Systems
{
    public class Task <T> : TaskType
    {
        private T                       m_returnData;

        //-----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------
        public Task(ManualResetEvent resetEvent, object userData, ParameterizedThreadStart task, bool bContinuousExecution)
        {
            m_returnData = default(T);
            TaskOp += task;
            Event = resetEvent;
            UserData = userData;
            ContinuousExecution = bContinuousExecution;
        }
        //-----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------
        public Task(TaskCompleted taskCompletedCallBack, object userData, ParameterizedThreadStart task, bool bContinuousExecution)
        {
            m_returnData = default(T);
            TaskOp += task;
            Event = null;
            SetCompleteEvent(taskCompletedCallBack);
            UserData = userData;
            ContinuousExecution = bContinuousExecution;
        }
        //-----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------
        public Task(TaskCompleted[] aTaskCompletedCallBacks, object userData, ParameterizedThreadStart task, bool bContinuousExecution)
        {
            m_returnData = default(T);
            TaskOp += task;
            Event = null;
            UserData = userData;
            ContinuousExecution = bContinuousExecution;

            SetCompleteEvent(aTaskCompletedCallBacks);
        }
        //-----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------
        public T ReturnData { get { return m_returnData; } set { m_returnData = value; } }
    }
}
