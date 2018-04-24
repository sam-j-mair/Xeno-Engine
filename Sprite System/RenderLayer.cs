using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.GeneralSystems;
using XenoEngine.ParticleSystems;
using XenoEngine.Serialization;
using XenoEngine.Serialization.Proxies;

namespace XenoEngine.Systems.Sprite_Systems
{
    [Serializable]
    public class EffectSettings
    {
        private SerializableEffect m_serializableEffect = new SerializableEffect();
        public dynamic m_effectSettings;
        public InitialiseSettings m_fpInitialise;
        public UpdateSettings m_fpUpdate;

        public Effect m_effect { get { return m_serializableEffect.WrappedType; } set { m_serializableEffect.WrappedType = value; } }
    }

    //This could be made so that it sub classes a sprite batch directly.
    [Serializable]
    public abstract class RenderLayer<TInfoType> : SerializableComponent, IGameComponent, IUpdateable, IDrawable, IRenderLayer<TInfoType>
    {
        protected event Action<DeltaTime>       RequestInfo;
        protected List<TInfoType>               m_RenderInfoList;

        private int                             m_nUpdateOrder, m_nDrawOrder, m_nMaxItems;
        private bool                            m_bEnabled, m_bVisible;
        private SerialiableBlendState           m_blendState;
        private SerializableDepthStencil        m_depthStencil;
        private string                          m_szBlendStateName;
        private EffectSettings                  m_effectSettings;

        public event EventHandler<EventArgs>    EnabledChanged;
        public event EventHandler<EventArgs>    UpdateOrderChanged;
        public event EventHandler<EventArgs>    DrawOrderChanged;
        public event EventHandler<EventArgs>    VisibleChanged;
        public event EventHandler<EventArgs>    Disposed;

        public RenderLayer(int nMaxItems)
        {
            m_RenderInfoList = new List<TInfoType>(nMaxItems);
            EngineServices.GetSystem<IGameSystems>().Components.Add(this);
            m_blendState = new SerialiableBlendState();
            BlendState = BlendState.AlphaBlend;
            m_depthStencil = new SerializableDepthStencil();
            DepthStencilState = DepthStencilState.Default;
            m_nMaxItems = nMaxItems;
            Enabled = true;
            Visible = true;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        ~RenderLayer()
        {
            Dispose(false);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void RegisterSprite(IInfoBase<TInfoType> item)
        {
            RequestInfo += item.InfoRequested;
            item.SendInfo += AddInfo;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void DeregisterSprite(IInfoBase<TInfoType> item)
        {
            RequestInfo -= item.InfoRequested;
            item.SendInfo -= AddInfo;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void AddInfo(TInfoType spriteInfo)
        {
            if (m_RenderInfoList.Count < m_nMaxItems)
                m_RenderInfoList.Add(spriteInfo);
            else
                Debug.Write("Capacity has been reached!!");
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        protected void CallRequestInfo(DeltaTime deltaTime)
        {
            if(RequestInfo != null)
                RequestInfo(deltaTime);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public virtual void Initialize() { }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        void IUpdateable.Update(GameTime gameTime)
        {
            Update(new DeltaTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime));
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public virtual void Update(DeltaTime deltaTime) { }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        void IDrawable.Draw(GameTime gameTime)
        {
            Draw(new DeltaTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime));
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public virtual void Draw(DeltaTime deltaTime) { }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                bool bFlag = false;
                try
                {
                    Monitor.Enter(this, ref bFlag);
                    m_RenderInfoList.Clear();
                    m_RenderInfoList = null;

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
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public EffectSettings EffectSettings
        {
            get { return m_effectSettings; }
            set
            {
                Debug.Assert(value.m_fpInitialise != null && value.m_effect != null && value.m_fpUpdate != null);

                if (value.m_fpInitialise != null)
                {
                    value.m_fpInitialise(value.m_effect, value.m_effectSettings);
                }

                m_effectSettings = value;
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public Effect Effect
        {
            get
            { 
                if(m_effectSettings != null)
                    return m_effectSettings.m_effect; 
                else 
                    return null;
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public BlendState BlendState { get { return m_blendState.WrappedType; } set { m_blendState.WrappedType = value; } }
        public DepthStencilState DepthStencilState { get { return m_depthStencil.WrappedType; } set { m_depthStencil.WrappedType = value; } }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
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
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
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
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public int DrawOrder
        {
            get { return m_nDrawOrder; }
            set
            {
                if (m_nDrawOrder != value)
                {
                    m_nDrawOrder = value;
                    if (DrawOrderChanged != null) DrawOrderChanged(this, EventArgs.Empty);
                }
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public bool Visible
        {
            get { return m_bVisible; }
            set
            {
                if (m_bVisible != value)
                {
                    m_bVisible = value;
                    if (VisibleChanged != null) VisibleChanged(this, EventArgs.Empty);
                }
            }
        }
    }
}
