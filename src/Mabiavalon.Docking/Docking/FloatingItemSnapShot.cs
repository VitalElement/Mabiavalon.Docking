using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex;
using Perspex.Controls;
using Perspex.Layout;

namespace Mabiavalon.Docking
{
    /// <summary>
    /// Experimentational. Might have to push this back to mvvm only
    /// </summary>    
    internal class FloatingItemSnapShot
    {
        private readonly object _content;
        private readonly Rect _location;
        private readonly int _zIndex;
        private readonly WindowState _state;

        public FloatingItemSnapShot(object content, Rect location, int zIndex, WindowState state)
        {
            if (content == null) throw new ArgumentNullException("content");

            _content = content;
            _location = location;
            _zIndex = zIndex;
            _state = state;
        }

        public static FloatingItemSnapShot Take(DockItem dockItem)
        {
            if (dockItem == null) throw new ArgumentNullException("dockItem");

            return new FloatingItemSnapShot(
                dockItem.Content,
                new Rect(dockItem.X, dockItem.Y, dockItem.Width, dockItem.Height),
                dockItem.GetValue(Visual.ZIndexProperty),
                Layout.GetFloatingItemState(dockItem));
        }

        public void Apply(DockItem dockItem)
        {
            if (dockItem == null) throw new ArgumentNullException("dockItem");

            dockItem.SetValue(DockItem.XProperty, Location.X);
            dockItem.SetValue(DockItem.YProperty, Location.Y);
            dockItem.SetValue(Layoutable.WidthProperty, Location.Width);
            dockItem.SetValue(Layoutable.HeightProperty, Location.Height);
            Layout.SetFloatingItemState(dockItem, State);
            dockItem.SetValue(Visual.ZIndexProperty, ZIndex);
        }

        public object Content
        {
            get { return _content; }
        }

        public Rect Location
        {
            get { return _location; }
        }

        public int ZIndex
        {
            get { return _zIndex; }
        }

        public WindowState State
        {
            get { return _state; }
        }
    }
}
