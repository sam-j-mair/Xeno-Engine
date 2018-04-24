using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;

//                 StreamChunkLoader chunkLoader = new StreamChunkLoader(ref m_serviceProvider,
//                                                                       szAssetGroupName,
//                                                                       ref m_szRootDirectory);


namespace XenoEngine.Systems
{
    public delegate void AssetLoadedCallback(object assetType);
    
    [Serializable]
    public class AssetLoader
    {
        ILoader             m_iLoader;
        IServiceProvider    m_serviceProvider;
        Dictionary<int, Asset>   m_loadedAssets;

        public AssetLoader(ContentManager globalContentManager,
            IServiceProvider iServiceProvider,
            int nMaxChunks,
            string szRootDirectory)
        {
            m_iLoader = new StreamChunkLoader(globalContentManager.ServiceProvider); ;
            m_serviceProvider = iServiceProvider;
            m_loadedAssets = new Dictionary<int, Asset>(nMaxChunks);
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public AssetLoader(ContentManager globalContentManager,
            IServiceProvider iServiceProvider,
            ILoader iLoader,
            int nMaxChunks,
            string szRootDirectory)
        {
            m_iLoader = iLoader;
            m_serviceProvider = iServiceProvider;
            m_loadedAssets = new Dictionary<int, Asset>(nMaxChunks);
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public void Load(String szAssetGroupName, TaskCompleted callBack)
        {
            TaskHandle taskHandle;

            if(!IsChunkAlreadyLoaded(szAssetGroupName, callBack))
            {
                taskHandle = TaskManager.Instance.CreateTask<Object>(new TaskCompleted[] { AssetLoadedCallBack, callBack }, szAssetGroupName, m_iLoader.Load, false);
                TaskManager.Instance.ExecuteTask(taskHandle);
                
            }
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        private bool IsChunkAlreadyLoaded(String szAssetGroupName, TaskCompleted callback)
        {
            bool bIsLoaded = false;

            if (m_loadedAssets.ContainsKey(szAssetGroupName.GetHashCode()))
            {
                Asset asset = m_loadedAssets[szAssetGroupName.GetHashCode()];
                asset.RefCount++;

                if(callback != null)
                    callback(asset);

                bIsLoaded = true;
            }

            return bIsLoaded;
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        private void AssetLoadedCallBack(Object data)
        {
            Asset asset = data as Asset;
            Debug.WriteLine(asset.AssetName + " Loaded");

            if (!m_loadedAssets.ContainsKey(asset.AssetName.GetHashCode()))
            {
                m_loadedAssets.Add(asset.AssetName.GetHashCode(), asset);
            }
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        private void AssetUnloadedCallBack(Object data)
        {
            Asset asset = data as Asset;
            m_loadedAssets.Remove(asset.AssetName.GetHashCode());
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public Asset GetAssetTypeByName(String szAssestGroupName)
        {
            return m_loadedAssets[szAssestGroupName.GetHashCode()];
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public void Unload(String szAssetGroupName)
        {
            if (IsChunkAlreadyLoaded(szAssetGroupName, null))
            {
                Asset asset = GetAssetTypeByName(szAssetGroupName);

                if (asset.RefCount <= 0)
                {
                    TaskHandle handle;
                    asset.RefCount--;
                    handle = TaskManager.Instance.CreateTask<Object>(AssetUnloadedCallBack, asset, m_iLoader.Unload, false);
                    TaskManager.Instance.ExecuteTask(handle);
                }
            }
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public void UnloadAll()
        {
            Asset[] aAssets = new Asset[m_loadedAssets.Count];
            m_loadedAssets.Values.CopyTo(aAssets, 0);

            foreach (StreamChunk asset in aAssets)
            {
                Unload(asset.AssetName);
            }

            Debug.Assert(m_loadedAssets.Count == 0, "something has failed to unload.");
        }
        //----------------------------------------------------------------------------------
        //Warning: This is relatively slow
        //----------------------------------------------------------------------------------
        public T GetAssetByName<T>(String szAssetName)
        {
            Object assetObject = null;
            Asset[] aAssets = new Asset[m_loadedAssets.Count];
            string szName;

            m_loadedAssets.Values.CopyTo(aAssets, 0);

            foreach (Asset asset in aAssets)
            {
                Debug.WriteLine(asset.AssetName);
                if (asset.Contains(szAssetName))
                    assetObject = asset.GetAssetObjectByName<T>(szAssetName);

                if (assetObject != null)
                    break;

                szName = asset.AssetName;
            }

            if (assetObject == null)
            {
                Debug.WriteLine("WARNING: Asset " + szAssetName + " does not exist....have you loaded it");
                
            }

            return (T)assetObject;
        }

     }
}
