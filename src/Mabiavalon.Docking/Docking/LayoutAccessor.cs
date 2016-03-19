using System;
using System.Collections.Generic;

namespace Mabiavalon.Docking
{
    /// <summary>
    /// Provides information about the <see cref="Layout"/> instance.
    /// </summary>
    public class LayoutAccessor
    {
        private readonly Layout _layout;
        private readonly BranchAccessor _branchAccessor;
        private readonly DockControl _dockControl;

        public LayoutAccessor(Layout layout)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            _layout = layout;

            var branch = Layout.Content as Branch;
            if (branch != null)
                _branchAccessor = new BranchAccessor(branch);
            else
                _dockControl = Layout.Content as DockControl;
        }

        public Layout Layout
        {
            get { return _layout; }
        }

        public IEnumerable<DockItem> FloatingItems
        {
            get { return _layout.FloatingDockItems(); }
        }

        /// <summary>
        /// <see cref="BranchAccessor"/> and <see cref="DockControl"/> are mutually exclusive, according to whether the layout has been split, or just contains a tab control.
        /// </summary>
        public BranchAccessor BranchAccessor
        {
            get { return _branchAccessor; }
        }

        /// <summary>
        /// <see cref="BranchAccessor"/> and <see cref="DockControl"/> are mutually exclusive, according to whether the layout has been split, or just contains a tab control.
        /// </summary>
        public DockControl DockControl
        {
            get { return _dockControl; }
        }

        /// <summary>
        /// Visits the content of the layout, according to its content type.  No more than one of the provided <see cref="Action"/>
        /// callbacks will be called.  
        /// </summary>        
        public LayoutAccessor Visit(
            Action<BranchAccessor> branchVisitor = null,
            Action<DockControl> dockControlVisitor = null,
            Action<object> contentVisitor = null)
        {
            if (_branchAccessor != null)
            {
                if (branchVisitor != null)
                    branchVisitor(_branchAccessor);

                return this;
            }

            if (_dockControl != null)
            {
                if (dockControlVisitor != null)
                    dockControlVisitor(_dockControl);

                return this;
            }

            if (_layout.Content != null && contentVisitor != null)
                contentVisitor(_layout.Content);

            return this;
        }
    }
}
