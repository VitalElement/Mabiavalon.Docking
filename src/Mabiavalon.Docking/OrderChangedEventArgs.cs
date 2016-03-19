﻿using System;

namespace Mabiavalon
{
    public class OrderChangedEventArgs : EventArgs
    {
        private readonly object[] _previousOrder;
        private readonly object[] _newOrder;

        public OrderChangedEventArgs(object[] previousOrder, object[] newOrder)
        {
            if (newOrder == null) throw new ArgumentNullException(nameof(newOrder));

            _previousOrder = previousOrder;
            _newOrder = newOrder;
        }

        public object[] PreviousOrder
        {
            get { return _previousOrder; }
        }

        public object[] NewOrder
        {
            get { return _newOrder; }
        }
    }
}
