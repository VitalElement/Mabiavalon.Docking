using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex;
using Perspex.Controls;

namespace Mabiavalon
{
    public class DockItemsControl : ItemsControl
    {
        private int _fixedItemCount;

        public static readonly DirectProperty<DockItemsControl, int> FixedItemCountProperty =
        PerspexProperty.RegisterDirect<DockItemsControl, int>("FixedItemCount", o => o.FixedItemCount,
            (o, v) => o.FixedItemCount = v);

        static DockItemsControl()
        {
            
        }
        
        public DockItemsControl()
        {
            
        }

        public int FixedItemCount
        {
            get { return _fixedItemCount; }
            set { _fixedItemCount = value; }
        }

        internal IEnumerable<DockItem> DockItems()
        {
            throw new NotImplementedException();
        }
    }
}
