using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mabiavalon.Docking
{
    internal class FloatTransfer
    {
        private readonly double _width;
        private readonly double _height;
        private readonly object _content;

        public FloatTransfer(double width, double height, object content)
        {
            if (content == null) throw new ArgumentNullException("content");

            _width = width;
            _height = height;
            _content = content;
        }

        public static FloatTransfer TakeSnapshot(DockItem dockItem, DockControl sourceDockControl)
        {
            if (dockItem == null) throw new ArgumentNullException("dockItem");

            return new FloatTransfer(sourceDockControl.Width, sourceDockControl.Height, dockItem.UnderlyingContent ?? dockItem.Content ?? dockItem);
        }

        [Obsolete]
        //TODO width and height transfer obsolete
        public double Width
        {
            get { return _width; }
        }

        [Obsolete]
        //TODO width and height transfer obsolete
        public double Height
        {
            get { return _height; }
        }

        public object Content
        {
            get { return _content; }
        }
    }
}
