using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex.Interactivity;

namespace Mabiavalon.Docking
{
    public delegate void FloatRequestedEventHandler(object sender, FloatRequestedEventArgs e);

    public class FloatRequestedEventArgs : DockItemEventArgs
    {
        private readonly DockItem _dockItem;

        public FloatRequestedEventArgs(RoutedEvent routedEvent, IInteractive source, DockItem dockItem)
            : base(routedEvent, source, dockItem)
        {
            _dockItem = dockItem;
        }

        public FloatRequestedEventArgs(RoutedEvent routedEvent, DockItem dockItem)
            : base(routedEvent, dockItem)
        { }
    }
}
