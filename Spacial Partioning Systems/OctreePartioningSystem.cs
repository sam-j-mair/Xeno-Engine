using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.Systems.Structures;


namespace XenoEngine.Systems.SpacialPartioningSystem
{
    [Serializable]
    public class ProximityMap<T>
    {

        SearchFunctionDelegate<T>       m_searchFunc;
        AddObjectsFunctionDelegate<T>   m_findFunc;
        OctTreeNode<T>                  m_octreeRoot;

        public ProximityMap(  SearchFunctionDelegate<T> searchFunc,
                                        AddObjectsFunctionDelegate<T> findFunc, 
                                        float fWorldSize,
                                        int nMaxDepth,
                                        Vector3 v3Center,
                                        object searchableData,
                                        GraphicsDevice device)
        {
            m_searchFunc = searchFunc;
            m_findFunc = findFunc;

            OctTreeNode<T>.InitialiseGraphics(device);

            m_octreeRoot = new OctTreeNode<T>(nMaxDepth, 0, fWorldSize, v3Center, findFunc, searchableData);
        }

        public bool Search(out T[] aObjects, object data)
        {
            return m_octreeRoot.Search(m_searchFunc, data, out aObjects);
        }

        public void Draw(DeltaTime deltaTime, ICamera camera)
        {
            m_octreeRoot.Draw(deltaTime, camera);
        }
    }
}
