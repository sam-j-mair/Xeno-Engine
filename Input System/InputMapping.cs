using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace XenoEngine.Systems
{

    
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ActionMapBuilder
    {
        private int m_nControllerIndex;

        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public ActionMapBuilder(int nPlayerIndex)
        {
            //TEST
            //ActionMap map = new ActionMap();
            //Type mapType = map.GetType();
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "ActionMaps";

            // Create a new assembly with one module
            AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModule = newAssembly.DefineDynamicModule("Actions");

            //  Define a public class named "BruteForceSums " 
            //  in the assembly.
            TypeBuilder myType = newModule.DefineType("ActionMap", TypeAttributes.Public);

            // Define a method on the type to call. Pass an
            // array that defines the types of the parameters,
            // the type of the return type, the name of the 
            // method, and the method attributes.
            
          //  Type[] aParams = { typeof(object), typeof(EventArgs) };
          //  Type returnType = typeof(int);
            
//             MethodBuilder method =
//                myType.DefineMethod(
//                "TestEvent",
//                MethodAttributes.Public |
//                MethodAttributes.Virtual,
//                returnType,
//                aParams);

            //This would be a foreach loop doing through the xml data

            //for (int nCounter = 0; nCounter < 20; ++nCounter)
            EventBuilder field = myType.DefineEvent("testEvent", EventAttributes.None, typeof(ActionTriggered));

            

            myType.CreateType();



//             // Get an ILGenerator. This is used
//             // to emit the IL that you want.
//             ILGenerator generator = method.GetILGenerator();
// 
//             generator.BeginScope();
//             //generator.Emit(OpCodes.)
//             generator.EndScope();


            //TESTING
            object actionMap = newAssembly.CreateInstance("ActionMap");

            Type testType = actionMap.GetType();

            //EventInfo eventInfo = testType.GetEvent("testEvent");
            EventInfo eventInfo = myType.GetEvent("testEvent");
            //MethodInfo methodInfo = testType.GetMethod("OnTestEvent");

            Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, typeof(ActionMapBuilder), "OnTestEvent");

            eventInfo.AddEventHandler(actionMap, handler);
           

            m_nControllerIndex = nPlayerIndex;
            // TODO: Construct any child components here

            
        }
        //-------------------------------------------------------
        //-------------------------------------------------------
        public void OnTestEvent(object sender, EventArgs eventArgs)
        {

        }
        //-------------------------------------------------------
        //-------------------------------------------------------
        public virtual void Update(GameTime gameTime)
        {
            
            KeyboardState state = Keyboard.GetState((PlayerIndex)m_nControllerIndex);
            Keys[] keys = null;

            keys = state.GetPressedKeys();

            if (keys != null)
            {
            }




            // TODO: Add your update code here
           
        }
    }
}