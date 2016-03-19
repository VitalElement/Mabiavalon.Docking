using Mabiavalon.Core;
using Mabiavalon.Docking;
using Perspex;
using Perspex.Controls;
using Perspex.Controls.Presenters;
using Perspex.LogicalTree;
using Perspex.Markup.Xaml.Templates;
using Perspex.Styling;
using Perspex.VisualTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Disposables;

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

        private string _headerMemberPath;
        private string _headerPrefixContentStringFormat;
        private Func<object> _newItemFactory;
        private bool _isDraggingWindow;
        private ItemActionCallback _consolidatingOrphanedItemCallback;
        private ItemActionCallback _closingItemCallback;
        private bool _consolidateOrphanedItems;
        private bool _isEmpty;
        private int _fixedHeaderCount;
        private string _headerSUffixContentStringFormate;
        private bool _showDefaultCloseButton;
        private bool _showDefaultAddButton;
        private AddLocationHint _addLocationHint;

        public static readonly StyledProperty<Style> CustomHeaderItemStyleProperty =
            PerspexProperty.Register<DockControl, Style>("CustomHeaderItemStyle");

        public static readonly StyledProperty<InterTabController> InterTabControllerProperty =
            PerspexProperty.Register<DockControl, InterTabController>("InterTabController");

        public static readonly StyledProperty<DataTemplate> CustomHeaderItemTemplateProperty =
            PerspexProperty.Register<DockControl, DataTemplate>("CustomHeaderItemTemplate");

        public static readonly StyledProperty<double> AdjacentHeaderOffsetProperty =
            PerspexProperty.Register<DockControl, double>("AdjacentHeaderOffset");

        public static readonly StyledProperty<IItemsOrganizer> HeaderItemsOrganizerProperty =
            PerspexProperty.Register<DockControl, IItemsOrganizer>("HeaderItemsOrganizer", defaultValue: new HorizontalOrganizer());

        public static readonly StyledProperty<EmptyHeaderSizingHint> EmptyHeaderSizingHintProperty =
            PerspexProperty.Register<DockControl, EmptyHeaderSizingHint>("EmptyHeaderSizingHint");

        public static readonly StyledProperty<DataTemplate> HeaderItemTemplateProperty =
            PerspexProperty.Register<DockControl, DataTemplate>("HeaderItemTemplate");

        public static readonly StyledProperty<object> HeaderPrefixContentProperty =
            PerspexProperty.Register<DockControl, object>("HeaderPrefixContent");

        public static readonly StyledProperty<DataTemplate> HeaderPrefixContentTemplateProperty =
            PerspexProperty.Register<DockControl, DataTemplate>("HeaderPrefixContentTemplate");

        public static readonly StyledProperty<object> HeaderSuffixContentProperty =
            PerspexProperty.Register<DockControl, object>("HeaderSuffixContent");

        public static readonly StyledProperty<DataTemplate> HeaderSuffixContentTemplateProperty =
            PerspexProperty.Register<DockControl, DataTemplate>("HeaderSuffixContentTemplate");

        public static readonly DirectProperty<DockControl, string> HeaderMemberPathProperty =
            PerspexProperty.RegisterDirect<DockControl, string>("HeaderMemberPath", o => o.HeaderMemberPath,
                (o, v) => o.HeaderMemberPath = v);

        public static readonly DirectProperty<DockControl, string> HeaderPrefixContentStringFormatProperty =
            PerspexProperty.RegisterDirect<DockControl, string>("HeaderPrefixContentStringFormat", o => o.HeaderPrefixContentStringFormat,
                (o, v) => o.HeaderPrefixContentStringFormat = v);

        public static readonly DirectProperty<DockControl, string> HeaderSUffixContentStringFormateProperty =
            PerspexProperty.RegisterDirect<DockControl, string>("HeaderSUffixContentStringFormate", o => o.HeaderSUffixContentStringFormate,
                (o, v) => o.HeaderSUffixContentStringFormate = v);

        public static readonly DirectProperty<DockControl, bool> ShowDefaultCloseButtonProperty =
            PerspexProperty.RegisterDirect<DockControl, bool>("ShowDefaultCloseButton", o => o.ShowDefaultCloseButton,
                (o, v) => o.ShowDefaultCloseButton = v);

        public static readonly DirectProperty<DockControl, bool> ShowDefaultAddButtonProperty =
            PerspexProperty.RegisterDirect<DockControl, bool>("ShowDefaultAddButton", o => o.ShowDefaultAddButton,
                (o, v) => o.ShowDefaultAddButton = v);

        public static readonly DirectProperty<DockControl, AddLocationHint> AddLocationHintProperty =
            PerspexProperty.RegisterDirect<DockControl, AddLocationHint>("AddLocationHint", o => o.AddLocationHint,
                (o, v) => o.AddLocationHint = v);

        public static readonly DirectProperty<DockControl, int> FixedHeaderCountProperty =
            PerspexProperty.RegisterDirect<DockControl, int>("FixedHeaderCount", o => o.FixedHeaderCount,
                (o, v) => o.FixedHeaderCount = v);

        public static readonly DirectProperty<DockControl, bool> ConsolidateOrphanedItemsProperty =
            PerspexProperty.RegisterDirect<DockControl, bool>("ConsolidateOrphanedItems", o => o.ConsolidateOrphanedItems,
                (o, v) => o.ConsolidateOrphanedItems = v);

        public static readonly DirectProperty<DockControl, ItemActionCallback> ClosingItemCallbackProperty =
            PerspexProperty.RegisterDirect<DockControl, ItemActionCallback>("ClosingItemCallback", o => o.ClosingItemCallback,
                (o, v) => o.ClosingItemCallback = v);

        public static readonly DirectProperty<DockControl, ItemActionCallback> ConsolidatingOrphanedItemCallbackProperty =
            PerspexProperty.RegisterDirect<DockControl, ItemActionCallback>("ConsolidatingOrphanedItemCallback", o => o.ConsolidatingOrphanedItemCallback,
                (o, v) => o.ConsolidatingOrphanedItemCallback = v);

        public static readonly DirectProperty<DockControl, Func<object>> NewItemFactoryProperty =
            PerspexProperty.RegisterDirect<DockControl, Func<object>>("NewItemFactory", o => o.NewItemFactory,
                (o, v) => o.NewItemFactory = v);

        public static readonly DirectProperty<DockControl, bool> IsDraggingWindowProperty =
            PerspexProperty.RegisterDirect<DockControl, bool>("IsDraggingWindow",
                o => o.IsDraggingWindow);

        public static readonly DirectProperty<DockControl, bool> IsEmptyProperty =
            PerspexProperty.RegisterDirect<DockControl, bool>("IsEmpty",
                o => o.IsEmpty);

        public static readonly AttachedProperty<bool> IsClosingAsPartOfDragOperationProperty =
            PerspexProperty.RegisterAttached<DockControl, Control, bool>("IsClosingAsPartOfDragOperation");

        public static readonly AttachedProperty<bool> IsWrappingTabProperty =
            PerspexProperty.RegisterAttached<DockControl, Control, bool>("IsWrappingTab");

        static DockControl()
        {
            DockItem.DragStartedEvent.AddClassHandler<DockControl>(x => x.OnItemDragStart);
            DockItem.DragDeltaEvent.AddClassHandler<DockControl>(x => x.OnItemDragDelta);
            DockItem.PreviewDragDeltaEvent.AddClassHandler<DockControl>(x => x.OnItemPreviewDragDelta);
            DockItem.DragCompletedEvent.AddClassHandler<DockControl>(x => x.OnItemDragCompleted);
        }

        public DockControl()
        {

        }

        public Style CustomHeaderItemStyle
        {
            get { return GetValue(CustomHeaderItemStyleProperty); }
            set { SetValue(CustomHeaderItemStyleProperty, value); }
        }

        public DataTemplate CustomHeaderItemTemplate
        {
            get { return GetValue(CustomHeaderItemTemplateProperty); }
            set { SetValue(CustomHeaderItemTemplateProperty, value); }
        }

        public double AdjacentHeaderOffset
        {
            get { return GetValue(AdjacentHeaderOffsetProperty); }
            set { SetValue(AdjacentHeaderOffsetProperty, value); }
        }

        public IItemsOrganizer HeaderItemsOrganizer
        {
            get { return GetValue(HeaderItemsOrganizerProperty); }
            set { SetValue(HeaderItemsOrganizerProperty, value); }
        }

        public InterTabController InterTabController
        {
            get { return GetValue(InterTabControllerProperty); }
            set { SetValue(InterTabControllerProperty, value); }
        }

        public EmptyHeaderSizingHint EmptyHeaderSizingHint
        {
            get { return GetValue(EmptyHeaderSizingHintProperty); }
            set { SetValue(EmptyHeaderSizingHintProperty, value); }
        }

        public DataTemplate HeaderSuffixContentTemplate
        {
            get { return GetValue(HeaderSuffixContentTemplateProperty); }
            set { SetValue(HeaderSuffixContentTemplateProperty, value); }
        }

        public object HeaderSuffixContent
        {
            get { return GetValue(HeaderSuffixContentProperty); }
            set { SetValue(HeaderSuffixContentProperty, value); }
        }

        public DataTemplate HeaderPrefixContentTemplate
        {
            get { return GetValue(HeaderPrefixContentTemplateProperty); }
            set { SetValue(HeaderPrefixContentTemplateProperty, value); }
        }

        public object HeaderPrefixContent
        {
            get { return GetValue(HeaderPrefixContentProperty); }
            set { SetValue(HeaderPrefixContentProperty, value); }
        }

        public DataTemplate HeaderItemTemplate
        {
            get { return GetValue(HeaderItemTemplateProperty); }
            set { SetValue(HeaderItemTemplateProperty, value); }
        }

        public string HeaderMemberPath
        {
            get { return _headerMemberPath; }
            set { SetAndRaise(HeaderMemberPathProperty, ref _headerMemberPath, value); }
        }

        public string HeaderPrefixContentStringFormat
        {
            get { return _headerPrefixContentStringFormat; }
            set { SetAndRaise(HeaderPrefixContentStringFormatProperty, ref _headerPrefixContentStringFormat, value); }
        }

        public Func<object> NewItemFactory
        {
            get { return _newItemFactory; }
            set { SetAndRaise(NewItemFactoryProperty, ref _newItemFactory, value); }
        }

        public bool IsDraggingWindow
        {
            get { return _isDraggingWindow; }
            private set { SetAndRaise(IsDraggingWindowProperty, ref _isDraggingWindow, value); }
        }

        public ItemActionCallback ConsolidatingOrphanedItemCallback
        {
            get { return _consolidatingOrphanedItemCallback; }
            set { SetAndRaise(ConsolidatingOrphanedItemCallbackProperty, ref _consolidatingOrphanedItemCallback, value); }
        }

        public ItemActionCallback ClosingItemCallback
        {
            get { return _closingItemCallback; }
            set { SetAndRaise(ClosingItemCallbackProperty, ref _closingItemCallback, value); }
        }

        public bool ConsolidateOrphanedItems
        {
            get { return _consolidateOrphanedItems; }
            set { SetAndRaise(ConsolidateOrphanedItemsProperty, ref _consolidateOrphanedItems, value); }
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
            private set { SetAndRaise(IsEmptyProperty, ref _isEmpty, value); }
        }

        public int FixedHeaderCount
        {
            get { return _fixedHeaderCount; }
            set { SetAndRaise(FixedHeaderCountProperty, ref _fixedHeaderCount, value); }
        }

        public AddLocationHint AddLocationHint
        {
            get { return _addLocationHint; }
            set { SetAndRaise(AddLocationHintProperty, ref _addLocationHint, value); }
        }

        public bool ShowDefaultAddButton
        {
            get { return _showDefaultAddButton; }
            set { SetAndRaise(ShowDefaultAddButtonProperty, ref _showDefaultAddButton, value); }
        }

        public bool ShowDefaultCloseButton
        {
            get { return _showDefaultCloseButton; }
            set { SetAndRaise(ShowDefaultCloseButtonProperty, ref _showDefaultCloseButton, value); }
        }

        public string HeaderSUffixContentStringFormate
        {
            get { return _headerSUffixContentStringFormate; }
            set { SetAndRaise(HeaderSUffixContentStringFormateProperty, ref _headerSUffixContentStringFormate, value); }
        }

        public static bool GetIsWrappingTab(Control element)
        {
            return element.GetValue(IsWrappingTabProperty);
        }

        public static void SetIsWrappingTab(Control element, bool value)
        {
            element.SetValue(IsWrappingTabProperty, value);
        }

        public static bool GetIsClosingAsPartOfDragOperation(Control element)
        {
            return element.GetValue(IsClosingAsPartOfDragOperationProperty);
        }

        public static void SetIsClosingAsPartOfDragOperation(Control element, bool value)
        {
            element.SetValue(IsClosingAsPartOfDragOperationProperty, value);
        }


        private bool IsMyItem(DockItem item)
        {
            return _dockItemsControl != null && _dockItemsControl.DockItems().Contains(item);
        }


        /// <summary>
        /// Helper method to add an item next to an existing item.
        /// </summary>
        /// <remarks>
        /// Due to the organizable nature of the control, the order of items may not reflect the order in the source collection.  This method
        /// will add items to the source collection, managing their initial appearance on screen at the same time. 
        /// If you are using a <see cref="InterTabController.InterTabClient"/> this will be used to add the item into the source collection.
        /// </remarks>
        /// <param name="item">New item to add.</param>
        /// <param name="nearItem">Existing object/tab item content which defines which tab control should be used to add the object.</param>
        /// <param name="addLocationHint">Location, relative to the <paramref name="nearItem"/> object</param>
        public static void AddItem(object item, object nearItem, AddLocationHint addLocationHint)
        {
            if (nearItem == null) throw new ArgumentNullException(nameof(nearItem));

            var existingLocation = GetLoadedInstances().SelectMany(dockControl =>
                (dockControl.Items).OfType<object>()
                    .Select(existingObject => new { docControl = dockControl, existingObject }))
                .SingleOrDefault(a => nearItem.Equals(a.existingObject));

            if (existingLocation == null)
                throw new ArgumentException("Did not find precisely one instance of adjacentTo", nameof(nearItem));

            existingLocation.docControl._dockItemsControl?.MoveItem(new MoveItemRequest(item, nearItem, addLocationHint));
        }

        /// <summary>
        /// Finds and selects an item.
        /// </summary>
        /// <param name="item"></param>
        public static void SelectItem(object item)
        {
            var existingLocation = GetLoadedInstances().SelectMany(dockControl =>
                (dockControl.Items).OfType<object>()
                    .Select(existingObject => new { dockControl, existingObject }))
                    .FirstOrDefault(a => item.Equals(a.existingObject));

            if (existingLocation == null) return;

            existingLocation.dockControl.SelectedItem = item;
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

        public object RemoveItem(DockItem dockItem)
        {
            var item = dockItem.GetLogicalAncestors().OfType<ItemsControl>().FirstOrDefault() as DockItemsControl;

            var minSize = EmptyHeaderSizingHint == EmptyHeaderSizingHint.PreviousTab
                ? new Size(_dockItemsControl.Width, _dockItemsControl.Height)
                : new Size();

            System.Diagnostics.Debug.WriteLine($"A {minSize}");
            _dockItemsControl.MinWidth = 0;
            _dockItemsControl.MinHeight = 0;

            var contentPresenter = item.GetVisualChildren().OfType<ContentPresenter>().FirstOrDefault();
            RemoveFromSource(item);
            _itemsHolder.Children.Remove(contentPresenter);

            if (((ICollection)Items).Count != 0) return item;

            var window = this.GetVisualAncestors().OfType<Window>().FirstOrDefault();
            if (window != null
                && InterTabController != null
                && InterTabController.InterTabClient.TabEmptiedHandler(this, window) == TabEmptiedResponse.CloseWindowOrLayoutBranch)
            {
                if (Layout.ConsolidateBranch(this)) return item;

                try
                {
                    SetIsClosingAsPartOfDragOperation(window, true);
                    window.Close();
                }
                finally
                {
                    SetIsClosingAsPartOfDragOperation(window, false);
                }
            }
            else
            {
                _dockItemsControl.MinHeight = minSize.Height;
                _dockItemsControl.MinWidth = minSize.Width;
            }

            return item;
        }

        public void AddToSource(object item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var manualInterTabController = InterTabController?.InterTabClient as IManualInterTabClient;
            if (manualInterTabController != null)
                manualInterTabController.Add(item);
            else
            {
                ((IList)Items).Add(item);
            }

        }

        public void RemoveFromSource(object item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var manualInterTabController = InterTabController?.InterTabClient as IManualInterTabClient;
            if (manualInterTabController != null)
                manualInterTabController.Remove(item);
            else
            {
                ((IList)Items).Remove(item);
            }
        }

        private void OnItemDragStart(DockDragStartedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnItemDragDelta(DockDragDeltaEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnItemPreviewDragDelta(DockDragDeltaEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnItemDragCompleted(DockDragCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}