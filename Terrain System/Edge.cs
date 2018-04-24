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
    class Edge
    {
        VertexPositionColor[] m_aVertices;

        Vector3 m_v3Line;

        public Edge(Vector3 v3StartPosition, Vector3 v3EndPosition)
        {
            m_aVertices = new VertexPositionColor[2];

            m_v3Line = (v3EndPosition - v3StartPosition);

            //We create the vertices to making it easy for rendering.
            m_aVertices[0] = new VertexPositionColor(v3StartPosition, new Color(1.4f, 1.4f, 1.4f));
            m_aVertices[0] = new VertexPositionColor(v3EndPosition, new Color(1.4f, 1.4f, 1.4f));
        }

        public Vector3 Line
        {
            get { return m_v3Line; }
        }
    }
}
