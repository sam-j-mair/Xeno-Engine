using System;

namespace XenoEngine.Systems
{
    public interface ILoader
    {
        void Load(Object task);
        void Unload(Object data);
    }

    public abstract class Asset
    {
        protected String m_szAssetName;
        protected RegionLoader m_regionLoader;
        protected int m_nRefCount;
        protected bool m_bLoading;

        public abstract void AddAsset(int nHashCode, Object assetObject);
        public abstract void UnloadAssets(Object userData);
        public abstract bool Contains(string szAssetName);
        public abstract T GetAssetObjectByName<T>(string szAssetName);

        public RegionLoader OwnContentManager { get { return m_regionLoader; } set { m_regionLoader = value; } }
        public String AssetName { get { return m_szAssetName; } }
        public int RefCount { get { return m_nRefCount; } set { m_nRefCount = value; } }
        public bool Loading { get { return m_bLoading; } }
    }
}
