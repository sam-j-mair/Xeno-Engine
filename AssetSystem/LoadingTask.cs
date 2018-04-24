using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using XmlDataPipeLine;

namespace XenoEngine.Systems
{
    //Maybe there is a way to abstract this out further.
    
    class LoadingTask
    {
        //ManualResetEvent    m_resetEvent;
        private Object              m_assetObject;
        private int                 m_nHashCode;

        public LoadingTask()
        {
            //m_resetEvent = resetEvent;
        }
        
        public void Load(object task)
        {
            Task<Object> theTask = task as Task<Object>;
            Type creationType;
            Object assetObject;
            LoadingData data = theTask.UserData as LoadingData;

            Type contentType = data.m_contentLoader.GetType();
            MethodInfo methodInfo = contentType.GetMethod("Load");
            String szFullName = data.m_assetDefinition.m_szDirectory + data.m_assetDefinition.m_szAssetName;

            creationType = Type.GetType(data.m_assetDefinition.m_szAssetType, true);
            Debug.Assert(creationType != null);

            MethodInfo genericMethod = methodInfo.MakeGenericMethod(creationType);

            Console.WriteLine("loading asset " + szFullName);

            assetObject = genericMethod.Invoke(data.m_contentLoader, new object[] { szFullName });
            Debug.Assert(assetObject != null);

            m_assetObject = assetObject;
            m_nHashCode = data.m_assetDefinition.m_szAssetName.GetHashCode();

            theTask.Event.Set();
        }

        public Object AssetObject { get { return m_assetObject; } }
        public int HashCode { get { return m_nHashCode; } }
        
    }

    class LoadingData 
    {
        public AssetDefinition  m_assetDefinition;
        public ContentManager   m_contentLoader;

        public LoadingData(AssetDefinition assetDefinition, ContentManager contentLoader)
        {
            m_assetDefinition = assetDefinition;
            m_contentLoader = contentLoader;
        }
    }
}
