using Perspex;
using Perspex.Collections;
using Perspex.Controls;
using Perspex.Controls.Presenters;
using Perspex.Controls.Primitives;
using Perspex.Input;
using Perspex.LogicalTree;
using Perspex.Markup.Xaml.Templates;
using Perspex.Styling;
using Perspex.Threading;
using Perspex.VisualTree;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Mabiavalon.Docking
{
    public delegate void ClosingFloatingItemCallback(ItemActionCallbackArgs<Layout> args);

    public class Layout : ContentControl
    {
        private static readonly HashSet<Layout> LoadedLayouts = new HashSet<Layout>();

        private const string TopDropZonePartName = "PART_TopDropZone";
        private const string RightDropZonePartName = "PART_RightDropZone";
        private const string BottomDropZonePartName = "PART_BottomDropZone";
        private const string LeftDropZonePartName = "PART_LeftDropZone";
        private const string FloatingDropZonePartName = "PART_FloatDropZone";
        private const string FloatingContentPresenterPartName = "PART_FloatContentPresenter";

        public ReactiveCommand<object> UnfloatItemCommand;
        public ReactiveCommand<object> MaximizeFloatingItem;
        public ReactiveCommand<object> RestoreFloatingItem;
        public ReactiveCommand<object> CloseFloatingItem;
        public ReactiveCommand<object> TileFloatingItemsCommand;
        public ReactiveCommand<object> TileFloatingItemsVerticallyCommand;
        public ReactiveCommand<object> TileFloatingItemsHorizontallyCommand;

        private readonly IDictionary<DropZoneLocation, DropZone> _dropZones =
            new Dictionary<DropZoneLocation, DropZone>();

        private static Tuple<Layout, DropZone> _currentlyOfferedDropZone;

        private readonly DockItemsControl _floatingItems;
        private static bool _isDragOpWireUpPending;
        private FloatTransfer _floatTransfer;

        private ClosingFloatingItemCallback _closingFloatingItemCallback;
        private bool _isParticipatingInDrag;

        public static readonly StyledProperty<IInterLayoutClient> InterLayoutClientProperty =
            PerspexProperty.Register<Layout, IInterLayoutClient>("InterLayoutClient",
                new DefaultInterLayoutClient());

        public static readonly StyledProperty<DataTemplate> BranchTemplateProperty =
            PerspexProperty.Register<Layout, DataTemplate>("BranchTemplate");

        public static readonly StyledProperty<bool> IsFloatDropZoneEnabledProperty =
            PerspexProperty.Register<Layout, bool>("IsFloatDropZoneEnabled");

        public static readonly StyledProperty<Thickness> FloatingItemsContainerMarginProperty =
            PerspexProperty.Register<Layout, Thickness>("FloatingItemsContainerMargin");

        public static readonly StyledProperty<IPerspexList<object>> FloatingItemsProperty =
            PerspexProperty.Register<Layout, IPerspexList<object>>("FloatingItems");

        public static readonly StyledProperty<Style> FloatItemsControlStyleProperty =
            PerspexProperty.Register<Layout, Style>("FloatItemsControlStyle");

        public static readonly StyledProperty<Style> FloatingItemContainerStyleProperty =
            PerspexProperty.Register<Layout, Style>("FloatingItemContainerStyle");

        public static readonly StyledProperty<DataTemplate> FloatingItemTemplateProperty =
            PerspexProperty.Register<Layout, DataTemplate>("FloatingItemTemplate");

        public static readonly StyledProperty<string> FloatingItemHeaderMemberPathProperty =
            PerspexProperty.Register<Layout, string>("FloatingItemHeaderMemberPath");

        public static readonly StyledProperty<string> FloatingItemDisplayMemberPathProperty =
            PerspexProperty.Register<Layout, string>("FloatingItemDisplayMemberPath");

        public static readonly DirectProperty<Layout, ClosingFloatingItemCallback> ClosingFloatingItemCallbackProperty =
            PerspexProperty.RegisterDirect<Layout, ClosingFloatingItemCallback>("ClosingFloatingItemCallback",
                o => o.ClosingFloatingItemCallback,
                (o, v) => o.ClosingFloatingItemCallback = v);

        public static readonly DirectProperty<Layout, bool> IsParticipatingInDragProperty =
            PerspexProperty.RegisterDirect<Layout, bool>("IsParticipatingInDrag", o => o.IsParticipatingInDrag);

        public static readonly AttachedProperty<bool> IsFloatingInLayoutProperty =
            PerspexProperty.RegisterAttached<Layout, Control, bool>("IsFloatingInLayout");

        internal static readonly AttachedProperty<LocationSnapShot> LocationSnapShotProperty =
            PerspexProperty.RegisterAttached<Layout, Control, LocationSnapShot>("LocationSnapShot");

        public static readonly AttachedProperty<WindowState> FloatingItemStateProperty =
            PerspexProperty.RegisterAttached<Layout, Control, WindowState>("FloatingItemState");



        static Layout()
        {
            DockItem.DragStartedEvent.AddClassHandler<Layout>(x => x.ItemDragStarted);
            DockItem.PreviewDragDeltaEvent.AddClassHandler<Layout>(x => x.ItemPreviewDragDelta, handledEventsToo: true);
            DockItem.DragCompletedEvent.AddClassHandler<Layout>(x => x.ItemDragCompleted);
        }

        private void ItemDragStarted(DockDragStartedEventArgs e)
        {
            //we wait until drag is in full flow so we know the partition has been setup by the owning tab control
            _isDragOpWireUpPending = true;
        }

        private void ItemPreviewDragDelta(DockDragDeltaEventArgs e)
        {
            if (e.Cancel) return;

            if (_isDragOpWireUpPending)
            {
                SetupParticipatingLayouts(e.DockItem);
                _isDragOpWireUpPending = false;
            }

            foreach (var layout in LoadedLayouts.Where(l => l.IsParticipatingInDrag))
            {
                var cursorPos = MouseDevice.Instance.Position;
                layout.MonitorDropZones(cursorPos);
            }
        }

        private void ItemDragCompleted(DockDragCompletedEventArgs e)
        {
            _isDragOpWireUpPending = false;

            foreach (var loadedLayout in LoadedLayouts)
                loadedLayout.IsParticipatingInDrag = false;

            if (_currentlyOfferedDropZone == null || e.DockItem.IsDropTargetFound) return;

            DockControl dockControl;
            if (TryGetSourceTabControl(e.DockItem, out dockControl))
            {
                if (((ICollection)dockControl.Items).Count > 1) return;

                if (_currentlyOfferedDropZone.Item2.Location == DropZoneLocation.Floating)
                    Float(_currentlyOfferedDropZone.Item1, e.DockItem);
                else
                    _currentlyOfferedDropZone.Item1.Branch(_currentlyOfferedDropZone.Item2.Location, e.DockItem);
            }

            _currentlyOfferedDropZone = null;
        }

        public Layout()
        {

            this.AttachedToLogicalTree += (sender, args) => LoadedLayouts.Add(this);
            this.DetachedFromLogicalTree += (sender, args) => LoadedLayouts.Remove(this);

            var canExecuteUnfloat = this.WhenAny(x => x.IsHostingTab(), x => x.Value);
            var canExecuateMaximize = this.GetObservable(FloatingItemStateProperty)
                .Select(x => x != WindowState.Maximized);

            UnfloatItemCommand = ReactiveCommand.Create(canExecuteUnfloat);
            MaximizeFloatingItem = ReactiveCommand.Create(canExecuateMaximize);
            RestoreFloatingItem = ReactiveCommand.Create();
            CloseFloatingItem = ReactiveCommand.Create();
            TileFloatingItemsCommand = ReactiveCommand.Create();
            TileFloatingItemsVerticallyCommand = ReactiveCommand.Create();
            TileFloatingItemsHorizontallyCommand = ReactiveCommand.Create();

            UnfloatItemCommand.Subscribe(UnfloatItemExecuted);
            MaximizeFloatingItem.Subscribe(MaximizeFloatingItemExecuted);
            RestoreFloatingItem.Subscribe(RestoreFloatingItemExecuted);
            CloseFloatingItem.Subscribe(CloseFloatingItemExecuted);
            TileFloatingItemsCommand.Subscribe(TileFloatingItemsExecuted);
            TileFloatingItemsHorizontallyCommand.Subscribe(TileFloatingItemsHorizonallyExecuted);
            TileFloatingItemsVerticallyCommand.Subscribe(TileFloatingItemsVerticallyExecuted);

            // quick hack 
            _floatingItems = new DockItemsControl
            {

            };
        }

        public IInterLayoutClient InterLayoutClient
        {
            get { return GetValue(InterLayoutClientProperty); }
            set { SetValue(InterLayoutClientProperty, value); }
        }

        public DataTemplate BranchTemplate
        {
            get { return GetValue(BranchTemplateProperty); }
            set { SetValue(BranchTemplateProperty, value); }
        }

        public bool IsFloatDropZoneEnabled
        {
            get { return GetValue(IsFloatDropZoneEnabledProperty); }
            set { SetValue(IsFloatDropZoneEnabledProperty, value); }
        }

        public Thickness FloatingItemsContainerMargin
        {
            get { return GetValue(FloatingItemsContainerMarginProperty); }
            set { SetValue(FloatingItemsContainerMarginProperty, value); }
        }

        public IPerspexList<object> FloatingItems
        {
            get { return GetValue(FloatingItemsProperty); }
            set { SetValue(FloatingItemsProperty, value); }
        }

        public Style FloatItemsControlStyle
        {
            get { return GetValue(FloatItemsControlStyleProperty); }
            set { SetValue(FloatItemsControlStyleProperty, value); }
        }

        public Style FloatingItemContainerStyle
        {
            get { return GetValue(FloatingItemContainerStyleProperty); }
            set { SetValue(FloatingItemContainerStyleProperty, value); }
        }

        public DataTemplate FloatingItemTemplate
        {
            get { return GetValue(FloatingItemTemplateProperty); }
            set { SetValue(FloatingItemTemplateProperty, value); }
        }

        public string FloatingItemHeaderMemberPath
        {
            get { return GetValue(FloatingItemHeaderMemberPathProperty); }
            set { SetValue(FloatingItemHeaderMemberPathProperty, value); }
        }

        public string FloatingItemDisplayMemberPath
        {
            get { return GetValue(FloatingItemDisplayMemberPathProperty); }
            set { SetValue(FloatingItemDisplayMemberPathProperty, value); }
        }

        public ClosingFloatingItemCallback ClosingFloatingItemCallback
        {
            get { return _closingFloatingItemCallback; }
            set { SetAndRaise(ClosingFloatingItemCallbackProperty, ref _closingFloatingItemCallback, value); }
        }

        public bool IsParticipatingInDrag
        {
            get { return _isParticipatingInDrag; }
            set { SetAndRaise(IsParticipatingInDragProperty, ref _isParticipatingInDrag, value); }
        }

        public string Partition { get; set; }

        public static bool GetIsFloatingInLayout(Control element)
        {
            return element.GetValue(IsFloatingInLayoutProperty);
        }

        private static void SetIsFloatingInLayout(Control element, bool value)
        {
            element.SetValue(IsFloatingInLayoutProperty, value);
        }

        public static WindowState GetFloatingItemState(Control element)
        {
            return element.GetValue(FloatingItemStateProperty);
        }

        public static void SetFloatingItemState(Control element, WindowState value)
        {
            element.SetValue(FloatingItemStateProperty, value);
        }

        internal static LocationSnapShot GetLocationSnapShot(Control element)
        {
            return element.GetValue(LocationSnapShotProperty);
        }

        internal static void SetLocationSnapShot(Control element, LocationSnapShot value)
        {
            element.SetValue(LocationSnapShotProperty, value);
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            var floatingItemsContentPresenter = e.NameScope.Find<ContentPresenter>(FloatingContentPresenterPartName);
            if (floatingItemsContentPresenter != null)
                floatingItemsContentPresenter.Content = _floatingItems;

            _dropZones[DropZoneLocation.Top] = e.NameScope.Find<DropZone>(TopDropZonePartName);
            _dropZones[DropZoneLocation.Right] = e.NameScope.Find<DropZone>(RightDropZonePartName);
            _dropZones[DropZoneLocation.Bottom] = e.NameScope.Find<DropZone>(BottomDropZonePartName);
            _dropZones[DropZoneLocation.Left] = e.NameScope.Find<DropZone>(LeftDropZonePartName);
            _dropZones[DropZoneLocation.Floating] = e.NameScope.Find<DropZone>(FloatingDropZonePartName);

            base.OnTemplateApplied(e);
        }

        private void Branch(DropZoneLocation location, DockItem sourceDockItem)
        {
            if (InterLayoutClient == null)
                throw new InvalidOperationException("InterLayoutClient is not set.");

            var sourceOfdockItemsControl =
                sourceDockItem.GetSelfAndLogicalAncestors().OfType<ItemsControl>().FirstOrDefault() as DockItemsControl;
            if (sourceOfdockItemsControl == null) return;

            var sourceDockControl = DockControl.GetOwnerOfHeaderItems(sourceOfdockItemsControl);
            if (sourceDockControl == null) throw new ApplicationException("Unable to determine source dock control ");

            var floatingItemSnapShots = sourceDockControl.GetVisualChildren().OfType<Layout>()
                .SelectMany(l => l.FloatingDockItems().Select(FloatingItemSnapShot.Take))
                .ToList();

            var sourceItemIndex = sourceOfdockItemsControl.ItemContainerGenerator.IndexFromContainer(sourceDockItem);
            var sourceItem = (DockItem)((IList)sourceDockControl.Items)[sourceItemIndex];
            sourceDockControl.RemoveItem(sourceItem);

            var branchItem = new Branch
            {
                Orientation = (location == DropZoneLocation.Right || location == DropZoneLocation.Left) ? Orientation.Horizontal : Orientation.Vertical,
            };

            object newContent;
            if (BranchTemplate == null)
            {
                var newTabHost = InterLayoutClient.GetNewHost(Partition, sourceDockControl);
                if (newTabHost == null)
                    throw new ApplicationException("InterLayoutClient did not provide a new tab host.");
                newTabHost.DockControl.AddToSource(sourceItem);
                newTabHost.DockControl.SelectedItem = sourceItem;
                newContent = newTabHost.Container;

                Dispatcher.UIThread.InvokeAsync(new Action(() => RestoreFloatingItemSnapShots(newTabHost.DockControl, floatingItemSnapShots)), DispatcherPriority.Loaded);
            }
            else
            {
                newContent = new ContentControl
                {
                    Content = new object(),
                };

                ((ContentControl)newContent).DataTemplates.Add(BranchTemplate);

                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    var newDockControl = ((ContentControl)newContent).GetVisualChildren().OfType<DockControl>().FirstOrDefault();
                    if (newDockControl == null) return;

                    newDockControl.DataContext = sourceDockControl.DataContext;
                    newDockControl.AddToSource(sourceItem);
                    newDockControl.SelectedItem = sourceItem;
                    Dispatcher.UIThread.InvokeAsync(new Action(() => RestoreFloatingItemSnapShots(newDockControl, floatingItemSnapShots)), DispatcherPriority.Loaded);
                }), DispatcherPriority.Loaded);
            }

            if (location == DropZoneLocation.Right || location == DropZoneLocation.Bottom)
            {
                branchItem.FirstItem = Content;
                branchItem.SecondItem = newContent;
            }
            else
            {
                branchItem.FirstItem = newContent;
                branchItem.SecondItem = Content;
            }

            SetValue(ContentProperty, branchItem);
        }

        private void TileFloatingItemsExecuted(object _)
        {
            var dockItems = _floatingItems.DockItems();
            Tiler.Tile(dockItems, new Size(_floatingItems.Height, _floatingItems.Width));
        }

        private void TileFloatingItemsHorizonallyExecuted(object _)
        {
            var dockItems = _floatingItems.DockItems();
            Tiler.TileHorizontally(dockItems, new Size(_floatingItems.Height, _floatingItems.Width));
        }

        private void TileFloatingItemsVerticallyExecuted(object _)
        {
            var dockItems = _floatingItems.DockItems();
            Tiler.TileVertically(dockItems, new Size(_floatingItems.Height, _floatingItems.Width));
        }

        private void CloseFloatingItemExecuted(object _)
        {
            var dockItem = _ as DockItem;
            if (dockItem == null) return;

            var cancel = false;
            if (ClosingFloatingItemCallback != null)
            {
                var window = this.GetVisualAncestors().OfType<Window>().FirstOrDefault();
                if (window == null) throw new ApplicationException("Unable to ascertain window.");

                var callbackArgs = new ItemActionCallbackArgs<Layout>(window, this, dockItem);

                ClosingFloatingItemCallback(callbackArgs);

                cancel = callbackArgs.IsCancelled;
            }

            if (cancel) return;

            var index = _floatingItems.ItemContainerGenerator.IndexFromContainer(dockItem);
            ((IList)_floatingItems.Items).RemoveAt(index);
        }

        internal IEnumerable<DockItem> FloatingDockItems()
        {
            return _floatingItems.DockItems();
        }

        public bool IsHostingTab()
        {
            return this.GetVisualChildren().OfType<DockControl>()
                .FirstOrDefault(t => t.InterTabController != null && t.InterTabController.Partition == Partition)
                   != null;
        }

        private void MonitorDropZones(Point cursorPos)
        {
            var window = this.GetLogicalAncestors().OfType<TopLevel>().FirstOrDefault() as Window;
            if (window == null) return;

            foreach (var dropZone in _dropZones.Values.Where(dz => dz != null))
            {
                var pointFromScreen = window.PointToClient(cursorPos);
                //var pointRelativeToDropZone = pointFromScreen;
                var inputHitTest = dropZone.InputHitTest(pointFromScreen);

                if (inputHitTest != null)
                {
                    if (_currentlyOfferedDropZone != null)
                        _currentlyOfferedDropZone.Item2.IsOffered = false;
                    dropZone.IsOffered = true;
                    _currentlyOfferedDropZone = new Tuple<Layout, DropZone>(this, dropZone);
                }
                else
                {
                    dropZone.IsOffered = false;
                    if (_currentlyOfferedDropZone != null && _currentlyOfferedDropZone.Item2 == dropZone)
                        _currentlyOfferedDropZone = null;
                }
            }
        }

        /// <summary>
        /// Helper method to get all the currently loaded layouts.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Layout> GetLoadedInstances()
        {
            return LoadedLayouts.ToList();
        }

        /// <summary>
        /// Finds the location of a tab control withing a layout.
        /// </summary>
        /// <param name="dockControl"></param>
        /// <returns></returns>
        public static LocationReport Find(DockControl dockControl)
        {
            if (dockControl == null) throw new ArgumentNullException("dockControl");

            return Finder.Find(dockControl);
        }

        /// <summary>
        /// Creates a split in a layout, at the location of a specified <see cref="TabablzControl"/>.
        /// </summary>
        /// <para></para>
        /// <param name="dockControl">Tab control to be split.</param>
        /// <param name="orientation">Direction of split.</param>
        /// <param name="makeSecond">Set to <c>true</c> to make the current tab control push into the right hand or bottom of the split.</param>
        /// <remarks>The tab control to be split must be hosted in a layout control.</remarks>
        public static BranchResult Branch(DockControl dockControl, Orientation orientation, bool makeSecond)
        {
            return Branch(dockControl, orientation, makeSecond, .5);
        }

        /// <summary>
        /// Creates a split in a layout, at the location of a specified <see cref="dockControl"/>.
        /// </summary>
        /// <para></para>
        /// <param name="dockControl">Tab control to be split.</param>
        /// <param name="orientation">Direction of split.</param>
        /// <param name="makeSecond">Set to <c>true</c> to make the current tab control push into the right hand or bottom of the split.</param>
        /// <param name="firstItemProportion">Sets the proportion of the first tab control, with 0.5 being 50% of available space.</param>
        /// <remarks>The tab control to be split must be hosted in a layout control.</remarks>
        public static BranchResult Branch(DockControl dockControl, Orientation orientation, bool makeSecond,
            double firstItemProportion)
        {
            if (firstItemProportion < 0.0 || firstItemProportion > 1.0)
                throw new ArgumentOutOfRangeException("firstItemProportion", "Must be >= 0.0 and <= 1.0");

            var locationReport = Find(dockControl);

            Action<Branch> applier;
            object existingContent;
            if (!locationReport.IsLeaf)
            {
                existingContent = locationReport.RootLayout.Content;
                applier = branch => locationReport.RootLayout.Content = branch;
            }
            else if (!locationReport.IsSecondLeaf)
            {
                existingContent = locationReport.ParentBranch.FirstItem;
                applier = branch => locationReport.ParentBranch.FirstItem = branch;
            }
            else
            {
                existingContent = locationReport.ParentBranch.SecondItem;
                applier = branch => locationReport.ParentBranch.SecondItem = branch;
            }

            var selectedItem = dockControl.SelectedItem;
            var branchResult = Layout.Branch(orientation, firstItemProportion, makeSecond,
                locationReport.RootLayout.BranchTemplate, existingContent, applier);
            dockControl.SelectedItem = selectedItem;

            return branchResult;
        }

        private static BranchResult Branch(Orientation orientation, double proportion, bool makeSecond,
            DataTemplate branchTemplate, object existingContent, Action<Branch> applier)
        {
            var branchItem = new Branch
            {
                Orientation = orientation
            };

            var newContent = new ContentControl
            {
                Content = new object(),
            };

            newContent.DataTemplates.Add(branchTemplate);

            if (!makeSecond)
            {
                branchItem.FirstItem = existingContent;
                branchItem.SecondItem = newContent;
            }
            else
            {
                branchItem.FirstItem = newContent;
                branchItem.SecondItem = existingContent;
            }

            branchItem.SetValue(Docking.Branch.FirstItemLengthProperty, new GridLength(proportion, GridUnitType.Star));
            branchItem.SetValue(Docking.Branch.SecondItemLengthProperty,
                new GridLength(1 - proportion, GridUnitType.Star));

            applier(branchItem);

            var newDockControl = newContent.GetVisualChildren().OfType<DockControl>().FirstOrDefault();

            if (newDockControl == null)
                throw new ApplicationException("New DockControl was not generated inside branch.");

            return new BranchResult(branchItem, newDockControl);
        }

        private static bool TryGetSourceTabControl(DockItem dockItem, out DockControl dockControl)
        {
            var sourceOfDockItemsControl =
                dockItem.GetSelfAndLogicalAncestors().OfType<ItemsControl>().FirstOrDefault() as DockItemsControl;
            if (sourceOfDockItemsControl == null)
                throw new ApplicationException("Unable to determine source items control.");

            dockControl = DockControl.GetOwnerOfHeaderItems(sourceOfDockItemsControl);

            return dockControl != null;
        }

        internal static void RestoreFloatingItemSnapShots(Control ancestor,
            IEnumerable<FloatingItemSnapShot> floatingItemSnapShots)
        {
            var layouts = ancestor.GetSelfAndVisualDescendents().OfType<Layout>().ToList();
            foreach (var floatingDockItem in layouts.SelectMany(l => l.FloatingDockItems()))
            {
                var itemSnapShots = floatingItemSnapShots as FloatingItemSnapShot[] ?? floatingItemSnapShots.ToArray();
                var floatingItemSnapShot = itemSnapShots.FirstOrDefault(
                    ss => ss.Content == floatingDockItem.Content);
                if (floatingItemSnapShot != null)
                    floatingItemSnapShot.Apply(floatingDockItem);
            }
        }

        private static void SetupParticipatingLayouts(DockItem dockItem)
        {
            var sourceOfdockItemsControl =
                dockItem.GetSelfAndLogicalAncestors().OfType<ItemsControl>().FirstOrDefault() as DockItemsControl;
            if (sourceOfdockItemsControl == null ||
                (sourceOfdockItemsControl.Items as ICollection).Count != 1) return;

            var draggingWindow = dockItem.GetSelfAndVisualAncestors().OfType<Window>().First();
            if (draggingWindow == null) return;

            foreach (var loadedLayout in LoadedLayouts.Where(l =>
                l.Partition == dockItem.PartitionAtDragStart &&
                !Equals(l.GetSelfAndVisualAncestors().OfType<Window>().FirstOrDefault(), draggingWindow)))
            {
                loadedLayout.IsParticipatingInDrag = true;
            }
        }

        internal static bool ConsolidateBranch(Control control)
        {
            bool isSecondLineageWhenOwnerIsBranch;
            var ownerBranch = FindLayoutOrBranchOwner(control, out isSecondLineageWhenOwnerIsBranch) as Branch;
            if (ownerBranch == null) return false;

            var survivingItem = isSecondLineageWhenOwnerIsBranch ? ownerBranch.FirstItem : ownerBranch.SecondItem;

            var grandParent = FindLayoutOrBranchOwner(ownerBranch, out isSecondLineageWhenOwnerIsBranch);
            if (grandParent == null)
                throw new ApplicationException("Unexpected structure, grandparent Layout or Branch not found");

            var layout = grandParent as Layout;
            if (layout != null)
            {
                layout.Content = survivingItem;
                return true;
            }

            var branch = (Branch)grandParent;
            if (isSecondLineageWhenOwnerIsBranch)
                branch.SecondItem = survivingItem;
            else
                branch.FirstItem = survivingItem;

            return true;

        }



        private static object FindLayoutOrBranchOwner(Control control, out bool isSecondLineageWhenOwnerIsBranch)
        {
            isSecondLineageWhenOwnerIsBranch = false;

            var ancestoryStack = new Stack<Control>();
            do
            {
                ancestoryStack.Push(control);
                control = control.GetVisualParent<Control>();
                if (control is Layout)
                    return control;

                var branch = control as Branch;
                if (branch == null) continue;

                isSecondLineageWhenOwnerIsBranch = ancestoryStack.Contains(branch.SecondContentPresenter);
                return branch;


            } while (control != null);

            return null;
        }

        private static void MaximizeFloatingItemExecuted(object _)
        {
            var dockItem = _ as DockItem;
            if (dockItem == null) return;

            SetLocationSnapShot(dockItem, LocationSnapShot.Take(dockItem));
            SetFloatingItemState(dockItem, WindowState.Maximized);
        }

        private static void RestoreFloatingItemExecuted(object _)
        {
            var dockItem = _ as DockItem;
            if (dockItem == null) return;

            SetFloatingItemState(dockItem, WindowState.Normal);
            var locationSnapShot = GetLocationSnapShot(dockItem);
            if (locationSnapShot != null)
                locationSnapShot.Apply(dockItem);
        }

        private void UnfloatItemExecuted(object _)
        {
            var dockItem = _ as DockItem;
            if (dockItem == null)
                return;

            var executingDockControl = this.GetVisualChildren().OfType<DockControl>().
                FirstOrDefault(t => t.InterTabController != null && t.InterTabController.Partition == Partition);

            if (executingDockControl == null) return;

            var newTabHost =
                executingDockControl.InterTabController.InterTabClient.GetNewHost(
                    executingDockControl.InterTabController.InterTabClient,
                    executingDockControl.InterTabController.Partition, executingDockControl);
            if (newTabHost?.DockControl == null || newTabHost.Container == null)
                throw new ApplicationException("New tab host was not correctly provided");

            var floatingItemSnapShots = dockItem.GetVisualChildren()
                .OfType<Layout>()
                .SelectMany(l => l.FloatingDockItems().Select(FloatingItemSnapShot.Take))
                .ToList();

            var content = dockItem.Content ?? dockItem;

            FloatingItems.Remove(content);

            var window = this.GetVisualAncestors().OfType<Window>().FirstOrDefault();
            if (window == null) throw new ApplicationException("Unable to find owning window.");
            newTabHost.Container.Width = window.Width;
            newTabHost.Container.Height = window.Height;

            Dispatcher.UIThread.InvokeAsync(new Action(() =>
            {
                newTabHost.DockControl.AddToSource(content);
                newTabHost.DockControl.SelectedItem = content;
                newTabHost.Container.Show();
                newTabHost.Container.Activate();

                Dispatcher.UIThread.InvokeAsync(
                    new Action(() => RestoreFloatingItemSnapShots(newTabHost.DockControl, floatingItemSnapShots)));
            }), DispatcherPriority.DataBind);
        }

        internal static bool IsContainedWithinBranch(Control control)
        {
            do
            {
                control = control.GetVisualParent() as Control;
                if (control is Branch)
                    return true;
            } while (control != null);
            return false;
        }

        private static void Float(Layout layout, DockItem dockItem)
        {
            var sourceOfDockItemsControl =
                dockItem.GetSelfAndLogicalAncestors().OfType<ItemsControl>().FirstOrDefault() as DockItemsControl;
            if (sourceOfDockItemsControl == null)
                throw new ApplicationException("Unable to determine source items control.");
            var sourceDockControl = DockControl.GetOwnerOfHeaderItems(sourceOfDockItemsControl);
            layout._floatTransfer = FloatTransfer.TakeSnapshot(dockItem, sourceDockControl);
            var floatingItemSnapShots = sourceDockControl.GetVisualDescendents().OfType<Layout>()
                .SelectMany(l => l.FloatingDockItems().Select(FloatingItemSnapShot.Take))
                .ToList();
            if (sourceDockControl == null) throw new ApplicationException("Unable to determine source tab control.");
            sourceDockControl.RemoveItem(dockItem);

            ((IList)layout.FloatingItems).Add(layout._floatTransfer.Content);

            Dispatcher.UIThread.InvokeAsync(
                new Action(() => RestoreFloatingItemSnapShots(layout, floatingItemSnapShots)), DispatcherPriority.Loaded);
        }
    }
}
