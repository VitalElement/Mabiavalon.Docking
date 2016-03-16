using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex.Input;
using Perspex.Interactivity;

namespace Mabiavalon
{
    public delegate void DockDragCompletedEventHandler(object sender, DockDragCompletedEventArgs e);

    public class DockDragCompletedEventArgs : RoutedEventArgs
    {
        private readonly DockItem _dockItem;
        private readonly bool _isDropTargetFound;
        private readonly VectorEventArgs _dragCompletedEventArgs;

        public DockDragCompletedEventArgs(DockItem dockItem, VectorEventArgs dragCompletedEventArgs)
        {
            if (dockItem == null) throw new ArgumentNullException("dockItem");
            if (dragCompletedEventArgs == null) throw new ArgumentNullException("dragCompletedEventArgs");

            _dockItem = dockItem;
            _dragCompletedEventArgs = dragCompletedEventArgs;
        }

        public DockDragCompletedEventArgs(RoutedEvent routedEvent, DockItem dockItem, VectorEventArgs dragCompletedEventArgs)
            : base(routedEvent)
        {
            if (dockItem == null) throw new ArgumentNullException("dockItem");
            if (dragCompletedEventArgs == null) throw new ArgumentNullException("dragCompletedEventArgs");

            _dockItem = dockItem;
            _dragCompletedEventArgs = dragCompletedEventArgs;
        }

        public DockDragCompletedEventArgs(RoutedEvent routedEvent, IInteractive source, DockItem dockItem, VectorEventArgs dragCompletedEventArgs)
            : base(routedEvent, source)
        {
            if (dockItem == null) throw new ArgumentNullException("dockItem");
            if (dragCompletedEventArgs == null) throw new ArgumentNullException("dragCompletedEventArgs");

            _dockItem = dockItem;
            _dragCompletedEventArgs = dragCompletedEventArgs;
        }

        public DockItem DockItem
        {
            get { return _dockItem; }
        }

        public VectorEventArgs DragCompletedEventArgs
        {
            get { return _dragCompletedEventArgs; }
        }
    }
}
