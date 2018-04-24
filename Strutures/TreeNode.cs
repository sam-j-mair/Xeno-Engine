using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XenoEngine.Systems
{
    public enum TreeSearchDirection
    {
        Backward,
        Forward
    }

    public class TreeNode<TUserType> : IDisposable, IEnumerable<TreeNode<TUserType>>
    {
        public static int           FirstChild = 0;
        TreeNode<TUserType>         m_parent;
        
        // NOTE: This could be a LinkedList<T> but its 
        //       probably not the important unless deletion/insertion
        //       is important.

        List<TreeNode<TUserType>>   m_childList;
        TUserType                   m_UserData;
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public TreeNode(int nMaxChildren = 0)
        {
            m_parent = null;

            InitilizeChildList(nMaxChildren);

        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public TreeNode(TreeNode<TUserType> parent, int nMaxChildren = 0)
        {
            m_parent = parent;
            InitilizeChildList(nMaxChildren);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public TreeNode(TreeNode<TUserType> parent, int nMaxChildren = 0, params TreeNode<TUserType>[] children)
        {
            m_parent = parent;
            InitilizeChildList(nMaxChildren);
            
            foreach (TreeNode<TUserType> childNode in children)
            {
                m_childList.Add(childNode);
            }
        }
        //--------------------------------------------------------------
        //This function searches for the tree node where the data is located. An inverted search.
        //--------------------------------------------------------------
        public static TreeNode<TNodeType> GetNodeFromData<TNodeType>(TreeNode<TNodeType> startNode, TreeSearchDirection eDirection, TNodeType data)
        {
            switch (eDirection)
            {
                case TreeSearchDirection.Forward:
                {
                    return null;
                }

                case TreeSearchDirection.Backward:
                {
                    return null;
                }
            }

            return null;

        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        private void InitilizeChildList(int nMaxChildren)
        {
            if (nMaxChildren > 0)
                m_childList = new List<TreeNode<TUserType>>(nMaxChildren);
            else
                m_childList = new List<TreeNode<TUserType>>();
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        private void AddChildernToNode(ref List<TreeNode<TUserType>> childList)
        {
            foreach (TreeNode<TUserType> child in childList)
            {
                m_childList.Add(child);
            }
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void InsertNodeAfter(TreeNode<TUserType> node)
        {
            node.Parent = this;
            m_childList.Add(node);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void InsertNodeBefore(TreeNode<TUserType> node)
        {
            node.Parent = Parent;
            node.AddChild(this);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void AddChild(TreeNode<TUserType> child)
        {
            child.m_parent = this;
            m_childList.Add(child);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void AddChildren(params TreeNode<TUserType>[] children)
        {
            foreach(TreeNode<TUserType> node in children)
            {
                node.m_parent = this;
                m_childList.Add(node);
            }
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void RemoveChild(TreeNode<TUserType> child)
        {
            child.Parent = null;
            m_childList.Remove(child);
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void RemoveNode(bool bReattach)
        {
            if(bReattach)
            {
                if (m_parent != null)
                {
                    foreach (TreeNode<TUserType> child in m_childList)
                    {
                        m_parent.AddChild(child);
                    }
                }
                else
                {
                    foreach (TreeNode<TUserType> child in m_childList)
                    {
                        child.Parent = null;
                    }
                }
            }
            else
            {
                foreach (TreeNode<TUserType> child in m_childList)
                {
                    child.Parent = null;
                }
            }
            
            if(m_parent != null)
                m_parent.RemoveChild(this);

            m_parent = null;
            m_childList.Clear();
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public void Dispose()
        {
            RemoveNode(true);
            m_parent = null;
            m_UserData = default(TUserType);
            m_childList.Clear();
            IsDisposed = true;
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        public IEnumerator<TreeNode<TUserType>> GetEnumerator()
        {
            foreach (TreeNode<TUserType> treeNode in m_childList)
            {
                yield return treeNode;
            }
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        //--------------------------------------------------------------
        //--------------------------------------------------------------

        public List<TreeNode<TUserType>> Children { get { return m_childList; } } 
        public TreeNode<TUserType> Parent { get { return m_parent; } set { m_parent = value; } }
        public TUserType UserData { get { return m_UserData; } set { m_UserData = value; } }
        public bool IsDisposed { get; private set; }
        //--------------------------------------------------------------
        //--------------------------------------------------------------
        
    }
}
