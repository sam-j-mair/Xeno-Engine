using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using XenoEngine.GeneralSystems;
using XenoEngine.Serialization;

namespace XenoEngine.Systems.MenuSystem
{
    public enum MenuActions
    {
        ButtonPressed,
        ButtonReleased,
    }

    //public delegate void MenuAction(Button button, MenuAction eAction);

    struct ButtonData
    {
        public Button m_button;
        public object m_userData;

        public ButtonData(Button button, object userData)
        {
            m_button = button;
            m_userData = userData;
        }
    }
    //----------------------------------------------------------------------------
    /// <summary>
    /// Container for guiobjects, base class for all menus .
    /// </summary>
    //----------------------------------------------------------------------------
    abstract class Menu : SerializableComponent, IGameComponent, IUpdateable, IDisposable
    {
        #region PRIVATE
        private TreeNode<Menu>                      m_node;
        private StateMachine<Menu>                  m_stateMachine;
        private Dictionary<String, State<Menu>>     m_states;
        private Dictionary<String, Button>          m_buttonDictionary;
        protected MouseObserver                     m_mouseObserver;
        private bool                                m_bIsActive, m_bEnabled;
        private int                                 m_nUpdateOrder;
        #endregion

        #region PUBLIC EVENTS
        public event Action                         Idle, Activating, Active, Deactivating;
        public event EventHandler<EventArgs>        EnabledChanged, UpdateOrderChanged, Disposed;
        #endregion

        public Menu()
        {
            Input inputSystem = EngineServices.GetSystem<IGameSystems>().InputSystem;
            m_mouseObserver = inputSystem.GetMouseObserver();
            m_states = new Dictionary<String, State<Menu>>(5);
            m_buttonDictionary = new Dictionary<String, Button>(50);

            m_states.Add("Idle", new StateIdle());
            m_states.Add("Activating", new StateActivating());
            m_states.Add("Active", new StateActive());
            m_states.Add("Deactivating", new StateDeactivating());

            m_stateMachine = new StateMachine<Menu>(m_states["Idle"], this);
            m_node = new TreeNode<Menu>();
            m_node.UserData = this;
            m_bIsActive = false;

            EngineServices.GetSystem<IGameSystems>().Components.Add(this);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        ~Menu()
        {
            Dispose(false);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Initialize() { }
        //----------------------------------------------------------------------------
        /// <summary>
        /// Add button to this menu.
        /// </summary>
        /// <param name="szName">name given to the button.</param>
        /// <param name="button">the in question.</param>
        //----------------------------------------------------------------------------
        protected void AddButton(string szName, Button button)
        {
            Button value;
            Debug.Assert(!m_buttonDictionary.TryGetValue(szName, out value), "A button has already been added with the name: " + szName);
            m_buttonDictionary.Add(szName, button);
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// get the button by name.
        /// </summary>
        /// <param name="szName">the name of the button.</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------
        protected Button RetrieveButton(string szName)
        {
            return m_buttonDictionary[szName];
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// add a Menu as a child of this Menu.
        /// </summary>
        /// <param name="childMenu">The Menu to add as a child.</param>
        //----------------------------------------------------------------------------
        protected void AddChildMenu(Menu childMenu)
        {
#warning Make it so only one child menu for each menu.
            m_node.AddChild(childMenu.m_node);
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// remove child menu.
        /// </summary>
        /// <param name="childMenu"></param>
        //----------------------------------------------------------------------------
        protected void RemoveChildMenu(Menu childMenu)
        {
            m_node.RemoveChild(childMenu.m_node);
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// get parent of this menu.
        /// </summary>
        /// <returns></returns>
        //----------------------------------------------------------------------------
        protected Menu GetParent()
        {
            return m_node.Parent.UserData;
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// 'switch' on this menu.
        /// </summary>
        //----------------------------------------------------------------------------
        public void Activate()
        {
            Debug.Assert(m_stateMachine.CurrentState == m_states["Idle"]);
            m_stateMachine.ChangeState(m_states["Activating"]);
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// 'switch' off this menu.
        /// </summary>
        //----------------------------------------------------------------------------
        public void Deactivate()
        {
            Debug.Assert(m_stateMachine.CurrentState == m_states["Active"]);
            m_stateMachine.ChangeState(m_states["Deactivating"]);
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// Copy the buttons in this menu into an array and pass out ref
        /// </summary>
        /// <param name="aButtons"></param>
        //----------------------------------------------------------------------------
        public void ButtonsToArray(out Button[] aButtons)
        {
            aButtons = new Button[m_buttonDictionary.Count];
            m_buttonDictionary.Values.CopyTo(aButtons, 0);
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// Explicit Override for interface.
        /// </summary>
        /// <param name="gameTime"></param>
        //----------------------------------------------------------------------------
        void IUpdateable.Update(GameTime gameTime)
        {
            Update(new DeltaTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime));
        }
        //----------------------------------------------------------------------------
        /// <summary>
        /// update this menu.
        /// </summary>
        /// <param name="deltaTime"></param>
        //----------------------------------------------------------------------------
        public virtual void Update(DeltaTime deltaTime)
        {
            m_stateMachine.Update(deltaTime);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                bool bFlag = false;
                try
                {
                    Monitor.Enter(this, ref bFlag);

                    m_stateMachine = null;
                    m_buttonDictionary.Clear();
                    m_buttonDictionary = null;
                    m_states.Clear();
                    m_states = null;

                    if (EngineServices.GetSystem<IGameSystems>() != null)
                    {
                        EngineServices.GetSystem<IGameSystems>().Components.Remove(this);
                    }

                    if (Disposed != null) Disposed(this, EventArgs.Empty);
                }
                finally
                {
                    if (bFlag)
                    {
                        Monitor.Exit(this);
                    }
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public bool IsActive { get { return m_bIsActive; } }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public bool Enabled
        {
            get { return m_bEnabled; }
            set
            {
                if (m_bEnabled != value)
                {
                    m_bEnabled = value;
                    if (EnabledChanged != null) EnabledChanged(this, EventArgs.Empty);
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public int UpdateOrder
        {
            get { return m_nUpdateOrder; }
            set
            {
                if (m_nUpdateOrder != value)
                {
                    m_nUpdateOrder = value;
                    if (UpdateOrderChanged != null) UpdateOrderChanged(this, EventArgs.Empty);
                }
            }
        }


        //NOTE: These may not be needed.
        //STATES
        //----------------------------------------------------------------------------
        /// <summary>
        /// This are the different state of each menu...controlling the flow
        /// of the menu system. This will mainly be used for animation purposes.
        /// </summary>
        //----------------------------------------------------------------------------
        class StateIdle : State<Menu>
        {
            public override void OnEnter(StateMachine<Menu> stateMachine)
            {
                Menu menu = stateMachine.UserData as Menu;

                if(menu.Idle != null)
                    menu.Idle();
            }
            //----------------------------------------------------------------------------
            //----------------------------------------------------------------------------
            public override void OnUpdate(StateMachine<Menu> stateMachine, DeltaTime deltaTime)
            {

            }
            //----------------------------------------------------------------------------
            //----------------------------------------------------------------------------
            public override void OnExit(StateMachine<Menu> stateMachine)
            {

            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        class StateActivating : State<Menu>
        {
            public override void OnEnter(StateMachine<Menu> stateMachine)
            {
                Menu menu = stateMachine.UserData as Menu;

                if (menu.Activating != null)
                    menu.Activating();
            }
            //----------------------------------------------------------------------------
            //----------------------------------------------------------------------------
            public override void OnUpdate(StateMachine<Menu> stateMachine, DeltaTime deltaTime)
            {
                Menu menu = stateMachine.UserData as Menu;
                stateMachine.ChangeState(menu.m_states["Active"]);
            }
            //----------------------------------------------------------------------------
            //----------------------------------------------------------------------------
            public override void OnExit(StateMachine<Menu> stateMachine)
            {

            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        class StateActive : State<Menu>
        {
            public override void OnEnter(StateMachine<Menu> stateMachine)
            {
                Menu menu = stateMachine.UserData as Menu;
                
                if(menu.Active != null)
                    menu.Active();

                menu.m_bIsActive = true;

            }
            //----------------------------------------------------------------------------
            //----------------------------------------------------------------------------
            public override void OnUpdate(StateMachine<Menu> stateMachine, DeltaTime deltaTime)
            {

            }
            //----------------------------------------------------------------------------
            //----------------------------------------------------------------------------
            public override void OnExit(StateMachine<Menu> stateMachine)
            {
                Menu menu = stateMachine.UserData as Menu;

                menu.m_bIsActive = false;
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        class StateDeactivating : State<Menu>
        {
            public override void OnEnter(StateMachine<Menu> stateMachine)
            {
                Menu menu = stateMachine.UserData as Menu;
                
                if(menu.Deactivating != null)
                    menu.Deactivating();

            }
            //----------------------------------------------------------------------------
            //----------------------------------------------------------------------------
            public override void OnUpdate(StateMachine<Menu> stateMachine, DeltaTime deltaTime)
            {
                Menu menu = stateMachine.UserData as Menu;
                stateMachine.ChangeState(menu.m_states["Idle"]);


            }
            //----------------------------------------------------------------------------
            //----------------------------------------------------------------------------
            public override void OnExit(StateMachine<Menu> stateMachine)
            {

            }
        }
    }
}
