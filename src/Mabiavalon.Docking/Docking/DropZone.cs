using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex;
using Perspex.Controls.Primitives;

namespace Mabiavalon.Docking
{
    public class DropZone : TemplatedControl
    {
        private DropZoneLocation _location;
        private bool _isOffered;

        public static readonly DirectProperty<DropZone, DropZoneLocation> LocationProperty =
            PerspexProperty.RegisterDirect<DropZone, DropZoneLocation>("Location", o => o.Location,
                (o, v) => o.Location = v);

        public static readonly DirectProperty<DropZone, bool> IsOfferedProperty =
            PerspexProperty.RegisterDirect<DropZone, bool>("IsOffered", o => o.IsOffered);

        static DropZone()
        {
            // Style
        }

        public DropZoneLocation Location
        {
            get { return _location; }
            set { SetAndRaise(LocationProperty, ref _location, value); }
        }

        public bool IsOffered
        {
            get { return _isOffered; }
            internal set { SetAndRaise(IsOfferedProperty, ref _isOffered, value); }
        }
    }
}
