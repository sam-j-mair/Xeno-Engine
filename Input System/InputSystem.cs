using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XenoEngine.Utilities;
using XenoEngine.Systems;

namespace XenoEngine.Systems
{
    [Serializable]public delegate void ActionTriggered(object sender);

    public enum ControllerType
    {
        Mouse,
        Keyboard,
        GamePad
    }
    
    [Serializable]
    public class Input
    {
        private List<ControllerMap> m_inputListners = new List<ControllerMap>(4);
        private Dictionary<ControllerType, Type> m_dynamicStore; 

        public Input()
        {

        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void CreateController<InputListnerType>(int nControllerIndex, ActionMap actionMap) where InputListnerType : InputListner, new()
        {
            InputListnerType inputController = new InputListnerType();
            inputController.ActionMap = actionMap;
            inputController.Initialise();
            m_inputListners.Add(new ControllerMap(inputController, nControllerIndex));
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public ActionMap RegisterActionMap(int nPlayerIndex, ActionMap map)
        {
            foreach (ControllerMap controllerMap in m_inputListners)
            {
                if (controllerMap.m_nControllerIndex == nPlayerIndex)
                {
                    ActionMap oldMap = controllerMap.m_inputListner.ActionMap;
                    controllerMap.m_inputListner.ActionMap = map;
                    return oldMap;
                }
            }

            return null;
        }
        //----------------------------------------------------------------------------
        // This is a mouse specific function...i dont like it but i need to expose a special 
        // NOTE: i might be able to merge getcontroller and get mouse observer into one method???
        //----------------------------------------------------------------------------
        public MouseObserver GetMouseObserver()
        {
            //This is ugly..but i need specific behaviour for a mouse controller.
            foreach (ControllerMap controllerMap in m_inputListners)
            {
                if(controllerMap.m_inputListner.Type == ControllerType.Mouse)
                {
                    return ((InputMouseListner)controllerMap.m_inputListner).CreateMouseObserver();
                }
            }

            Debug.WriteLine("There is no controller set up for the mouse");

            return null;
        }

        public InputListner GetInputListner(ControllerType eControllerType, PlayerIndex ePlayerIndex = PlayerIndex.One)
        {
            foreach (ControllerMap controllerMap in m_inputListners)
            {
                if ((eControllerType == ControllerType.Mouse && controllerMap.m_inputListner.Type == eControllerType) ||
                    (eControllerType == ControllerType.Keyboard && controllerMap.m_inputListner.Type == eControllerType) ||
                    (controllerMap.m_inputListner.Type == eControllerType && controllerMap.m_inputListner.ActionMap.PlayerIndex == (int)ePlayerIndex))
                {
                    return controllerMap.m_inputListner;
                }
            }
            return null;
        }

        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void Update(GameTime gameTime)
        {
            foreach (ControllerMap controller in m_inputListners)
            {
                controller.m_inputListner.Update(gameTime);
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public ActionMap GetController(int nControllerIndex)
        {
            //this is a bit slow but it will work for now.
            foreach (ControllerMap controllerMap in m_inputListners)
            {
                if (controllerMap.m_nControllerIndex == nControllerIndex)
                {
                    return controllerMap.m_inputListner.ActionMap;
                }
            }

            return null;
        }
    }
}
