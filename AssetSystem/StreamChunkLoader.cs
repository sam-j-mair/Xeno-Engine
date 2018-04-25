#define MT_LOAD

using System;
using System.Collections.Generic;
using System.Threading;
using XmlDataPipeLine;



namespace XenoEngine.Systems
{
    class StreamChunkLoader : ILoader, IDisposable
    {
        IServiceProvider    m_iServerProvider;

        public StreamChunkLoader(IServiceProvider iServerProvider)
        {
            m_iServerProvider = iServerProvider;
        }
        //----------------------------------------------------------------------------------
        //This will be designed to load stream chunks
        //----------------------------------------------------------------------------------
        public void Load(Object task)
        {
            Task<Object> theTask = task as Task<Object>;
            StreamChunk streamChunk = null;
         
            streamChunk = StreamAssets((String)theTask.UserData);
            theTask.ReturnData = streamChunk;
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Stream assets in.
        /// </summary>
        /// <param name="szAssetGroupName"></param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
        private StreamChunk StreamAssets(String szAssetGroupName)
        {
            List<LoadingTask> loadingTasks = new List<LoadingTask>();
            List<ManualResetEvent> resetEvents = new List<ManualResetEvent>();

            RegionLoader regionLoader = new RegionLoader(m_iServerProvider);
            regionLoader.RootDirectory = "Content";
            
            StreamChunkDefinition streamChunkDefinition;
            String szFullPath = szAssetGroupName;
            streamChunkDefinition = regionLoader.Load<StreamChunkDefinition>(szFullPath);

            List<TaskHandle> aTaskHandles = new List<TaskHandle>(streamChunkDefinition.m_assetDefinitions.Count);

            StreamChunk streamChunk = new StreamChunk(szAssetGroupName);

            foreach (AssetDefinition assetDef in streamChunkDefinition.m_assetDefinitions)
            {
#if MT_LOAD
                #region Multi-Threaded Version

                LoadingTask loadTask = new LoadingTask();
                TaskHandle taskHandle = TaskManager.Instance.CreateTask<Object>(new LoadingData(assetDef, regionLoader), loadTask.Load, false);
                aTaskHandles.Add(taskHandle);
                loadingTasks.Add(loadTask);

                #endregion
#else
                #region Single-Thread Version
                //                 Object assetObject;
                //                 String szFullName = assetDef.m_szDirectory + assetDef.m_szAssetName;
                //                 Type creationType;
                // 
                //                 creationType = Type.GetType(assetDef.m_szAssetType, true);
                //                 Debug.Assert(creationType != null);
                // 
                //                 MethodInfo genericMethod = methodInfo.MakeGenericMethod(creationType);
                //                 assetObject = genericMethod.Invoke(contentManager, new object[] { szFullName });
                //                 Debug.Assert(assetObject != null);
                // 
                //                 streamChunk.AddAsset(assetObject);
                #endregion
#endif
            }

            //These two steps could be merged to one...saving memory ...but doing it like this for clarity atm.
            foreach (TaskHandle taskHandle in aTaskHandles)
            {
                TaskManager.Instance.ExecuteTask(taskHandle);
                resetEvents.Add(TaskManager.Instance.GetTaskByHandle(taskHandle).Event);
            }

            WaitHandle.WaitAll(resetEvents.ToArray());
            Console.WriteLine("Load Job done.");

            foreach (LoadingTask task in loadingTasks)
            {
                streamChunk.AddAsset(task.HashCode, task.AssetObject);
            }

            streamChunk.OwnContentManager = regionLoader;

            return streamChunk;
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// unload assets
        /// </summary>
        /// <param name="task"></param>
        //----------------------------------------------------------------------------------
        public void Unload(Object task)
        {
            Task<Object> theTask = task as Task<Object>;
            Asset theAsset = theTask.UserData as Asset;
            TaskHandle handle = TaskManager.Instance.CreateTask<bool>(theAsset, theAsset.UnloadAssets, false);
            TaskManager.Instance.ExecuteTask(handle);
            TaskManager.Instance.GetTaskByHandle(handle).Event.WaitOne();
            theTask.ReturnData = theAsset;
        }
    }
}
