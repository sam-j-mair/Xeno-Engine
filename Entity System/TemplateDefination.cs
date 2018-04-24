using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using XenoEngine.GeneralSystems;

using Microsoft.Xna.Framework;

namespace XenoEngine.EntitySystem
{
    internal struct FieldEntry
    {
        public FieldInfo Field { get; set; }
        public object Value { get; set; }
    }

    internal class Template
    {
        private List<FieldEntry> m_Fields = new List<FieldEntry>();
        private Dictionary<string, dynamic> m_CustomDefines;

        public void AddField(FieldInfo field, object value)
        {
            var newField = new FieldEntry();
            newField.Field = field;
            newField.Value = value;

            m_Fields.Add(newField);
        }

        public void AddDefine(string szName, dynamic value)
        {
            if (m_CustomDefines == null)
            {
                m_CustomDefines = new Dictionary<string, dynamic>();
            }
            m_CustomDefines.Add(szName, value);
        }

        internal List<FieldEntry> Fields { get { return m_Fields; } }
        internal Dictionary<string, dynamic> Defines { get { return m_CustomDefines; } }
    }

    public static class TemplateDefinitions
    {
        public static class TemplateTypes
        {
            private static Dictionary<string, Func<string[], object>> m_Types = new Dictionary<string, Func<string[], object>>();

            static TemplateTypes()
            {
                //These are predefined Types but other types can be added dynamically
                AddType(typeof(int), ConvertValueToInt);
                AddType(typeof(long), ConvertValueToLong);
                AddType(typeof(bool), ConvertValueToBool);
                AddType(typeof(short), ConvertValueToShort);
                AddType(typeof(byte), ConvertValueToByte);
                AddType(typeof(Vector3), ConvertValueToVector3);
                AddType(typeof(Single), ConvertValueToFloat);
                AddType(typeof(string), ConvertValueToString);
                
            }

            public static void AddType(Type type, Func<string[], object> predicate)
            {
                m_Types.Add(type.ToString(), predicate);
            }

            public static Func<string[], object> GetConverter(string szType)
            {
                Func<string[], object> convertMethod;

                if (m_Types.TryGetValue(szType, out convertMethod))
                {
                    return convertMethod;
                }
                return null;
            }

            public static object ConvertValueToString(string[] aszValue)
            {
                return aszValue[0];
            }

            public static object ConvertValueToFloat(string[] aszValue)
            {
                float fValue;
                if (float.TryParse(aszValue[0], out fValue))
                {
                    return fValue;
                }
                return null;
            }

            public static object ConvertValueToBool(string[] aszValue)
            {
                bool bValue;
                if (bool.TryParse(aszValue[0], out bValue))
                {
                    return bValue;
                }
                return null;
            }

            public static object ConvertValueToInt(string[] aszValue)
            {
                int nValue;
                if (int.TryParse(aszValue[0], out nValue))
                {
                    return nValue;
                }
                return null;
            }

            public static object ConvertValueToLong(string[] aszValue)
            {
                long lValue;
                if (long.TryParse(aszValue[0], out lValue))
                {
                    return lValue;
                }
                return lValue;
            }

            public static object ConvertValueToShort(string[] aszValue)
            {
                short sValue;
                if (short.TryParse(aszValue[0], out sValue))
                {
                    return sValue;
                }
                return null;
            }

            public static object ConvertValueToByte(string[] aszValue)
            {
                byte byValue;
                if (byte.TryParse(aszValue[0], out byValue))
                {
                    return byValue;
                }
                return null;
            }

            public static object ConvertValueToVector2(string[] aszValue)
            {
                object nXValue;
                object nYValue;

                nXValue = ConvertValueToFloat(new string[] { aszValue[0] });
                nYValue = ConvertValueToFloat(new string[] { aszValue[1] });

                if (nXValue != null && nYValue != null)
                {
                    return new Vector2((float)nXValue, (float)nYValue);
                }
                return null;
            }

            public static object ConvertValueToVector3(string[] aszValue)
            {
                object nZValue;
                Debug.Assert(aszValue.Length == 3);

                //we use a vector2 conversion to convert the first 2 params,
                //then process the third here.
                Vector2 vec2 = (Vector2)ConvertValueToVector2(aszValue);

                nZValue = ConvertValueToFloat(new string[] { aszValue[2] });

                if (vec2 != null &&
                    nZValue != null)
                {
                    return new Vector3(vec2, (float)nZValue);
                }
                return null;
            }
        }

        private static Dictionary<Type, Template> m_Templates = new Dictionary<Type, Template>();

        public static void PreLoadTemplates()
        {
            string szTemplateDirectory = EngineServices.GetSystem<IGameSystems>().WorkingDirectory + @"\Templates";
            //string szTypeName = GetType().FullName;
            XDocument template = null;
            //Type runtimeType = GetType();
            Assembly assembly = Assembly.GetEntryAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (Directory.Exists(szTemplateDirectory))
                {
                    if (File.Exists(szTemplateDirectory + @"\" + type.FullName + ".xml"))
                    {
                        FileStream fStream = File.OpenRead(szTemplateDirectory + @"\" + type.FullName + ".xml");
                        template = XDocument.Load(fStream);
                        LoadTemplate(type, template);
                    }
                }
            }
            
        }

        private static bool LoadTemplate(Type type, XDocument template)
        {
            bool bResult = false;
            Template newTemplate = new Template();

            if (template != null)
            {
                var fieldsNode = template.Root.Elements("Fields");

                foreach (XElement element in fieldsNode.Elements())
                {
                    if (element.Value == "not defined")
                    {
                        Debug.WriteLine("Element " + element.Name + " doesn't have an assigned value");
                        continue;
                    }

                    FieldInfo member = type.GetField(element.Name.ToString());

                    if (member != null)
                    {
                        XAttribute attr = element.Attribute("Type");
                        var converter = TemplateTypes.GetConverter(attr.Value);

                        if (converter != null)
                        {
                            string[] aszParams = element.Value.Split(",".ToCharArray());
                            object value = converter(aszParams);

                            if (value != null)
                            {
                                newTemplate.AddField(member, value);
                            }
                            else
                            {
                                Debug.Assert(false, "incorrect string format, unable to process value from element: " + element.Name);
                            }
                        }
                        else
                        {
                            Debug.Assert(false, "There is no converter for element:" + attr.Value);
                        }
                    }
                }

                //process custom defines
                var defines = template.Root.Elements("CUSTOM_TEMPLATE_DEFINES");
                foreach (XElement element in defines.Elements())
                {
                    var attr = element.Attribute("Type");
                    var converter = TemplateTypes.GetConverter(attr.Value);

                    if (converter != null)
                    {
                        string[] aszParams = element.Value.Split(",".ToCharArray());
                        dynamic value = converter(aszParams);

                        if (value != null)
                        {
                            newTemplate.AddDefine(element.Name.ToString(), value);
                        }
                        else
                        {
                            Debug.Assert(false, "incorrect string format, unable to process value from element: " + element.Name);
                        }
                    }
                    else
                    {
                        Debug.Assert(false, "There is no converter for element:" + attr.Value);
                    }
                }
            }

            m_Templates.Add(type, newTemplate);

            return bResult;
        }

        public static dynamic GetCustomDefine(Type type, string szDefineName)
        {
            Template template;
            if (m_Templates.TryGetValue(type, out template))
            {
                var defines = template.Defines;

                if (defines != null)
                {
                    dynamic value;
                    if (defines.TryGetValue(szDefineName, out value))
                    {
                        return value;
                    }
                    else
                    {
                        Debug.Assert(false, "No define exists with this name");
                    }
                }
                else
                {
                    Debug.Assert(false, "No defines exists for this type");
                }
            }
            return null;
        }

        public static bool LoadTemplateDefaults(object instance)
        {
            bool bRslt = false;
            Template template;

            if (m_Templates.TryGetValue(instance.GetType(), out template))
            {
                bRslt = true;
                foreach (FieldEntry entry in template.Fields)
                {
                    //NOTE: We spit out a warning out if the value is a
                    //      reference type.
                    Debug.WriteLineIf(entry.Value.GetType().IsClass, entry.Value, "Reference Type warning");
                    entry.Field.SetValue(instance, entry.Value);
                }
            }

            return bRslt;
        }
    }
}
