using Perspex.Controls;
using Perspex.VisualTree;
using System;
using System.Linq;

namespace Mabiavalon.Docking
{
    public class BranchAccessor
    {
        private readonly Branch _branch;
        private readonly BranchAccessor _firstBranchAccessor;
        private readonly BranchAccessor _secondBranchAccessor;
        private readonly DockControl _firstDockControl;
        private readonly DockControl _secondDockControl;

        public BranchAccessor(Branch branch)
        {
            if (branch == null) throw new ArgumentNullException(nameof(branch));

            _branch = branch;

            var firstChildBranch = branch.FirstItem as Branch;
            if (firstChildBranch != null)
                _firstBranchAccessor = new BranchAccessor(firstChildBranch);
            else
                _firstDockControl = FindDockControl(branch.FirstItem, branch.FirstContentPresenter);

            var secondChildBranch = branch.SecondItem as Branch;
            if (secondChildBranch != null)
                _secondBranchAccessor = new BranchAccessor(secondChildBranch);
            else
                _secondDockControl = FindDockControl(branch.SecondItem, branch.SecondContentPresenter);

        }

        public Branch Branch
        {
            get { return _branch; }
        }

        public BranchAccessor FirstBranchAccessor
        {
            get { return _firstBranchAccessor; }
        }

        public BranchAccessor SecondBranchAccessor
        {
            get { return _secondBranchAccessor; }
        }

        public DockControl FirstDockControl
        {
            get { return _firstDockControl; }
        }

        public DockControl SecondDockControl
        {
            get { return _secondDockControl; }
        }

        private static DockControl FindDockControl(object item, Control contentPresenter)
        {
            var result = item as DockControl;
            return result ??
                   contentPresenter.GetSelfAndVisualDescendents()
                       .OfType<DockControl>()
                       .FirstOrDefault();
        }

        public BranchAccessor Visit(BranchItem childItem, Action<BranchAccessor> childBranchVisitor = null,
            Action<DockControl> childDockControlVisitor = null, Action<object> childContentVisitor = null)
        {
            Func<BranchAccessor> branchGetter;
            Func<DockControl> tabGetter;
            Func<object> contentGetter;

            switch (childItem)
            {
                case BranchItem.First:
                    branchGetter = () => _firstBranchAccessor;
                    tabGetter = () => _firstDockControl;
                    contentGetter = () => _branch.FirstItem;
                    break;
                case BranchItem.Second:
                    branchGetter = () => _secondBranchAccessor;
                    tabGetter = () => _secondDockControl;
                    contentGetter = () => _branch.SecondItem;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(childItem));
            }

            var branchDescription = branchGetter();
            if (branchDescription != null)
            {
                if (childBranchVisitor != null)
                    childBranchVisitor(branchDescription);
                return this;
            }

            var dockControl = tabGetter();
            if (dockControl != null)
            {
                if (childDockControlVisitor != null)
                    childDockControlVisitor(dockControl);
                return this;
            }

            if (childContentVisitor == null) return this;

            var content = contentGetter();
            if (content != null)
                childContentVisitor(content);

            return this;
        }
    }
}
