using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using MatCollCallBack = System.Action<XenoEngine.Systems.Physics.MaterialPair, XenoEngine.Systems.Physics.PhysicsBody, XenoEngine.Systems.Physics.PhysicsBody>;
using TSBool = XenoEngine.TaskSystem.ThreadSafeField<bool>;
using TSFloat = XenoEngine.TaskSystem.ThreadSafeField<float>;
using TSMatrix = XenoEngine.TaskSystem.ThreadSafeField<Microsoft.Xna.Framework.Matrix>;
using TSV3 = XenoEngine.TaskSystem.ThreadSafeField<Microsoft.Xna.Framework.Vector3>;
using V3 = Microsoft.Xna.Framework.Vector3;

namespace XenoEngine.Systems.Physics
{
    internal delegate void UpdateRequest(PhysicsBody body);

    public class PhysicsBody
    {
        //private Matrix m_transform;
        private BoundingCollider    m_collider;
        private event UpdateRequest UpdateRequired;

        private TSFloat             m_fSpeed, m_fAcceleration, m_fMass, m_fRotationalVelocity, m_fLinearDamp;
        private TSBool              m_bIsStatic;
        private TSV3                m_v3Position, m_v3Direction;
        private TSMatrix            m_transform;
        private Material            m_material;


        public PhysicsBody(BoundingCollider collider)
        {
            m_collider = collider;
            m_collider.Body = this;
            LockObject = new object();

            InitializeThreadSafeVariables();
        }

        protected void InitializeThreadSafeVariables()
        {
            m_fSpeed = new TSFloat(LockObject);
            m_fMass = new TSFloat(LockObject);
            m_fAcceleration = new TSFloat(LockObject);
            m_fLinearDamp = new TSFloat(LockObject, 2);
            m_fRotationalVelocity = new TSFloat(LockObject);
            m_transform = new TSMatrix(LockObject, Matrix.Identity);
            m_v3Position = new TSV3(LockObject, Vector3.Zero);
            m_v3Direction = new TSV3(LockObject, Vector3.Forward);
            m_bIsStatic = new TSBool(LockObject);
        }

        public TSFloat Mass { get { return m_fMass; } }
        public TSFloat Speed { get { return m_fSpeed; } }
        public TSFloat Acceleration { get { return m_fAcceleration; } }
        public TSFloat LinearDamp { get { return m_fLinearDamp; } }

        public TSV3 Direction { get { return m_v3Direction; } }
        public TSV3 Position { get { return m_v3Position; } }
        public TSMatrix Transform { get { return m_transform; } }
        public TSBool IsStatic { get { return m_bIsStatic; } }

        public BoundingCollider Collider { get { return m_collider; } }

        internal object LockObject { get; private set; }
    }
}
