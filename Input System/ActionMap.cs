using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace XenoEngine.Systems
{
    [Serializable]
    public class ActionMap
    {
        private Dictionary<int, ActionTriggered>    m_actions;
        private ArrayList                           m_listBindings;
        private int                                 m_nPlayerIndex;

        public ActionMap(int nPlayerIndex)
        {
            m_listBindings = new ArrayList();
            m_actions = new Dictionary<int, ActionTriggered>();
            m_nPlayerIndex = nPlayerIndex;
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public bool TryCreateandSetHandler<T>(string szActionName, T bind, ButtonState state, bool bIsPolling, ActionTriggered actionTriggered)
        {
            ActionTriggered action;
            bool bRslt = false; ;
            if (m_actions.TryGetValue(szActionName.GetHashCode(), out action))
            {
                SetHandler(szActionName, actionTriggered);
                bRslt = true;

            }
            else
            {
                AddAction(szActionName);
                AddBinding(bind, state, szActionName, bIsPolling);
                SetHandler(szActionName, actionTriggered);
                bRslt = true;
            }

            return bRslt;
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public void SetHandler(string szActionName, ActionTriggered actionTriggered)
        {
            ActionTriggered action;
            Debug.Assert(m_actions.TryGetValue(szActionName.GetHashCode(), out action), "The action " + szActionName + " doesn't exist");
            m_actions[szActionName.GetHashCode()] += actionTriggered;
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public void RemoveHandler(string szActionName, ActionTriggered actionTriggered)
        {
            ActionTriggered action;
            Debug.Assert(m_actions.TryGetValue(szActionName.GetHashCode(), out action), "The action " + szActionName + " doesn't exist");
            m_actions[szActionName.GetHashCode()] -= actionTriggered;
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public ActionTriggered GetAction(string szActionName)
        {
            ActionTriggered action;
            Debug.Assert(m_actions.TryGetValue(szActionName.GetHashCode(), out action), "The action " + szActionName + " doesn't exist");
            return m_actions[szActionName.GetHashCode()];
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        internal static void InternalHandler(object sender)
        {
            InputListner listner = sender as InputListner;
            //Debug.WriteLine(listner.ActionMap.LastAction);
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public virtual void AddAction(string szActionName)
        {
            m_actions.Add(szActionName.GetHashCode(), new ActionTriggered(InternalHandler));
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public void AddBinding<T>(T bind, string szActionName)
        {
            ActionTriggered action = null;

            try
            {
                action = m_actions[szActionName.GetHashCode()];
            }
            catch (KeyNotFoundException)
            {
                Debug.WriteLine("The action " + szActionName + " doesn't exist in this action Map");
            }

            m_listBindings.Add(new ActionBinding<T>(bind, szActionName, false));
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public void AddBinding<T>(T bind, string szActionName, bool bIsPolling)
        {
            ActionTriggered action = null;

            try
            {
                action = m_actions[szActionName.GetHashCode()];
            }
            catch (KeyNotFoundException)
            {
                Debug.WriteLine("The action " + szActionName + " doesn't exist in this action Map");
            }

            m_listBindings.Add(new ActionBinding<T>(bind, szActionName, bIsPolling));
        }

        public void AddBinding<T>(T bind, ButtonState buttonState, string szActionName, bool bIsPolling)
        {
            ActionTriggered action = null;

            try
            {
                action = m_actions[szActionName.GetHashCode()];
            }
            catch (KeyNotFoundException)
            {
                Debug.WriteLine("The action " + szActionName + " doesn't exist in this action Map");
            }

            m_listBindings.Add((new ActionBinding<T>(bind, buttonState, szActionName, bIsPolling)));
        }

        public string LastAction { get; set; }
        public virtual int PlayerIndex { get { return 0; } }
        public virtual ArrayList Bindings { get { return m_listBindings; } }
    }

    [Serializable]
    public class ActionBinding <K>
    {
        private String          m_szEventName;
        private bool            m_bIsPolling;
        private ButtonState     m_buttonState;
        private K               m_key;

        public ActionBinding(K key, String szEventName, bool bIsPolling)
        {
            m_key = key;
            m_szEventName = szEventName;
            m_bIsPolling = bIsPolling;
            m_buttonState = ButtonState.Pressed;
        }

        public ActionBinding(K key, ButtonState buttonState, String szEventName, bool bIsPolling)
        {
            m_key = key;
            m_szEventName = szEventName;
            m_bIsPolling = bIsPolling;
            m_buttonState = buttonState;
        }

        public K Key { get { return m_key; } }
        public ButtonState ButtonState { get { return m_buttonState; } }
        public String Event { get { return m_szEventName; } }
        public bool IsPolling { get { return m_bIsPolling; } }
    }
}
