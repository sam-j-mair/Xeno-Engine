using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace XenoEngine.Systems.Sprite_Systems
{
    [Serializable]
    public class TextSprite : Sprite
    {
        public TextSprite(IRenderLayer<SpriteInfo> spriteSystem, string szFontName, StreamChunk streamChunk,string szTextString, Vector3 v3Position, Color colour, bool bStartActive) :
            base(spriteSystem, szFontName, streamChunk, szTextString, v3Position, colour, bStartActive)
        {
            Debug.Assert(spriteSystem is FontLayer);
        }

        public Vector2 StringSize { get { return m_spriteInfo.m_spriteFont.MeasureString(m_spriteInfo.m_szTextString); } }
        public String TextString { get { return m_spriteInfo.m_szTextString; } set { m_spriteInfo.m_szTextString = value; } }
    }
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------
    [Serializable]
    public class GameSprite : Sprite
    {
        public GameSprite(IRenderLayer<SpriteInfo> spriteSystem, string szAssetName, Vector3 v3Position, Color colour, bool bStartActive) :
            base(spriteSystem, szAssetName, v3Position, colour, bStartActive)
        {
            Validate(spriteSystem);
            
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public GameSprite(IRenderLayer<SpriteInfo> spriteSystem, string szAssetName, StreamChunk streamChunk, Vector3 v3Position, Color colour, bool bStartActive) :
            base(spriteSystem, szAssetName, streamChunk, v3Position, colour, bStartActive)
        {
            Validate(spriteSystem);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public GameSprite(IRenderLayer<SpriteInfo> spriteSystem, string szAssetName, StreamChunk streamChunk, Vector3 v3Position, Color colour, List<AnimationDescription> animations) :
            base(spriteSystem, szAssetName, streamChunk, v3Position, colour, animations)
        {
            Validate(spriteSystem);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void Validate(IRenderLayer<SpriteInfo> spriteSystem)
        {
            Debug.Assert(spriteSystem is RenderLayer<SpriteInfo>);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        protected void OnSerializing(StreamingContext context)
        {

        }
    }
}
