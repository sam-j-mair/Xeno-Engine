using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XenoEngine.GeneralSystems
{
    public static class EngineServices
    {
        public static TISystemsType GetSystem<TISystemsType>() where TISystemsType : ISystems
        {
            ISystems iSystem;
            if (m_services.TryGetValue(typeof(TISystemsType), out iSystem))
            {
                return (TISystemsType)iSystem;
            }
            else
            {
                Debug.Fail("No system of this type has been registered.");
                return default(TISystemsType);
            }
        }
        private static Dictionary<Type, ISystems> m_services;

        public static void RegisterSystems<TISystemsType>(ISystems provider) where TISystemsType : ISystems
        {
            Type type = provider.GetType();
            LazyInit();

            if (!m_services.ContainsKey(type))
            {
                m_services.Add(typeof(TISystemsType), provider);
            }
            else
            {
                Debug.Fail("This service already exists!");
            }
        }

        private static void LazyInit()
        {
            if (m_services == null)
            {
                m_services = new Dictionary<Type, ISystems>();
            }
        }
    }
}
