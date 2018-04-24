using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;
using XenoEngine.GeneralSystems;
using XenoEngine.Network;
using XenoEngine.Serialization;
using Xeno_Engine.Utilities;
using XenoEngine.Utilities;
using System.Dynamic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using XenoEngine.EntitySystem;

namespace XenoEngine
{
    public struct EntityMsgRslt
    {
        private Entity m_sendingEntity;
        private Entity m_receivingEntity;
        private Rslt m_nRslt;

        public EntityMsgRslt(Entity sendingEntity, Entity receiveEntity, Rslt rslt)
        {
            m_sendingEntity = sendingEntity;
            m_receivingEntity = receiveEntity;
            m_nRslt = rslt;
        }

        public Entity SendingEntity { get { return m_sendingEntity; } }
        public Entity ReceivingEntity { get { return m_receivingEntity; } }
        public Rslt RSLT { get { return m_nRslt; } }
    }

    

    public struct EntityIdentification
    {
        private int m_nAuthoritativeID;
        private int m_nEntityID;

        public EntityIdentification(int nEntityID, int nAuthoritativeID)
        {
            m_nAuthoritativeID = nAuthoritativeID;
            m_nEntityID = nEntityID;
        }

        public int AuthoritativeID { get { return m_nAuthoritativeID; } }
        public int EntityID { get { return m_nEntityID; } }
    }

    

    public enum Status
    {
        INITILIZING = 0,
        PENDING_DELETION = 1,
        DELETED = 2
    }

    [Serializable]
    public abstract class Entity : SerializableComponent, IGameComponent, IUpdateable, IDisposable
    {
        #region members
        public event EventHandler<EventArgs>                        EnabledChanged, UpdateOrderChanged, DrawOrderChanged, VisibleChanged, Disposed;

        private byte                                                m_nDeviceID;
        private int                                                 m_nUpdateOrder, m_idParentID, m_idEntityID;
        private bool                                                m_bEnabled;
        private BitArray                                            m_flags;
        private List<int>                                           m_childList;

        [NonSerialized] private Dictionary<string, FieldInfo>       m_sharedData;

        private static int                                          s_idEntityIdCount = 1;

        #endregion
             
        //------------------------------------------------------
        //------------------------------------------------------
        #region con/de-structor
        protected Entity()
        {
            m_nDeviceID = 0;
            m_idEntityID = s_idEntityIdCount++;

            m_idParentID = -1;
            m_childList = new List<int>();
            Active = true;
            
            m_flags = new BitArray(3);
            m_flags.Set((int)Status.PENDING_DELETION, false);
            m_flags.Set((int)Status.DELETED, false);
            m_flags.Set((int)Status.INITILIZING, true);

            TemplateDefinitions.LoadTemplateDefaults(this);

            EngineServices.GetSystem<IGameSystems>().Components.Add(this);
        }
        //------------------------------------------------------
        //------------------------------------------------------
        ~Entity()
        {
            Dispose(false);
        }
        #endregion
        
        #region properties
        //------------------------------------------------------
        //------------------------------------------------------
        public int EntityID { get { return m_idEntityID; } }
        public byte AuthoritativeID { get { return m_nDeviceID; } set { m_nDeviceID = value; } }
        public int ParentID { get { return m_idParentID; } set { m_idParentID = value; } }
        public bool Active { get { return Enabled; } set { Enabled = value; } }
        public BitArray Flags { get { return m_flags; } }
        public bool IsValid { get { return (!m_flags[0] && !m_flags[1]); } }
        public bool IsInitialized { get { return !m_flags[0]; } }

        public bool Enabled
        {
            get { return m_bEnabled; }
            set 
            {
                if (m_bEnabled != value)
                {
                    m_bEnabled = value;
                    if (EnabledChanged != null) EnabledChanged(this, EventArgs.Empty);
                }
            } 
        }

        public int UpdateOrder
        {
            get { return m_nUpdateOrder; }
            set
            {
                if (m_nUpdateOrder != value)
                {
                    m_nUpdateOrder = value;
                    if (UpdateOrderChanged != null) UpdateOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        public Dictionary<string, FieldInfo> SharedData
        {
            get { return m_sharedData; }
            set { m_sharedData = value; }
        }


        //------------------------------------------------------
        //------------------------------------------------------
        #endregion
        //------------------------------------------------------
        //------------------------------------------------------
        #region methods
        public void AddChild(Entity entity)
        {
            m_childList.Add(entity.m_idEntityID);
        }
        //------------------------------------------------------
        //------------------------------------------------------
        public void RemoveChild(Entity entity)
        {
            m_childList.Remove(entity.m_idEntityID);
        }
        //------------------------------------------------------
        // The IsNetworkMessage being optional could make this open to abuse as it allows the consumer to send it.
        // maybe the flags should be made private?
        //------------------------------------------------------
        public EntityMsgRslt SendMessage(String szMessageName, Entity entity, object msgData = null, bool IsNetworkMessage = false)
        {
            Rslt rslt = new Rslt();

            rslt |= DynamicStores.Enums.NonAuthoritative;
            rslt |= DynamicStores.Enums.Invalid_Sender;
            rslt |= DynamicStores.Enums.Invalid_Receiver;
//             rslt |= ResultValue.NonAuthoritative;
//             rslt |= ResultValue.Invalid_Sender;
//             rslt |= ResultValue.Invalid_Receiver;

            if (IsValid && entity.IsValid && entity.IsAuthoritative())
            {
                MethodInfo methodInfo = null;
                Type type = entity.GetType();

                methodInfo = type.GetMethod(szMessageName);

                if (methodInfo == null)
                {
                    Console.WriteLine("Message '" + szMessageName + "' does not exist for entity type " + type.ToString());
                    rslt.ClearRslt();
                    rslt |= DynamicStores.Enums.Fail;
                }
                else
                {
                    Object[] aMsgData = msgData != null ? new Object[2] : new Object[1];
                    aMsgData[0] = this; //This is the sending entity.
                    
                    if(msgData != null)
                        aMsgData[1] = msgData; //any data sent with it...this can be a struct etc.

                    methodInfo.Invoke(entity, aMsgData);

                    if(!IsNetworkMessage)
                    {
                        NetworkEngine network = EngineServices.GetSystem<IGameSystems>().Network;
                        //send this through the network.
                        if (network.IsNetworkActive)
                        {
                            NetworkDataPacket packet = new NetworkDataPacket();

                            packet.EntityIDs.SendingEntityID = this.EntityID;
                            packet.EntityIDs.ReceiveEntityID = entity.EntityID;
                            packet.MessageType = MessageType.EntityMessage;
                            packet.SendMethod = NetworkSendMethod.InOrder;
                            packet.UserData = msgData;

                            network.SendMessage(packet);
                        }
                    }

                    rslt.ClearRslt();
                    rslt |= DynamicStores.Enums.Success;
                }
            }

            return new EntityMsgRslt(this, entity, rslt);
        }
        //------------------------------------------------------
        //------------------------------------------------------
        public EntityIdentification GetIdentStruct()
        {
            return new EntityIdentification(m_idEntityID, m_nDeviceID);
        }
        //------------------------------------------------------
        //------------------------------------------------------
        // make this a property
        public int GetParentID()
        {
            Debug.Assert(!m_flags[(int)Status.INITILIZING], "The entity is still initializing.");
            return m_idParentID;
        }

        //------------------------------------------------------
        // make this private or protected public
        //------------------------------------------------------
        public List<int> GetChildList()
        {
            return m_childList;
        }
        //------------------------------------------------------
        //------------------------------------------------------'
        public bool IsAuthoritative()
        {
            return AuthoritativeID == 0;
        }
        #endregion
        //------------------------------------------------------
        //------------------------------------------------------
        #region general methods
        public virtual void Initialize() { }

        public virtual void PostInitialize() { }

        //-------------------------------------
        //-------------------------------------
        void IUpdateable.Update(GameTime gameTime)
        {
            Update(new DeltaTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime));
        }
        //-------------------------------------
        //-------------------------------------
        public virtual void Update(DeltaTime gameTime) { }
        //-------------------------------------
        //-------------------------------------

        public virtual void OnDestroy() { }

        protected virtual void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                bool bFlag = false;
                try
                {
                    Monitor.Enter(this, ref bFlag);
                    if (EngineServices.GetSystem<IGameSystems>() != null)
                    {
                        EngineServices.GetSystem<IGameSystems>().Components.Remove(this);
                    }

                    if (Disposed != null) Disposed(this, EventArgs.Empty);
                }
                finally
                {
                    if (bFlag)
                    {
                        Monitor.Exit(this);
                   }
                }
            }
        }
        //-------------------------------------
        //-------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public virtual string FullName
        {
            get { return this.GetType().FullName; }
        }
        //------------------------------------------------------
        //------------------------------------------------------
        #region static methods

        #endregion
    }
}
