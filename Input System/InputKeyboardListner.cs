using System;
using XenoEngine.Systems;
using System.Collections;


namespace XenoEngine.Systems
{
    [Serializable]
    public class InputKeyboardListner : InputListner
    {
        KeyboardState m_oldKeyboardState;
        const int m_knBufferSize = 20;
        static string[] saEventBuffer = new string[m_knBufferSize];

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Initlize listener. This needs to be called before anything else can be done.
        /// </summary>
        //-----------------------------------------------------------------------------------
        public override void Initialise()
        {
            Type = ControllerType.Keyboard;
            m_oldKeyboardState = Keyboard.GetState((PlayerIndex)ActionMap.PlayerIndex);
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// get the input buffer for the keyboard.
        /// </summary>
        /// <param name="eControllerIndex">the controller index used for the keyboard.</param>
        /// <returns></returns>
        //-----------------------------------------------------------------------------------
        public Keys[] GetRawInputBuffer(PlayerIndex eControllerIndex)
        {
            KeyboardState state = Keyboard.GetState(eControllerIndex);

            return state.GetPressedKeys();
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// update the keyboard listner and store events for processing in the next frame.
        /// </summary>
        /// <param name="gameTime"></param>
        //-----------------------------------------------------------------------------------
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState((PlayerIndex)ActionMap.PlayerIndex);
            int nBufferIndex = 0;

            foreach (ActionBinding<Keys> binding in ActionMap.Bindings)
            {
                if (keyState.IsKeyDown(binding.Key))
                {
                    if(!m_oldKeyboardState.IsKeyDown(binding.Key) || binding.IsPolling)
                    {
                        if(binding.ButtonState == ButtonState.Pressed)
                        {
                            if (nBufferIndex < m_knBufferSize)
                            {
                                //This is probably better to be bits but we will see what happens.
                                saEventBuffer[nBufferIndex] = binding.Event;
                                nBufferIndex++;
                            }
                        }
                        //FireEvent(binding.Event);
                    }
                }
                else if(m_oldKeyboardState.IsKeyDown(binding.Key))
                {
                    if (!keyState.IsKeyDown(binding.Key))
                    {
                        if (binding.ButtonState == ButtonState.Released)
                        {
                            if(nBufferIndex < m_knBufferSize)
                            {
                                saEventBuffer[nBufferIndex] = binding.Event;
                                nBufferIndex++;
                            }
                        }
                        //Key Released; Fire Released Event;
                    }
                }
            }

            if(nBufferIndex > 0)
            {
                foreach (string szEvent in saEventBuffer)
                {
                    if(!string.IsNullOrEmpty(szEvent))
                        FireEvent(szEvent);
                }
            }

            Array.Clear(saEventBuffer, 0, m_knBufferSize);
            // update the keyboard state for the next frame.
            m_oldKeyboardState = keyState;
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public override IEnumerable GetRawInputBuffer()
        {
            return Keyboard.GetState().GetPressedKeys();
        }

    }
}
