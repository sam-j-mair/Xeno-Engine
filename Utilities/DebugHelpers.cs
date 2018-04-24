
//This is the Logger
using System;
using System.Collections.Generic;
using System.Diagnostics;
using XenoEngine.Utilities;

//#define DEBUG_HELPERS

namespace XenoEngine.DebugHelpers
{
    internal class PerformanceCategory
    {
        public Stopwatch        m_watch = Stopwatch.StartNew();
        public List<TimeSpan>   m_measurements = new List<TimeSpan>();
        public int              m_nCurrentIndex = 0;
        public int              m_nNumberOfCalls = 0;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProfilerAttribute : Attribute
    {
        public string m_szCategory;

        public ProfilerAttribute(string szCatName) { m_szCategory = szCatName; }
    }

    public static class Profiler
    {
        private static Dictionary<int, PerformanceCategory> m_stopWatches = new Dictionary<int, PerformanceCategory>();
        private static object m_lockObject = new object();

        public static void CreateTimer(string szName)
        {
            lock(m_lockObject)
                m_stopWatches.Add(szName.GetHashCode(), new PerformanceCategory());
        }

        public static bool StartTimer(string szName)
        {
            PerformanceCategory cat = null;
            bool bExists = false;
            
            lock(m_lockObject)
            {
                if (m_stopWatches.TryGetValue(szName.GetHashCode(), out cat))
                {
                    cat.m_watch.Start();
                    cat.m_nNumberOfCalls++;
                    bExists = true;
                }
            }

            return bExists;
        }

        public static TimeSpan? StopTimer(string szName)
        {
            PerformanceCategory cat;
            TimeSpan? timeSpan = null;

            lock(m_lockObject)
            {
                if (m_stopWatches.TryGetValue(szName.GetHashCode(), out cat))
                {
                    //Debug.Assert(cat.m_watch.IsRunning);
                    cat.m_watch.Stop();
                    cat.m_measurements.Add(cat.m_watch.Elapsed);
                    ++cat.m_nCurrentIndex;
                    timeSpan = cat.m_watch.Elapsed;
                }
            }

            return timeSpan;
        }

        public static void ResetTimer(string szName)
        {
            PerformanceCategory cat;

            lock(m_lockObject)
            {
                if (m_stopWatches.TryGetValue(szName.GetHashCode(), out cat))
                {
                    Debug.Assert(cat.m_watch.ElapsedTicks > 0);
                    cat.m_watch.Reset();
                }
            }
        }

        public static void RestartTimer(string szName)
        {
            PerformanceCategory cat;

            lock(m_lockObject)
            {
                if (m_stopWatches.TryGetValue(szName.GetHashCode(), out cat))
                {
                    Debug.Assert(cat.m_watch.IsRunning);
                    cat.m_measurements.Add(cat.m_watch.Elapsed);
                    cat.m_watch.Restart();
                }
            }
        }

        public static List<TimeSpan> GetMeasurements(string szName)
        {
            PerformanceCategory cat;
            List<TimeSpan> measurements = null;

            lock(m_lockObject)
            {
                if (m_stopWatches.TryGetValue(szName.GetHashCode(), out cat))
                {
                    measurements = cat.m_measurements;
                }
            }
         
            return measurements;
        }

        public static void CreateAndStartTimer(string szName)
        {
            if (!Profiler.StartTimer(szName))
            {
                Profiler.CreateTimer(szName);
                Profiler.StartTimer(szName);
            }
            else
            {
                Profiler.StartTimer(szName);
            }
        }

        private static void StartProfileSection(string szProfileName)
        {
            Profiler.CreateAndStartTimer(szProfileName);
        }

        private static TimeSpan? EndProfileSection(string szProfileName)
        {
            TimeSpan? timeSpan = Profiler.StopTimer(szProfileName);
            LogSystem.Log("ProfileSection " + szProfileName + ": " + timeSpan.ToString(), false);
            return timeSpan;
        }

//         public static int GetSizeOf(object obj)
//         {
//             int nSizeInBytes = 0;
// 
//             if(obj != null)
//             {
//                 Type type = obj.GetType();
//                 nSizeInBytes = Marshal.SizeOf(type);
//             }
// 
//             return nSizeInBytes;
//         }


        //This is a struct to help avoid heap allocation
        public struct TrackingObject : IDisposable
        {
            private string m_szProfileSection;

            public TrackingObject(string szProfileSection)
            {
                m_szProfileSection = szProfileSection;
                StartProfileSection(m_szProfileSection);
            }

            public void Dispose()
            {
                EndProfileSection(m_szProfileSection);
            }
        }

        public struct MemoryAllocMonitor: IDisposable
        {
            private long   m_fBeforeAlloc;
            private long   m_fAfterAlloc;
            private float   m_fTotalMemoryAlloc;
            private string  m_szMessage;
            
            public MemoryAllocMonitor(string szMessage)
            {
                m_fBeforeAlloc = GC.GetTotalMemory(true);
                m_fAfterAlloc = 0;
                m_fTotalMemoryAlloc = 0;
                m_szMessage = szMessage;
            }

            public void Dispose()
            {
                m_fAfterAlloc = GC.GetTotalMemory(true);

                m_fTotalMemoryAlloc = (float)(m_fAfterAlloc - m_fBeforeAlloc);

                LogSystem.Log(m_szMessage + ": " + (m_fTotalMemoryAlloc) + " bytes ", false);
                
           }

            public float Amount { get { return m_fTotalMemoryAlloc; } }
            public float BeforeAlloc { get { return m_fBeforeAlloc; } }
            public float AfterAlloc { get { return m_fAfterAlloc; } }
        }
    }

    

}