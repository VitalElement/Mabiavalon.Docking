using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex;

namespace Mabiavalon
{
    public interface IItemsOrganizer
    {
        void Organise(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> items);
        void Organise(DockItemsControl requestor, Size measureBounds, IOrderedEnumerable<DockItem> items);
        void OrganiseOnMouseDownWithin(DockItemsControl requestor, Size measureBounds, List<DockItem> siblingItems, DockItem dockItem);
        void OrganiseOnDragStarted(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem);
        void OrganiseOnDrag(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem);
        void OrganiseOnDragCompleted(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem);
        Point ConstrainLocation(DockItemsControl requestor, Size measureBounds, Point itemCurrentLocation, Size itemCurrentSize, Point itemDesiredLocation, Size itemDesiredSize);
        Size Measure(DockItemsControl requestor, Size availableSize, IEnumerable<DockItem> items);
        IEnumerable<DockItem> Sort(IEnumerable<DockItem> items);
    }
}
