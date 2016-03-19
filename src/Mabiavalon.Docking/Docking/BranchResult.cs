using System;

namespace Mabiavalon.Docking
{
    public class BranchResult
    {
        private readonly Branch _branch;
        private readonly DockControl _dockControl;

        public BranchResult(Branch branch, DockControl dockControl)
        {
            if (branch == null) throw new ArgumentNullException(nameof(branch));
            if (dockControl == null) throw new ArgumentNullException(nameof(dockControl));

            _branch = branch;
            _dockControl = dockControl;
        }

        public Branch Branch
        {
            get { return _branch; }
        }

        public DockControl DockControl
        {
            get { return _dockControl; }
        }
    }
}
