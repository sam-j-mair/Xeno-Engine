using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XenoEngine.Utilities;

namespace XenoEngine.Systems.MenuSystem
{
    public class DockPanel : GUIObject
    {
        private PlaceHolder[]   m_DockedItems;
        private PixelSpaceAllocator[]          m_pages;
        private int             m_nCurrentIndex;
        private int             m_nLastAddedIndex;
        private int             m_nCurrentPage;

        private const int       MAX_ENTRIES = 20;
        private const int       ITEM_OFFSET = 10;

        public DockPanel(
            Vector2 v2InitialPos,
            int nWidth,
            int nHeight,
            int nSlotCount,
            int nNumberOfPages) : base(null)
        {
            m_DockedItems = new PlaceHolder[nSlotCount];
            m_pages = new PixelSpaceAllocator[nNumberOfPages];
            m_nCurrentPage = 0;

            //initialize the first page.
            m_pages[m_nCurrentPage] = new PixelSpaceAllocator(v2InitialPos, nWidth, nHeight);
        }

        public void Add(IDockable dockingObject)
        {
            var entry = new PlaceHolder();
            entry.DockableObject = dockingObject;
            

            if (m_pages[m_nCurrentPage].IsFull)
            {
                ++m_nCurrentPage;
            }

            if(m_pages[m_nCurrentPage].AllocPixelSpace(dockingObject, new Rectangle((int)dockingObject.Position.X,
                                                                                (int)dockingObject.Position.Y,
                                                                                (int)dockingObject.Width,
                                                                                (int)dockingObject.Height)))
            {
                m_DockedItems[m_nCurrentIndex++] = entry;
                var lastItem = m_DockedItems[m_nLastAddedIndex];
            }



            
            
//             dockingObject.Position = new Vector2(
//                 lastItem.DockableObject.Position.X, 
//                 lastItem.DockableObject.Position.Y + lastItem.DockableObject.Height + ITEM_OFFSET);

            m_nLastAddedIndex = m_nCurrentIndex - 1;
        }

//         private Vector2 FindNextAvailablePosition()
//         {
// 
//         }

        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
            }
        }


        internal class PlaceHolder
        {
            public bool IsEmpty { get; set; }
            public Vector2 Position { get; set; }
            public Rectangle Bounds{ get; set; }
            public IDockable DockableObject { get; set; }
        }

        internal class PixelSpaceAllocator
        {
            LinkedList<PlaceHolder> m_pixelSpace = new LinkedList<PlaceHolder>();
            //LinkedList<PlaceHolder> m_allocSpace = new LinkedList<PlaceHolder>();

            public PixelSpaceAllocator(Vector2 v2Pos, int nWidth, int nHeight)
            {
                m_pixelSpace = new LinkedList<PlaceHolder>();

                //The whole space is empty to start with.
                PlaceHolder placeHolder = new PlaceHolder();
                placeHolder.Bounds = new Rectangle((int)v2Pos.X, (int)v2Pos.Y, nWidth, nHeight);
                placeHolder.DockableObject = null;
                placeHolder.IsEmpty = true;
                placeHolder.Position = v2Pos;

                m_pixelSpace.AddFirst(placeHolder);
            }

            public bool AllocPixelSpace(IDockable iDockable, Rectangle bounds)
            {
                bool bIsFull = true;
                bool bAlloced = false;
                const int OBJECT_OFFSET = 10;

                foreach (PlaceHolder location in m_pixelSpace)
                {
                    if (location.IsEmpty)
                    {
                        if (location.Bounds.Width >= bounds.Width && 
                            location.Bounds.Height >= bounds.Height)
                        {
                            PlaceHolder placeHolder = new PlaceHolder();
                            placeHolder.Bounds = bounds;
                            placeHolder.IsEmpty = false;
                            placeHolder.DockableObject = iDockable;

                            iDockable.Position = (bounds.Location = location.Bounds.Location).ToVec2();
                            placeHolder.Position = bounds.Location.ToVec2();

                            m_pixelSpace.AddAfter(m_pixelSpace.First, placeHolder);

                            

                            location.Bounds = new Rectangle(bounds.X,
                                bounds.Y + bounds.Height + OBJECT_OFFSET,
                                location.Bounds.Width,
                                location.Bounds.Height - bounds.Height);

                            bAlloced = true;
                        }

                        bIsFull = false;

                        if(bAlloced)
                            break;
                    }
                }

                IsFull = bIsFull;

                //if the page isn't full but we failed to find an allocation
                //we defragment the space and try again.
                if (!IsFull && !bAlloced)
                {
                    ConsolidatePixelSpace();
                    AllocPixelSpace(iDockable, bounds);
                }

                return bAlloced;
            }

            public void DeallocPixelSpace(Rectangle bounds)
            {
                foreach (PlaceHolder placeHolder in m_pixelSpace)
                {
                    if (bounds.Equals(placeHolder.Bounds))
                    {
                        placeHolder.DockableObject = null;
                        placeHolder.IsEmpty = true;
                    }
                }
            }

            private void ConsolidatePixelSpace()
            {
                PlaceHolder consoledSpace = new PlaceHolder();
                consoledSpace.IsEmpty = true;
                consoledSpace.DockableObject = null;
                
                foreach (PlaceHolder placeHolder in m_pixelSpace.ToArray())
                {
                    if (placeHolder.IsEmpty)
                    {
                        consoledSpace.Position = Vector2.Max(consoledSpace.Position, placeHolder.Position);
                        consoledSpace.Bounds = new Rectangle(
                            consoledSpace.Bounds.Location.X + placeHolder.Bounds.Location.X,
                            consoledSpace.Bounds.Location.Y + placeHolder.Bounds.Location.Y,
                            consoledSpace.Bounds.Width + placeHolder.Bounds.Width,
                            consoledSpace.Bounds.Height + placeHolder.Bounds.Height);

                        m_pixelSpace.Remove(placeHolder);
                    }
                }

                m_pixelSpace.AddAfter(m_pixelSpace.First, consoledSpace);
            }

            public bool IsFull { get; private set; }
        }
    }
}
