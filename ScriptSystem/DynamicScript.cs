using System.IO;
using Microsoft.Scripting.Hosting;
using XenoEngine.EntitySystem;

namespace XenoEngine.ScriptSystem
{
    public struct VariableInitializer
    {
        string  m_szVariableName;
        dynamic m_value;

        public VariableInitializer(string szVariableName,
                                    dynamic value)
        {
            m_szVariableName = szVariableName;
            m_value = value;
        }

        public string VarName { get { return m_szVariableName; } }
        public dynamic Value { get { return m_value; } }
    }
    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    public class DynamicScript : ScriptBase<dynamic>
    {
        private dynamic m_pythonScript;

        public DynamicScript(string szScriptName)
        {
            m_szScriptName = szScriptName;

            string szPath = Path.GetFullPath("Scripts");
            m_pythonScript = IronPython.GetRuntime().UseFile(szPath + "\\" + m_szScriptName + ".py");
            m_eScriptType = ScriptType.Dynamic;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void OnMessageRecieved(dynamic origin, DataPacket dataPkt)
        {
            m_pythonScript.OnMessageRecieved(origin, dataPkt);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override bool GetVariable(string szVariableName, out dynamic variable)
        {
            return m_pythonScript.TryGetVariable(szVariableName, out variable);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override bool SetVariable(string szVariableName, dynamic value)
        {
            bool bSuccess = false;

            if (m_pythonScript.ContainsVariable(szVariableName))
            {
                m_pythonScript.SetVariable(szVariableName, value);
                bSuccess = true;
            }

            return bSuccess;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected override void FireCompleteEvent()
        {
            base.FireCompleteEvent();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void OnInitialise(params VariableInitializer[] initParams)
        {
            ScriptScope functionScope = m_pythonScript.CreateScope();
            
            //foreach (VariableInitializer var in initParams)
            {
                functionScope.SetVariable("initParams", initParams);
            }

            m_pythonScript.OnInitialize(initParams);
            base.OnInitialise(initParams);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void OnPostInitialise()
        {
            m_pythonScript.OnPostInitialize();
            base.OnPostInitialise();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void OnPreUpdate()
        {
            m_pythonScript.OnPreUpdate();
            base.OnPreUpdate();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void OnUpdate(DeltaTime deltaTime)
        {
            var target = m_pythonScript.GetVariable("ITarget");
            m_pythonScript.OnUpdate(deltaTime);
            base.OnUpdate(deltaTime);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void OnPostUpdate()
        {
            m_pythonScript.OnPostUpdate();
            base.OnPostUpdate();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void OnFinalize()
        {
            m_pythonScript.OnFinalize();
            base.OnFinalize();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override string ToString()
        {
            return m_szScriptName;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override dynamic DynamicUserData
        {
            get
            {
                return base.DynamicUserData;
            }
            set
            {
                m_pythonScript.SetVariable("owner", value);
                base.DynamicUserData = value;
            }
        }
    }
}
    

