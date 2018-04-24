using BEPUphysics;
using BEPUphysics.DataStructures;
using BEPUphysics.MathExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.GeneralSystems;

namespace XenoEngine.Systems
{
    public class StaticMesh
    {
        //StaticTriangleGroup m_body;
        Model               m_model;
        Vector3             m_v3Position;
        BEPUphysics.Collidables.StaticMesh m_staticMesh;

        public StaticMesh(Model model, Vector3 v3Position)
        {
            Space space = EngineServices.GetSystem<IGameSystems>().PhysicsSpace;
            Vector3[] aVertices;
            int[] aIndices;

            m_model = model;
            m_v3Position = v3Position;
            
            TriangleMesh.GetVerticesAndIndicesFromModel(m_model, out aVertices, out aIndices);
            m_staticMesh = new BEPUphysics.Collidables.StaticMesh(aVertices, aIndices, new AffineTransform(Matrix3X3.CreateFromAxisAngle(Vector3.Up, MathHelper.Pi), m_v3Position));

            space.Add(m_staticMesh);
        }

        ~StaticMesh()
        {
            Space space = EngineServices.GetSystem<IGameSystems>().PhysicsSpace;
            space.Remove(m_staticMesh);
        }

        public Model GraphicModel { get { return m_model; } }
        public Vector3 Position 
        { 
            get { return m_v3Position; }
        }
    }
}
