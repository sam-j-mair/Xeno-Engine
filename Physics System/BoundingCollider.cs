using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V3 = Microsoft.Xna.Framework.Vector3;
using Microsoft.Xna.Framework;

namespace XenoEngine.Systems.Physics
{
    public class CollisionPair
    {
        private BoundingCollider m_colliderOne;
        private BoundingCollider m_colliderTwo;

        public CollisionPair(BoundingCollider colliderOne, BoundingCollider colliderTwo)
        {
            m_colliderOne = colliderOne;
            m_colliderTwo = colliderTwo;
        }

        public BoundingCollider ColliderOne { get { return m_colliderOne; } }
        public BoundingCollider ColliderTwo { get { return m_colliderTwo; } }
    }

    public abstract class BoundingCollider
    {
        private BoundingCollider m_innerCollider;

        public BoundingCollider(PhysicsBody body, BoundingCollider innerCollider)
        {
            Body = body;
            m_innerCollider = innerCollider;
        }

        public PhysicsBody Body { get; set; }
        public Material Material { get; set; }
        public abstract V3 Position { get; }
        public abstract V3 Center { get; }
        public abstract V3 Min { get; }
        public abstract V3 Max { get; }
        public abstract float Radius { get; }

        public abstract bool Contains(BoundingCollider collider);
        public abstract bool Intersects(BoundingCollider collider);

        public abstract BoundingCollider InnerCollider { get; }
    }

    public class Sphere : BoundingCollider
    {
        BoundingSphere m_sphere;

        public Sphere(V3 v3Center, float fRadius, PhysicsBody body, BoundingCollider innerCollider) : base(body, innerCollider)
        {
            m_sphere = new BoundingSphere(v3Center, fRadius);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override V3 Center
        {
            get { return m_sphere.Center; }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override V3 Max
        {
            get { return new V3(Center.X + Radius, Center.Y + Radius, Center.Z + Radius); }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override V3 Min
        {
            get { return new V3(Center.X - Radius, Center.Y - Radius, Center.Z - Radius); }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override V3 Position
        {
            get { return Center; }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override float Radius
        {
            get { return m_sphere.Radius; }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override BoundingCollider InnerCollider
        {
            get { return InnerCollider; }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override bool Contains(BoundingCollider collider)
        {
            float fDistance;
            fDistance = (this.Center - collider.Center).Length();
            float radius = this.Radius;
            float radius2 = collider.Radius;
            if (radius + radius2 < fDistance)
            {
                return false;
            }
            if (radius - radius2 < fDistance)
            {
                return true;
            }
            return true;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override bool Intersects(BoundingCollider collider)
        {
            return Contains(collider);
        }
    }

    public class Box : BoundingCollider
    {
        BoundingBox m_box;

        public Box(V3 v3Min, V3 v3Max, PhysicsBody body, BoundingCollider coliider) : base(body, coliider)
        {
            m_box = new BoundingBox(v3Min, v3Max);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override V3 Position
        {
            get
            {
                return (m_box.Max - m_box.Min) / 2;
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override V3 Center
        {
            get { return Position; }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override V3 Max
        {
            get { return m_box.Max; }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override V3 Min
        {
            get { return m_box.Min; }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override float Radius
        {
            get { return (m_box.Max - m_box.Min).Length(); }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override bool Contains(BoundingCollider collider)
        {
            float num = this.Radius * this.Radius;
            Vector3 vector;
            vector.X = this.Center.X - collider.Min.X;
            vector.Y = this.Center.Y - collider.Max.Y;
            vector.Z = this.Center.Z - collider.Max.Z;
            if (vector.LengthSquared() > num)
            {
                return true;
            }
            vector.X = this.Center.X - collider.Max.X;
            vector.Y = this.Center.Y - collider.Max.Y;
            vector.Z = this.Center.Z - collider.Max.Z;
            if (vector.LengthSquared() > num)
            {
                return true;
            }
            vector.X = this.Center.X - collider.Max.X;
            vector.Y = this.Center.Y - collider.Min.Y;
            vector.Z = this.Center.Z - collider.Max.Z;
            if (vector.LengthSquared() > num)
            {
                return true;
            }
            vector.X = this.Center.X - collider.Min.X;
            vector.Y = this.Center.Y - collider.Min.Y;
            vector.Z = this.Center.Z - collider.Max.Z;
            if (vector.LengthSquared() > num)
            {
                return true;
            }
            vector.X = this.Center.X - collider.Min.X;
            vector.Y = this.Center.Y - collider.Max.Y;
            vector.Z = this.Center.Z - collider.Min.Z;
            if (vector.LengthSquared() > num)
            {
                return true;
            }
            vector.X = this.Center.X - collider.Max.X;
            vector.Y = this.Center.Y - collider.Max.Y;
            vector.Z = this.Center.Z - collider.Min.Z;
            if (vector.LengthSquared() > num)
            {
                return true;
            }
            vector.X = this.Center.X - collider.Max.X;
            vector.Y = this.Center.Y - collider.Min.Y;
            vector.Z = this.Center.Z - collider.Min.Z;
            if (vector.LengthSquared() > num)
            {
                return true;
            }
            vector.X = this.Center.X - collider.Min.X;
            vector.Y = this.Center.Y - collider.Min.Y;
            vector.Z = this.Center.Z - collider.Min.Z;
            if (vector.LengthSquared() > num)
            {
                return true;
            }
            return true;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override bool Intersects(BoundingCollider collider)
        {
            return Contains(collider);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override BoundingCollider InnerCollider
        {
            get
            {
                return InnerCollider;
            }
        }
    }
}
