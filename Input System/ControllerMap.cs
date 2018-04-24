using System;

namespace XenoEngine.Systems
{
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------
    public enum Controller
    {
        Keyboard,
        Player_1_Mouse,
        Controller_1,
        Controller_2,
        Controller_3,
        Controller_4,
        Controller_Count = 5
    }
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------
    [Serializable]
    class ControllerMap
    {
        public InputListner m_inputListner;
        public int m_nControllerIndex;

        public ControllerMap(InputListner listner, int nIndex)
        {
            m_inputListner = listner;
            m_nControllerIndex = nIndex;
        }
    }
}
