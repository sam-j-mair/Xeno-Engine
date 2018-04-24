using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.GeneralSystems;

namespace XenoEngine.Serialization.Proxies
{
    [Serializable]
    public abstract class SerializableAssetProxie<T, TRestoreData>
    {
        protected TRestoreData m_restoreData;
        
        [NonSerialized] protected T m_WrappedType;

        protected void OnSerializing(StreamingContext context) { throw new NotImplementedException("You have to implement this in your subclass use the new keyword to avoid conflicts"); }
        protected void OnDeserialized(StreamingContext context) { throw new NotImplementedException("You have to implement this in your subclass use the new keyword to avoid conflicts"); }

        public virtual T WrappedType { get { return m_WrappedType; } set { m_WrappedType = value; } }
    }

    [Serializable]
    public class SerializableTexture : SerializableAssetProxie<Texture2D, string>
    {
        [OnSerializing]
        protected new void OnSerializing(StreamingContext context)
        {
            if(m_WrappedType != null)
                m_restoreData = m_WrappedType.Name;
        }

        [OnDeserialized]
        protected new void OnDeserialized(StreamingContext context)
        {
            m_WrappedType = EngineServices.GetSystem<IGameSystems>().Content.Load<Texture2D>(m_restoreData);
        }
    }

    [Serializable]
    public class SerializableFont : SerializableAssetProxie<SpriteFont, string>
    {
        [OnSerializing]
        protected new void OnSerializing(StreamingContext context)
        {
            if(m_WrappedType != null)
            {
                Type type = m_WrappedType.GetType();
                FieldInfo field = type.GetField("textureValue", BindingFlags.Instance | BindingFlags.NonPublic);

                Texture2D texture = field.GetValue(m_WrappedType) as Texture2D;

                if (texture != null)
                    m_restoreData = texture.Name;
            }
        }

        [OnDeserialized]
        protected new void OnDeserialized(StreamingContext context)
        {
            if(!string.IsNullOrEmpty(m_restoreData))
                m_WrappedType = EngineServices.GetSystem<IGameSystems>().Content.Load<SpriteFont>(m_restoreData);
        }
    }

    [Serializable]
    public class SerialiableBlendState : SerializableAssetProxie<BlendState, string>
    {
        [OnSerializing]
        protected new void OnSerializing(StreamingContext context)
        {
            m_restoreData = m_WrappedType.Name;
        }

        protected new void OnDeserialized(StreamingContext context)
        {
            Type type = typeof(BlendState);
            MethodInfo method = type.GetMethod(m_restoreData, BindingFlags.Static | BindingFlags.Public);

            m_WrappedType = method.Invoke(null, null) as BlendState;
        }
    }

    [Serializable]
    public class SerializableDepthStencil : SerializableAssetProxie<DepthStencilState, string>
    {
        [OnSerializing]
        protected new void OnSerializing(StreamingContext context)
        {
            m_restoreData = m_WrappedType.Name;
        }

        [OnDeserialized]
        protected new void OnDeserialized(StreamingContext context)
        {
            m_WrappedType = EngineServices.GetSystem<IGameSystems>().Content.Load<DepthStencilState>(m_restoreData);
        }
    }

    [Serializable]
    public class SerializableEffect : SerializableAssetProxie<Effect, byte[]>
    {
        [OnSerializing]
        protected new void OnSerializing(StreamingContext context)
        {
            if(m_WrappedType != null)
            {
                Type type = typeof(Effect);
                FieldInfo dataField = type.GetField("pCachedEffectData", BindingFlags.Instance | BindingFlags.NonPublic);

                m_restoreData = dataField.GetValue(m_WrappedType) as byte[];
                Debug.Assert(m_restoreData != null);
            }
        }

        [OnDeserialized]
        protected new void OnDeserialized(StreamingContext context)
        {
#warning I need to switch this to use the AssetLoader instead of the contentManager
            GraphicsDevice gdevice = EngineServices.GetSystem<IGameSystems>().GraphicsDevice;
            m_WrappedType = new Effect(gdevice, m_restoreData);
            m_restoreData = null;
        }

    }
}
