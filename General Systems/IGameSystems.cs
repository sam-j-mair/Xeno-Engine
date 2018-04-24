using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.Network;
using XenoEngine.Systems;
using XenoEngine.Systems.Sprite_Systems;
using XenoEngine.Renderer;
using XenoEngine.Systems;

namespace XenoEngine.GeneralSystems
{
    public interface IGameSystems : ISystems
    {
        RenderEngine RenderEngine { get; }
        GameServiceContainer Services { get; }
        GameComponentCollection Components { get; }
        GraphicsDevice GraphicsDevice { get; }
        ContentManager Content { get; }
        IEntityController EntityController { get; }
        AssetLoader AssetLoader { get; }
        NetworkEngine Network { get; }
        IRenderLayer<SpriteInfo> SpriteSystem { get; }
        FontLayer FontSystem { get; }
        Input InputSystem { get; }
        ICamera Camera { get; set; }
        LightRig LightRig { get; }
        Space PhysicsSpace { get; }
        string WorkingDirectory { get; }
    }
}
