using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex;
using Perspex.Controls;

namespace Mabiavalon
{
    public class CanvasOrganizer : IItemsOrganizer
    {
        public void Organize(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> items)
        {
        }

        public void Organize(DockItemsControl requestor, Size measureBounds, IOrderedEnumerable<DockItem> items)
        {
        }

        public void OrganizeOnMouseDownWithin(DockItemsControl requestor, Size measureBounds, List<DockItem> siblingItems, DockItem dockItem)
        {
            var zIndex = int.MaxValue;
            foreach (var source in siblingItems.OrderByDescending(x => x.GetValue(Visual.ZIndexProperty)))
            {
                source.SetValue(Visual.ZIndexProperty, --zIndex);
            }
            dockItem.SetValue(Visual.ZIndexProperty, int.MaxValue);
        }

        public void OrganizeOnDragStarted(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem)
        {
        }

        public void OrganizeOnDrag(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem)
        {
        }

        public void OrganizeOnDragCompleted(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem)
        {
        }

        public Point ConstrainLocation(DockItemsControl requestor, Size measureBounds, Point itemCurrentLocation, Size itemCurrentSize,
            Point itemDesiredLocation, Size itemDesiredSize)
        {
            var reduceBoundsWidth = itemCurrentLocation.X + itemCurrentSize.Width > measureBounds.Width
                ? 0
                : itemDesiredSize.Width;
            var reduceBoundsHeight = itemCurrentLocation.Y + itemCurrentSize.Height > measureBounds.Height
                ? 0
                : itemDesiredSize.Height;

            return new Point(
                Math.Min(Math.Max(itemDesiredLocation.X, 0), measureBounds.Width - reduceBoundsWidth),
                Math.Min(Math.Max(itemDesiredLocation.Y, 0), measureBounds.Height - reduceBoundsHeight));
        }

        public Size Measure(DockItemsControl requestor, Size availableSize, IEnumerable<DockItem> items)
        {
            return availableSize;
        }

        public IEnumerable<DockItem> Sort(IEnumerable<DockItem> items)
        {
            return items;
        }
    }
}
