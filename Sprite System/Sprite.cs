using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XenoEngine.Systems.Sprite_Systems
{
    [Serializable]
    public class Sprite : SpriteBase, ISprite, IDisposable
    {
        private IRenderLayer<SpriteInfo> m_system;
        //private bool        m_bUseParentTransform;
        private bool m_bIsActive;
        #region constructors
        public Sprite(IRenderLayer<SpriteInfo> spriteSystem, string szAssetName, Vector3 v3Position, Color colour, bool bStartActive) :
            base(szAssetName, null, v3Position, colour)
        {
            Init(spriteSystem, bStartActive);

        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public Sprite(IRenderLayer<SpriteInfo> spriteSystem, string szAssetName, StreamChunk streamChunk, Vector3 v3Position, Color colour, bool bStartActive) :
            base(szAssetName, streamChunk, v3Position, colour)
        {
            Init(spriteSystem, bStartActive);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public Sprite(IRenderLayer<SpriteInfo> spriteSystem, string szFontName, StreamChunk streamChunk, string szTextString, Vector3 v3Position, Color colour, bool bStartActive) :
            base(szFontName, streamChunk, szTextString, v3Position, colour)
        {
            Init(spriteSystem, bStartActive);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public Sprite(IRenderLayer<SpriteInfo> spriteSystem, string szAssetName, StreamChunk streamChunk, Vector3 v3Position, Color colour, List<AnimationDescription> animations) :
            base(szAssetName, v3Position, colour, animations)
        {
            Init(spriteSystem, true);
        }
        #endregion
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void Init(IRenderLayer<SpriteInfo> spriteSystem, bool bStartActive)
        {
            //This could be done externally maybe.
            m_system = spriteSystem;
            Debug.Assert(m_system != null, "A SpriteSystem hasn't been registered with the services");
#warning There is a bug here because we cant assert that this is a spriteLayer due to the text sprite inheriting from this??!!!!

            if (bStartActive)
                m_system.RegisterSprite(this);

            m_bIsActive = bStartActive;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void Deactivate()
        {
            if (m_bIsActive)
            {
                m_system.DeregisterSprite(this);
                m_bIsActive = false;
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void Activate()
        {
            if (!m_bIsActive)
            {
                m_system.RegisterSprite(this);
                m_bIsActive = true;
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public new void Dispose()
        {
            //This could be done externally maybe.
            m_system.DeregisterSprite(this);


            base.Dispose();
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public Color Colour { get { return m_spriteInfo.m_colour; } set { m_spriteInfo.m_colour = value; } }
        public Texture2D Graphic { get { return m_spriteInfo.m_texture2D; } set { m_spriteInfo.m_texture2D = value; } }
        public Vector2 Position { get { return m_spriteInfo.m_v2Position; } set { m_spriteInfo.m_v2Position = value; } }
        public Vector2 Origin { get { return m_spriteInfo.m_v2Origin; } set { m_spriteInfo.m_v2Origin = value; } }
        public Vector2 ScaleFactor { get { return m_spriteInfo.m_v2ScaleFactor; } set { m_spriteInfo.m_v2ScaleFactor = value; } }
        public SpriteEffects SpriteEffects { get { return m_spriteInfo.m_eSpriteEffects; } set { m_spriteInfo.m_eSpriteEffects = value; } }
        public float Rotation { get { return m_spriteInfo.m_fRotation; } set { m_spriteInfo.m_fRotation = value; } }
        public float Depth { get { return m_spriteInfo.m_fDepth; } set { m_spriteInfo.m_fDepth = value; } }
        public bool IsActive { get { return m_bIsActive; } }
    }
}
