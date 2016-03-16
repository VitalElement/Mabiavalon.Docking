using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mabiavalon.Core
{
    internal class TabHeaderDragStartInformation
    {
        private readonly DockItem _dockItem;
        private readonly double _dockItemsControlHorizontalOffset;
        private readonly double _dockItemControlVerticalOffset;
        private readonly double _dockItemHorizontalOffset;
        private readonly double _dockItemVerticalOffset;

        public TabHeaderDragStartInformation(
            DockItem dockItem,
            double dockItemsControlHorizontalOffset, double dockItemControlVerticalOffset, double dockItemHorizontalOffset, double dockItemVerticalOffset)
        {
            if (dockItem == null) throw new ArgumentNullException("dragItem");

            _dockItem = dockItem;
            _dockItemsControlHorizontalOffset = dockItemsControlHorizontalOffset;
            _dockItemControlVerticalOffset = dockItemControlVerticalOffset;
            _dockItemHorizontalOffset = dockItemHorizontalOffset;
            _dockItemVerticalOffset = dockItemVerticalOffset;
        }

        public DockItem Item
        {
            get { return _dockItem; }
        }

        public double DockItemsControlHorizontalOffset
        {
            get { return _dockItemsControlHorizontalOffset; }
        }

        public double DockItemControlVerticalOffset
        {
            get { return _dockItemControlVerticalOffset; }
        }

        public double DockItemHorizontalOffset
        {
            get { return _dockItemHorizontalOffset; }
        }

        public double DockItemVerticalOffset
        {
            get { return _dockItemVerticalOffset; }
        }
    }
}
