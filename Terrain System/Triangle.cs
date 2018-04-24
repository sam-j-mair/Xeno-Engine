using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace XenoEngine.Terrain
{
    class Triangle
    {
        #region members
        Triangle m_leftNeighbour;
        Triangle m_rightNeighbour;
        Triangle m_backNeighbour;

        Triangle m_parent;

        Triangle m_leftChild;
        Triangle m_rightChild;

        Vector3 m_v3LeftPosition;
        Vector3 m_v3CentrePosition;

        int m_nTopIndex;
        int m_nLeftIndex;
        int m_nRightIndex;

        public bool m_bSplit = false;
        public bool m_bAddedToMergeList;
        #endregion
        //------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------
        //Constructor
        #region contructor
        public Triangle(Triangle parent, Vector2 topPoint, Vector2 leftPoint, Vector2 rightPoint, float[,] heightMap)
        {
            int resolution = heightMap.GetLength(0);
            m_nTopIndex = (int)(topPoint.X + topPoint.Y * resolution);
            m_nLeftIndex = (int)(leftPoint.X + leftPoint.Y * resolution);
            m_nRightIndex = (int)(rightPoint.X + rightPoint.Y * resolution);

            m_v3LeftPosition = new Vector3(leftPoint.X, heightMap[(int)leftPoint.X, (int)leftPoint.Y], -leftPoint.Y);

            Vector2 v3Centre = (leftPoint + rightPoint) / 2;

            m_v3CentrePosition = new Vector3(v3Centre.X, heightMap[(int)v3Centre.X, (int)v3Centre.Y], -v3Centre.Y);

            m_parent = parent;

            //Recursively create the rest of the triangle mesh...
            if(Vector2.Distance(leftPoint, topPoint) > 1)
            {
                m_leftChild = new Triangle(this, v3Centre, topPoint, leftPoint, heightMap);
                m_rightChild = new Triangle(this, v3Centre, rightPoint, topPoint, heightMap);
            }
        }
        #endregion
        //------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------
        #region methods
        public void AddNeighbours(Triangle leftNeighbour, Triangle rightNeighbour, Triangle backNeighbour)
        {
            m_leftNeighbour = leftNeighbour;
            m_rightNeighbour = rightNeighbour;
            m_backNeighbour = backNeighbour;

            if(m_leftChild != null)
            {
                Triangle backNeighbourRightChild = null;
                Triangle backNeighbourLeftChild = null;
                Triangle leftNeighbourRightChild = null;
                Triangle rightNeighbourLeftChild = null;

                if(backNeighbour != null)
                {
                    backNeighbourLeftChild = backNeighbour.m_leftChild;
                    backNeighbourRightChild = backNeighbour.m_rightChild;
                }

                if (leftNeighbour != null)
                    leftNeighbourRightChild = leftNeighbour.m_rightChild;

                if (rightNeighbour != null)
                    rightNeighbourLeftChild = rightNeighbour.m_leftChild;

                //Recurse into children.
                m_leftChild.AddNeighbours(m_rightChild, backNeighbourRightChild, leftNeighbourRightChild);
                m_rightChild.AddNeighbours(backNeighbourLeftChild, m_leftChild, rightNeighbourLeftChild);

            }
        }
        //------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------

        public void AddIndices(ref List<int> indicesList)
        {
            indicesList.Add(m_nLeftIndex);
            indicesList.Add(m_nTopIndex);
            indicesList.Add(m_nRightIndex);
        }

        //This allows the triangle to decide if it should split or not.
        public bool ShouldSplit(ref Matrix m44Wvp, ref BoundingFrustum boundingFrustrum)
        {
            bool bShouldSplit = false;

            if(boundingFrustrum.Contains(m_v3CentrePosition) != ContainmentType.Disjoint)
            {
                Vector4 v4LeftScreenPos = Vector4.Transform(m_v3LeftPosition, m44Wvp);
                Vector4 v4ScreenPos = Vector4.Transform(m_v3CentrePosition, m44Wvp);

                v4LeftScreenPos /= v4LeftScreenPos.W;
                v4ScreenPos /= v4ScreenPos.W;

                Vector4 v4Difference = v4LeftScreenPos - v4ScreenPos;
                Vector2 v2ScreenDifference = new Vector2(v4Difference.X, v4Difference.Y);

                float fThreshold = 0.05f;

                if (v2ScreenDifference.Length() > fThreshold)
                    bShouldSplit = true;
            }

            return bShouldSplit;
        }

        public bool CheckMerge(ref List<Triangle> mergeList, ref Matrix m44Wvp, ref BoundingFrustum boundingFrustrum)
        {
            bool bShouldMerge = false;

            if(!m_bAddedToMergeList)
            {
                if(CanMerge())
                {
                    if(!ShouldSplit(ref m44Wvp, ref boundingFrustrum))
                    {
                        bShouldMerge = true;

                        if(m_backNeighbour != null)
                        {
                            if(m_backNeighbour.ShouldSplit(ref m44Wvp, ref boundingFrustrum))
                            {
                                bShouldMerge = false;
                            }
                        }

                    }
                }
            }

            if(bShouldMerge)
            {
                m_bAddedToMergeList = true;
                mergeList.Add(this);

                if(m_backNeighbour != null)
                {
                    m_backNeighbour.m_bAddedToMergeList = true;
                    mergeList.Add(m_backNeighbour);
                }
            }

            return m_bAddedToMergeList;
        }

        public bool CanMerge()
        {
            bool bCannotMerge = false;

            if (m_leftChild != null)
                bCannotMerge |= m_leftChild.m_bSplit;

            if (m_rightChild != null)
                bCannotMerge |= m_rightChild.m_bSplit;

            if (m_backNeighbour != null)
            {
                if(m_backNeighbour.m_leftChild != null)
                    bCannotMerge |= m_backNeighbour.m_leftChild.m_bSplit;

                if (m_backNeighbour.m_rightChild != null)
                    bCannotMerge |= m_backNeighbour.m_rightChild.m_bSplit;
            }

            return !bCannotMerge;
        }

        // This propagates the split to the required neighbors recursively.
        public void PropagateSplit(ref List<Triangle> splitList)
        {
            if(!m_bSplit)
            {
                m_bSplit = true;
                splitList.Add(this);

                if (m_backNeighbour != null)
                    m_backNeighbour.PropagateSplit(ref splitList);

                if (m_parent != null)
                    m_parent.PropagateSplit(ref splitList);
            }
        }

        public void CreateMergeList(ref List<Triangle> mergeList, ref List<Triangle> leftOverList, ref Matrix m44Wvp, ref BoundingFrustum boundingFrustrum)
        {
            bool bCannotMerge = true;

            if (m_parent != null)
                bCannotMerge = !m_parent.CheckMerge(ref mergeList, ref m44Wvp, ref boundingFrustrum);

            if (bCannotMerge)
                leftOverList.Add(this);
        }

        public void CreateSplitList(ref List<Triangle> splitList, ref List<Triangle> remainderList, ref Matrix m44Wvp, ref BoundingFrustum boundingFrustrum)
        {
            bool bHasSplit = false;

            m_bAddedToMergeList = false;

            if(m_leftChild != null && (!m_bSplit))
            {
                if(ShouldSplit(ref m44Wvp, ref boundingFrustrum))
                {
                    PropagateSplit(ref splitList);
                    bHasSplit = true;
                }
            }

            if(!bHasSplit)
            {
                remainderList.Add(this);
            }
        }

        public void ProcessSplitList(ref List<Triangle> toDrawList)
        {
            if (!m_rightChild.m_bSplit)
                toDrawList.Add(m_rightChild);

            if (!m_leftChild.m_bSplit)
                toDrawList.Add(m_leftChild);
        }

        public void ProcessMergeList(ref List<Triangle> toDrawList, ref Matrix m44Wvp, ref BoundingFrustum boundingFrustrum)
        {
            m_bSplit = false;
            toDrawList.Add(this);
        }

        public void ProcessLeftOvers(ref List<Triangle> toDrawList)
        {
            if (!m_bSplit)
                toDrawList.Add(this);
        }
        #endregion
    }
}
