using System.Collections.Generic;
using System.Diagnostics;
//using MatCollCallBack = System.Action<XenoEngine.Systems.Physics.MaterialPair, XenoEngine.Systems.Physics.PhysicsBody, XenoEngine.Systems.Physics.PhysicsBody>;
using V3 = Microsoft.Xna.Framework.Vector3;
using System;
using System.Threading.Tasks;

namespace XenoEngine.Systems.Physics
{
    public delegate CollisionBehaviour MatCollCallBack(MaterialPair pair, PhysicsBody body1, PhysicsBody body2);

    public enum CollisionBehaviour
    {
        DefaultCollision,
        OverrideCollision,

        CollisionBehaviour_Count
    }

    public class PhysicsEngine
    {
        //we don't initialize this until the instance constructor.
        private static Dictionary<string, Material>             m_Materials;
        private static Dictionary<int, MaterialCollisionItem>   m_materialPairs;
        private static List<PhysicsBody>                        m_updateList;
        private static List<BoundingCollider>                   m_collisionsBroadList;
        private static List<CollisionPair>                      m_collisionsFineList;

        private DeltaTime                                       m_deltaTime;
        private DeltaTime                                       m_updateTime;
        private object                                          m_UpdateLock, m_colliderLock;

        private bool                                            m_bfixedTimeStep;
        private float                                           m_fFixedTimeStep;

        public PhysicsEngine()
        {
            m_Materials = new Dictionary<string, Material>();
            m_materialPairs = new Dictionary<int, MaterialCollisionItem>();
            m_updateList = new List<PhysicsBody>();
            m_collisionsBroadList = new List<BoundingCollider>();
            m_collisionsFineList = new List<CollisionPair>();
            m_UpdateLock = new object();
            m_colliderLock = new object();
        }

        public bool Paused { get; set; }
        public bool FixedTimeStep { get { return m_bfixedTimeStep; } set { m_bfixedTimeStep = value; } }
        public int TargetFrameRate { get { return (int)(m_fFixedTimeStep / 1.0f); } set { m_fFixedTimeStep = (1 / value); } }

        //This can only be used in multi-threading
        private DeltaTime AdjustFrameRate(DeltaTime deltaTime)
        {
            if (m_bfixedTimeStep)
            {
                float fCurrentStep = deltaTime.ElapsedGameTime.Milliseconds;
                
                while (fCurrentStep > m_fFixedTimeStep)
                {
                    fCurrentStep -= m_fFixedTimeStep;
                }
                return new DeltaTime(deltaTime.TotalGameTime, new TimeSpan(0, 0, 0, 0, (int)fCurrentStep));
            }
            return deltaTime;
        }

        public void AddMaterial(string szName, Material material)
        {
            m_Materials.Add(szName, material);
        }

        public void AddMaterial(params MaterialSlotItem[] aMaterials)
        {
            foreach (MaterialSlotItem item in aMaterials)
            {
                AddMaterial(item.Name, item.Mat);
            }
        }

        internal bool GetMaterialPair(Material one, Material two, out MaterialCollisionItem pair)
        {
            bool bResult = false;
            int nKey = MaterialPair.GetMaterialPairHashKey(one, two);

            if (m_materialPairs.TryGetValue(nKey, out pair)) { bResult = true; }

            return bResult;
        }

        public MaterialPairKey AddMaterialPair(Material materialOne, Material materialTwo, MatCollCallBack callBack)
        {
            MaterialCollisionItem pair;
            int nKey = MaterialPair.GetMaterialPairHashKey(materialOne, materialTwo);

            if (m_materialPairs.TryGetValue(nKey, out pair))
            {
                pair.AddCallback(callBack);
            }
            else
            {
                pair = new MaterialCollisionItem(materialOne, materialTwo, callBack);
            }

            return new MaterialPairKey(nKey);
        }

        public void RemoveMaterialPair(MaterialPairKey key, MatCollCallBack callBack)
        {
            try
            {
                m_materialPairs[key.Key].RemoveCallback(callBack);
            }
            catch (System.Exception ex)
            {
                Debug.Assert(false, "MaterialPair doesn't exist");
            }
        }

        public void UpdateRequested(PhysicsBody body)
        {
            Debug.Assert(!body.IsStatic.Value);

            if (!body.IsStatic.Value)
            {
                lock (m_UpdateLock)
                    m_updateList.Add(body);
            }
        }

        //I have made this public so the consumer has the choice of running this multi-threaded or not.
        public void UpdateSimulation(DeltaTime deltaTime = default(DeltaTime))
        {
            PhysicsBody[] aBodies = null;

            lock (m_UpdateLock)
            {
                if (m_updateList.Count > 0)
                {
                    aBodies = m_updateList.ToArray();
                    m_updateList.Clear();
                }
            }

            if (aBodies == null)
                return;

            m_updateTime = deltaTime;
            Parallel.ForEach(aBodies, Simulate);
        }

        private void Simulate(PhysicsBody body)
        {
            lock (body.LockObject)
            {
                float fMass, fSpeed, fAcceleration, fLinearDamp;
                V3 v3Direction, v3Position;

                fMass = body.Mass.Value;
                fSpeed = body.Speed.Value;
                fAcceleration = body.Acceleration.Value;
                v3Direction = body.Direction.Value;
                v3Position = body.Position.Value;
                fLinearDamp = body.LinearDamp.Value;

                fSpeed *= (fSpeed * m_updateTime.ElapsedGameTime.Milliseconds / fLinearDamp);

                v3Direction *= fSpeed;
            }
        }

        internal void ProcessBroadCollisions(DeltaTime deltaTime)
        {
            BoundingCollider[] aColliders = null;

            lock (m_colliderLock)
            {
                if (m_collisionsBroadList.Count > 0)
                {
                    aColliders = m_collisionsBroadList.ToArray();
                }
            }

            foreach (BoundingCollider collider in aColliders)
            {
                foreach (BoundingCollider otherCollider in aColliders)
                {
                    if (collider != otherCollider)
                    {
                        if (collider.Contains(otherCollider))
                        {
                            m_collisionsFineList.Add(new CollisionPair(collider.InnerCollider, otherCollider.InnerCollider));
                        }
                    }
                }
            }
        }

        internal void ProcessFineCollisions(DeltaTime deltaTime)
        {
            if (m_collisionsFineList.Count > 0)
            {
                foreach (CollisionPair pair in m_collisionsFineList)
                {
                    if (pair.ColliderOne.Contains(pair.ColliderTwo))
                    {
                        MaterialCollisionItem item;
                        PhysicsBody bodyOne = pair.ColliderOne.Body;
                        PhysicsBody bodyTwo = pair.ColliderTwo.Body;
                        CollisionBehaviour behaviour = CollisionBehaviour.DefaultCollision;

                        if (GetMaterialPair(pair.ColliderOne.Material, pair.ColliderTwo.Material, out item))
                        {
                            MatCollCallBack callBack = item.GetDelegate();

                            if (callBack != null)
                               behaviour = callBack(item.Pair, bodyOne, bodyTwo);
                        }

                        if (behaviour == CollisionBehaviour.DefaultCollision)
                        {
                            //do default collision here.
                        }
                    }
                }

                m_collisionsFineList.Clear();
            }
        }

        public void UpdateThreadProcess()
        {
            while (true)
            {
                DeltaTime deltaTime = AdjustFrameRate(m_deltaTime);
                UpdateSimulation(deltaTime);
                ProcessBroadCollisions(deltaTime);
                ProcessFineCollisions(deltaTime);
            }
        }

        public void AddColliderSim(BoundingCollider collider)
        {
            lock (m_colliderLock)
                m_collisionsBroadList.Add(collider);
        }

        public void RemoveColliderSim(BoundingCollider collider)
        {
            lock (m_colliderLock)
                m_collisionsBroadList.Remove(collider);
        }

        public DeltaTime DeltaTime { get { return m_deltaTime; } set { m_deltaTime = value; } }
    }
}
