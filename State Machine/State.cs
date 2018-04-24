using System;
using Microsoft.Xna.Framework;

namespace XenoEngine.Systems
{
    //this class is designed to be overridden.
    //this could be done with delegates.
    [Serializable]
    public abstract class State<TUserData>
    {
        public abstract void OnEnter(StateMachine<TUserData> stateMachine);
        public abstract void OnUpdate(StateMachine<TUserData> stateMachine, DeltaTime deltaTime);
        public abstract void OnExit(StateMachine<TUserData> stateMachine);
    }

    [Serializable]
    public abstract class State
    {
        public abstract void OnEnter(StateMachine stateMachine);
        public abstract void OnUpdate(StateMachine stateMachine, DeltaTime deltaTime);
        public abstract void OnExit(StateMachine stateMachine);
    }
}
