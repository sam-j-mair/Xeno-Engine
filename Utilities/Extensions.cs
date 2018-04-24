using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XenoEngine;
using XenoEngine.Systems;
using Microsoft.Xna.Framework;

using V2 = Microsoft.Xna.Framework.Vector2;
using V3 = Microsoft.Xna.Framework.Vector3;
using V4 = Microsoft.Xna.Framework.Vector4;
using P2 = Microsoft.Xna.Framework.Point;

namespace XenoEngine.Utilities
{
    public static class UtilityFunctions
    {
        private static Random m_sRandomGenerator = new Random();

        public static V3 RandomVector3()
        {
            return new V3(m_sRandomGenerator.Next(),
                m_sRandomGenerator.Next(),
                m_sRandomGenerator.Next());
        }

        public static V3 RandomVector3(int? nXMinValue,
                                            int? nXMaxValue,
                                            int? nYMinValue,
                                            int? nYMaxValue,
                                            int? nZMinValue,
                                            int? nZMaxValue)
        {
            return new V3(m_sRandomGenerator.Next((nXMinValue != null) ? (int)nXMinValue : 0, (nXMaxValue != null) ? (int)nXMaxValue : int.MaxValue),
                m_sRandomGenerator.Next((nYMinValue != null) ? (int)nYMinValue : 0, (nYMaxValue != null) ? (int)nYMaxValue : int.MaxValue),
                m_sRandomGenerator.Next((nZMinValue != null) ? (int)nZMinValue : 0, (nZMaxValue != null) ? (int)nZMaxValue : int.MaxValue));
        }

        public static V2 RandomVector2()
        {
            return new V2(m_sRandomGenerator.Next(),
                m_sRandomGenerator.Next());
        }

        public static P2 ToPoint(this V3 v3vec)
        {
            return new P2((int)v3vec.X, (int)v3vec.Y);
        }

        public static P2 ToPoint(this V2 v2vec)
        {
            return new P2((int)v2vec.X, (int)v2vec.Y);
        }

        public static V3 ToVec3(this P2 point)
        {
            return new V3(point.X, point.Y, 0);
        }

        public static V2 ToVec2(this P2 point)
        {
            return new V2(point.X, point.Y);
        }

        public static V3 ToVec3(this V2 v2vec)
        {
            return new V3(v2vec.X, v2vec.Y, 0);
        }

        public static V2 ToVec2(this V3 v3vec)
        {
            return new V2(v3vec.X, v3vec.Y);
        }
    }
}
