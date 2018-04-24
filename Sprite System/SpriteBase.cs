using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.GeneralSystems;

namespace XenoEngine.Systems.Sprite_Systems
{
    [Serializable]
    public abstract class SpriteBase : IInfoBase<SpriteInfo>, IDisposable
    {
        public event Action<SpriteInfo> SendInfo;
        protected SpriteInfo m_spriteInfo;
        protected bool m_bIsDisposed;
        protected SpriteAnimation m_spriteAnimation;

        public SpriteBase(string szAssetName, StreamChunk streamChunk, Vector3 v3Position, Color colour)
        {
#warning these need to be switched to stream loading.
            m_spriteInfo = new SpriteInfo();

            if(streamChunk != null)
                m_spriteInfo.m_texture2D = streamChunk.GetAssetObjectByName<Texture2D>(szAssetName);
            else
                m_spriteInfo.m_texture2D = EngineServices.GetSystem<IGameSystems>().Content.Load<Texture2D>(szAssetName);

            InitialiseSprite(ref v3Position, colour);

            //m_animDirection = AnimationDirection
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public SpriteBase(string szFontName, StreamChunk streamChunk, string szTextString, Vector3 v3Position, Color colour)
        {
            m_spriteInfo = new SpriteInfo();

            if (streamChunk != null)
                m_spriteInfo.m_spriteFont = streamChunk.GetAssetObjectByName<SpriteFont>(szFontName);
            else
                m_spriteInfo.m_spriteFont = EngineServices.GetSystem<IGameSystems>().Content.Load<SpriteFont>(szFontName);

            m_spriteInfo.m_szTextString = szTextString;
            InitialiseSprite(ref v3Position, colour);
            
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public SpriteBase(string szAssetName, Vector3 v3Position, Color colour, List<AnimationDescription> animList)
        {
            m_spriteInfo = new SpriteInfo();
            m_spriteAnimation = new SpriteAnimation(animList);
            m_spriteInfo.m_texture2D = EngineServices.GetSystem<IGameSystems>().Content.Load<Texture2D>(szAssetName);
            InitialiseSprite(ref v3Position, colour);
            //m_animDirection = AnimationDirection
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void InitialiseSprite(ref Vector3 v3Position, Color colour)
        {
            m_spriteInfo.m_v2Position.X = v3Position.X;
            m_spriteInfo.m_v2Position.Y = v3Position.Y;
            m_spriteInfo.m_fDepth = v3Position.Z;
            m_spriteInfo.m_fRotation = 0.0f;
            m_spriteInfo.m_eSpriteEffects = SpriteEffects.None;
            m_spriteInfo.m_v2Origin = Vector2.Zero;
            m_spriteInfo.m_v2ScaleFactor = Vector2.One;
            m_spriteInfo.m_colour = colour;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void PlayAnimation(string szAnimName)
        {
            Debug.Assert(m_spriteAnimation != null, "No animation data is set up for this sprite");
            m_spriteAnimation.PlayAnimation(szAnimName);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void SetFrame(Frame frame)
        {
            Debug.Assert(m_spriteAnimation != null, "No animation data is set up for this sprite");
            m_spriteAnimation.SetFrame(frame);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void SetSequence(string szSequenceName)
        {
            Debug.Assert(m_spriteAnimation != null, "No animation data is set up for this sprite");
            m_spriteAnimation.SetSequence(szSequenceName);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void InfoRequested(DeltaTime deltaTime)
        {
            //This clean out any old data.
            SpriteAnimationData animData = new SpriteAnimationData();
            m_spriteInfo.m_animationData = default(SpriteAnimationData);

            if(m_spriteAnimation != null)
            {
                animData = m_spriteAnimation.UpdateCurrentAnimation(deltaTime);
            }
            else
            {
                animData.m_currentFrame = Frame.Zero;

                if (m_spriteInfo.m_texture2D != null)
                {
                    animData.m_frameSize.X = m_spriteInfo.m_texture2D.Bounds.Width;
                    animData.m_frameSize.Y = m_spriteInfo.m_texture2D.Bounds.Height;
                }
                else
                {
                    animData.m_frameSize.X = 0;
                    animData.m_frameSize.Y = 0;
                }
            }

            m_spriteInfo.m_animationData = animData;    

            if(SendInfo != null) SendInfo(m_spriteInfo);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public AnimationDirection Direction { get { return m_spriteAnimation.Direction; } set { m_spriteAnimation.Direction = value; } }
        public float DesiredAnimationRate { get { return m_spriteAnimation.DesiredAnimationRate; } set { m_spriteAnimation.DesiredAnimationRate = value; } }
        public bool Paused { get { return m_spriteAnimation.Paused; } set { m_spriteAnimation.Paused = value; } }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void Dispose()
        {
            m_bIsDisposed = true;
        }

        public bool Active { get; set; }
    }
}
