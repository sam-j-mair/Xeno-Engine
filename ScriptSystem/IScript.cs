using System;
using XenoEngine.ScriptSystem;
using XenoEngine.EntitySystem;

namespace XenoEngine
{
    //Generic
    public interface IScriptUpdateable<T> : IScriptUpdateable
    {
        T UserData { get; set; }
    }
    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    public interface IScriptUpdateable
    {
        dynamic DynamicUserData { get; set; }

        event Action FireIntialise;
        event Action FirePostInitilise;
        event Action FirePreUpdate;
        event Action FireUpdate;
        event Action FirePostUpdate;
        event Action FireFinalise;
        event Action FireComplete;

        void OnMessageRecieved(dynamic origin, DataPacket dataPkt);
        bool SetVariable(string szVariableName, dynamic value);
        bool GetVariable(string szVariableName, out dynamic value);
        void OnInitialise(params VariableInitializer[] initParams);
        void OnPostInitialise();
        void OnPreUpdate();
        void OnUpdate(DeltaTime deltaTime);
        void OnPostUpdate();
        void OnFinalize();
    }
}
