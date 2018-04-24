using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections;

namespace XenoEngine.Terrain
{
    //The drawing will be taken out ...its just for debug.
//     public class NavigationMesh : DrawableGameComponent
//     {
//         Model m_navMesh;
// 
//         Hashtable m_navigationCellHashTable;
// 
//         List<NavigationCell> m_navigationCellList;
// 
// 
//         public NavigationMesh(Game game, string szModelName) : base(game)
//         {
//             m_navigationCellList = new List<NavigationCell>();
//             m_navigationCellHashTable = new Hashtable();
//             m_navMesh = Game.Content.Load<Model>("Model\\" + szModelName);
// 
//             ProcessNavMesh();
//         }
// 
//         private void ProcessNavMesh()
//         {
//             MeshTriangle[] aTriangles = (MeshTriangle[])m_navMesh.Tag;
// 
//             //Create the navigation cells.
//             foreach (MeshTriangle meshTriangle in aTriangles)
//             {
//                 m_navigationCellList.Add(new NavigationCell(meshTriangle)); 
//             }
// 
//             SetNeighbours();
//         }
// 
//         private void SetNeighbours()
//         {
//             foreach (NavigationCell navCell in m_navigationCellList)
//             {
//                 foreach (NavigationCell testNavCell in m_navigationCellList)
//                 {
//                     if(navCell != testNavCell)
//                     {
//                         for(int nSide = 0; nSide < (int)NavigationCellSides.Count; ++nSide)
//                         {
//                             //This is ugly but is just a test.
//                             if(navCell.m_aEdges[(int)NavigationCellSides.Left].Line == -testNavCell.m_aEdges[nSide].Line)
//                             {
//                                 navCell.m_aNeighborCells[(int)NavigationCellSides.Left] = testNavCell; 
//                             }
//                             else if(navCell.m_aEdges[(int)NavigationCellSides.Right].Line == -testNavCell.m_aEdges[nSide].Line)
//                             {
//                                 navCell.m_aNeighborCells[(int)NavigationCellSides.Right] = testNavCell;
//                             }
//                             else if(navCell.m_aEdges[(int)NavigationCellSides.Bottom].Line == -testNavCell.m_aEdges[nSide].Line)
//                             {
//                                 navCell.m_aNeighborCells[(int)NavigationCellSides.Bottom] = testNavCell;
//                             }
// 
//                         }
//                     }
//                 }
//             }
//         }
//     }
}
