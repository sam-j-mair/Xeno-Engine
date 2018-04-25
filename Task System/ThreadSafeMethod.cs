using System;

namespace Xeno_Engine.Task_System
{
    public class ThreadSafeMethod<TDelegate> where TDelegate : class
    {
        private dynamic m_pfFunction;
        private object m_lockObject;

        //--------------------------------------------------------------
        /// <summary>
        /// C/TOR
        /// </summary>
        /// <param name="lockObject">the object which is used to lock with.</param>
        /// <param name="pfFunction">the delegate the is to be called upon locking.</param>
        //--------------------------------------------------------------
        public ThreadSafeMethod(TDelegate pfFunction)
        {
            if (pfFunction == null || pfFunction.GetType().BaseType != typeof(MulticastDelegate))
                throw new NotSupportedException("only sub types of MulticastDelegate can be used");

            m_lockObject = new object();
            m_pfFunction = pfFunction;
        }
        //--------------------------------------------------------------
        /// <summary>
        /// C/Tor
        /// </summary>
        /// <param name="lockObject">the object which is used to lock with.</param>
        /// <param name="pfFunction">the delegate the is to be called upon locking.</param>
        //--------------------------------------------------------------
        public ThreadSafeMethod(object lockObject, TDelegate pfFunction)
        {
            m_lockObject = lockObject;
            m_pfFunction = pfFunction;
        }
        //--------------------------------------------------------------
        /// <summary>
        /// call the underlying delegate.
        /// </summary>
        /// <param name="aParams">input parameters</param>
        /// <returns></returns>
        //--------------------------------------------------------------
        public dynamic Function(params object[] aParams)
        {
            dynamic value = null;
            var pfFunc = m_pfFunction;

            if (pfFunc != null)
            {
                lock (m_lockObject)
                    value = pfFunc.DynamicInvoke(aParams);
            }

            return value;
        }
        //--------------------------------------------------------------
        /// <summary>
        /// call the underlying delegate.
        /// </summary>
        /// <param name="aParams">input parameters</param>
        /// <returns></returns>
        //--------------------------------------------------------------
        public TReturnType Function<TReturnType>(params object[] aParams)
        {
            dynamic value = null;
            var pfFunc = m_pfFunction;

            if (pfFunc != null)
            {
                lock (m_lockObject)
                    value = pfFunc.DynamicInvoke(aParams);
            }

            return (TReturnType)value;
        }
    }
}
