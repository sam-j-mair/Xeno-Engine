using System;
using System.Diagnostics;
using XenoEngine.GeneralSystems;

namespace XenoEngine.Systems
{
    public class AssetGroup : Entity
    {
        public  Action<StreamChunk>     LoadCompleted;
        public  Action                  Unloading;
        private String                  m_szAssetGroupName;
        private bool                    m_bLoaded;

        public AssetGroup(String szAssetGroupName)
        {
            m_szAssetGroupName = szAssetGroupName;
            m_bLoaded = false;
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        private void LoadComplete(object streamChunk)
        {
            if (LoadCompleted != null)
                LoadCompleted((StreamChunk)streamChunk);
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        private void StartUnloading()
        {
            if (Unloading != null)
                Unloading();
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public void Load()
        {
            Debug.Assert(!m_bLoaded, "you are trying to load a region that is already loaded.");
            {
                AssetLoader assetLoader = EngineServices.GetSystem<IGameSystems>().AssetLoader;

                assetLoader.Load(m_szAssetGroupName, LoadComplete);
                m_bLoaded = true;
            }
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public void UnLoad()
        {
            Debug.Assert(m_bLoaded, "you are trying to unload a region that is not loaded.");
            {
                AssetLoader assetLoader = EngineServices.GetSystem<IGameSystems>().AssetLoader;

                StartUnloading();
                
                assetLoader.Unload(m_szAssetGroupName);
                m_bLoaded = false;
            }
        }
        
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public new void Dispose()
        {
            AssetLoader assetLoader = EngineServices.GetSystem<IGameSystems>().AssetLoader;
            
            assetLoader.Unload(m_szAssetGroupName);
        }

        public bool Loaded { get { return m_bLoaded; } }

        
    }
}
