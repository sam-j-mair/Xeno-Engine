using System;

namespace XenoEngine
{
    //this is in order to hide away the xna stuff without having to re write the whole game host.
    public struct DeltaTime
    {
        private TimeSpan m_elapsedGameTime;
        private TimeSpan m_totalGameTime;

        public DeltaTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
        {
            m_elapsedGameTime = elapsedGameTime;
            m_totalGameTime = totalGameTime;
        }

        public TimeSpan ElapsedGameTime { get { return m_elapsedGameTime; } }
        public TimeSpan TotalGameTime { get { return m_totalGameTime; } }
    }
}
