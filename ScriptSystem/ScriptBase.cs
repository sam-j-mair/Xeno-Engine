using System;
using XenoEngine.EntitySystem;

namespace XenoEngine.ScriptSystem
{
    public enum ScriptType
    {
        Compiled,
        Dynamic
    }

#warning I don't like constraining the type to a class ...but we w
    public abstract class ScriptBase<T> : IScriptUpdateable<T>
    {
        public event Action     FireIntialise;
        public event Action     FirePostInitilise;
        public event Action     FirePreUpdate;
        public event Action     FireUpdate;
        public event Action     FirePostUpdate;
        public event Action     FireFinalise;
        public event Action     FireComplete;
        public string           m_szScriptName = null;
        public virtual dynamic  DynamicUserData { get; set; }

        protected ScriptType      m_eScriptType;
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public T UserData { get { return (T)DynamicUserData; } set { DynamicUserData = value; } }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void OnMessageRecieved(dynamic origin, DataPacket dataPkt) { }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual bool GetVariable(string szVariableName, out dynamic variable) { variable = null;  return false; }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual bool SetVariable(string szVariableName, dynamic value) { return false; }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void FireCompleteEvent()
        {
            if (FireComplete != null)
                FireComplete();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void OnInitialise(params VariableInitializer[] initParams)
        {
            if (FireFinalise != null)
                FireIntialise();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void OnPostInitialise()
        {
            if (FirePostInitilise != null)
                FirePostInitilise();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void OnPreUpdate()
        {
            if (FirePreUpdate != null)
                FirePreUpdate();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void OnUpdate(DeltaTime deltaTime)
        {
            if (FireUpdate != null)
                FireUpdate();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void OnPostUpdate()
        {
            if (FirePostUpdate != null)
                FirePostUpdate();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void OnFinalize()
        {
            if (FireFinalise != null)
                FireFinalise();
        }
    }
}
