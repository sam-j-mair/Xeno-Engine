using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using XenoEngine.GeneralSystems;

namespace XenoEngine.EntitySystem
{
    [Serializable]
    public class SharedDataValue
    {
        dynamic m_Value;
        dynamic m_OldValue;
        bool m_bUpdated;

        public SharedDataValue(Entity entity)
        {
            OwnEntity = entity;
        }

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

    public struct DataPacket
    {
        Dictionary<string, dynamic> m_dataObjects;

        //         public MessageData()
        //         {
        //             m_dataObjects = new Dictionary<string, object>();
        //         }
        // 
        public DataPacket(params DataDefinition[] aData)
        {
            m_dataObjects = new Dictionary<string, object>();

            foreach (DataDefinition dataDef in aData)
            {
                m_dataObjects.Add(dataDef.m_szName, dataDef.m_data);
            }
        }

        public void AddData(string szName, object data)
        {
            LazyInit();

            m_dataObjects.Add(szName, data);
        }

        public object GetData(string szName)
        {
            object value = null;

            LazyInit();
            m_dataObjects.TryGetValue(szName, out value);

            return value;
        }

        private void LazyInit()
        {
            // lazy initialization
            if (m_dataObjects == null)
                m_dataObjects = new Dictionary<string, object>();
        }
    }
}
