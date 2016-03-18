using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mabiavalon.Docking
{
    public class LocationReport
    {
        private readonly DockControl _dockControl;
        private readonly Layout _rootLayout;
        private readonly Branch _parentBranch;
        private readonly bool _isLeaf;
        private readonly bool _isSecondLeaf;

        internal LocationReport(DockControl dockControl, Layout rootLayout)
            : this(dockControl, rootLayout, null, false)
        { }

        internal LocationReport(DockControl dockControl, Layout rootLayout, Branch parentBranch, bool isSecondLeaf)
        {
            if (dockControl == null) throw new ArgumentNullException("dockControl");
            if (rootLayout == null) throw new ArgumentNullException("rootLayout");

            _dockControl = dockControl;
            _rootLayout = rootLayout;
            _parentBranch = parentBranch;
            _isLeaf = _parentBranch != null;
            _isSecondLeaf = isSecondLeaf;
        }


        public DockControl DockControl
        {
            get { return _dockControl; }
        }

        public Layout RootLayout
        {
            get { return _rootLayout; }
        }

        /// <summary>
        /// Gets the parent branch if this is a leaf. If the <see cref="DockControl"/> is directly under the <see cref="RootLayout"/> will be <c>null</c>.
        /// </summary>
        public Branch ParentBranch
        {
            get { return _parentBranch; }
        }

        /// <summary>
        /// Idicates if this is a leaf in a branch. <c>True</c> if <see cref="ParentBranch"/> is not null.
        /// </summary>
        public bool IsLeaf
        {
            get { return _isLeaf; }
        }

        public bool IsSecondLeaf
        {
            get { return _isSecondLeaf; }
        }
    }
}
