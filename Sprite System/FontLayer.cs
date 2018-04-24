using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.GeneralSystems;

namespace XenoEngine.Systems.Sprite_Systems
{
    [Serializable]
    public class FontLayer : RenderLayer<SpriteInfo>
    {
        [NonSerialized] protected SpriteBatch m_spriteBatch;

        public FontLayer(int nMaxSprites) : base(nMaxSprites)
        {
            m_spriteBatch = new SpriteBatch(EngineServices.GetSystem<IGameSystems>().GraphicsDevice);
        }

        //This is done for type safety
        public void RegisterSprite(TextSprite sprite)
        {
            base.RegisterSprite(sprite);
        }

        //This is done for type safety
        public void DeregisterSprite(TextSprite sprite)
        {
            base.DeregisterSprite(sprite);
        }

        [OnDeserializing]
        internal void OnDeserializing(StreamingContext context)
        {
            m_spriteBatch = new SpriteBatch(EngineServices.GetSystem<IGameSystems>().GraphicsDevice);
        }

        public override void Draw(DeltaTime deltaTime)
        {
            CallRequestInfo(deltaTime);

            if (m_RenderInfoList.Count > 0)
            {
                m_spriteBatch.Begin();

                foreach (SpriteInfo spriteInfo in m_RenderInfoList)
                {
                    m_spriteBatch.DrawString(spriteInfo.m_spriteFont,
                        spriteInfo.m_szTextString,
                        spriteInfo.m_v2Position,
                        spriteInfo.m_colour,
                        spriteInfo.m_fRotation,
                        spriteInfo.m_v2Origin,
                        spriteInfo.m_v2ScaleFactor,
                        spriteInfo.m_eSpriteEffects,
                        spriteInfo.m_fDepth);
                }

                m_spriteBatch.End();

                m_RenderInfoList.Clear();
            }

            base.Update(deltaTime);
        }

        public new void Dispose()
        {
            m_spriteBatch = null;
            base.Dispose();
        }

    }
}
