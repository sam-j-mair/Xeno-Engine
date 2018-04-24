using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XenoEngine;
using XenoEngine.Systems;
using XenoEngine.GeneralSystems;
using XenoEngine.DebugHelpers;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModel;
using System.Threading.Tasks;
using System.Diagnostics;



namespace XenoEngine.Renderer
{
    public struct MHandle : IDisposable
    {
        internal int m_nHashCode;
        internal int m_nRefNumber;
        internal Action<Action<Model>> CallbackChanged;
        internal Action<bool> VisibilityChanged;
        internal Action<Vector3> PositionChanged;
        internal Action<Quaternion> OrientationChanged;

        private bool m_bVisibilty;
        private Action<Model> m_callback;
        private Vector3 m_v3initialPosition;
        private Quaternion m_qInitialOrientation;

        internal MHandle(int hashCode,
                          int nRefNumber,
                          bool bInitialVis,
                          Action<Model> callback,
                          Vector3 v3InitialPosition,
                          Quaternion qInitialOrientation,
                          Action<Action<Model>> callbackChanged,
                          Action<bool> visibilty,
                          Action<Vector3> positionChanged,
                          Action<Quaternion> orientationChanged)
                          
        {
            m_nHashCode = hashCode;
            m_nRefNumber = nRefNumber;
            m_bVisibilty = bInitialVis;
            m_callback = callback;

            m_v3initialPosition = v3InitialPosition;
            m_qInitialOrientation = qInitialOrientation;

            VisibilityChanged = visibilty;
            CallbackChanged = callbackChanged;
            PositionChanged = positionChanged;
            OrientationChanged = orientationChanged;
        }

        public static bool operator ==(MHandle lhs, MHandle rhs)
        {
            return lhs.m_nHashCode == rhs.m_nHashCode;
        }

        public static bool operator !=(MHandle lhs, MHandle rhs)
        {
            return lhs.m_nHashCode != rhs.m_nHashCode;
        }

        public static bool operator ==(MHandle modelKey, Model model)
        {
            return modelKey.m_nHashCode == model.GetHashCode();
        }

        public static bool operator !=(MHandle modelKey, Model model)
        {
            return modelKey.m_nHashCode != model.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Visible
        {
            get { return m_bVisibilty; }
            set 
            {
                if (m_bVisibilty != value)
                {
                    m_bVisibilty = value;

                    if (VisibilityChanged != null) VisibilityChanged(value);
                }
            }
        }

        public Action<Model> Callback
        {
            get { return m_callback; }
            set
            {
                if (m_callback != value)
                {
                    m_callback = value;

                    if (CallbackChanged != null) CallbackChanged(value);
                }
            }
        }

        public Vector3 Position
        {
            get { return m_v3initialPosition; }
            set
            {
                if (m_v3initialPosition != value)
                {
                    m_v3initialPosition = value;

                    if (PositionChanged != null) PositionChanged(value);
                }
            }
        }

        public Quaternion Orientation
        {
            get { return m_qInitialOrientation; }
            set
            {
                if (m_qInitialOrientation != value)
                {
                    m_qInitialOrientation = value;

                    if (OrientationChanged != null) OrientationChanged(value);
                }
            }
        }

        internal int RefNumber { get { return m_nRefNumber; } }

        void IDisposable.Dispose()
        {
            m_qInitialOrientation = Quaternion.Identity;
            m_callback = null;
            m_bVisibilty = false;
            m_nHashCode = 0;
            m_v3initialPosition = Vector3.Zero;
            OrientationChanged = null;
            PositionChanged = null;
            CallbackChanged = null;
            VisibilityChanged = null;
        }
    }

    internal class RenderData
    {
        private List<int> m_references;
        internal RenderData(Model model, Action<Model> renderCallback, AnimationPlayer animPlayer)
        {
            Model = model;
            RenderCallback = renderCallback;
            m_references = new List<int>();
            Visible = true;
        }

        internal void CallbackChanged(Action<Model> callback) { RenderCallback = callback; }
        internal void VisibilityChanged(bool bValue) { Visible = bValue; }
        internal void PositionChanged(Vector3 v3Position) { Position = v3Position; }
        internal void OrientationChanged(Quaternion quart) { Orientation = quart; }

        internal void AddRef(int nRef) { m_references.Add(nRef); }
        internal void RemoveRef(int nRef) { m_references.Remove(nRef); }
        internal int RefNumber { get; set; }

        internal Model Model { get; private set; }
        internal int RefCount { get { return m_references.Count; } }
        internal Action<Model> RenderCallback { get; private set; }
        internal AnimationPlayer AnimPlayer { get; private set; }
        internal bool Visible { get; set; }
        internal Vector3 Position { get; set; }
        internal Quaternion Orientation { get; set; }

        internal void Dispose()
        {
            Model = null;
            RenderCallback = null;
        }
    }

    public class RenderEngine : IGameComponent, IDrawable, IDisposable
    {
        private Dictionary<int, List<RenderData>> m_Renderables;
        public event EventHandler<EventArgs> DrawOrderChanged, VisibleChanged, Disposed;

        private int m_nDrawOrder;
        private bool m_bVisible;

        public RenderEngine()
        {
            m_Renderables = new Dictionary<int, List<RenderData>>();
            Visible = true;
            EngineServices.GetSystem<IGameSystems>().Components.Add(this);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Initialize() { }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public MHandle AddModel(Model model, Action<Model> customRenderCallback = null, AnimationPlayer animPlayer = null)
        {
            List<RenderData> data = null;
            RenderData renderData = new RenderData(model, customRenderCallback, animPlayer);
            int nHashCode = model.GetHashCode();
            bool bFound = false;

            if (m_Renderables.TryGetValue(nHashCode, out data))
            {
                /*data[renderData.RefNumber] = renderData;*/
                data.Add(renderData);
                renderData.RefNumber = data.FindIndex(0, test => test == renderData);
            }
            else
            {
                List<RenderData> newList = new List<RenderData>();
                newList.Add(renderData);
                m_Renderables.Add(nHashCode, newList);
            }

            return new MHandle(nHashCode,
                                renderData.RefNumber,
                                renderData.Visible,
                                renderData.RenderCallback,
                                Vector3.Zero,
                                Quaternion.Identity,
                                renderData.CallbackChanged,
                                renderData.VisibilityChanged,
                                renderData.PositionChanged,
                                renderData.OrientationChanged);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void RemoveModel(MHandle modelKey)
        {
            int nHashCode = modelKey.m_nHashCode;
            List<RenderData> data = m_Renderables[nHashCode];
            
            data[modelKey.RefNumber] = null;
            ((IDisposable)modelKey).Dispose();

            if (data.Count == 0)
            {
                m_Renderables.Remove(modelKey.m_nHashCode);
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public Model GetModel(MHandle modelKey)
        {
            return m_Renderables[modelKey.m_nHashCode][modelKey.RefNumber].Model;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        void IDrawable.Draw(GameTime gameTime)
        {
            Draw(new DeltaTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime));
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Draw(DeltaTime gameTime)
        {
            //using(new Profiler.TrackingObject("Render"))
            //    Parallel.ForEach(m_Renderables.Values, Render);

            foreach (List<RenderData> list in m_Renderables.Values)
            {
                Render(list);
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        private void Render(List<RenderData> renderList)
        {
            foreach(RenderData data in renderList)
            {
                if (data.Model != null)
                {
                    Model model = data.Model;
                    AnimationPlayer animPlayer = data.AnimPlayer;

                    if (data.RenderCallback != null)
                    {
                        data.RenderCallback(data.Model);
                        return;
                    }
                    // Copy any parent transforms.
                    //Matrix[] a44Bones = null;

                    //if(animPlayer != null)
                    //    a44Bones = animPlayer.GetSkinTransforms();

                    Matrix[] transforms = new Matrix[model.Bones.Count];
                    ICamera camera = EngineServices.GetSystem<IGameSystems>().Camera;
                    LightRig lightRig = EngineServices.GetSystem<IGameSystems>().LightRig;

                    model.CopyAbsoluteBoneTransformsTo(transforms);

                    // Draw the model. A model can have multiple meshes, so loop.
                    foreach (ModelMesh mesh in model.Meshes)
                    {
                        // This is where the mesh orientation is set, as well 
                        // as our camera and projection.
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            //Matrix.CreateFromQuaternion()
                            effect.EnableDefaultLighting();
                            effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(data.Position) * Matrix.CreateFromQuaternion(data.Orientation);
                            effect.Projection = camera.ProjectionMatrix;
                            effect.View = camera.ViewMatrix;

                            //SetLightingData(lightRig, effect);

                            //foreach (EffectTechnique technique in effect.Techniques)
                            //{
                            //if (a44Bones != null) effect.Parameters["Bones"].SetValue(a44Bones);


                            //effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                            //effect.Parameters["View"].SetValue(camera.ViewMatrix);
                            //}
                        }
                        // Draw the mesh, using the effects set above.
                        mesh.Draw();
                    }
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        private void SetLightingData(LightRig lightRig, Effect effect)
        {
            if (lightRig.LightCount > 0)
            {
                //This assert is in here to ensure we don't overflow the array in the shader.
                Debug.Assert(lightRig.LightCount < 8);

                //we set this so we know how many lights to process in the shaders
                effect.Parameters["nLightCount"].SetValue(lightRig.LightCount);

                for (int nIndex = 0; nIndex < lightRig.LightCount; ++nIndex)
                {
                    Light light = lightRig.Lights[nIndex];
                    EffectParameter effectParam = effect.Parameters["Lights"].Elements[nIndex];
                    effectParam.StructureMembers["Position"].SetValue(light.Position);
                    effectParam.StructureMembers["Target"].SetValue(light.Target);
                    effectParam.StructureMembers["Direction"].SetValue(light.Direction);
                    //effectParam.StructureMembers["Colour"].SetValue(light.Colour);
                    effectParam.StructureMembers["Intensity"].SetValue(light.Intensity);
                    effectParam.StructureMembers["OuterRadius"].SetValue(light.OuterRadius);
                    effectParam.StructureMembers["InnerRadius"].SetValue(light.InnerRadius);
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
