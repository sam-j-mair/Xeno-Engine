using System.Threading;

namespace XenoEngine.TaskSystem
{
    public class ThreadSafeField<T> where T : struct
    {
        T m_value;
        private object m_lockObject;
        #region constructors
        public ThreadSafeField()
        {
            m_lockObject = new object();
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public ThreadSafeField(object lockObject)
        {
            m_lockObject = lockObject;
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public ThreadSafeField(object lockObject, T initialValue)
        {
            m_lockObject = lockObject;
            Value = initialValue;
        }
        #endregion
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public T StartLock()
        {
            Monitor.Enter(m_lockObject);
            return m_value;
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void EndLock()
        {
            Monitor.Exit(m_lockObject);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public T StartLock(object lockObject)
        {
            Monitor.Enter(lockObject);
            return m_value;
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void EndLock(object lockObject)
        {
            Monitor.Exit(lockObject);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public T Value { get { lock (m_lockObject) { return m_value; } } set { lock (m_lockObject) { m_value = value; } } }
    }

    public class ThreadSafeField
    {
        dynamic m_value;
        private object m_lockObject;

        public ThreadSafeField()
        {
            m_lockObject = new object();
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public ThreadSafeField(dynamic initialValue, object lockObject)
        {
            m_lockObject = lockObject;
            m_value = initialValue;
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public dynamic StartLock()
        {
            Monitor.Enter(m_lockObject);
            return m_value;
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void EndLock()
        {
            Monitor.Exit(m_lockObject);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public dynamic StartLock(object lockObject)
        {
            Monitor.Enter(lockObject);
            return m_value;
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void EndLock(object lockObject)
        {
            Monitor.Exit(lockObject);
        }
        //public dynamic Value { get { lock (m_lockObject) { return m_value; } } set { lock (m_lockObject) { m_value = value; } } }
    }
}
