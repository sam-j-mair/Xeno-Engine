using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using XenoEngine.GeneralSystems;

namespace XenoEngine.Serialization
{
    [Serializable]
    public abstract class SerializableComponent
    {
        private Dictionary<string, dynamic> m_SerializationHelper;
        private string m_szFileName;
        //------------------------------------------------------
        //------------------------------------------------------
        #region Serialization
        protected void SaveSerializationData(string szName, dynamic data)
        {
            if (m_SerializationHelper != null)
            {
                m_SerializationHelper.Add(szName, data);
            }
            else
            {
                m_SerializationHelper = new Dictionary<string, dynamic>();
            }
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected bool RestoreSerializationData(string szName, out dynamic data)
        {
            bool bResult = false;
            data = null;

            try
            {
                bResult = m_SerializationHelper.TryGetValue(szName, out data);
            }
            catch (System.Exception ex)
            {
                Debug.Assert(false, "No data is available because nothing was saved. " + "Error: " + ex.Message);
            }

            return bResult;
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected dynamic RestoreSerializationData(string szName)
        {
            dynamic data = null;

            try
            {
                m_SerializationHelper.TryGetValue(szName, out data);
            }
            catch (System.Exception ex)
            {
                Debug.Assert(false, "No data is available because nothing was saved. " + "Error: " + ex.Message);
            }

            return data;
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected dynamic UpdateSavedData(string szName, dynamic newData)
        {
            dynamic data = m_SerializationHelper[szName];
            m_SerializationHelper[szName] = newData;
            return data;
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected void ClearValue(string szName)
        {
            m_SerializationHelper.Remove(szName);
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected void DestroyData()
        {
            m_SerializationHelper.Clear();
            m_SerializationHelper = null;
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected void SerializeData(string szFileName = null)
        {
            if (m_SerializationHelper != null && m_SerializationHelper.Count > 0)
            {
                if(szFileName == null)
                {
                    if (string.IsNullOrEmpty(m_szFileName))
                    {
                        m_szFileName = Path.GetRandomFileName();
                    }
                }

                using (FileStream file = File.Create(szFileName == null ? m_szFileName : szFileName))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(file, m_SerializationHelper);
                    file.Flush();
                }
            }
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected void DeserializeData(string szFileName = null)
        {
            using (FileStream file = File.OpenRead(szFileName == null ? m_szFileName : szFileName))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                m_SerializationHelper = formatter.Deserialize(file) as Dictionary<string, dynamic>; ;
                file.Flush();
            }
        }
        #endregion
        //------------------------------------------------------
        //------------------------------------------------------
        [OnSerializing]
        protected void OnSerializing(StreamingContext context)
        {
            EngineServices.GetSystem<IGameSystems>().Components.Remove((IGameComponent)this);
        }
        //------------------------------------------------------
        //------------------------------------------------------
        [OnSerialized]
        protected void OnSerialized(StreamingContext context)
        {
            EngineServices.GetSystem<IGameSystems>().Components.Add((IGameComponent)this);
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected void OnDeserialized(StreamingContext context)
        {
            EngineServices.GetSystem<IGameSystems>().Components.Add((IGameComponent)this);
        }
    }
}
