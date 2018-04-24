using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace XenoEngine.Systems
{
    [Serializable]
    public class MouseObserver
    {
        public MouseObserver()
        {

        }

        public void Update(int nMouseX, int nMouseY)
        {
            MouseX = nMouseX;
            MouseY = nMouseY;
        }

        //Some special magic to avoid references in the event handler
        [OnSerializing]
        protected void OnSerializing(StreamingContext context)
        {
            InputMouseListner.MouseObserverUpdate -= Update;
        }

        [OnSerialized]
        protected void OnSerialized(StreamingContext context)
        {
            InputMouseListner.MouseObserverUpdate += Update;
        }

        //This happens on the remote machine ..or after a restore.
        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context)
        {
            InputMouseListner.MouseObserverUpdate += Update;
        }

        public int MouseX { get; internal set; }
        public int MouseY { get; internal set; }
        public Vector2 Position { get { return new Vector2(MouseX, MouseY); } }
    }

    public enum MouseButton
    {
        LeftButton,
        MiddleButton,
        RightButton,
        XButton1,
        XButton2
    }


    //this remaps ButtonState to a new enum
    public enum MouseButtonStates
    {
        Pressed = ButtonState.Pressed,
        Released = ButtonState.Released
    }

    public class InputMouseListner : InputListner
    {
        public delegate void UpdateMouse(int nMouseX, int nMouseY);
        public static event UpdateMouse MouseObserverUpdate;
        private Vector2 m_v2OldValue;
        private MouseState m_oldMouseState;

        public override void Initialise()
        {
            Type = ControllerType.Mouse;
            base.Initialise();
        }

        public MouseObserver CreateMouseObserver()
        {
            MouseState mouseState = Mouse.GetState();
            MouseObserver mouseObserver = new MouseObserver();
            mouseObserver.MouseX = mouseState.X;
            mouseObserver.MouseY = mouseState.Y;

            MouseObserverUpdate += mouseObserver.Update;

            return mouseObserver;
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            if (MouseObserverUpdate != null)
            {
                if(m_v2OldValue.X != mouseState.X || m_v2OldValue.Y != mouseState.Y)
                {
                    MouseObserverUpdate(mouseState.X, mouseState.Y);
                    m_v2OldValue.X = mouseState.X;
                    m_v2OldValue.Y = mouseState.Y;
                }
            }

            foreach (ActionBinding<MouseButton> binding in ActionMap.Bindings)
            {
                if (mouseState.LeftButton == ButtonState.Pressed &&
                    (m_oldMouseState.LeftButton != ButtonState.Pressed || binding.IsPolling))
                {
                    if (binding.Key == MouseButton.LeftButton && binding.ButtonState == ButtonState.Pressed)
                    {
                        FireEvent(binding.Event);
                    }
                }
                else if(mouseState.LeftButton == ButtonState.Released && m_oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    if(binding.Key == MouseButton.LeftButton && binding.ButtonState == ButtonState.Released)
                    {
                        FireEvent(binding.Event);
                    }
                }

                if (mouseState.MiddleButton == ButtonState.Pressed &&
                    (m_oldMouseState.MiddleButton != ButtonState.Pressed || binding.IsPolling))
                {
                    if(binding.Key == MouseButton.MiddleButton)
                    {
                        FireEvent(binding.Event);
                    }
                }

                if(mouseState.RightButton == ButtonState.Pressed &&
                    (m_oldMouseState.RightButton != ButtonState.Pressed || binding.IsPolling))
                {
                    if (binding.Key == MouseButton.RightButton)
                    {
                        FireEvent(binding.Event);
                    }
                }
            }

            m_oldMouseState = mouseState;
        }
    }
}
