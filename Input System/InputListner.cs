using System;
using Microsoft.Xna.Framework;
using XenoEngine.Systems;
using System.Linq;
using System.Collections;
using System.Collections.Generic;




namespace XenoEngine.Systems
{
//     public class InputButtonsCollection : IEnumerable
//     {
//         int nCurrentIndex = 0;
//         RawInputBuffer m_buffer;
// 
//         public InputButtonsCollection(RawInputBuffer buffer)
//         {
//             m_buffer = buffer;
//         }
//         
//         public IEnumerable GetEnumerator()
//         {
// 
//             yield return 
//         }
//         /// <summary>
//         /// This is actually an OfType call.
//         /// </summary>
//         /// <typeparam name="TCastType"></typeparam>
//         public IEnumerable Cast<TCastType>()
//         {
//             
//         }
//     }
// 
//     public class RawInputBuffer
//     {
//         Type m_castType;
//         internal RawInputBuffer(Type type, dynamic inputBuffer)
//         {
//             m_InternalInputBuffer = inputBuffer;
//             m_castType = type;
//         }
//         internal object[] m_InternalInputBuffer;
// 
//         //public IEnumerable InternalInputBuffer { get { return //m_InternalInputBuffer(); } protected set { m_InternalInputBuffer = value; } }
//     }

//     public class RawInputBuffer<T> : RawInputBuffer where T : class
//     {
//         public RawInputBuffer(T[] inputBuffer) : base(inputBuffer)
//         {
//             m_InternalInputBuffer = (object[])inputBuffer;
//         }
// 
//         //public T[] InputBuffer { get { return (T[])InternalInputBuffer; } }
//     }

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

        public ControllerType Type { get; protected set; }

        public virtual IEnumerable GetRawInputBuffer() { return null; }
    }
}
