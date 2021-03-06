﻿using Perspex.Controls;
using System;

namespace Mabiavalon
{
    public class NewTabHost<TElement> : INewTabHost<TElement> where TElement : Control
    {
        private readonly TElement _container;
        private readonly DockControl _dockControl;

        public NewTabHost(TElement container, DockControl dockControl)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (dockControl == null) throw new ArgumentNullException(nameof(dockControl));

            _container = container;
            _dockControl = dockControl;
        }

        public TElement Container
        {
            get { return _container; }
        }

        public DockControl DockControl
        {
            get { return _dockControl; }
        }
    }
}
