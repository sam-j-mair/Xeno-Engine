using System;
using System.Reflection;
using XenoEngine.ScriptSystem;
using XenoEngine.EntitySystem;


namespace XenoEngine
{
    public abstract class CompiledScript<T> : ScriptBase<T>
    {

        //public event Action FireMessageProccessed;
        public CompiledScript()
        {
            m_eScriptType = ScriptType.Compiled;
        }

        public override void OnMessageRecieved(dynamic origin, DataPacket msgData)
        {
            Type type = GetType();

            MethodInfo methodInfo = type.GetMethod((string)msgData.GetData("msg_name"));

            if (methodInfo != null)
            {
                dynamic sendingThroughEntity = msgData.GetData("sendingOrigin");
                object[] aMsg = new object[2];
                aMsg[0] = sendingThroughEntity != null ? sendingThroughEntity : origin;
                aMsg[1] = msgData;

                methodInfo.Invoke(this, aMsg);
            }
        }

        public override bool GetVariable(string szVariableName, out dynamic variable)
        {
            Type type = GetType();
            FieldInfo field = type.GetField(szVariableName);
            bool bSuccess = false;
            
            variable = null;

            if (field != null)
            {
                variable = field.GetValue(this);
                bSuccess = true;
            }

            return bSuccess;
        }

        public override bool SetVariable(string szVariableName, dynamic value)
        {
            Type type = GetType();
            FieldInfo field = type.GetField(szVariableName);
            bool bSuccess = false;

            value = null;

            if (field != null)
            {
                field.SetValue(this, value);
                bSuccess = true;
            }

            return bSuccess;
        }

        protected override void FireCompleteEvent()
        { 
            base.FireCompleteEvent();
        }

        public override void OnInitialise(params VariableInitializer[] initParams)
        {
            base.OnInitialise(initParams);
        }

        public override void OnPostInitialise()
        {
            base.OnPostInitialise();
        }

        public override void OnPreUpdate()
        {
            base.OnPreUpdate();
        }

        public override void OnUpdate(DeltaTime deltaTime)
        {
            base.OnUpdate(deltaTime);
        }

        public override void OnPostUpdate()
        {
            base.OnPostUpdate();
        }

        public override  void OnFinalize()
        {
            base.OnFinalize();
        }

        public override string ToString()
        {
            return this.GetType().ToString();
        }
    }
}
