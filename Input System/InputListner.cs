using System;
using Microsoft.Xna.Framework;
using XenoEngine.Systems;
using System.Linq;
using System.Collections;
using System.Collections.Generic;




namespace XenoEngine.Systems
{
    [Serializable]
    public class InputListner
    {
        protected ActionMap m_actionMap;

        //Leaving this one for legacy reasons
        public InputListner(){ }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Initialise() { }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public ActionMap ActionMap { get { return m_actionMap; } set { m_actionMap = value; } }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Update(GameTime gameTime) { }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected void FireEvent(String szEventName)
        {
            Type type = m_actionMap.GetType();
            Object[] aParams = new Object[1];
            m_actionMap.LastAction = szEventName;
            aParams[0] = m_actionMap;
 
            ActionTriggered action = m_actionMap.GetAction(szEventName);

            if(action != null)
            {
                action(this);
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public ControllerType Type { get; protected set; }

        public virtual IEnumerable GetRawInputBuffer() { return null; }
    }
}
