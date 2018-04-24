using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace XenoEngine.EntitySystem
{
    public class TemplateGenerator
    {
        public TemplateGenerator()
        {
            var assembly = Assembly.GetEntryAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if(type.BaseType != null)
                {
                    if (type.BaseType == typeof(Entity))
                    {

                    }
                }
            }
        }
    }
}
