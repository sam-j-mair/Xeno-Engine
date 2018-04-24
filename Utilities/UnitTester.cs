using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Xeno_Engine.Utilities
{
    public class UnitTestAttribute : Attribute
    {
        public UnitTestAttribute(string Name, int nNumberOfCycles, Action testDelegate)
        {
            TestName = Name;
            NumberOfCycles = nNumberOfCycles;
            TestDelegate = testDelegate;
        }

        public string TestName { get; private set; }
        public int NumberOfCycles { get; private set; }
        public Action TestDelegate { get; private set; }
    }

    static class UnitTester
    {
        private static List<UnitTestAttribute> m_tests = new List<UnitTestAttribute>();

        public static void CreateNewTestSet(Type type, bool bInherit)
        {
            MethodInfo[] aMethods = type.GetMethods();

            foreach (MethodInfo method in aMethods)
            {
                object[] aAttr = method.GetCustomAttributes(typeof(UnitTestAttribute), bInherit);

                foreach (object attr in aAttr)
                {
                    m_tests.Add((UnitTestAttribute)attr);
                }
            }
        }

        public static void ExecuteTests()
        {
            foreach (UnitTestAttribute attr in m_tests)
            {
                try
                {
                    int nCycles = attr.NumberOfCycles;
                    Console.WriteLine("Testing " + attr.TestName);

                    while (0 != nCycles--)
                    {
                        if (attr.TestDelegate != null) attr.TestDelegate();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
            }
        }
    }
}
