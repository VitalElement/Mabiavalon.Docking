using Perspex;
using Perspex.Controls;
using Perspex.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mabiavalon
{
    public class DockItemsControl : ItemsControl
    {
        private object[] _previousSortQueryResult;

        private double _itemsPresenterWidth;
        private double _itemsPresenterHeight;

        public static readonly StyledProperty<int> FixedItemCountProperty =
            PerspexProperty.Register<DockItemsControl, int>("FixedItemCount");

        public static readonly StyledProperty<IItemsOrganizer> ItemsOrganizerProperty =
            PerspexProperty.Register<DockItemsControl, IItemsOrganizer>("ItemsOrganizer");

        public static readonly StyledProperty<PositionMonitor> PositionMonitorProperty =
            PerspexProperty.Register<DockItemsControl, PositionMonitor>("PositionMonitor");

        public static readonly DirectProperty<DockItemsControl, double> ItemsPresenterWidthProperty =
            PerspexProperty.RegisterDirect<DockItemsControl, double>("ItemsPresenterWidth",
                o => o.ItemsPresenterWidth);

        public static readonly DirectProperty<DockItemsControl, double> ItemsPresenterHeightProperty =
            PerspexProperty.RegisterDirect<DockItemsControl, double>("ItemsPresenterHeight",
                o => o.ItemsPresenterHeight);

        static DockItemsControl()
        {
            DockItem.XChangedEvent.AddClassHandler<DockItemsControl>(x => x.ItemXChanged);
            DockItem.XChangedEvent.AddClassHandler<DockItemsControl>(x => x.ItemYChanged);
            DockItem.DragStartedEvent.AddClassHandler<DockItemsControl>(x => x.ItemDragStarted);
            DockItem.PreviewDragDeltaEvent.AddClassHandler<DockItemsControl>(x => x.ItemPreviewDragDelta);
            DockItem.DragCompletedEvent.AddClassHandler<DockItemsControl>(x => x.ItemDragCompleted);
            DockItem.MouseDownWithinEvent.AddClassHandler<DockItemsControl>(x => x.ItemMouseDownWithin);
        }

        public DockItemsControl()
        {

        }

        public int FixedItemCount
        {
            get { return GetValue(FixedItemCountProperty); }
            set { SetValue(FixedItemCountProperty, value); }
        }

        public IItemsOrganizer ItemsOrganizer
        {
            get { return GetValue(ItemsOrganizerProperty); }
            set { SetValue(ItemsOrganizerProperty, value); }
        }

        public PositionMonitor PositionMonitor
        {
            get { return GetValue(PositionMonitorProperty); }
            set { SetValue(PositionMonitorProperty, value); }
        }

        public double ItemsPresenterWidth
        {
            get { return _itemsPresenterWidth; }
            private set { SetAndRaise(ItemsPresenterWidthProperty, ref _itemsPresenterWidth, value); }
        }

        public double ItemsPresenterHeight
        {
            get { return _itemsPresenterHeight; }
            private set { SetAndRaise(ItemsPresenterHeightProperty, ref _itemsPresenterHeight, value); }
        }

        internal ContainerCustomizations ContainerCustomisations { get; set; }
        internal Size? LockedMeasure { get; set; }

        private void UpdateMonitor(RoutedEventArgs e)
        {
            if (PositionMonitor == null) return;

            var dockItem = e.Source;

            var linearPositionMonitor = PositionMonitor as StackPositionMonitor;
            if (linearPositionMonitor == null) return;

            var sortedItems = linearPositionMonitor.Sort(this.ItemContainerGenerator.Containers.OfType<DockItemsControl>().SelectMany(x => x.DockItems())).Select(di => di.Content).ToArray();
            if (_previousSortQueryResult == null || !_previousSortQueryResult.SequenceEqual(sortedItems))
                linearPositionMonitor.OnOrderChanged(new OrderChangedEventArgs(_previousSortQueryResult, sortedItems));

            _previousSortQueryResult = sortedItems;
        }

        private void ItemXChanged(RoutedEventArgs e)
        {

        }

        private void ItemYChanged(RoutedEventArgs e)
        {

        }

        private void ItemDragStarted(DockDragStartedEventArgs e)
        {

        }

        private void ItemPreviewDragDelta(DockDragDeltaEventArgs e)
        {

        }

        private void ItemDragCompleted(DockDragCompletedEventArgs e)
        {

        }

        private void ItemMouseDownWithin(DockItemEventArgs e)
        {

        }

        internal IEnumerable<DockItem> DockItems()
        {
            throw new NotImplementedException();
        }

        internal void MoveItem(MoveItemRequest moveItemRequest)
        {
            throw new NotImplementedException();
        }
    }
}
