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

        //Leaving this one for legacy reasons
        public InputListner(){ }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Initialise() { }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public ActionMap ActionMap { get; protected set; }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Update(GameTime gameTime) { }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected void FireEvent(String szEventName)
        {
            Type type = ActionMap.GetType();
            Object[] aParams = new Object[1];
            ActionMap.LastAction = szEventName;
            aParams[0] = ActionMap;
 
            ActionTriggered action = ActionMap.GetAction(szEventName);

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
