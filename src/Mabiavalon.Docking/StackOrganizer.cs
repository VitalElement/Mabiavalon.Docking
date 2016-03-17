using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mabiavalon.Core;
using Perspex;
using Perspex.Animation;
using Perspex.Controls;

namespace Mabiavalon
{
    public class StackOrganizer : IItemsOrganizer
    {
        private readonly Orientation _orientation;
        private readonly double _itemOffset;
        private readonly Func<DockItem, double> _getDesiredSize;
        private readonly Func<DockItem, double> _getLocation;
        private readonly PerspexProperty _canvasPerspexProperty;
        private readonly Action<DockItem, double> _setLocation;

        private readonly Dictionary<DockItem, double> _activeAnimationTargetLocations =
            new Dictionary<DockItem, double>();
        private IDictionary<DockItem, LocationInfo> _siblingItemLocationOnDragStart;


        protected StackOrganizer(Orientation orientation, double itemOffset = 0)
        {
            _orientation = orientation;
            _itemOffset = itemOffset;

            switch (orientation)
            {
                case Orientation.Horizontal:
                    _getDesiredSize = item => item.DesiredSize.Width;
                    _getLocation = item => item.X;
                    _setLocation = (item, coord) => item.SetValue(DockItem.XProperty, coord);
                    _canvasPerspexProperty = Canvas.LeftProperty;
                    break;
                case Orientation.Vertical:
                    _getDesiredSize = item => item.DesiredSize.Height;
                    _getLocation = item => item.Y;
                    _setLocation = (item, coord) => item.SetValue(DockItem.YProperty, coord);
                    _canvasPerspexProperty = Canvas.TopProperty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation");
            }
        }

        private class LocationInfo
        {
            private readonly DockItem _item;
            private readonly double _start;
            private readonly double _mid;
            private readonly double _end;

            public LocationInfo(DockItem item, double start, double mid, double end)
            {
                _item = item;
                _start = start;
                _mid = mid;
                _end = end;
            }

            public double Start
            {
                get { return _start; }
            }

            public double Mid
            {
                get { return _mid; }
            }

            public double End
            {
                get { return _end; }
            }

            public DockItem Item
            {
                get { return _item; }
            }
        }

        public virtual Orientation Orientation
        {
            get { return _orientation; }
        }

        public void Organise(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            OrganiseInternal(
                requestor,
                measureBounds,
                items.Select((di, idx) => new Tuple<int, DockItem>(idx, di))
                        .OrderBy(tuple => tuple,
                            MultiComparer<Tuple<int, DockItem>>.Ascending(tuple => _getLocation(tuple.Item2))
                                .ThenAscending(tuple => tuple.Item1))
                        .Select(tuple => tuple.Item2));
        }

        public void Organise(DockItemsControl requestor, Size measureBounds, IOrderedEnumerable<DockItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            OrganiseInternal(
                requestor,
                measureBounds,
                items);
        }

        public void OrganiseOnMouseDownWithin(DockItemsControl requestor, Size measureBounds, List<DockItem> siblingItems, DockItem dockItem)
        {
            
        }

        public void OrganiseOnDragStarted(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            if (dockItem == null) throw new ArgumentNullException("dockItem");

            _siblingItemLocationOnDragStart = siblingItems.Select(GetLocationInfo).ToDictionary(loc => loc.Item);
        }

        public void OrganiseOnDrag(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            if (dockItem == null) throw new ArgumentNullException("dockItem");

            var currentLocations = siblingItems
                .Select(GetLocationInfo)
                .Union(new[] { GetLocationInfo(dockItem) })
                .OrderBy(loc => loc.Item == dockItem ? loc.Start : _siblingItemLocationOnDragStart[loc.Item].Start);

            var currentCoord = 0.0;
            var zIndex = int.MaxValue;
            foreach (var location in currentLocations)
            {
                if (!Equals(location.Item, dockItem))
                {
                    SendToLocation(location.Item, currentCoord);
                    location.Item.SetValue(Visual.ZIndexProperty, --zIndex);
                }
                currentCoord += _getDesiredSize(location.Item) + _itemOffset;
            }
            dockItem.SetValue(Visual.ZIndexProperty, int.MaxValue);
        }

        public void OrganiseOnDragCompleted(DockItemsControl requestor, Size measureBounds, IEnumerable<DockItem> siblingItems, DockItem dockItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            var currentLocations = siblingItems
                .Select(GetLocationInfo)
                .Union(new[] { GetLocationInfo(dockItem) })
                .OrderBy(loc => loc.Item == dockItem ? loc.Start : _siblingItemLocationOnDragStart[loc.Item].Start);

            var currentCoord = 0.0;
            var z = int.MaxValue;
            var logicalIndex = 0;
            foreach (var location in currentLocations)
            {
                SetLocation(location.Item, currentCoord);
                currentCoord += _getDesiredSize(location.Item) + _itemOffset;
                location.Item.SetValue(Visual.ZIndexProperty, --z);
                location.Item.LogicalIndex = logicalIndex++;
            }
            dockItem.SetValue(Visual.ZIndexProperty, int.MaxValue);
        }

        public Point ConstrainLocation(DockItemsControl requestor, Size measureBounds, Point itemCurrentLocation, Size itemCurrentSize,
            Point itemDesiredLocation, Size itemDesiredSize)
        {
            var fixedItems = requestor.FixedItemCount;
            var lowerBound = fixedItems == 0
                ? -1d
                : GetLocationInfo(requestor.DockItems()
                    .Take(fixedItems)
                    .Last()).End + _itemOffset - 1;

            return new Point(
                _orientation == Orientation.Vertical
                    ? 0
                    : Math.Min(Math.Max(lowerBound, itemDesiredLocation.X), (measureBounds.Width) + 1),
                _orientation == Orientation.Horizontal
                    ? 0
                    : Math.Min(Math.Max(lowerBound, itemDesiredLocation.Y), (measureBounds.Height) + 1)
                );
        }

        public Size Measure(DockItemsControl requestor, Size availableSize, IEnumerable<DockItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

            double width = 0, height = 0;
            var isFirst = true;
            foreach (var dockItem in items)
            {
                dockItem.Measure(size);
                if (_orientation == Orientation.Horizontal)
                {
                    width += dockItem.Width;
                    if (!isFirst)
                        width += _itemOffset;
                    height = Math.Max(height, dockItem.Height);
                }
                else
                {
                    width = Math.Max(width, dockItem.Width);
                    height += dockItem.Height;
                    if (!isFirst)
                        height += _itemOffset;
                }

                isFirst = false;
            }

            return new Size(Math.Max(width, 0), Math.Max(height, 0));
        }

        public IEnumerable<DockItem> Sort(IEnumerable<DockItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return items.OrderBy(i => GetLocationInfo(i).Start);
        }

        private void OrganiseInternal(DockItemsControl requestor, Size measureBounds,
            IEnumerable<DockItem> items)
        {
            var currentCoord = 0.0;
            var z = int.MaxValue;
            var logicalIndex = 0;
            foreach (var newItem in items)
            {
                newItem.SetValue(Visual.ZIndexProperty, newItem.IsSelected ? int.MaxValue : --z);
                SetLocation(newItem, currentCoord);
                newItem.LogicalIndex = logicalIndex++;
                newItem.Measure(measureBounds);
                currentCoord += _getDesiredSize(newItem) + _itemOffset;
            }
        }

        private void SendToLocation(DockItem dockItem, double location)
        {
            double activeTarget;
            if (Math.Abs(_getLocation(dockItem) - location) < 1.0
                ||
                _activeAnimationTargetLocations.TryGetValue(dockItem, out activeTarget)
                && Math.Abs(activeTarget - location) < 1.0)
            {
                return;
            }

            _activeAnimationTargetLocations[dockItem] = location;

            var animation = Animate.Property(dockItem, _canvasPerspexProperty, dockItem.GetValue(_canvasPerspexProperty),
                location, LinearEasing.For<double>(), TimeSpan.FromMilliseconds(200));

            animation.Subscribe(_ => { }, () =>
            {
                _setLocation(dockItem, location);
                _activeAnimationTargetLocations.Remove(dockItem);
            });

            
        }

        private void SetLocation(DockItem dockItem, double location)
        {
            _setLocation(dockItem, location);
        }

        private LocationInfo GetLocationInfo(DockItem item)
        {
            var size = _getDesiredSize(item);
            double startLocation;
            if (!_activeAnimationTargetLocations.TryGetValue(item, out startLocation))
                startLocation = _getLocation(item);
            var midLocation = startLocation + size / 2;
            var endLocation = startLocation + size;

            return new LocationInfo(item, startLocation, midLocation, endLocation);
        }
    }
}
