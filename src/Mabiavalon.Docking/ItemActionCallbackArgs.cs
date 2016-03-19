using Perspex.Controls;
using System;

namespace Mabiavalon
{
    public delegate void ItemActionCallback(ItemActionCallbackArgs<DockControl> args);

    public class ItemActionCallbackArgs<TOwner> where TOwner : Control
    {
        private readonly Window _window;
        private readonly TOwner _owner;
        private readonly DockItem _dockItem;

        public ItemActionCallbackArgs(Window window, TOwner owner, DockItem dockItem)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (dockItem == null) throw new ArgumentNullException(nameof(dockItem));

            _window = window;
            _owner = owner;
            _dockItem = dockItem;
        }

        public Window Window
        {
            get { return _window; }
        }

        public TOwner Owner
        {
            get { return _owner; }
        }

        public DockItem DockItem
        {
            get { return _dockItem; }
        }

        public bool IsCancelled { get; private set; }

        public void Cancel()
        {
            IsCancelled = true;
        }
    }
}
