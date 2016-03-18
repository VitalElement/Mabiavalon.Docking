using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Mabiavalon.Core;
using Perspex;
using Perspex.Controls;
using Perspex.Styling;

namespace Mabiavalon
{
    public class DockControl : TabControl
    {
        public const string HeaderItemsControlPartName = "PART_HeaderItemsControl";
        public const string ItemsHolderPartName = "PART_ItemsHolder";

        private static readonly HashSet<DockControl> LoadedInstances = new HashSet<DockControl>();

        private Panel _itemsHolder;
        private TabHeaderDragStartInformation _tabHeaderDragStartInformation;
        private WeakReference _previousSelection;
        private DockItemsControl _dockItemsControl;
        private IDisposable _templateSubscription;
        private readonly SerialDisposable _windowSubscription = new SerialDisposable();

        private InterTabTransfer _interTabTransfer;

        public static readonly StyledProperty<Style> CustomHeaderItemStyleProperty =
            PerspexProperty.Register<DockControl, Style>("CustomHeaderItemStyle");

        public static readonly StyledProperty<InterTabController> InterTabControllerProperty =
            PerspexProperty.Register<DockControl, InterTabController>("InterTabController");

        static DockControl()
        {
            
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DockControl()
        {
            
        }

        public Style CustomHeaderItemStyle
        {
            get { return GetValue(CustomHeaderItemStyleProperty); }
            set { SetValue(CustomHeaderItemStyleProperty, value); }
        }

        public InterTabController InterTabController
        {
            get { return GetValue(InterTabControllerProperty); }
            set { SetValue(InterTabControllerProperty, value); }
        }

        /// <summary>
        /// Helper method to add an item next to an existing item.
        /// </summary>
        /// <remarks>
        /// Due to the organisable nature of the control, the order of items may not reflect the order in the source collection.  This method
        /// will add items to the source collection, managing their initial appearance on screen at the same time. 
        /// If you are using a <see cref="InterTabController.InterTabClient"/> this will be used to add the item into the source collection.
        /// </remarks>
        /// <param name="item">New item to add.</param>
        /// <param name="nearItem">Existing object/tab item content which defines which tab control should be used to add the object.</param>
        /// <param name="addLocationHint">Location, relative to the <paramref name="nearItem"/> object</param>
        public static void AddItem(object item, object nearItem, AddLocationHint addLocationHint)
        {
            if (nearItem == null) throw new ArgumentNullException("nearItem");

            var existingLocation = GetLoadedInstances().SelectMany(tabControl =>
                (tabControl.Items).OfType<object>()
                    .Select(existingObject => new { tabControl, existingObject }))
                .SingleOrDefault(a => nearItem.Equals(a.existingObject));

            if (existingLocation == null)
                throw new ArgumentException("Did not find precisely one instance of adjacentTo", "nearItem");

            if (existingLocation.tabControl._dockItemsControl != null)
                existingLocation.tabControl._dockItemsControl.MoveItem(new MoveItemRequest(item, nearItem, addLocationHint));
        }

        internal static DockControl GetOwnerOfHeaderItems(DockItemsControl itemsControl)
        {
            return LoadedInstances.FirstOrDefault(t => Equals(t._dockItemsControl, itemsControl));
        }

        /// <summary>
        /// Helper method which returns all the currently loaded instances.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<DockControl> GetLoadedInstances()
        {
            return LoadedInstances.ToList();
        }

        public void RemoveItem(DockItem dockItem)
        {
            throw new NotImplementedException();
        }

        public void AddToSource(object content)
        {
            throw new NotImplementedException();
        }
    }
}