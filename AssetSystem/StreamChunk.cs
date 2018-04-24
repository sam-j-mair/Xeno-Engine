using System;
using System.Collections.Generic;


namespace XenoEngine.Systems
{
    //This is a container class for loading up
    //chunks of data.
    [Serializable]
    public class StreamChunk : Asset
    {
        private Dictionary<int, Object> m_assetData;

        public StreamChunk(String szChunkName)
        {
            m_assetData = new Dictionary<int, Object>();
            m_szAssetName = szChunkName;
            m_nRefCount = 1;
        }

        public override void AddAsset(int nHashCode, Object assetObject)
        {
            m_assetData.Add(nHashCode, assetObject);
        }

        public override void UnloadAssets(Object userData)
        {
            m_regionLoader.Unload();
        }

        public override bool Contains(string szAssetName)
        {
            return m_assetData.ContainsKey(szAssetName.GetHashCode());
        }

        public override T GetAssetObjectByName<T>(string szAssetName)
        {
            Object asset = m_assetData[szAssetName.GetHashCode()];

            return (T)asset;
        }
    }
}
