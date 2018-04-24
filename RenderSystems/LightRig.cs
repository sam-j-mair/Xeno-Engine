using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace XenoEngine.Systems
{
    //  This class will hold references to all the lights in a scene..
    //  so that that lighting data is globally available.
    //  via singleton/Services access.
    [Serializable]
    public class Light
    {
        public Vector3      Position { get; internal set; }
        public Vector3      Target { get; internal set; }
        public Vector3      Direction { get; internal set; }
        public Color        Colour { get; internal set; }
        public float        Intensity { get; internal set; }
        public float        OuterRadius { get; internal set; }
        public float        InnerRadius { get; internal set; }
    }

    [Serializable]
    public class LightRig
    {
        Dictionary<string, Light> m_lights;

        public LightRig(int nMaxLights)
        {
            m_lights = new Dictionary<string, Light>(nMaxLights);
        }

        //NOTE: This only supports directional lights at the moment as they are quick
        public void CreateLight(string szLightName, Vector3 v3Position, Vector3 v3Target)
        {
            Light newLight = new Light();
            newLight.Position = v3Position;
            newLight.Target = v3Target;
            newLight.Direction = Vector3.Normalize(v3Target - v3Position);

            m_lights.Add(szLightName, newLight);
        }

        public void DestroyLight(string szLightName)
        {
            m_lights.Remove(szLightName);
        }

        public void DestroyAllLights()
        {
            m_lights.Clear();
        }

        public Light[] Lights 
        {
            get 
            { 
                Light[] aLights = new Light[m_lights.Count];
                m_lights.Values.CopyTo(aLights, 0);
                return aLights;
            }
        }

        public int LightCount { get { return m_lights.Count; } }
    }
}
