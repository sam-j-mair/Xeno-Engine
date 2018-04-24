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
        public abstract void AddAsset(int nHashCode, Object assetObject);
        public abstract void UnloadAssets(Object userData);
        public abstract bool Contains(string szAssetName);
        public abstract T    GetAssetObjectByName<T>(string szAssetName);

        public RegionLoader OwnContentManager { get; set; }
        public String       AssetName { get; }
        public int          RefCount { get; set; }
        public bool         Loading { get; }
    }
}
