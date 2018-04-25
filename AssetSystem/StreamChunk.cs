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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Add an asset to eht chunk
        /// </summary>
        /// <param name="nHashCode">hashcode to enter under.</param>
        /// <param name="assetObject">object to store.</param>
        //----------------------------------------------------------------------------------
        public override void AddAsset(int nHashCode, Object assetObject)
        {
            m_assetData.Add(nHashCode, assetObject);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// unload assets for this chunk.
        /// </summary>
        /// <param name="userData"></param>
        //----------------------------------------------------------------------------------
        public override void UnloadAssets(Object userData)
        {
            m_regionLoader.Unload();
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Check if an asset belongs to this chunk.
        /// </summary>
        /// <param name="szAssetName">name of asset</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
        public override bool Contains(string szAssetName)
        {
            return m_assetData.ContainsKey(szAssetName.GetHashCode());
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// gets and returns the asset from this chunk
        /// </summary>
        /// <typeparam name="T">the type of the asset</typeparam>
        /// <param name="szAssetName">the name of the asset</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
        public override T GetAssetObjectByName<T>(string szAssetName)
        {
            Object asset = m_assetData[szAssetName.GetHashCode()];

            return (T)asset;
        }
    }
}
