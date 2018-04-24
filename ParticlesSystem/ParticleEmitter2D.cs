using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.GeneralSystems;
using XenoEngine.Serialization;
using XenoEngine.Serialization.Proxies;
using XenoEngine.Systems.Sprite_Systems;



namespace XenoEngine.ParticleSystems
{
    public interface IParticle : IDisposable
    {
        void UpdateParticle(DeltaTime deltaTime);
        void OnInitilized();
        event Action<IParticle> ParticleDead;
        bool Active { get; set; }
        Vector3 Position { get; set; }
        Vector3 Direction { get; set; }
        Vector3 Velocity { get; set; }
        float Speed { get; set; }
        float TimeAlive { get; set; }
        float LifeTime { get; set; }
        Vector2 Scale { get; set; }
        object Tag { get; set; }
    }
    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    [Serializable]
    public abstract class ParticleController<TInfoType>
    {
        private SerializableEffect m_effect = new SerializableEffect();

        public int GenerationRate { get; set; }
        public float GenerationTimer { get; set; }
        public float Seed { get; set; }
        public float Spread { get; set; }
        public float LifeTime { get; set; }
        public float Angle { get; set; }
        public Vector2 InitialScale { get; set; }
        public Effect Effect { get { return m_effect.WrappedType; } set { m_effect.WrappedType = value;  } }

        public abstract void InitializeParticle(ref IParticle particle, ParticleEmitter<TInfoType> emitter);
        public abstract void UpdateController(DeltaTime deltaTime, ParticleEmitter<TInfoType> emitter);
    }
    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    [Serializable]
    public abstract class Particle<TInfoType> : IParticle
    {
        public event Action<IParticle> ParticleDead;

        //This constructor has these 
        public Particle(IRenderLayer<TInfoType> spriteSystem,
            string szAssetName,
            Vector3 v3Position,
            Vector3 v3Direction,
            float fLifeTime,
            float fSpeed,
            bool bStartActive)
        {
            //Active = bStartActive;
            RenderLayer = spriteSystem;
            //Position = Vector2.Zero;
            Direction= v3Direction;
            Speed = fSpeed;
            LifeTime = fLifeTime;
            TimeAlive = 0;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public abstract void OnInitilized();

        public abstract void UpdateParticle(DeltaTime deltaTime);
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected void SetParticleDead()
        {
            if (ParticleDead != null) ParticleDead(this);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void Dispose()
        {
            Active = false;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public IRenderLayer<TInfoType> RenderLayer { get; private set; }
        public virtual Vector3 Position { get; set; }
        public virtual Vector3 Direction { get; set; }
        public virtual Vector3 Velocity { get; set; }
        public virtual float Speed { get; set; }
        public virtual float TimeAlive { get; set; }
        public virtual float LifeTime { get; set; }
        public virtual bool Active { get; set; }
        public virtual Vector2 Scale { get; set; }
        public object Tag { get; set; }
    }

    [Serializable]
    public class ParticleEmitter<TInfoType> : SerializableComponent, IGameComponent, IUpdateable
    {
        //Each particle system will have its own sprite layer
        [NonSerialized] IRenderLayer<TInfoType> m_RenderLayer;
        [NonSerialized] List<IParticle> m_Particles;
        [NonSerialized] Queue<IParticle> m_freeParticleList;

        private bool m_bActive, m_bContinuosEmission, m_bAllowGeneration, m_bEnabled;
        private int m_nUpdateOrder;

        public event EventHandler<EventArgs> EnabledChanged, UpdateOrderChanged, Disposed;
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public ParticleEmitter(IRenderLayer<TInfoType> renderLayer, Type particleType, ParticleController<TInfoType> controller, string szAssetName, Vector3 v3Position, int nMaxSprites)
        {
            m_RenderLayer = renderLayer;
            m_Particles = new List<IParticle>(nMaxSprites);
            m_freeParticleList = new Queue<IParticle>(nMaxSprites);
            
            SaveSerializationData("MaxSprites", nMaxSprites);
            SaveSerializationData("AssetName", szAssetName);
            SaveSerializationData("ParticleType", particleType);

            ParticleController = controller;

            m_bActive = false;
            m_bContinuosEmission = false;
            m_bAllowGeneration = false;
            Position = v3Position;
            Enabled = true;

            EngineServices.GetSystem<IGameSystems>().Components.Add(this);

            for (int nCounter = 0; nCounter < nMaxSprites; ++nCounter)
            {
                IParticle particle = Activator.CreateInstance(particleType, new object[] { m_RenderLayer, szAssetName, Position, Vector3.Zero, 0, 0, false }) as IParticle;
                particle.ParticleDead += DeadParticle_Event;
                m_Particles.Add(particle);
                m_freeParticleList.Enqueue(particle);
            }
        }

        ~ParticleEmitter()
        {
            Dispose(false);
        }

        #region Serialization
        [OnDeserialized]
        protected new void OnDeserialized(StreamingContext context)
        {
            dynamic nMaxSprites;
            RestoreSerializationData("MaxSprites", out nMaxSprites);
            m_Particles = new List<IParticle>(nMaxSprites);
            m_freeParticleList = new Queue<IParticle>(nMaxSprites);

            for (int nCounter = 0; nCounter < nMaxSprites; ++nCounter)
            {
                IParticle particle = Activator.CreateInstance(RestoreSerializationData("ParticleType"), new object[] { m_RenderLayer, RestoreSerializationData("AssetName"), Position, Vector3.Zero, 0, 0, false }) as IParticle;
                particle.ParticleDead += DeadParticle_Event;
                m_Particles.Add(particle);
                m_freeParticleList.Enqueue(particle);
            }
        }
        #endregion

        public void Start(bool bContinuos) { m_bActive = true; m_bContinuosEmission = bContinuos; m_bAllowGeneration = true; }
        public void Stop() { m_bActive = false; m_bAllowGeneration = false; m_bContinuosEmission = false; }


        public virtual void Initialize() { }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        void IUpdateable.Update(GameTime gameTime)
        {
            Update(new DeltaTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime));
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Update(DeltaTime deltaTime)
        {
            if ((m_bActive || !m_bActive && (m_freeParticleList.Count != m_Particles.Count)) && ParticleController != null)
            {
                if (m_bAllowGeneration)
                {
                    //We create new ones
                    GenerateParticles(deltaTime);

                    if (!m_bContinuosEmission)
                        m_bAllowGeneration = false;
                }

                ParticleController.UpdateController(deltaTime, this);

                //We update each active particle.
                foreach (IParticle particle in m_Particles)
                {
                    if (particle.Active)
                        particle.UpdateParticle(deltaTime);
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        private void DeadParticle_Event(IParticle particle)
        {
            particle.Active = false;
            m_freeParticleList.Enqueue(particle);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        private void GenerateParticles(DeltaTime deltaTime)
        {
            if (deltaTime.ElapsedGameTime.Milliseconds > ParticleController.GenerationTimer)
            {
                for (int nCounter = 0; nCounter < ParticleController.GenerationRate; ++nCounter)
                {
                    if (m_freeParticleList.Count > 0)
                    {
                        IParticle particle = m_freeParticleList.Dequeue();
                        particle.Scale = ParticleController.InitialScale;
                        ParticleController.InitializeParticle(ref particle, this);
                        particle.OnInitilized();
                        particle.Active = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                bool bFlag = false;
                try
                {
                    Monitor.Enter(this, ref bFlag);
                    m_freeParticleList.Clear();

                    foreach (IParticle particle in m_Particles)
                    {
                        particle.Dispose();
                    }

                    m_Particles.Clear();
                    m_RenderLayer = null;

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
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public Effect Effect { get { return m_RenderLayer.Effect; } }
        public EffectSettings EffectSettings { get { return m_RenderLayer.EffectSettings; } set { m_RenderLayer.EffectSettings = value; } }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public ParticleController<TInfoType> ParticleController { get; set; }
        public dynamic UserData { get; set; }
        public bool IsActive { get { return m_bActive; } }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
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
    }
}

