using System;

namespace Mabiavalon.Docking
{
    public class LocationReportBuilder
    {
        private readonly DockControl _targetDockControl;
        private Branch _branch;
        private bool _isSecondLeaf;
        private Layout _layout;

        public LocationReportBuilder(DockControl targetDockControl)
        {
            _targetDockControl = targetDockControl;
        }

        public DockControl TargetDockControl
        {
            get { return _targetDockControl; }
        }

        public bool IsFound { get; private set; }

        public void MarkFound()
        {
            if (IsFound)
                throw new InvalidOperationException("Already found.");

            IsFound = true;

            _layout = CurrentLayout;
        }

        public void MarkFound(Branch branch, bool isSecondLeaf)
        {
            if (branch == null) throw new ArgumentNullException(nameof(branch));
            if (IsFound)
                throw new InvalidOperationException("Already found.");

            IsFound = true;

            _layout = CurrentLayout;
            _branch = branch;
            _isSecondLeaf = isSecondLeaf;
        }

        public Layout CurrentLayout { get; set; }

        public LocationReport ToLocationReport()
        {
            return new LocationReport(_targetDockControl, _layout, _branch, _isSecondLeaf);
        }
    }
}
