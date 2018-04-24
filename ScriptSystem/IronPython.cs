using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace XenoEngine.ScriptSystem
{
    public static class IronPython
    {
        private static ScriptEngine m_engine;
        private static ScriptRuntime m_scriptRuntime;

        static IronPython()
        {
            m_engine = Python.CreateEngine();
            m_scriptRuntime = Python.CreateRuntime();
        }

        public static ScriptEngine GetScriptEngine()
        {
            return m_engine;
        }

        public static ScriptRuntime GetRuntime()
        {
            return m_scriptRuntime;
        }
    }
}
