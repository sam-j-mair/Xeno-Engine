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
    //----------------------------------------------------------------------------------
    /// <summary>
    /// A struct for returning the details and result of an Entity message.
    /// </summary>
    //----------------------------------------------------------------------------------
    public struct EntityMsgRslt
    {
        private Entity m_sendingEntity;
        private Entity m_receivingEntity;
        private Rslt m_nRslt;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="sendingEntity">Entity that is sent the message</param>
        /// <param name="receiveEntity">Target entity that received the message</param>
        /// <param name="rslt"></param>
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

    //----------------------------------------------------------------------------------
    /// <summary>
    /// Entity identification struct holds both authoritative ID and regular Entity ID.
    /// This is just a POD object.
    /// </summary>
    //----------------------------------------------------------------------------------
    public struct EntityIdentification
    {
        private int m_nAuthoritativeID;
        private int m_nEntityID;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="nEntityID">The defined entity ID</param>
        /// <param name="nAuthoritativeID">The Authoritative ID of the machine/client.</param>
        public EntityIdentification(int nEntityID, int nAuthoritativeID)
        {
            m_nAuthoritativeID = nAuthoritativeID;
            m_nEntityID = nEntityID;
        }

        public int AuthoritativeID { get { return m_nAuthoritativeID; } }
        public int EntityID { get { return m_nEntityID; } }
    }

    //----------------------------------------------------------------------------------
    /// <summary>
    /// Status of entity creation and deletion.
    /// </summary>
    //----------------------------------------------------------------------------------
    public enum Status
    {
        INITILIZING = 0,
        PENDING_DELETION = 1,
        DELETED = 2
    }

    //----------------------------------------------------------------------------------
    /// <summary>
    /// This is the main base class that all entities inherit from.
    /// </summary>
    //----------------------------------------------------------------------------------
    [Serializable]
    public abstract class Entity : SerializableComponent, IGameComponent, IUpdateable, IDisposable
    {
        #region members
        
        //private members
        private byte                            m_nDeviceID;
        private int                             m_nUpdateOrder,
                                                m_idParentID,
                                                m_idEntityID;

        private bool                            m_bEnabled;
        private BitArray                        m_flags;
        private List<int>                       m_childList;

        [NonSerialized]
        private Dictionary<string, FieldInfo>   m_sharedData;

        //static members
        private static int                      s_idEntityIdCount = 1;

        //event handlers
        public event EventHandler<EventArgs>    EnabledChanged,
                                                UpdateOrderChanged, 
                                                DrawOrderChanged,
                                                VisibleChanged,
                                                Disposed;

        #endregion

        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        #region con/de-structor
        protected Entity()
        {
            //the device id is where we get the authoritative ID from.
            m_nDeviceID = 0;
            //the Entity id ...which is allocated from a static member across all entities and incremented.
            m_idEntityID = s_idEntityIdCount++;

            //set defaults.
            m_idParentID = -1;
            m_childList = new List<int>();
            Active = true;

            //flags for storing status.
            m_flags = new BitArray(3);
            m_flags.Set((int)Status.PENDING_DELETION, false);
            m_flags.Set((int)Status.DELETED, false);
            m_flags.Set((int)Status.INITILIZING, true);

            //if there is a template definition file (XML) for this entity type.
            //they get loaded here..
            TemplateDefinitions.LoadTemplateDefaults(this);

            //Add the entity to the update loop.
            EngineServices.GetSystem<IGameSystems>().Components.Add(this);
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        ~Entity()
        {
            Dispose(false);
        }
        #endregion

        #region properties
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public int      EntityID { get { return m_idEntityID; } }
        public byte     AuthoritativeID { get { return m_nDeviceID; } set { m_nDeviceID = value; } }
        public int      ParentID { get { return m_idParentID; } set { m_idParentID = value; } }
        public bool     Active { get { return Enabled; } set { Enabled = value; } }
        public BitArray Flags { get { return m_flags; } }
        public bool     IsValid { get { return (!m_flags[0] && !m_flags[1]); } }
        public bool     IsInitialized { get { return !m_flags[0]; } }
        //----------------------------------------------------------------------------------
        // Whether the Entity is enabled in the update loop or not.
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        // Get or Set update order.
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        // Get or Set  SharedData values. This data is automatically updated across all slaves from the master
        // is controlled and updated via the EntityController for networking purposes
        //----------------------------------------------------------------------------------
        public Dictionary<string, FieldInfo> SharedData
        {
            get { return m_sharedData; }
            set { m_sharedData = value; }
        }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public virtual string FullName
        {
            get { return this.GetType().FullName; }
        }
        #endregion

        #region methods
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Adding a existing entity as a child to another entity.
        /// </summary>
        /// <param name="entity">The entity to add as a child.</param>
        //----------------------------------------------------------------------------------
        public void AddChild(Entity entity)
        {
            m_childList.Add(entity.m_idEntityID);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// remove a child entity.
        /// </summary>
        /// <param name="entity">entity to remove.</param>
        //----------------------------------------------------------------------------------
        public void RemoveChild(Entity entity)
        {
            m_childList.Remove(entity.m_idEntityID);
        }
        //----------------------------------------------------------------------------------
        // The IsNetworkMessage being optional could make this open to abuse as it allows the consumer to send it.
        // maybe the flags should be made private?
        /// <summary>
        /// Send entity message. The message will get pushed across the network to all slaves if there any available.
        /// </summary>
        /// <param name="szMessageName">Name of message.</param>
        /// <param name="entity">The entity we are sending this message to.</param>
        /// <param name="msgData">Data to sent with the message. (optional)</param>
        /// <param name="IsNetworkMessage">Is this being sent as a network message. (optional)</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
        public EntityMsgRslt SendMessage(String szMessageName, Entity entity, object msgData = null, bool IsNetworkMessage = false)
        {
            Rslt rslt = new Rslt();

            //set flags
            rslt |= DynamicStores.Enums.NonAuthoritative;
            rslt |= DynamicStores.Enums.Invalid_Sender;
            rslt |= DynamicStores.Enums.Invalid_Receiver;
            
            //check entity is valid and authoritative.
            if (IsValid && entity.IsValid && entity.IsAuthoritative())
            {
                //use reflection to get method call.
                MethodInfo methodInfo = null;
                Type type = entity.GetType();

                methodInfo = type.GetMethod(szMessageName);

                if (methodInfo == null)
                {
                    //If this method doesn't exist.
                    Console.WriteLine("Message '" + szMessageName + "' does not exist for entity type " + type.ToString());
                    rslt.ClearRslt();
                    rslt |= DynamicStores.Enums.Fail;
                }
                else
                {
                    //pack data into and message header in an object array.
                    Object[] aMsgData = msgData != null ? new Object[2] : new Object[1];
                    aMsgData[0] = this; //This is the sending entity.

                    if (msgData != null)
                        aMsgData[1] = msgData; //any data sent with it...this can be a struct etc.

                    methodInfo.Invoke(entity, aMsgData);

                    if (!IsNetworkMessage)
                    {
                        NetworkEngine network = EngineServices.GetSystem<IGameSystems>().Network;
                        //send this through the network.
                        if (network.IsNetworkActive)
                        {
                            //create network packet.
                            NetworkDataPacket packet = new NetworkDataPacket();

                            //set up packet.
                            packet.EntityIDs.SendingEntityID = this.EntityID;
                            packet.EntityIDs.ReceiveEntityID = entity.EntityID;
                            packet.MessageType = MessageType.EntityMessage;
                            packet.SendMethod = NetworkSendMethod.InOrder;
                            packet.UserData = msgData;

                            //push message.
                            network.SendMessage(packet);
                        }
                    }

                    rslt.ClearRslt();
                    rslt |= DynamicStores.Enums.Success;
                }
            }

            return new EntityMsgRslt(this, entity, rslt);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Get the EntityIdentifcation struct.
        /// </summary>
        //----------------------------------------------------------------------------------
        public EntityIdentification GetIdentStruct()
        {
            return new EntityIdentification(m_idEntityID, m_nDeviceID);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// The Entity ID of the parent Entity
        /// </summary>
        //----------------------------------------------------------------------------------
        // make this a property
        public int GetParentID()
        {
            Debug.Assert(!m_flags[(int)Status.INITILIZING], "The entity is still initializing.");
            return m_idParentID;
        }

        //----------------------------------------------------------------------------------
        // make this private or protected public
        /// <summary>
        /// Gets list of child entities of this entity.
        /// </summary>
        //----------------------------------------------------------------------------------
        public List<int> GetChildList()
        {
            return m_childList;
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Get if this entity on this device is authoritative.
        /// </summary>
        //----------------------------------------------------------------------------------
        public bool IsAuthoritative()
        {
            return AuthoritativeID == 0;
        }
        #endregion
        #region virtual functions
        //----------------------------------------------------------------------------------
        // Virtual functions that can be overridden in inheriting clases
        //----------------------------------------------------------------------------------
        #region general methods
        public virtual void Initialize() { }
        public virtual void PostInitialize() { }
        public virtual void Update(DeltaTime gameTime) { }
        public virtual void OnDestroy() { }
        #endregion
        //----------------------------------------------------------------------------------
        // Explicit implementation of interface memeber.
        //----------------------------------------------------------------------------------
        void IUpdateable.Update(GameTime gameTime)
        {
            Update(new DeltaTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime));
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Virtual function that is be overriddein in inherited classes for disposing of native assets, if needed.
        /// Uses diposable pattern.
        /// </summary>
        /// <param name="bDisposing">To be set from overridden method to base.</param>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        #region static methods

        #endregion
    }
}
