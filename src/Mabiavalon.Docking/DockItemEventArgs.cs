﻿using Perspex.Interactivity;
using System;

namespace Mabiavalon
{
    public delegate void DockItemEventHandler(object sender, DockItemEventArgs e);

    public class DockItemEventArgs : RoutedEventArgs
    {
        private readonly DockItem _dockItem;

        public DockItemEventArgs(DockItem dockItem)
        {
            if (dockItem == null) throw new ArgumentNullException(nameof(dockItem));

            _dockItem = dockItem;
        }

        public DockItemEventArgs(RoutedEvent routedEvent, DockItem dockItem)
            : base(routedEvent)
        {
            _dockItem = dockItem;
        }

        public DockItemEventArgs(RoutedEvent routedEvent, IInteractive source, DockItem dockItem)
            : base(routedEvent, source)
        {
            _dockItem = dockItem;
        }

        public DockItem DockItem
        {
            get { return _dockItem; }
        }
    }
}