using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace XenoEngine.Systems.Sprite_Systems
{
    [Serializable]
    public struct SpriteAnimationData
    {
        public Frame m_frameSize;
        public Frame m_currentFrame;
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public static bool operator !=(SpriteAnimationData lhs, SpriteAnimationData rhs)
        {
            return (lhs.m_currentFrame.X != rhs.m_currentFrame.X || lhs.m_currentFrame.Y != rhs.m_currentFrame.Y) || 
                (lhs.m_frameSize.X != rhs.m_frameSize.X || lhs.m_frameSize.Y != rhs.m_frameSize.Y);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public static bool operator ==(SpriteAnimationData lhs, SpriteAnimationData rhs)
        {
            return (lhs.m_currentFrame.X == rhs.m_currentFrame.X && lhs.m_currentFrame.Y == rhs.m_currentFrame.Y) &&
                (lhs.m_frameSize.X == rhs.m_frameSize.X && lhs.m_frameSize.Y == rhs.m_frameSize.Y);
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }

    [Serializable]
    public struct Frame
    {
        Point m_value;

        public Frame(int nX, int nY)
        {
            m_value.X = nX;
            m_value.Y = nY;
        }

        public static Frame Zero { get { return new Frame(0, 0); } }
        public int X { get { return m_value.X; } set { m_value.X = value; } }
        public int Y { get { return m_value.Y; } set { m_value.Y = value; } }
    }
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------
    [Serializable]
    public class AnimationDescription
    {
        public string   m_szName;    //This is the name the animation is stored under.
        public Frame    m_frameSize;
        public Frame    m_sheetSize; //This point represent the start and end frame of the animation.
        public bool     m_bLooping;

        public AnimationDescription(string szName, int nXFrameSize, int nYFrameSize,  int nXAnimationStart, int nYAnimationStart, bool bLooping)
        {
            m_szName = szName;
            m_frameSize = new Frame(nXFrameSize, nYFrameSize);
            m_sheetSize = new Frame(nXAnimationStart, nYAnimationStart);
            m_bLooping = bLooping;
        }
    }
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------
    public enum AnimationDirection
    {
        Forward,
        Backwards
    }

    [Serializable]
    public class SpriteAnimation : IDisposable
    {
        Frame m_currentFrame;
        AnimationDescription m_currentAnimation;
        Dictionary<string, AnimationDescription> m_animations;
        double m_fTimeElapsedScinceLastFrame;
        float m_fDesiredElapsedTime;

        public SpriteAnimation(List<AnimationDescription> animations)
        {
            m_currentFrame = Frame.Zero;
            m_currentAnimation = null;
            m_animations = new Dictionary<string, AnimationDescription>(20);
            IsDisposed = false;
            Direction = AnimationDirection.Forward;
            DesiredAnimationRate = 10.0f;

            Debug.Assert(animations.Count < 20, "There is more than 20 animations");

            foreach (AnimationDescription desc in animations)
            {
                m_animations.Add(desc.m_szName, desc);
            }
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void PlayAnimation(string szAnimationName)
        {
            AnimationDescription desc;
            Debug.Assert(m_animations.TryGetValue(szAnimationName, out desc), "Animation " + szAnimationName + " doesn't exist!!!");

            m_currentAnimation = desc;
            m_fTimeElapsedScinceLastFrame = 0.0f;
            Paused = false;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void SetFrame(Frame frame)
        {
            Debug.Assert(frame.X <= m_currentAnimation.m_sheetSize.X && frame.Y <= m_currentAnimation.m_sheetSize.Y,
                "This frame is larger than the current sheet.");
            m_currentFrame = frame;
        }
        //-------------------------------------------------------------------------------
        //This is allow us to select the animation sequence without playing it.
        //-------------------------------------------------------------------------------
        public void SetSequence(string szSequence)
        {
            AnimationDescription desc;
            Debug.Assert(m_animations.TryGetValue(szSequence, out desc), "Animation " + szSequence + " doesn't exist!!!");

            m_currentAnimation = desc;
            Paused = true;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public SpriteAnimationData UpdateCurrentAnimation(DeltaTime deltaTime)
        {
            SpriteAnimationData data = new SpriteAnimationData();
            
            if(m_currentAnimation != null && !Paused)
            {
                m_fTimeElapsedScinceLastFrame += deltaTime.ElapsedGameTime.TotalMilliseconds;

                if (m_fTimeElapsedScinceLastFrame >= m_fDesiredElapsedTime)
                {
                    data.m_currentFrame.X = Direction == AnimationDirection.Forward ? m_currentFrame.X++ : m_currentFrame.X--;
                    m_fTimeElapsedScinceLastFrame = 0;
                }
                else
                {
                    data.m_currentFrame.X = m_currentFrame.X;
                }

                data.m_currentFrame.Y = m_currentFrame.Y;
                data.m_frameSize = m_currentAnimation.m_frameSize;

                if ((m_currentFrame.X * m_currentAnimation.m_frameSize.X) >= m_currentAnimation.m_sheetSize.X)
                {
                    if (m_currentAnimation.m_bLooping)
                        m_currentFrame = Frame.Zero;
                    else
                        m_currentAnimation = null;
                }
            }
            //We need to do this if it is paused with an animation.
            else if (m_currentAnimation != null && Paused)
            {
                data.m_currentFrame = m_currentFrame;
                data.m_frameSize = m_currentAnimation.m_frameSize;
            }

            return data;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public bool IsDisposed { get; private set; }
        public bool Paused { get; set; }
        public AnimationDirection Direction { get; set; }
        public float DesiredAnimationRate { get { return ((m_fDesiredElapsedTime/1000) * 1); } set { m_fDesiredElapsedTime = ((1 / value) * 1000); } }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void Dispose()
        {
            m_animations.Clear();
            IsDisposed = true;
        }
    }
}
