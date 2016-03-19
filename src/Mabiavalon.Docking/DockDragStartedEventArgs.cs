using Perspex.Input;
using Perspex.Interactivity;
using System;

namespace Mabiavalon
{
    public delegate void DockDragStartedEventHandler(object sender, DockDragStartedEventArgs e);

    public class DockDragStartedEventArgs : DockItemEventArgs
    {
        private readonly VectorEventArgs _dragStartedEventArgs;

        public DockDragStartedEventArgs(DockItem dockItem, VectorEventArgs dragStartedEventArgs)
            : base(dockItem)
        {
            if (dragStartedEventArgs == null) throw new ArgumentNullException(nameof(dragStartedEventArgs));

            _dragStartedEventArgs = dragStartedEventArgs;
        }

        public DockDragStartedEventArgs(RoutedEvent routedEvent, DockItem dockItem, VectorEventArgs dragStartedEventArgs)
            : base(routedEvent, dockItem)
        {
            _dragStartedEventArgs = dragStartedEventArgs;
        }

        public DockDragStartedEventArgs(RoutedEvent routedEvent, IInteractive source, DockItem dockItem,
            VectorEventArgs dragStartedEventArgs)
            : base(routedEvent, source, dockItem)
        {
            _dragStartedEventArgs = dragStartedEventArgs;
        }

        public VectorEventArgs DragStartedEventArgs
        {
            get { return _dragStartedEventArgs; }
        }
    }
}