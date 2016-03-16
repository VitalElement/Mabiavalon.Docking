using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex.Input;
using Perspex.Interactivity;

namespace Mabiavalon
{
    public delegate void DockDragDeltaEventHandler(object sender, DockDragDeltaEventArgs e);

    public class DockDragDeltaEventArgs : DockItemEventArgs
    {
        private readonly VectorEventArgs _dragDeltaEventArgs;

        public DockDragDeltaEventArgs(DockItem dockItem, VectorEventArgs dragDeltaEventArgs)
            : base(dockItem)
        {
            if (dragDeltaEventArgs == null) throw new ArgumentNullException("dragDeltaEventArgs");

            _dragDeltaEventArgs = dragDeltaEventArgs;
        }

        public DockDragDeltaEventArgs(RoutedEvent routedEvent, DockItem dockItem, VectorEventArgs dragDeltaEventArgs)
            : base(routedEvent, dockItem)
        {
            if (dragDeltaEventArgs == null) throw new ArgumentNullException("dragDeltaEventArgs");

            _dragDeltaEventArgs = dragDeltaEventArgs;
        }

        public DockDragDeltaEventArgs(RoutedEvent routedEvent, IInteractive source, DockItem dockItem, VectorEventArgs dragDeltaEventArgs)
            : base(routedEvent, source, dockItem)
        {
            if (dragDeltaEventArgs == null) throw new ArgumentNullException("dragDeltaEventArgs");

            _dragDeltaEventArgs = dragDeltaEventArgs;
        }

        public VectorEventArgs DragDeltaEventArgs
        {
            get { return _dragDeltaEventArgs; }
        }

        public bool Cancel { get; set; }
    }
}
