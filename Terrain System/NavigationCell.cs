
using Microsoft.Xna.Framework;


public enum NavigationCellSides
{
    Left,
    Right,
    Bottom,
    Count
}

namespace XenoEngine.Terrain
{
//     class NavigationCell
//     {
//         int              m_CellId;
//         static int m_nIndex;
//         MeshTriangle              m_meshTriangle;
//         internal NavigationCell[] m_aNeighborCells;
//         internal Edge[]           m_aEdges;
// 
//         public NavigationCell(Vector3 v3Left, Vector3 v3Top, Vector3 v3Right)
//         {
//             int nIndex = 0;
//             m_meshTriangle = new MeshTriangle(v3Left, v3Top, v3Right);
// 
//             m_aNeighborCells = new NavigationCell[(int)NavigationCellSides.Count];
//             m_aEdges = new Edge[(int)NavigationCellSides.Count];
// 
//             m_aEdges[nIndex++] = new Edge(v3Left, v3Top);
//             m_aEdges[nIndex++] = new Edge(v3Top, v3Right);
//             m_aEdges[nIndex++] = new Edge(v3Right, v3Left);
//         }
// 
//         public NavigationCell(MeshTriangle meshTriangle)
//         {
//             int nIndex = 0;
//             m_CellId = m_nIndex++;
// 
//             m_meshTriangle = meshTriangle;
// 
//             m_aNeighborCells = new NavigationCell[(int)NavigationCellSides.Count];
//             m_aEdges = new Edge[(int)NavigationCellSides.Count];
// 
//             m_aEdges[nIndex++] = new Edge(meshTriangle.Left, meshTriangle.Top);
//             m_aEdges[nIndex++] = new Edge(meshTriangle.Top, meshTriangle.Right);
//             m_aEdges[nIndex++] = new Edge(meshTriangle.Right, meshTriangle.Left);
//         }
//     }
}
