using System;

namespace XenoEngine.Systems.Sprite_Systems
{
    public interface IInfoBase<TInfoType>
    {
        void InfoRequested(DeltaTime deltaTime);
        event Action<TInfoType> SendInfo;
        bool Active { get; set; }
    }
}
