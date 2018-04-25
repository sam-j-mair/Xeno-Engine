using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using XenoEngine.GeneralSystems;

namespace XenoEngine.EntitySystem
{
    /// <summary>
    /// SharedDataValue is used for flushing data automaticly across the network
    /// </summary>
    [Serializable]
    public class SharedDataValue
    {
        dynamic m_Value;
        dynamic m_OldValue;
        bool m_bUpdated;

        //-------------------------------------------------------------------------------
        /// <summary>
        /// C/TOR
        /// </summary>
        /// <param name="entity">the entity that owns this data.</param>
        //-------------------------------------------------------------------------------
        public SharedDataValue(Entity entity)
        {
            OwnEntity = entity;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// gets and set the value of the data. It checks that the entity is authoritative before we set the data...and we also flag that the value has been updated.
        /// </summary>
        //-------------------------------------------------------------------------------
        public dynamic Value
        {
            get { return m_Value; }
            set
            {
                Debug.Assert(OwnEntity.IsInitialized, "This value is not initialized yet, you should put this in the post initialize method");
                if (EngineServices.GetSystem<IGameSystems>().EntityController.IsAuthoritative(OwnEntity))
                {
                    if (m_OldValue != m_Value)
                    {
                        m_OldValue = m_Value;
                        m_Value = value;
                        m_bUpdated = true;
                    }
                }
            }
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// check if the value has been updated.
        /// </summary>
        //-------------------------------------------------------------------------------
        public bool Updated
        {
            get
            {
                bool bResult = m_bUpdated;
                m_bUpdated = false;
                return bResult;
            }
        }

        internal Entity OwnEntity { get; set; }
        public bool IsNetworked { get; set; }
    }
    //-------------------------------------------------------------------------------
    /// <summary>
    /// store the name of data and the data its self.
    /// </summary>
    //-------------------------------------------------------------------------------
    public struct DataDefinition
    {
        public string m_szName;
        public object m_data;

        public DataDefinition(String szName, object data)
        {
            m_data = data;
            m_szName = szName;
        }
    }
    //-------------------------------------------------------------------------------
    /// <summary>
    /// specilized data packets for shared data, this is in order to make sending the shared data more efficent.
    /// </summary>
    //-------------------------------------------------------------------------------
    public struct DataPacket
    {
        Dictionary<string, dynamic> m_dataObjects;

        //-------------------------------------------------------------------------------
        /// <summary>
        /// C/TOR
        /// </summary>
        /// <param name="aData">the data to packeted up.</param>
        //-------------------------------------------------------------------------------
        public DataPacket(params DataDefinition[] aData)
        {
            m_dataObjects = new Dictionary<string, object>();

            foreach (DataDefinition dataDef in aData)
            {
                m_dataObjects.Add(dataDef.m_szName, dataDef.m_data);
            }
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// add more data after creation.
        /// </summary>
        /// <param name="szName">name of data.</param>
        /// <param name="data">the data.</param>
        //-------------------------------------------------------------------------------
        public void AddData(string szName, object data)
        {
            LazyInit();

            m_dataObjects.Add(szName, data);
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// gets the data of a particular name.
        /// </summary>
        /// <param name="szName">name of data to be retreived.</param>
        /// <returns></returns>
        //-------------------------------------------------------------------------------
        public object GetData(string szName)
        {
            object value = null;

            LazyInit();
            m_dataObjects.TryGetValue(szName, out value);

            return value;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// only initialized the data array if needed.
        /// </summary>
        //-------------------------------------------------------------------------------
        private void LazyInit()
        {
            // lazy initialization
            if (m_dataObjects == null)
                m_dataObjects = new Dictionary<string, object>();
        }
    }
}
