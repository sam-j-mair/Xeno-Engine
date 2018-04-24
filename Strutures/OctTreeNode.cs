using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XenoEngine.Systems.Structures
{
    
    [Serializable]public delegate bool SearchFunctionDelegate<T>(OctTreeNode<T> node, object data);
    [Serializable]public delegate void AddObjectsFunctionDelegate<T>(OctTreeNode<T> node, object data);

    [Serializable]
    public class OctTreeNode<T> : TreeNode<OctTreeNode<T>>
    {
        List<T> m_containmentList;
        float   m_fLength;
        int     m_nDepth;
        int     m_nMaxDepth;
        Vector3 m_v3Center;

        public Vector3 Center { get { return m_v3Center; } }
        public float Length { get { return m_fLength; } }

        private static readonly Vector3[] Vertices =
        {
            new Vector3(-1f, 1f, -1f), new Vector3(1f, 1f, -1f),
            new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f),
            new Vector3(-1f, -1f, -1f), new Vector3(1f, -1f, -1f),
            new Vector3(1f, -1f, 1f), new Vector3(-1f, -1f, 1f),
            new Vector3(0, 0, 0)
       };

       //private static readonly short[] Indices =
       //{
        //   0, 1, 1, 2, 2, 3, 3, 0,
        //   4, 5, 5, 6, 6, 7, 7, 4,
        //   0, 4, 1, 5, 2, 6, 3, 7,
        //   7, 8
       //};

       //private static readonly VertexElement[] VertexElements =
       //{
       //   new VertexElement(0, 0, VertexElementFormat.Vector3,
       //       VertexElementUsage.Default, VertexElementUsage.Position, 0)
       //};

        public static void InitialiseGraphics(GraphicsDevice device)
        {
            //OctTreeNode<T>.graphicsDevice = device;
            //OctTreeNode<T>.vertexDeclaration = new VertexDeclaration(device, OctTreeNode<T>.VertexElements);
            //OctTreeNode<T>.effect = new BasicEffect(device, null);
        }

        private static GraphicsDevice    graphicsDevice;
        private static VertexDeclaration vertexDeclaration;
        private static BasicEffect       effect;

        public OctTreeNode(int nMaxDepth, int nDepth, float fParentWorldSize, Vector3 v3Center, AddObjectsFunctionDelegate<T> findFunc, object searchableData)
        {
            m_containmentList = new List<T>();
            m_fLength = fParentWorldSize / (float)Math.Pow(2, nDepth);
            m_nDepth = nDepth;
            m_nMaxDepth = nMaxDepth;
            m_v3Center = v3Center;
            int nNextDepth = nDepth + 1;
            float fQuater = m_fLength / 4;

            AddObjects(findFunc, searchableData);

            if(nNextDepth <= nMaxDepth)
            {
                AddChild(new OctTreeNode<T>(nMaxDepth, nNextDepth, fParentWorldSize, m_v3Center + new Vector3(-fQuater, fQuater, -fQuater), findFunc, searchableData));
                AddChild(new OctTreeNode<T>(nMaxDepth, nNextDepth, fParentWorldSize, m_v3Center + new Vector3(fQuater, fQuater, -fQuater), findFunc, searchableData));
                AddChild(new OctTreeNode<T>(nMaxDepth, nNextDepth, fParentWorldSize, m_v3Center + new Vector3(-fQuater, fQuater, fQuater), findFunc, searchableData));
                AddChild(new OctTreeNode<T>(nMaxDepth, nNextDepth, fParentWorldSize, m_v3Center + new Vector3(fQuater, fQuater, fQuater), findFunc, searchableData));
                AddChild(new OctTreeNode<T>(nMaxDepth, nNextDepth, fParentWorldSize, m_v3Center + new Vector3(-fQuater, -fQuater, -fQuater), findFunc, searchableData));
                AddChild(new OctTreeNode<T>(nMaxDepth, nNextDepth, fParentWorldSize, m_v3Center + new Vector3(fQuater, -fQuater, -fQuater), findFunc, searchableData));
                AddChild(new OctTreeNode<T>(nMaxDepth, nNextDepth, fParentWorldSize, m_v3Center + new Vector3(-fQuater, -fQuater, fQuater), findFunc, searchableData));
                AddChild(new OctTreeNode<T>(nMaxDepth, nNextDepth, fParentWorldSize, m_v3Center + new Vector3(fQuater, -fQuater, fQuater), findFunc, searchableData));
            }
        }

        public void AddObjectType(T containedObject)
        {
            m_containmentList.Add(containedObject);
        }

        public T[] GetObjectTypeList()
        {
            T[] aObjectTypes = new T[m_containmentList.Count];
            m_containmentList.CopyTo(aObjectTypes);
            return aObjectTypes;
        }

        private void AddObjects(AddObjectsFunctionDelegate<T> findFunc, object data)
        {
            if(findFunc != null)
            {
                findFunc(this, data);
            }
        }

        public bool Search(SearchFunctionDelegate<T> searchFunc, object data, out T[] aObjects)
        {
            bool bFound = false;
            aObjects = null;

            if(m_nDepth <= m_nMaxDepth)
            {
                foreach (OctTreeNode<T> childNode in this)
                {
                    if (searchFunc != null)
                    {
                        if (searchFunc(childNode, data))
                        {
                            bFound = childNode.Search(searchFunc, data, out aObjects);

                            if(m_nDepth == m_nMaxDepth)
                            {
                                aObjects = GetObjectTypeList();
                            }

                            break;
                        }
                    }
                }
            }

            return bFound;
        }

        public void Draw(DeltaTime deltaTime, ICamera camera)
        {
//             if (OctTreeNode<T>.graphicsDevice != null)
//             {
//                 // Setup the graphics device.
//                 OctTreeNode<T>.graphicsDevice.VertexDeclaration = OctTreeNode<T>.vertexDeclaration;
//                 OctTreeNode<T>.graphicsDevice.RenderState.DepthBufferEnable = false;
//                 OctTreeNode<T>.graphicsDevice.RenderState.AlphaBlendEnable = false;
//                 OctTreeNode<T>.graphicsDevice.RenderState.AlphaTestEnable = false;
// 
//                 // Setup the effect.
//                 OctTreeNode<T>.effect.DiffuseColor =  m_containmentList.Count > 0 ? Color.Crimson.ToVector3() : Color.Cyan.ToVector3();
//                 OctTreeNode<T>.effect.World = Matrix.CreateScale(Length / 2) *
//                        Matrix.CreateTranslation(Center);
// 
//                 OctTreeNode<T>.effect.View = camera.ViewMatrix;
//                 OctTreeNode<T>.effect.Projection = camera.ProjectionMatrix;
//                 // Apply the effect to the box.
// 
//                 OctTreeNode<T>.effect.Begin();
//                 // Apply the effect passes to the box.
// 
//                 foreach (EffectPass pass in OctTreeNode<T>.effect.CurrentTechnique.Passes)
//                 {
//                    // Draw the box.
//                    pass.Begin();
// 
//                    OctTreeNode<T>.graphicsDevice.DrawUserIndexedPrimitives<Vector3>
//                        (PrimitiveType.LineList, OctTreeNode<T>.Vertices, 0, OctTreeNode<T>.Vertices.Length,
//                        OctTreeNode<T>.Indices, 0, OctTreeNode<T>.Indices.Length / 2);
//                     
//                     //Hack
//                     foreach (T type in m_containmentList)
//                     {
//                        IMapObject mapObject = type as IMapObject;
//                        if(mapObject != null)
//                        {
//                            OctTreeNode<T>.graphicsDevice.DrawUserIndexedPrimitives<Vector3>(
//                                                   PrimitiveType.LineStrip,
//                                                   new Vector3[] { Center, Center, mapObject.Position }, 0, 2, new short[] { 0, 1 }, 0, 1);
//                        }
//                     }
// 
//                    pass.End();
//                 }
// 
//                 OctTreeNode<T>.effect.End();
// 
//                 //draw children recursively
//                 foreach (OctTreeNode<T> child in this)
//                 {
//                     child.Draw(gameTime, camera);
//                 }
        
            }
        }
    }

