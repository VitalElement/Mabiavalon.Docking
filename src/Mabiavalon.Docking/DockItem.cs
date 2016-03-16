using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Perspex.Controls;

namespace Mabiavalon
{
    public class DockItem : ContentControl
    {
        public const string ThumbPartName = "PART_Thumb";

        private readonly SerialDisposable _templateSubscriptions = new SerialDisposable();
        private readonly SerialDisposable _rightMouseUpCleanUpDisposable = new SerialDisposable();
    }
}
