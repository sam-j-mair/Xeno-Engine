using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XenoEngine
{
    public static class SystemEvents
    {
        /// <summary>
        /// Dictionary the stores all events.
        /// </summary>
        static Dictionary<string, EntityEvent<dynamic>> m_events = new Dictionary<string, EntityEvent<dynamic>>();
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Static Function Gets the even of a particular name
        /// </summary>
        /// <param name="szEventName">name of event</param>
        /// <returns>returns event</returns>
        //-------------------------------------------------------------------------------
        public static EntityEvent<dynamic> GetEvent(this string szEventName)
        {
            return m_events[szEventName];
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Static Function creates an event.
        /// </summary>
        /// <param name="szEventName">name of event to be created</param>
        //-------------------------------------------------------------------------------
        private static void CreateEvent(string szEventName)
        {
            m_events.Add(szEventName, new EntityEvent<dynamic>());
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Static Function executes event.
        /// </summary>
        /// <param name="szEventName">name of event.</param>
        /// <param name="sendingEntity">the entity that is sending the event.</param>
        /// <param name="data">any data that is sent with the event.</param>
        //-------------------------------------------------------------------------------
        public static void FireEvent(this string szEventName, Entity sendingEntity, dynamic data)
        {
            m_events[szEventName].FireEvent(sendingEntity, data);
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Static Function adds an eventhandler to an existing event or it will create a new event if doesn't exist.
        /// </summary>
        /// <param name="szEventName">name of event.</param>
        /// <param name="handler">the event handler.</param>
        //-------------------------------------------------------------------------------
        public static void AddEventHandler(this string szEventName, Action<Entity, dynamic> handler)
        {
            EntityEvent<dynamic> entityEvent;
            if (m_events.TryGetValue(szEventName, out entityEvent))
            {
                entityEvent += handler;
            }
            else
            {
                CreateEvent(szEventName);
            }
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// removes event handler from event.
        /// </summary>
        /// <param name="szEventName">name of event.</param>
        /// <param name="handler">the event handler.</param>
        //-------------------------------------------------------------------------------
        public static void RemoveEventHandler(this string szEventName, Action<Entity, dynamic> handler)
        {
            try
            {
                m_events[szEventName] -= handler;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("No event exists of this name " + ex.Message);
            }
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// clears a registered event.
        /// </summary>
        /// <param name="szEventName">name of event.</param>
        //-------------------------------------------------------------------------------
        public static void ClearEvent(this string szEventName)
        {
            m_events[szEventName] = null;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// clears the event collection.
        /// </summary>
        //-------------------------------------------------------------------------------
        public static void ClearEvents()
        {
            m_events.Clear();
        }
    }
    //-------------------------------------------------------------------------------
    /// <summary>
    /// EntityEvent class.
    /// </summary>
    /// <typeparam name="TParameter">type of date to be store with event.</typeparam>
    //-------------------------------------------------------------------------------
    [Serializable]
    public class EntityEvent<TParameter>
    {
        private Type type;
        private Action<Entity, TParameter>  m_delegate;
        private object                      m_lockObject = new object();

        //-------------------------------------------------------------------------------
        /// <summary>
        /// operator +. overrides + operator to add handlers to events easily.
        /// </summary>
        /// <param name="entityEvent">the event you are join to another event.</param>
        /// <param name="action">the event handler</param>
        /// <returns>the new event</returns>
        //-------------------------------------------------------------------------------
        public static EntityEvent<TParameter> operator +(EntityEvent<TParameter> entityEvent, Action<Entity, TParameter> action)
        {
            Debug.Assert(action.Target is Entity);
            entityEvent.m_delegate += action;
            return entityEvent;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// operator -. overrides + operator to subtract handlers to events easily.
        /// </summary>
        /// <param name="entityEvent">the event you are remove to another event.</param>
        /// <param name="action">the event handler</param>
        /// <returns></returns>
        //-------------------------------------------------------------------------------
        public static EntityEvent<TParameter> operator -(EntityEvent<TParameter> entityEvent, Action<Entity, TParameter> action)
        {
            entityEvent.m_delegate += action;
            return entityEvent;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// clears the event handler.
        /// </summary>
        //-------------------------------------------------------------------------------
        public void Clear()
        {
            m_delegate = null;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// fires the event.
        /// </summary>
        /// <param name="sendingEntity">the entity the event is coming from.</param>
        /// <param name="parameter">data to be sent with the event.</param>
        //-------------------------------------------------------------------------------
        public void FireEvent(Entity sendingEntity, TParameter parameter)
        {
            //Profiler.StartProfileSection("EntityEvent");
            {
                lock (m_lockObject)
                {
                    if ((sendingEntity.IsValid))
                    {
                        if (m_delegate != null)
                        {
                            foreach (Delegate hook in m_delegate.GetInvocationList())
                            {
                                Entity targetEntity = hook.Target as Entity;

                                Debug.Assert(targetEntity != null, "The Target is not an entity");

                                if (targetEntity != null && targetEntity.IsValid)
                                {
                                    hook.DynamicInvoke(sendingEntity, parameter);
                                }
                            }

                        }
                    }
                }
            }
            //Profiler.EndProfileSection("EntityEvent");
        }
        
    }
}
