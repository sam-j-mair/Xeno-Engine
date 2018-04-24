using System;

using System.Diagnostics;

namespace XenoEngine.Systems
{
    [Serializable]
    public class StateMachine<TUserData>
    {
        State<TUserData> m_CurrentState;
        TUserData m_userData;

        public StateMachine(State<TUserData> startingState, TUserData userData)
        {
            m_userData = userData;
            m_CurrentState = startingState;
            m_CurrentState.OnEnter(this);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public TUserData UserData
        {
            get { return m_userData; }
            set { m_userData = value; }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public State<TUserData> CurrentState
        {
            get { return m_CurrentState; }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void Update(DeltaTime deltaTime)
        {
            if(m_CurrentState != null)
            {
                m_CurrentState.OnUpdate(this, deltaTime);
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void ChangeState(State<TUserData> newState)
        {
            Debug.Assert(newState != null);

            if(m_CurrentState != null)
            {
                m_CurrentState.OnExit(this); 
            }

            m_CurrentState = newState;

            m_CurrentState.OnEnter(this);
        }
    }

    [Serializable]
    public class StateMachine
    {
        State m_CurrentState;
        dynamic m_userData;

        public StateMachine(State startingState, dynamic userData)
        {
            m_userData = userData;
            m_CurrentState = startingState;
            m_CurrentState.OnEnter(this);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public dynamic UserData
        {
            get { return m_userData; }
            set { m_userData = value; }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public State CurrentState
        {
            get { return m_CurrentState; }
        }

        public void Update(DeltaTime deltaTime)
        {
            if (m_CurrentState != null)
            {
                m_CurrentState.OnUpdate(this, deltaTime);
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void ChangeState(State newState)
        {
            Debug.Assert(newState != null);

            if (m_CurrentState != null)
            {
                m_CurrentState.OnExit(this);
            }

            m_CurrentState = newState;

            m_CurrentState.OnEnter(this);
        }
    }
}
