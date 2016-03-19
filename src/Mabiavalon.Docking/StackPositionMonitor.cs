using Perspex.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mabiavalon
{
    /// <summary>
    /// A linear position monitor simplifies the montoring of the order of items, where they are laid out
    /// horizontally or vertically (typically via a <see cref="StackOrganizer"/>.
    /// </summary>
    public abstract class StackPositionMonitor : PositionMonitor
    {
        private readonly Func<DockItem, double> _getLocation;

        protected StackPositionMonitor(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Horizontal:
                    _getLocation = item => item.X;
                    break;
                case Orientation.Vertical:
                    _getLocation = item => item.Y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation));
            }
        }

        public event EventHandler<OrderChangedEventArgs> OrderChanged;

        internal virtual void OnOrderChanged(OrderChangedEventArgs e)
        {
            var handler = OrderChanged;
            if (handler != null) handler(this, e);
        }

        internal IEnumerable<DockItem> Sort(IEnumerable<DockItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            return items.OrderBy(i => _getLocation(i));
        }
    }
}
