using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using Values = System.Dynamic.ExpandoObject;

namespace XenoEngine.Utilities
{
    public static class DynamicStores
    {
        private static dynamic m_sEnums = new DynamicEnum();
        private static dynamic m_sDefines = new DynamicStore<string>();

        public static dynamic Enums { get { return m_sEnums; } }
        public static dynamic MessageDefines { get { return m_sDefines; } }

    }

    public class DynamicStore : DynamicObject
    {
        protected static Dictionary<string, dynamic> m_defines = new Dictionary<string, dynamic>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            dynamic value;
            result = null;

            if (m_defines.TryGetValue(binder.Name, out value))
            {
                result = value;
                return true;
            }

            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            dynamic outValue;

            if (m_defines.TryGetValue(binder.Name, out outValue))
            {
                value = outValue;
                return true;
            }
            else
            {
                m_defines.Add(binder.Name,  value);
                return true;
            }
        }
    }

    public class DynamicStore<T> : DynamicObject
    {
        protected static Dictionary<string, T> m_defines = new Dictionary<string, T>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            T value;
            result = null;

            if (m_defines.TryGetValue(binder.Name, out value))
            {
                result = value;
                return true;
            }
            
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            T outValue;

            if (m_defines.TryGetValue(binder.Name, out outValue))
            {
                value = outValue; 
                return true;
            }
            else
            {
                m_defines.Add(binder.Name, (T)value);
                return true;
            }
        }
    }

    public class DynamicEnum : DynamicStore<int>
    {
        public static int InvalidValue = -1;
        private static int m_nCurrentValue = 0;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            int nValue;
            result = null;

            if (m_defines.TryGetValue(binder.Name, out nValue))
            {
                result = nValue;
                return true;
            }

            //return false;
            else
            {
                result = m_nCurrentValue;
                m_defines.Add(binder.Name, m_nCurrentValue++);
                return true;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            bool bIsInt = value is int;
            
            if (bIsInt)
            {
                int nValue;
                if (m_defines.TryGetValue(binder.Name, out nValue))
                {
                    if ((int)value == InvalidValue)
                    {
                        m_defines.Remove(binder.Name);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
