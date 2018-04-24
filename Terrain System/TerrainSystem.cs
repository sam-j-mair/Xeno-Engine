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
using System.Diagnostics;

namespace XenoEngine.Terrain
{
//     public class TerrainSystem : DrawableGameComponent
//     {
//         List<Triangle>      m_triangleList;
//         List<int>           m_indicesList;
//         DynamicIndexBuffer  m_dynTerrainIndexBuffer;
//         BasicEffect         m_basicEffect;
//         Texture2D           m_customTexture;
// 
//         //probably don't need to store most of this....but will store it for debugging.
//         float[,]            m_aHeightMap;
//         VertexDeclaration   m_vertexDeclaration;
//         VertexBuffer        m_vertexBuffer;
// 
//         VertexPositionNormalTexture[]   m_aTerrainVertices;
//         int[]                           m_anTerrainIndices;
// 
//         //cache the first two triangles for reference.
//         Triangle            m_leftTriangle;
//         Triangle            m_rightTriangle;
// 
//         //Debug
//         Matrix              m_m44OrthoView;
//         Matrix              m_m44OrthoProj;
// 
//         public TerrainSystem(Game game, string szMapName) : base(game)
//         {
//             GraphicsDevice device = Game.GraphicsDevice;
//             Texture2D heightMapTexture = Game.Content.Load<Texture2D>("Textures\\" + szMapName);
// 
//             m_basicEffect = new BasicEffect(device);
// 
//             m_triangleList = new List<Triangle>();
//             m_indicesList = new List<int>();
// 
//             m_aHeightMap = LoadHeightData(heightMapTexture);
// 
//             m_vertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
//             m_aTerrainVertices = CreateTerrainVertices();
//             m_anTerrainIndices = CreateTerrainIndices();
//             m_aTerrainVertices = GenerateNormalForTriangleStrip(m_aTerrainVertices, m_anTerrainIndices);
// 
//             m_vertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.SizeInBytes * m_aTerrainVertices.Length, BufferUsage.WriteOnly);
//             m_vertexBuffer.SetData(m_aTerrainVertices);
// 
//             //debug
//             int nTerrainSize = 512;
// 
//             m_leftTriangle = new Triangle(null, Vector2.Zero, new Vector2(nTerrainSize, 0), new Vector2(0, nTerrainSize), m_aHeightMap);
//             m_rightTriangle = new Triangle(null, new Vector2(nTerrainSize, nTerrainSize), new Vector2(0, nTerrainSize), new Vector2(nTerrainSize, 0), m_aHeightMap);
//             m_leftTriangle.AddNeighbours(null, null, m_rightTriangle);
//             m_rightTriangle.AddNeighbours(null, null, m_leftTriangle);
// 
//             m_triangleList.Add(m_leftTriangle);
//             m_triangleList.Add(m_rightTriangle);
// 
//             foreach (Triangle triangle in m_triangleList)
//             {
//                 triangle.AddIndices(ref m_indicesList);
//             }
// 
//             m_dynTerrainIndexBuffer = new DynamicIndexBuffer(game.GraphicsDevice, typeof(int), m_indicesList.Count, BufferUsage.WriteOnly);
//             m_dynTerrainIndexBuffer.SetData(m_indicesList.ToArray(), 0, m_indicesList.Count, SetDataOptions.Discard);
//             m_dynTerrainIndexBuffer.ContentLost += new EventHandler<EventArgs>(TerrainIndexBuffer_ContentLost);
// 
//             m_customTexture = Game.Content.Load<Texture2D>("Textures\\grass");
// 
//             m_m44OrthoView = Matrix.CreateLookAt(new Vector3(nTerrainSize / 2, 100, -nTerrainSize / 2), new Vector3(nTerrainSize / 2, 0, -nTerrainSize / 2), Vector3.Forward);
//             m_m44OrthoProj = Matrix.CreateOrthographic(nTerrainSize, nTerrainSize, 1, 10000);
//         }
// 
//         public void Start()
//         {
//             Game.Components.Add(this);
//         }
// 
//         private void TerrainIndexBuffer_ContentLost(object sender, EventArgs e)
//         {
//             m_dynTerrainIndexBuffer.Dispose();
//             m_dynTerrainIndexBuffer.SetData(m_indicesList.ToArray(), 0, m_indicesList.Count, SetDataOptions.Discard);
//         }
// 
//         private VertexPositionNormalTexture[] GenerateNormalForTriangleStrip(VertexPositionNormalTexture[] aVertices, int[] aIndices)
//         {
//             bool bSwapWinding = false;
//             //reset normals to zero to ensure a clean start
//             for (int i = 0; i < aVertices.Length; i++)
//                 aVertices[i].Normal = Vector3.Zero;
// 
//             for (int i = 2; i < aIndices.Length; i++)
//             {
//                 Vector3 v3FirstVec = aVertices[aIndices[i - 1]].Position - aVertices[aIndices[i]].Position;
//                 Vector3 v3SecondVec = aVertices[aIndices[i - 2]].Position - aVertices[aIndices[i]].Position;
//                 Vector3 v3Normal = Vector3.Cross(v3FirstVec, v3SecondVec);
// 
//                 v3Normal.Normalize();
// 
//                 if (bSwapWinding)
//                     v3Normal *= -1; 
// 
//                 if(!float.IsNaN(v3Normal.X))
//                 {
//                     aVertices[aIndices[i]].Normal += v3Normal;
//                     aVertices[aIndices[i - 1]].Normal += v3Normal;
//                     aVertices[aIndices[i - 2]].Normal += v3Normal;
//                 }
// 
//                 bSwapWinding = !bSwapWinding;
//             }
// 
//             for (int i = 0; i < aVertices.Length; i++)
//                 aVertices[i].Normal.Normalize();
// 
//             return aVertices;
// 
//         }
// 
//         private VertexPositionNormalTexture[] GenerateNormalForTriangleList(VertexPositionNormalTexture[] aVertices, int[] aIndices)
//         {
//             //reset normals to zero to ensure a clean start
//             for (int i = 0; i < aVertices.Length; i++)
//                 aVertices[i].Normal = Vector3.Zero;
// 
//             for(int i = 0; i < (aIndices.Length / 3); i++)
//             {
//                 Vector3 v3FirstVec = aVertices[aIndices[i * 3 + 1]].Position - aVertices[aIndices[i * 3]].Position;
//                 Vector3 v3SecondVec = aVertices[aIndices[i * 3 + 2]].Position - aVertices[aIndices[i * 3]].Position;
//                 Vector3 v3Normal = Vector3.Cross(v3SecondVec, v3FirstVec);
// 
//                 v3Normal.Normalize();
// 
//                 aVertices[aIndices[i * 3]].Normal += v3Normal;
//                 aVertices[aIndices[i * 3 + 1]].Normal += v3Normal;
//                 aVertices[aIndices[i * 3 + 2]].Normal += v3Normal;
//             }
// 
//             for (int i = 0; i < aVertices.Length; i++)
//                 aVertices[i].Normal.Normalize();
// 
//             return aVertices;
//         }
// 
//         private VertexPositionNormalTexture[] CreateTerrainVertices()
//         {
//             int nWidth = m_aHeightMap.GetLength(0);
//             int nHeight = m_aHeightMap.GetLength(1);
// 
//             VertexPositionNormalTexture[] terrainVertices = new VertexPositionNormalTexture[nWidth * nHeight];
// 
//             int i = 0;
// 
//             for (int z = 0; z < nHeight; z++)
//             {
//                 for(int x = 0; x < nWidth; x++)
//                 {
//                     Vector3 v3Position = new Vector3(x, m_aHeightMap[x, z], -z);
//                     Vector3 v3Normal = new Vector3(0, 0, 1);
//                     Vector2 v2TexCoord = new Vector2((float)x / 30.0f, (float)z / 30.0f);
// 
//                     terrainVertices[i++] = new VertexPositionNormalTexture(v3Position, v3Normal, v2TexCoord);
//                 }
//             }
// 
//             return terrainVertices;
//         }
// 
//         private int[] CreateTerrainIndices()
//         {
//             int nWidth = m_aHeightMap.GetLength(0);
//             int nHeight = m_aHeightMap.GetLength(1);
// 
//             int[] terrainIndices = new int[(nWidth) * 2 * (nHeight - 1)];
// 
//             int i = 0;
//             int z = 0;
// 
//             while (z < (nHeight - 1))
//             {
//                 for (int x = 0; x < nWidth; x++)
//                 {
//                     terrainIndices[i++] = x + z * nWidth;
//                     terrainIndices[i++] = x + (z + 1) * nWidth;
//                 }
// 
//                 z++;
// 
//                 if(z < (nHeight - 1))
//                 {
//                     for (int x = (nWidth - 1); x >= 0; x--)
//                     {
//                         terrainIndices[i++] = z + (z + 1) * nWidth;
//                         terrainIndices[i++] = x + z * nWidth;
//                     }
//                 }
// 
//                 z++;
//             }
// 
//             return terrainIndices;
//         }
// 
//         private float[,] LoadHeightData(Texture2D heightMapTexture)
//         {
//             float fMinimumHeight = 255;
//             float fMaximumHeight = 0;
// 
//             int nWidth = heightMapTexture.Width;
//             int nHeight = heightMapTexture.Height;
// 
//             Color[] heightMapColors = new Color[nWidth * nHeight];
//             heightMapTexture.GetData<Color>(heightMapColors);
// 
//             float[,] afHeightMap = new float[nWidth + 1, nHeight + 1];
// 
//             for (int x = 0; x < nWidth - 1; x++)
//             {
//                 for (int y = 0; y < nHeight - 1; y++)
//                 {
//                     afHeightMap[x, y] = heightMapColors[x + y * nWidth].R;
// 
//                     if (afHeightMap[x, y] < fMinimumHeight) fMinimumHeight = afHeightMap[x, y];
// 
//                     if (afHeightMap[x, y] > fMaximumHeight) fMaximumHeight = afHeightMap[x, y];
//                 }
//             }
// 
//             for (int x = 0; x < nWidth + 1; x++)
//                 afHeightMap[x, nHeight] = afHeightMap[x, nHeight - 1];
// 
//             for (int y = 0; y < nHeight; y++)
//                 afHeightMap[nWidth, y] = afHeightMap[nWidth - 1, y];
// 
//             for (int x = 0; x < nWidth; x++)
//             {
//                 for (int y = 0; y < nHeight; y++)
//                 {
//                     afHeightMap[x, y] = (afHeightMap[x, y] - fMinimumHeight) / (fMaximumHeight - fMinimumHeight) * 30.0f;
//                 }
//             }
// 
//             return afHeightMap;
//         }
// 
//         private void DrawOrthoGrid()
//         {
//             GraphicsDevice device = Game.GraphicsDevice;
//             ICamera camera = (ICamera)Game.Services.GetService(typeof(ICamera));
// 
//             Debug.Assert(camera != null, "A camera hasn't been assigned to the ICamera Service.");
// 
//             int nWidth = m_aHeightMap.GetLength(0);
//             int nHeight = m_aHeightMap.GetLength(1);
// 
//             device.RenderState.FillMode = FillMode.WireFrame;
//             device.RenderState.AlphaBlendEnable = true;
// 
//             m_basicEffect.World = Matrix.Identity;
//             m_basicEffect.View = m_m44OrthoView;
//             m_basicEffect.Projection = m_m44OrthoProj;
//             m_basicEffect.TextureEnabled = false;
// 
// 
//             float fColour = 0.4f;
//             device.RenderState.BlendFactor = new Color(new Vector4(fColour, fColour, fColour, fColour));
//             device.RenderState.SourceBlend = Blend.BlendFactor;
//             device.RenderState.DestinationBlend = Blend.InverseBlendFactor;
// 
//             m_basicEffect.Begin();
// 
//             foreach (EffectPass pass in m_basicEffect.CurrentTechnique.Passes)
//             {
//                 pass.Begin();
// 
//                 device.Vertices[0].SetSource(m_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
//                 device.Indices = m_dynTerrainIndexBuffer;
//                 device.VertexDeclaration = m_vertexDeclaration;
//                 int nNumTriangles = m_indicesList.Count / 3;
//                 device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, nWidth * nHeight, 0, nNumTriangles);
// 
//                 pass.End();
//             }
// 
//             m_basicEffect.End();
//             device.RenderState.AlphaBlendEnable = false;
//         }
// 
//         private void DrawTerrain()
//         {
//             GraphicsDevice device = Game.GraphicsDevice;
//             ICamera camera = (ICamera)Game.Services.GetService(typeof(ICamera));
// 
//             Debug.Assert(camera != null, "A camera hasn't been assigned to the ICamera Service.");
// 
//             int nWidth = m_aHeightMap.GetLength(0);
//             int nHeight = m_aHeightMap.GetLength(1);
// 
//             device.RenderState.FillMode = FillMode.Solid;
//             device.RenderState.AlphaBlendEnable = false;
// 
//             m_basicEffect.World = Matrix.Identity;
//             m_basicEffect.View = camera.ViewMatrix;
//             m_basicEffect.Projection = camera.ProjectionMatrix;
//             m_basicEffect.Texture = m_customTexture;
//             m_basicEffect.TextureEnabled = true;
// 
// //             device.RenderState.FillMode = FillMode.WireFrame;
// //             device.RenderState.AlphaBlendEnable = true;
// // 
// //             m_basicEffect.World = Matrix.Identity;
// //             m_basicEffect.View = camera.ViewMatrix;
// //             m_basicEffect.Projection = camera.ProjectionMatrix;
// //             m_basicEffect.TextureEnabled = false;
// // 
// //             float fColour = 0.4f;
// //             device.RenderState.BlendFactor = new Color(new Vector4(fColour, fColour, fColour, fColour));
// //             device.RenderState.SourceBlend = Blend.BlendFactor;
// //             device.RenderState.DestinationBlend = Blend.InverseBlendFactor;
// 
//             m_basicEffect.Begin();
// 
//             foreach (EffectPass pass in m_basicEffect.CurrentTechnique.Passes)
//             {
//                 pass.Begin();
// 
//                 device.Vertices[0].SetSource(m_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
//                 device.Indices = m_dynTerrainIndexBuffer;
//                 device.VertexDeclaration = m_vertexDeclaration;
//                 int nNumTriangles = m_triangleList.Count;
//                 device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, nWidth * nHeight, 0, nNumTriangles);
// 
//                 pass.End();
//             }
// 
//             m_basicEffect.End();
//         }
// 
//         private void UpdateTrianlges()
//         {
//             GraphicsDevice device = Game.GraphicsDevice;
//             ICamera camera = (ICamera)Game.Services.GetService(typeof(ICamera));
// 
//             Debug.Assert(camera != null, "A camera hasn't been assigned to the ICamera Service.");
// 
//             List<Triangle> splitList = new List<Triangle>();
//             List<Triangle> mergeList = new List<Triangle>();
//             List<Triangle> remainderList = new List<Triangle>();
//             List<Triangle> leftOverList = new List<Triangle>();
//             List<Triangle> newTriangleList = new List<Triangle>(m_triangleList.Count);
// 
//             Matrix worldViewProjectionMatrix = Matrix.Identity * camera.ViewMatrix * camera.ProjectionMatrix;
//             BoundingFrustum cameraFrustrum = new BoundingFrustum(worldViewProjectionMatrix);
// 
//             foreach (Triangle triangle in m_triangleList)
//             {
//                 triangle.CreateSplitList(ref splitList, ref remainderList, ref worldViewProjectionMatrix, ref cameraFrustrum);
//             }
// 
//             foreach (Triangle triangle in splitList)
//             {
//                 triangle.ProcessSplitList(ref newTriangleList);
//             }
// 
//             foreach (Triangle triangle in remainderList)
//             {
//                 triangle.CreateMergeList(ref mergeList, ref leftOverList, ref worldViewProjectionMatrix, ref cameraFrustrum);
//             }
// 
//             foreach (Triangle triangle in mergeList)
//             {
//                 triangle.ProcessMergeList(ref newTriangleList, ref worldViewProjectionMatrix, ref cameraFrustrum);
//             }
// 
//             foreach(Triangle triangle in leftOverList)
//             {
//                 triangle.ProcessLeftOvers(ref newTriangleList);
//             }
// 
//             m_triangleList = newTriangleList;
//             m_triangleList.TrimExcess();
//         }
// 
//         private void UpdateIndexBuffer()
//         {
//             GraphicsDevice device = Game.GraphicsDevice;
// 
//             m_indicesList.Clear();
// 
//             foreach(Triangle triangle in m_triangleList)
//             {
//                 triangle.AddIndices(ref m_indicesList);
//             }
// 
//             if(m_dynTerrainIndexBuffer.SizeInBytes / sizeof(int) < m_indicesList.Count)
//             {
//                 m_dynTerrainIndexBuffer.Dispose();
//                 m_dynTerrainIndexBuffer = new DynamicIndexBuffer(device, typeof(int), m_indicesList.Count, BufferUsage.WriteOnly);
//             }
// 
//             m_dynTerrainIndexBuffer.SetData(m_indicesList.ToArray(), 0, m_indicesList.Count, SetDataOptions.Discard);
//         }
// 
//         public override void Update(GameTime gameTime)
//         {
//             UpdateTrianlges();
//             UpdateIndexBuffer();
// 
//             Game.Window.Title = "Triangles drawn: " + m_triangleList.Count.ToString();
// 
//             base.Update(gameTime);
//         }
// 
//         public override void Draw(GameTime gameTime)
//         {
//             GraphicsDevice device = Game.GraphicsDevice;
// 
//             device.RenderState.CullMode = CullMode.None;
// 
//             //DrawOrthoGrid();
//             DrawTerrain(); 
//             device.Indices = null;
// 
//             base.Draw(gameTime);
//         }
//     }
// 
}
