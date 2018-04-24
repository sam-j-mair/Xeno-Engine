using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using MatCollCallBack = System.Action<XenoEngine.Systems.Physics.MaterialPair, XenoEngine.Systems.Physics.PhysicsBody, XenoEngine.Systems.Physics.PhysicsBody>;

namespace XenoEngine.Systems.Physics
{
    public class MaterialSlotItem
    {
        public string Name { get; set; }
        public Material Mat { get; set; }
    }

    public struct MaterialPairKey
    {
        private int m_nKey;

        internal MaterialPairKey(int nKey)
        {
            m_nKey = nKey;
        }

        internal int Key { get { return m_nKey; } set { m_nKey = value; } }
    }

    public class MaterialPair
    {
        private Material m_materialOne;
        private Material m_materialTwo;

        public MaterialPair(Material one, Material two)
        {
            m_materialOne = one;
            m_materialTwo = two;
        }

        public override int GetHashCode()
        {
 	        return m_materialOne.GetHashCode() + m_materialTwo.GetHashCode();
        }
    
        public static int GetMaterialPairHashKey(Material materialOne, Material materialTwo)
        {
            return (materialOne.GetHashCode() + materialTwo.GetHashCode());
        }
    }

    internal class MaterialCollisionItem
    {
        private MaterialPair m_pair;
        private MatCollCallBack m_delegate;

        public MaterialCollisionItem(Material materialOne,
                                    Material materialTwo,
                                    MatCollCallBack callBack)
        {
            m_pair = new MaterialPair(materialOne, materialTwo);
            m_delegate += callBack;
        }

        public void AddCallback(MatCollCallBack callBack)
        {
            m_delegate += callBack;
        }

        public void RemoveCallback(MatCollCallBack callback)
        {
            m_delegate -= callback;
        }

        public void Clear_Callbacks()
        {
            m_delegate = null;
        }

        internal MatCollCallBack GetDelegate()
        {
            return m_delegate;
        }

        public override int GetHashCode()
        {
            return m_pair.GetHashCode();
        }

        internal MaterialPair Pair { get { return m_pair; } }
    }
}
