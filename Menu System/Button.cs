using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XenoEngine.GeneralSystems;
using XenoEngine.Serialization;
using XenoEngine.Systems.Sprite_Systems;


namespace XenoEngine.Systems.MenuSystem
{
    public class Button : GUIObject
    {
        
        private Sprite                              m_sprite;
        private TextSprite                          m_textSprite;
        private StreamChunk                         m_streamChunk;

        //Events
        public event Action<Button>                 EventComplete;
        private string                              m_szFontName;

        

        // a readonly proxy object will be here that will allow detection of
        // mouse position and clicks etc.

        public Button(  string szAssetName,
                        string szFontName,
                        Vector3 v3Position, 
                        Color colour,
                        List<IScriptUpdateable<Button>> buttonScripts) : base(buttonScripts)
        {
            Input inputSytem = EngineServices.GetSystem<IGameSystems>().InputSystem;
            IRenderLayer<SpriteInfo> spriteSystem = EngineServices.GetSystem<IGameSystems>().SpriteSystem;


            EngineServices.GetSystem<IGameSystems>().Components.Add(this);
            m_sprite = new Sprite(spriteSystem, szAssetName, v3Position, colour, false);


            CalculateBoundingRectangle(m_sprite);
        }

        public Button(  string szAssetName,
                        string szFontName,
                        Vector3 v3Position,
                        Color colour,
                        StreamChunk streamChunk,
                        List<IScriptUpdateable<Button>> buttonScripts)
            : base(buttonScripts)
        {
            
            IRenderLayer<SpriteInfo> spriteSystem = EngineServices.GetSystem<IGameSystems>().SpriteSystem;
            
            m_sprite = new Sprite(spriteSystem, szAssetName, streamChunk, v3Position, colour, false);
            m_szFontName = szFontName;
            m_bSelected = false;
            m_streamChunk = streamChunk;

            CalculateBoundingRectangle(m_sprite);
        }

        ~Button()
        {
            Dispose(false);
        }



//         internal void Right(object sender)
//         {
//             if (Active)
//             {
//                 OnRight(this);
//             }
//         }

//         public void Left(object sender)
//         {
//             if (Active)
//             {
//                 OnLeft(this);
//             }
//         }

//         public void Up(object sender)
//         {
//             if (Active)
//             {
//                 OnUp(this);
//             }
//         }

//         public void Down(object sender)
//         {
//             if (Active)
//             {
//                 OnDown(this);
//             }
//         }



        public override void Dispose(bool bDisposing)
        {
            if(bDisposing)
            {
                m_sprite = null;
                m_szFontName = "";

                if (m_textSprite != null)
                {
                    m_textSprite.Dispose();
                    m_textSprite = null;
                }
            }

            base.Dispose(bDisposing);
        }

        public Color Colour { get { return m_sprite.Colour; } set { m_sprite.Colour = value; } }
        public Texture2D Graphic { get { return m_sprite.Graphic; }
            set 
            { 
                m_sprite.Graphic = value;
                CalculateBoundingRectangle(m_sprite);
            } 
        }

        //This is using lazy initialization ...so that it is only used when needed.
        public Color TextColoir { get { return m_textSprite.Colour; } set { m_textSprite.Colour = value; } }
        public Vector2 TextPosition { get { return m_textSprite.Position; } set { m_textSprite.Position = value; } }

        public String Text 
        {
            get 
            {
                if(m_textSprite == null)
                {
                    IRenderLayer<SpriteInfo> spriteSystem = EngineServices.GetSystem<IGameSystems>().SpriteSystem;
                    m_textSprite = new TextSprite(  spriteSystem,
                                                    m_szFontName,
                                                    m_streamChunk,
                                                    "",
                                                    new Vector3(m_sprite.Position, 0.0f),
                                                    m_sprite.Colour,
                                                    Active ? true : false);
                }

                return m_textSprite.TextString;
            }
            set
            {
                if (m_textSprite != null)
                {
                    m_textSprite.TextString = value;
                }
                else
                {
                    IRenderLayer<SpriteInfo> spriteSystem = EngineServices.GetSystem<IGameSystems>().SpriteSystem;
                    m_textSprite = new TextSprite(  spriteSystem,
                                                    m_szFontName,
                                                    m_streamChunk,
                                                    value,
                                                    new Vector3(m_sprite.Position, 0.0f),
                                                    m_sprite.Colour,
                                                    Active ? true : false);
                }
            }
        }

        

        
    }
}
