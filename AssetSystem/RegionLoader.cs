using System;
using Microsoft.Xna.Framework.Content;


namespace XenoEngine.Systems
{
    //This is a special content manager that tracks assets by regions.
    //its specialized ..but more efficient that a more generalized version

    //Two samples...this is an instanced version.
    [Serializable]
    public class RegionLoader : ContentManager
    {
        //This is a blank dummy class
        Object m_lockObject;

        public RegionLoader(IServiceProvider iService) : base(iService)
        {
            m_lockObject = new Object();
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public override T Load<T>(string assetName)
        {
            T asset;

            lock(m_lockObject)
            {
                asset = base.Load<T>(assetName);
            }

            return (T)asset;
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public override void Unload()
        {
            lock(m_lockObject)
            {
                base.Unload();
            }
        }
    }
}
