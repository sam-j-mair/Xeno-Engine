using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace XenoEngine.Systems
{
    interface ISprite
    {
        Texture2D Graphic { get; set; }
        Vector2 Position { get; set; }
        Color Colour { get; set; }
    }
}


