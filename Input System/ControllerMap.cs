using System;

namespace XenoEngine.Systems
{
    //-------------------------------------------------------------------------------
    /// <summary>
    /// A controller Enum for defining input type.
    /// </summary>
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
    /// <summary>
    /// A mapping class that maps an input listener with a Controller index.
    /// </summary>
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
