using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.GeneralSystems;

namespace XenoEngine.Systems.Sprite_Systems
{
    [Serializable]
    public class SpriteRenderLayer : RenderLayer<SpriteInfo>
    {
        //we have to do this because this isn't serializable.
        [NonSerialized]
        protected SpriteBatch m_spriteBatch;

        public SpriteRenderLayer(int nMaxItems) : base(nMaxItems)
        {
            m_spriteBatch = new SpriteBatch(EngineServices.GetSystem<IGameSystems>().GraphicsDevice);
            BlendState = BlendState.Opaque;
        }

        public SpriteRenderLayer(int nMaxItems, EffectSettings settings) : base(nMaxItems)
        {
            m_spriteBatch = new SpriteBatch(EngineServices.GetSystem<IGameSystems>().GraphicsDevice);
            BlendState = BlendState.Opaque;
            EffectSettings = settings;
        }

        [OnDeserializing]
        internal new void OnDeserialized(StreamingContext context)
        {
            m_spriteBatch = new SpriteBatch(EngineServices.GetSystem<IGameSystems>().GraphicsDevice);
            BlendState = BlendState.Opaque; 
        }

        public override void Draw(DeltaTime deltaTime)
        {
            CallRequestInfo(deltaTime);
 
            if (m_RenderInfoList.Count > 0)
            {
                if(EffectSettings != null)
                    EffectSettings.m_fpUpdate(EffectSettings.m_effect, EffectSettings.m_effectSettings);
                //This is a little ugly but ill work it out later
                if (Effect != null)
                    m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState, null, null, null, Effect);
                else
                    m_spriteBatch.Begin();
 
                foreach (SpriteInfo spriteInfo in m_RenderInfoList)
                {
                    m_spriteBatch.Draw(spriteInfo.m_texture2D,
                        spriteInfo.m_v2Position,
                        new Rectangle((int)(((spriteInfo.m_animationData.m_frameSize.X) * spriteInfo.m_animationData.m_currentFrame.X)),
                                     (int)(((spriteInfo.m_animationData.m_frameSize.Y) * spriteInfo.m_animationData.m_currentFrame.Y)),
                                     spriteInfo.m_animationData.m_frameSize.X,
                                     spriteInfo.m_animationData.m_frameSize.Y),
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
            base.Draw(deltaTime);
        }

        public new void Dispose()
        {
            m_spriteBatch = null;
            base.Dispose();
        }
    }
}
