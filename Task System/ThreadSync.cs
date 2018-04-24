using System.Collections.Generic;
using System.Threading;

namespace XenoEngine.Systems
{
    class ThreadSync
    {
        private int m_nNumberOfSyncs;
        private List<Thread> m_threads;

        public ThreadSync(int nNumberOfSyncs)
        {
            m_nNumberOfSyncs = nNumberOfSyncs;
            m_threads = new List<Thread>(m_nNumberOfSyncs);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void SignalAndWait()
        {
            Thread currentThread = Thread.CurrentThread;

            lock(m_threads)
            {
                m_threads.Add(currentThread);

                if(m_threads.Count >= m_nNumberOfSyncs)
                {
                    InteruptThreads();
                }
                else
                {
                    Thread.Sleep(Timeout.Infinite);
                }
            }
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        private void InteruptThreads()
        {
            foreach (Thread threadRef in m_threads)
            {
                threadRef.Interrupt();
            }
        }

    }
}
