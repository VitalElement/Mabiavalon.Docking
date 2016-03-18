using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mabiavalon.Core;
using Perspex;
using Perspex.Controls;
using Perspex.Controls.Presenters;
using Perspex.Controls.Primitives;
using Perspex.LogicalTree;
using Perspex.Markup.Xaml.Templates;
using Perspex.Styling;
using Perspex.Threading;
using Perspex.VisualTree;

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

		private readonly IDictionary<DropZoneLocation, DropZone> _dropZones = new Dictionary<DropZoneLocation, DropZone>();
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

		public static readonly StyledProperty<IEnumerable> FloatingItemsProperty =
			PerspexProperty.Register<Layout, IEnumerable>("FloatingItems");

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
			PerspexProperty.RegisterDirect<Layout, ClosingFloatingItemCallback>("ClosingFloatingItemCallback", o => o.ClosingFloatingItemCallback,
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
			// Style
		}


		public Layout()
		{
			
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

		public IEnumerable FloatingItems
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

		private void Branch(DropZoneLocation location, DockControl sourceDockItem)
		{
			throw new NotImplementedException();
		}

		internal IEnumerable<DockItem> FloatingDockItems()
		{
			return _floatingItems.DockItems();
		}

		private bool IsHostingTab()
		{
			return this.GetVisualChildren().OfType<DockControl>()
				.FirstOrDefault(t => t.InterTabController != null && t.InterTabController.Partition == Partition)
				!= null;
		}

		private void MonitorDropZones(Point cursorPos)
		{
			new NotImplementedException();
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
		/// Creates a split in a layout, at the location of a specified <see cref="avalonViewControl"/>.
		/// </summary>
		/// <para></para>
		/// <param name="avalonViewControl">Tab control to be split.</param>
		/// <param name="orientation">Direction of split.</param>
		/// <param name="makeSecond">Set to <c>true</c> to make the current tab control push into the right hand or bottom of the split.</param>
		/// <param name="firstItemProportion">Sets the proportion of the first tab control, with 0.5 being 50% of available space.</param>
		/// <remarks>The tab control to be split must be hosted in a layout control.</remarks>
		public static BranchResult Branch(DockControl avalonViewControl, Orientation orientation, bool makeSecond, double firstItemProportion)
		{
			if (firstItemProportion < 0.0 || firstItemProportion > 1.0) throw new ArgumentOutOfRangeException("firstItemProportion", "Must be >= 0.0 and <= 1.0");

			var locationReport = Find(avalonViewControl);

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

			var selectedItem = avalonViewControl.SelectedItem;
			var branchResult = Layout.Branch(orientation, firstItemProportion, makeSecond, locationReport.RootLayout.BranchTemplate, existingContent, applier);
			avalonViewControl.SelectedItem = selectedItem;

			return branchResult;
		}

		private static BranchResult Branch(Orientation orientation, double proportion, bool makeSecond, DataTemplate branchTemplate, object existingContent, Action<Branch> applier)
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
			branchItem.SetValue(Docking.Branch.SecondItemLengthProperty, new GridLength(1 - proportion, GridUnitType.Star));

			applier(branchItem);

			var newDockControl = newContent.GetVisualChildren().OfType<DockControl>().FirstOrDefault();

			if (newDockControl == null)
				throw new ApplicationException("New DockControl was not generated inside branch.");

			return new BranchResult(branchItem, newDockControl);
		}

		private static bool TryGetSourceTabControl(DockItem dockItem, out DockControl dockControl)
		{
			var sourceOfDockItemsControl = dockItem.GetSelfAndLogicalAncestors().OfType<ItemsControl>().FirstOrDefault() as DockItemsControl;
			if (sourceOfDockItemsControl == null) throw new ApplicationException("Unable to determine source items control.");

			dockControl = DockControl.GetOwnerOfHeaderItems(sourceOfDockItemsControl);

			return dockControl != null;
		}

		internal static void RestoreFloatingItemSnapShots(Control ancestor, IEnumerable<FloatingItemSnapShot> floatingItemSnapShots)
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

		private static void SetupParticipatingLayouts(DockControl dockItem)
		{
			var sourceOfAvalonViewItemsControl = dockItem.GetSelfAndLogicalAncestors().OfType<ItemsControl>().FirstOrDefault() as DockItemsControl;
			if (sourceOfAvalonViewItemsControl == null || (sourceOfAvalonViewItemsControl.Items as ICollection).Count != 1) return;

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
			if (grandParent == null) throw new ApplicationException("Unexpected structure, grandparent Layout or Branch not found");

			var layout = grandParent as Layout;
			if (layout != null)
			{
				layout.Content = survivingItem;
				return true;
			}

			var branch = (Branch) grandParent;
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
			var sourceOfDockItemsControl = dockItem.GetSelfAndLogicalAncestors().OfType<ItemsControl>().FirstOrDefault() as DockItemsControl;
			if (sourceOfDockItemsControl == null) throw new ApplicationException("Unable to determine source items control.");
			var sourceDockControl = DockControl.GetOwnerOfHeaderItems(sourceOfDockItemsControl);
			layout._floatTransfer = FloatTransfer.TakeSnapshot(dockItem, sourceDockControl);
			var floatingItemSnapShots = sourceDockControl.GetVisualDescendents().OfType<Layout>()
				.SelectMany(l => l.FloatingDockItems().Select(FloatingItemSnapShot.Take))
				.ToList();
			if (sourceDockControl == null) throw new ApplicationException("Unable to determine source tab control.");
			sourceDockControl.RemoveItem(dockItem);

			CollectionTeaser collectionTeaser;
			if (CollectionTeaser.TryCreate((IList)layout.FloatingItems, out collectionTeaser))
				collectionTeaser.Add(layout._floatTransfer.Content);
			else
				((IList)layout.FloatingItems).Add(layout._floatTransfer.Content);


			Dispatcher.UIThread.InvokeAsync(new Action(() => RestoreFloatingItemSnapShots(layout, floatingItemSnapShots)), DispatcherPriority.Loaded);
		}
	}
}
