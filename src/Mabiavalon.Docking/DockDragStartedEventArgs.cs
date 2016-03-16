using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex.Input;
using Perspex.Interactivity;

namespace Mabiavalon
{
    public delegate void DockDragStartedEventHandler(object sender, DockDragStartedEventArgs e);

    public class DockDragStartedEventArgs : DockItemEventArgs
    {
        private readonly VectorEventArgs _dragStartedEventArgs;

        public DockDragStartedEventArgs(DockItem dockItem, VectorEventArgs dragStartedEventArgs)
            : base(dockItem)
        {
            if (dragStartedEventArgs == null) throw new ArgumentNullException("dragStartedEventArgs");

            _dragStartedEventArgs = dragStartedEventArgs;
        }

        public DockDragStartedEventArgs(RoutedEvent routedEvent, DockItem dockItem, VectorEventArgs dragStartedEventArgs)
            : base(routedEvent, dockItem)
        {
            _dragStartedEventArgs = dragStartedEventArgs;
        }

        public DockDragStartedEventArgs(RoutedEvent routedEvent, IInteractive source, DockItem dockItem, VectorEventArgs dragStartedEventArgs)
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
