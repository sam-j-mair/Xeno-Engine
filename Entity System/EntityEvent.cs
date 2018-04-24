using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XenoEngine
{
    public static class SystemEvents
    {
        static Dictionary<string, EntityEvent<dynamic>> m_events = new Dictionary<string, EntityEvent<dynamic>>();

        public static EntityEvent<dynamic> GetEvent(this string szEventName)
        {
            return m_events[szEventName];
        }

        private static void CreateEvent(string szEventName)
        {
            m_events.Add(szEventName, new EntityEvent<dynamic>());
        }

        public static void FireEvent(this string szEventName, Entity sendingEntity, dynamic data)
        {
            m_events[szEventName].FireEvent(sendingEntity, data);
        }

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

        public static void ClearEvent(this string szEventName)
        {
            m_events[szEventName] = null;
        }

        public static void ClearEvents()
        {
            m_events.Clear();
        }
    }

    [Serializable]
    public class EntityEvent<TParameter>
    {
        private Type type;
        private Action<Entity, TParameter>  m_delegate;
        private object                      m_lockObject = new object();

        public static EntityEvent<TParameter> operator +(EntityEvent<TParameter> entityEvent, Action<Entity, TParameter> action)
        {
            Debug.Assert(action.Target is Entity);
            entityEvent.m_delegate += action;
            return entityEvent;
        }

        public static EntityEvent<TParameter> operator -(EntityEvent<TParameter> entityEvent, Action<Entity, TParameter> action)
        {
            entityEvent.m_delegate += action;
            return entityEvent;
        }

        public void Clear()
        {
            m_delegate = null;
        }

//         protected void OnSerialize(StreamingContext context)
//         {
//             if()
//         }

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
