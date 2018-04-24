using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using XenoEngine.GeneralSystems;
using XenoEngine.Systems.Sprite_Systems;

namespace XenoEngine.ParticleSystems
{
    public delegate void InitialiseSettings(Effect effect, dynamic settings);
    public delegate void UpdateSettings(Effect effect, dynamic settings);

    [Serializable]
    public class ParticleInfo
    {
        public ParticleVertex[] m_aVertices;
    }

    [Serializable]
    public struct ParticleVertex
    {
        // Stores which corner of the particle quad this vertex represents.
        public Short2 Corner;

        // Stores the starting position of the particle.
        public Vector3 Position;

        // Stores the starting velocity of the particle.
        public Vector3 Velocity;

        // Four random values, used to make each particle look slightly different.
        public Color Random;

        // The time (in seconds) at which this particle was created.
        public float Time;


        // Describe the layout of this vertex structure.
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Short2,
                                 VertexElementUsage.Position, 0),

            new VertexElement(4, VertexElementFormat.Vector3,
                                 VertexElementUsage.Position, 1),

            new VertexElement(16, VertexElementFormat.Vector3,
                                  VertexElementUsage.Normal, 0),

            new VertexElement(28, VertexElementFormat.Color,
                                  VertexElementUsage.Color, 0),

            new VertexElement(32, VertexElementFormat.Single,
                                  VertexElementUsage.TextureCoordinate, 0)
        );


        // Describe the size of this vertex structure.
        public const int SizeInBytes = 36;
    }

    [Serializable]
    public class Particle3DRenderLayer : RenderLayer<ParticleInfo>
    {
        DynamicVertexBuffer m_vertexBuffer;
        IndexBuffer         m_indexBuffer;
        ParticleVertex[]    m_aVertices;

        [NonSerialized] EffectParameter m_effectViewParameter;
        [NonSerialized] EffectParameter m_effectProjectionParameter;
        [NonSerialized] EffectParameter m_effectViewportScaleParameter;
        [NonSerialized] EffectParameter m_effectTimeParameter;
        float               m_fCurrentTime;

        public Particle3DRenderLayer(int nMaxItems, EffectSettings settings)
            : base(nMaxItems)
        {
            m_vertexBuffer = new DynamicVertexBuffer(EngineServices.GetSystem<IGameSystems>().GraphicsDevice,
                ParticleVertex.VertexDeclaration,
                nMaxItems * 4,
                BufferUsage.WriteOnly);

            m_aVertices = new ParticleVertex[nMaxItems * 4];

            ushort[] indices = new ushort[nMaxItems * 6];

            for (int i = 0; i < nMaxItems; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            m_indexBuffer = new IndexBuffer(EngineServices.GetSystem<IGameSystems>().GraphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            m_indexBuffer.SetData(indices);

            if(settings != null)
            {
                EffectSettings = settings;
                    
            }
            else
            {
                InitialiseEffect(settings);    
            }

            m_fCurrentTime = 0;
        }

        [OnDeserialized]
        protected new void OnDeserialized(StreamingContext context)
        {
            InitialiseEffect(null);
        }

        public virtual void InitialiseEffect(dynamic settings = null)
        {
            EffectParameterCollection parameters = Effect.Parameters;

            // Look up shortcuts for parameters that change every frame.
            m_effectViewParameter = parameters["View"];
            m_effectProjectionParameter = parameters["Projection"];
            m_effectViewportScaleParameter = parameters["ViewportScale"];
            m_effectTimeParameter = parameters["CurrentTime"];
        
// 
            // Set the values of parameters that do not change.
            parameters["Duration"].SetValue((float)TimeSpan.FromSeconds(2).TotalSeconds);
            parameters["DurationRandomness"].SetValue(1);
            parameters["Gravity"].SetValue(new Vector3(0, 15, 0));
            parameters["EndVelocity"].SetValue(0);
            parameters["MinColor"].SetValue(new Color(255, 255, 255, 10).ToVector4());
            parameters["MaxColor"].SetValue(new Color(255, 255, 255, 40).ToVector4());

            parameters["RotateSpeed"].SetValue(
                new Vector2(-1, 1));

            parameters["StartSize"].SetValue(
                new Vector2(5, 10));

            parameters["EndSize"].SetValue(
                new Vector2(10, 40));

            // Load the particle texture, and set it onto the effect.
            Texture2D texture = EngineServices.GetSystem<IGameSystems>().Content.Load<Texture2D>("explosion");

            parameters["Texture"].SetValue(texture);
        }

        public override void Draw(DeltaTime deltaTime)
        {
            CallRequestInfo(deltaTime);
            m_fCurrentTime += (float)deltaTime.ElapsedGameTime.TotalSeconds;

            if (m_RenderInfoList.Count > 0)
            {
                //quick hack
                if (EngineServices.GetSystem<IGameSystems>().GraphicsDevice != null)
                {
                    GraphicsDevice device = EngineServices.GetSystem<IGameSystems>().GraphicsDevice;
                    ICamera camera = EngineServices.GetSystem<IGameSystems>().Camera;
                    Debug.Assert(camera != null, "There is no camera defined in the services.");

                    device.BlendState = BlendState;
                    device.DepthStencilState = DepthStencilState;

                    //we build up the vertices's from the submissions
                    BuildVertexBuffer();

                    if (EffectSettings.m_fpUpdate != null)
                    {
                        EffectSettings.m_fpUpdate(EffectSettings.m_effect, EffectSettings.m_effectSettings);
                    }
                    else
                    {
                        //NOTE:[SM] i want this stuff to be set inside the particle controller. maybe
                        // Set an effect parameter describing the view port size. This is
                        // needed to convert particle sizes into screen space point sizes.
                        m_effectViewportScaleParameter.SetValue(new Vector2(0.5f / device.Viewport.AspectRatio, -0.5f));

                        m_effectProjectionParameter.SetValue(camera.ProjectionMatrix);
                        m_effectViewParameter.SetValue(camera.ViewMatrix);

                        // Set an effect parameter describing the current time. All the vertex
                        // shader particle animation is keyed off this value.
                        m_effectTimeParameter.SetValue(m_fCurrentTime);
                    }

                    // Set the particle vertex and index buffer.
                    device.SetVertexBuffer(m_vertexBuffer);
                    device.Indices = m_indexBuffer;

                    // Activate the particle effect.
                    foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                     0, m_aVertices.Length,
                                                     0, m_aVertices.Length / 4);
                    }

                    device.DepthStencilState = DepthStencilState.Default;

                    //we clear the vertex buffer because this is a static field.
                    Array.Clear(m_aVertices, 0, 200);
                }

                // Reset some of the render states that we changed,
                // so as not to mess up any other subsequent drawing.

                m_RenderInfoList.Clear();
            }

            base.Draw(deltaTime);
        }

        private void BuildVertexBuffer()
        {
            int nIndexer = 0;
            int nStride = ParticleVertex.SizeInBytes;
            //This might be to expensive ..but lets try.
            foreach (ParticleInfo data in m_RenderInfoList)
            {
                if(nIndexer + data.m_aVertices.Length <= m_aVertices.Length)
                {
                    Array.Copy(data.m_aVertices, 0, m_aVertices, nIndexer, data.m_aVertices.Length);
                    nIndexer += data.m_aVertices.Length;
                }
//                 foreach (ParticleVertex vertex in data.m_aVertices)
//                 {
//                     saVertices[nIndexer++] = vertex;
//                 }
            }

            m_vertexBuffer.SetData(0 * nStride * 4,
                m_aVertices,
                0,
                m_aVertices.Length,
                nStride,
                SetDataOptions.NoOverwrite);
        }
    }
}
