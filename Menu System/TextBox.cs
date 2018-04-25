using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XenoEngine.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XenoEngine.Systems.MenuSystem;
using XenoEngine.GeneralSystems;
using XenoEngine.Systems.Sprite_Systems;

namespace XenoEngine.Systems.MenuSystem
{
    public class TextBox : GUIObject
    {
        private const int       CHAR_SPACE = 12;
        private const int       MAX_CHAR = 20;

        private Sprite          m_backGroun,
                                m_cursor;

        private TextSprite      m_textSprite;
        private bool            m_bAcceptingText;
        private TimeSpan        m_InputDelayTimer, 
                                m_accumulator;

        private StringBuilder   m_szBuilder;

        public Action<string>   TextEntered;

        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public TextBox(Vector2 v2Position,
            Color eColor,
            string szAssetNameBackground = null,
            string szAssetNameCursor = null,
            string szFontName = null)
            : base(null)
        {
            m_backGround = new Sprite(
                EngineServices.GetSystem<IGameSystems>().SpriteSystem,
                szAssetNameBackground != null ? szAssetNameBackground : "DefaultTextBox",
                new Vector3(v2Position, 0),
                eColor,
                false);

            m_backGround.ScaleFactor = new Vector2(1, 0.3f);

            m_textSprite = new TextSprite(
                EngineServices.GetSystem<IGameSystems>().FontSystem,
                szFontName != null ? szFontName : "DefaultFont",
                null,
                "",
                new Vector3(v2Position.X, v2Position.Y, 0.2f),
                Color.White,
                false);

            m_cursor = new Sprite(
                EngineServices.GetSystem<IGameSystems>().SpriteSystem,
                szAssetNameCursor != null ? szAssetNameCursor : "DefaultCursor",
                new Vector3(v2Position.X, v2Position.Y, 0.1f),
                Color.Red,
                false);

            OnClick += OnClickEvent;


            CalculateBoundingRectangle(m_backGround);

            //debug code.
            m_bAcceptingText = true;
            m_InputDelayTimer = new TimeSpan(0, 0, 0, 0, 100);
            m_accumulator = TimeSpan.Zero;
            m_szBuilder = new StringBuilder();
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void SetActive(bool bActive)
        {
            base.SetActive(bActive);

            if (Active)
            {
                m_backGround.Activate();
                m_cursor.Activate();
                m_textSprite.Activate();
            }
            else
            {
                m_bAcceptingText = false;
                m_backGround.Deactivate();
                m_cursor.Deactivate();
                m_textSprite.Deactivate();
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        private void OnClickEvent(GUIObject textBox)
        {
            if(Active)
                m_bAcceptingText = true;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void Update(DeltaTime deltaTime)
        {
            m_accumulator += deltaTime.ElapsedGameTime;
            if (m_bAcceptingText && (m_accumulator > m_InputDelayTimer))
            {
                Input input = EngineServices.GetSystem<IGameSystems>().InputSystem;
                //here we need to direct read the key inputs.
                InputKeyboardListner listner = (InputKeyboardListner)input.GetInputListner(ControllerType.Keyboard);

                m_accumulator = TimeSpan.Zero;

                foreach (Keys key in listner.GetRawInputBuffer())
                {
                    switch (key)
                    {
                        case Keys.Back:
                            {
                                if (m_textSprite.TextString.Length != 0)
                                {
                                    string szNewString = m_textSprite.TextString.Substring(0, m_textSprite.TextString.Length - 1);
                                    m_textSprite.TextString = szNewString;
                                }
                            }
                            break;

                        case Keys.Enter:
                            {
                                if (TextEntered != null)
                                {
                                    TextEntered(m_textSprite.TextString);
                                }
                            }
                            break;

                        default:
                            {
                                m_szBuilder.Append(key.ToString());
                            }
                            break;
                    }
                }

                if (m_szBuilder.Length > 0)
                {
                    m_textSprite.TextString += m_szBuilder.ToString();
                    m_szBuilder.Clear();
                }

                UpdateCursor();
            }

            base.Update(deltaTime);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        private void UpdateCursor()
        {
            m_cursor.Colour = m_cursor.Colour == Color.Red ? Color.Transparent : Color.Red;
            m_cursor.Position = new Vector2(m_textSprite.Position.X + (m_textSprite.TextString.Count() * CHAR_SPACE) - 20, m_textSprite.Position.Y);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {

            }
            base.Dispose(bDisposing);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {

                m_backGround.Position += value;
                m_cursor.Position += value;
                m_textSprite.Position += value;

                base.Position = value;
            }
        }
    }
}
