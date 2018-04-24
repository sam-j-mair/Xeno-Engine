using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XenoEngine.Serialization;
using XenoEngine.GeneralSystems;
using Microsoft.Xna.Framework;
using System.Threading;
using XenoEngine.Systems.Sprite_Systems;
using XenoEngine.Utilities;
using XenoEngine.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


//Aliases
using GUINode = XenoEngine.Systems.TreeNode<XenoEngine.Systems.MenuSystem.GUIObject>;
using System.Diagnostics;
using System.ComponentModel;

namespace XenoEngine.Systems.MenuSystem
{
    enum GUIInteractStates
    {
        RollOn,
        RollOff,
        Idle
    }
    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    public abstract class GUIObject : SerializableComponent, IGameComponent, IUpdateable, IDockable, IDisposable
    {
        #region PRIVATE
        private GUINode                                         m_treeNode;
        private GUIInteractStates                               m_currentState;
        private IScriptUpdateable<GUIObject>                      m_currentScript;
        private Dictionary<string, IScriptUpdateable<GUIObject>>  m_buttonScripts;
        private MouseObserver                                   m_mouseObserver;
        private int                                             m_nUpdateOrder;
        private bool                                            m_bEnabled;
        #endregion

        #region PROTECTED
        protected Rectangle                                     m_BoundingRectangle;
        protected bool                                          m_bSelected;
        #endregion

        #region PUBLIC
        public Action<GUIObject>                    OnOver;
        public Action<GUIObject>                    OnOff;
        public Action<GUIObject>                    OnClick;
        public Action<GUIObject>                    OnUnClick;
        public Action<GUIObject>                    OnClickPolling;
        public Action<GUIObject>                    OnHoldingButtonRollOver;
        public Action<GUIObject>                    OnReleasingButtonRollOver;
        public event EventHandler<EventArgs>        EnabledChanged;
        public event EventHandler<EventArgs>        UpdateOrderChanged;
        public event PropertyChangedEventHandler    PropertyChanged;
        #endregion

        public GUIObject(List<IScriptUpdateable<Button>> buttonScripts)
        {
            Input inputSytem = EngineServices.GetSystem<IGameSystems>().InputSystem;
            m_buttonScripts = new Dictionary<string, IScriptUpdateable<GUIObject>>(4);
            m_treeNode = new GUINode();
            m_treeNode.UserData = this;
            m_currentState = GUIInteractStates.Idle;

            m_mouseObserver = inputSytem.GetMouseObserver();

            InitialiseScripts(buttonScripts);
            ConnectedEvents(inputSytem);

            EngineServices.GetSystem<IGameSystems>().Components.Add(this);
            Enabled = true;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        ~GUIObject() { Dispose(false); } 

        public dynamic UserData { get; set; }
        public bool Active { get; private set; }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected void CalculateBoundingRectangle(Sprite sprite)
        {
            Vector2 v2Position = sprite.Position;
            Texture2D texture2D = sprite.Graphic;
            m_BoundingRectangle = new Rectangle((int)v2Position.X, (int)v2Position.Y, texture2D.Width, texture2D.Height);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected void CalculateBoundingRectangle(Vector2 v2Pos, Texture2D texture)
        {
            m_BoundingRectangle = new Rectangle((int)v2Pos.X, (int)v2Pos.Y, texture.Width, texture.Height);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void ConnectedEvents(Input inputSytem)
        {
            ActionMap actionMap = inputSytem.GetController((int)Controller.Player_1_Mouse);

            actionMap.TryCreateandSetHandler("LeftClick", MouseButton.LeftButton, ButtonState.Pressed, false, OnClickEvent);
            actionMap.TryCreateandSetHandler("LeftClickPolling", MouseButton.LeftButton, ButtonState.Pressed, true, OnClickEventPolling);
            actionMap.TryCreateandSetHandler("LeftClickReleased",MouseButton.LeftButton, ButtonState.Released, false, OnUnClickEvent);

            OnOver += OnOverEvent;
            OnOff += OnOffEvent;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void InitialiseScripts(List<IScriptUpdateable<Button>> buttonScripts)
        {
            if (buttonScripts != null)
            {
                foreach (IScriptUpdateable<GUIObject> script in buttonScripts)
                {
                    if (script != null)
                    {
                        script.OnInitialise(null);
                        script.OnPostInitialise();
                        script.FireComplete += ScriptComplete;
                        script.UserData = this;
                        m_buttonScripts.Add(script.ToString(), script);
                    }
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void SetActive(bool bActive)
        {
            if (!Active && bActive)
            {
                Active = bActive;
            }
            else if (Active && !bActive)
            {
                Active = bActive;
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void OnSelectEvent(Button sender) { }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void OnOverEvent(GUIObject button)
        {
            if (Active)
            {
                IScriptUpdateable<GUIObject> script;
                if (m_buttonScripts.TryGetValue("OnOver", out script))
                {
                    m_currentScript = script;
                }
                else
                {
                    //Debug.WriteLine("No OnOver Script has been defined..");
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void OnOffEvent(GUIObject button)
        {
            if (Active)
            {

                IScriptUpdateable<GUIObject> script;
                if (m_buttonScripts.TryGetValue("OnOff", out script))
                {
                    m_currentScript = script;
                }
                else
                {
                    //Debug.WriteLine("No OnOff Script has been defined..");
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void OnClickEvent(object sender)
        {
            if (Active && CheckInteraction())
            {
                IScriptUpdateable<GUIObject> script;
                if (m_buttonScripts.TryGetValue("OnSelect", out script))
                {
                    m_currentScript = script;
                }
                else
                {
                    //Debug.WriteLine("No OnClick Script has been defined..");
                }

                if (OnClick != null)
                    OnClick(this);

                m_bSelected = true;
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void OnUnClickEvent(object sender)
        {
            if (Active && m_bSelected)
            {
                IScriptUpdateable<GUIObject> script;
                if (m_buttonScripts.TryGetValue("OnUnSelect", out script))
                {
                    m_currentScript = script;
                }
                else
                {
                    //Debug.WriteLine("No OnClick Script has been defined..");
                }

                if (OnUnClick != null)
                    OnUnClick(this);

                m_bSelected = false;
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void OnClickEventPolling(object sender)
        {
            if (Active && CheckInteraction())
            {
                IScriptUpdateable<GUIObject> script;
                if (m_buttonScripts.TryGetValue("OnSelectPolling", out script))
                {
                    m_currentScript = script;
                }
                else
                {
                    //Debug.WriteLine("No OnClick Script has been defined..");
                }

                if (OnClickPolling != null)
                    OnClickPolling(this);

                m_bSelected = true;
            }

        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        private void ScriptComplete()
        {
            m_currentScript = null;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Initialize() { }
        void IUpdateable.Update(GameTime gameTime) {Update(new DeltaTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime)); }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void AddChild(GUIObject button)
        {
            Debug.Assert(button != this);
            m_treeNode.AddChild(button.m_treeNode);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public void RemoveChild(Button button)
        {
            m_treeNode.RemoveChild(button.m_treeNode);
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        private void RecursiveDeparent()
        {
            foreach (GUINode node in m_treeNode)
            {
                node.UserData.RecursiveDeparent();
                node.UserData.Dispose();
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Update(DeltaTime deltaTime)
        {
            CheckforRoll();

            if (m_currentScript != null)
            {
                m_currentScript.OnUpdate(deltaTime);
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual void CheckforRoll()
        {
            if (CheckInteraction())
            {
                if (m_currentState != GUIInteractStates.RollOn)
                {
                    if (OnOver != null)
                        OnOver(this);

                    m_currentState = GUIInteractStates.RollOn;
                }
            }
            else
            {
                if (m_currentState == GUIInteractStates.RollOn)
                {
                    if (OnOff != null)
                        OnOff(this);

                    m_currentState = GUIInteractStates.RollOff;
                }
            }
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        protected virtual bool CheckInteraction()
        {
            bool bTest = false;

            if (m_BoundingRectangle.Contains(m_mouseObserver.MouseX, m_mouseObserver.MouseY))
            {
                bTest = true;
            }

            return bTest;
        }
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                bool bFlag = false;
                try
                {
                    Monitor.Enter(this, ref bFlag);

                    m_treeNode.RemoveNode(true);
                    m_treeNode.Dispose();
                    m_treeNode = null;
                    m_currentScript = null;
                    m_currentState = GUIInteractStates.Idle;
                    m_buttonScripts.Clear();
                    m_buttonScripts = null;
                    m_mouseObserver = null;
                    OnClick -= OnClickEvent;
                    OnOff -= OnOffEvent;
                    OnOver -= OnOverEvent;
                    OnUnClick -= OnUnClickEvent;
                    OnClickPolling -= OnClickEventPolling;

                    if (EngineServices.GetSystem<IGameSystems>() != null)
                    {
                        EngineServices.GetSystem<IGameSystems>().Components.Remove(this);
                    }
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

        /// <summary>
        /// The center of the bounding rectangle
        /// </summary>
        public virtual Vector2 Center { get { return m_BoundingRectangle.Center.ToVec2(); } }

        public float Width { get { return m_BoundingRectangle.Width; } }
        public float Height { get { return m_BoundingRectangle.Height; } }

        /// <summary>
        /// The top left corner of the bounding rectangle.
        /// </summary>
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public virtual Vector2 Position
        {
            get { return m_BoundingRectangle.Location.ToVec2(); }
            set
            {
                //This is kind of crap but because of the way properties work ..i need to do this.
                Vector2 v2Start = m_BoundingRectangle.Location.ToVec2();
                m_BoundingRectangle.Location = value.ToPoint();

                foreach (GUINode treeNode in m_treeNode)
                {
                    Vector2 v2Offset = treeNode.UserData.Position - v2Start;
                    treeNode.UserData.Position = value + v2Offset;
                }
            }
        }
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
    }

}
