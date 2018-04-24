using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XenoEngine.Systems.Physics
{
    /// <summary>
    /// Material system for physics
    /// </summary>
    public class Material
    {
        public float LinearDamp { get; set; } //friction
        public float Bounce { get; set; }
    }
}
