using Microsoft.Xna.Framework.Graphics;

namespace XenoEngine.Systems.Sprite_Systems
{
    public interface IRenderLayer<TInfoType>
    {
        void RegisterSprite(IInfoBase<TInfoType> iten);
        void DeregisterSprite(IInfoBase<TInfoType> item);
        Effect Effect { get; }
        EffectSettings EffectSettings { get; set; }
    }
}
