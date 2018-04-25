using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XenoEngine.GeneralSystems
{
    /// <summary>
    /// This is a singleton class that allows easy access to the services that the engine uses.
    /// it also allow registration.
    /// </summary>
    public static class EngineServices
    {
        private static Dictionary<Type, ISystems> m_services;

        //----------------------------------------------------------------------------------
        /// <summary>
        /// Get a registered service.
        /// </summary>
        /// <typeparam name="TISystemsType">the typeof the service to get that it is registered under.</typeparam>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// allows for registering a new system.
        /// </summary>
        /// <typeparam name="TISystemsType">the type of the service to be registered.</typeparam>
        /// <param name="provider">the service provider.</param>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Lazy Initialization.
        /// </summary>
        //----------------------------------------------------------------------------------
        private static void LazyInit()
        {
            if (m_services == null)
            {
                m_services = new Dictionary<Type, ISystems>();
            }
        }
    }
}
