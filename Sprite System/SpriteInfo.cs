using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XenoEngine.Serialization.Proxies;

namespace XenoEngine.Systems.Sprite_Systems
{
    [Serializable]
    public class SpriteInfo
    {
        private SerializableTexture m_texture = new SerializableTexture();
        private SerializableFont m_font = new SerializableFont();

        //These are used instead of the texture
        public String           m_szTextString;
        public Vector2          m_v2Position;
        public Color            m_colour;
        public Vector2          m_v2Origin;
        public Vector2          m_v2ScaleFactor;
        public SpriteEffects    m_eSpriteEffects;
        public float            m_fRotation;
        public float            m_fDepth;

        public SpriteAnimationData m_animationData;

        public Texture2D m_texture2D { get { return m_texture.WrappedType; } set { m_texture.WrappedType = value; } }
        public SpriteFont m_spriteFont { get { return m_font.WrappedType; } set { m_font.WrappedType = value; } }

    }
}
