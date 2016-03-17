using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Perspex;
using Perspex.Controls;
using Perspex.Controls.Primitives;
using Perspex.Input;
using Perspex.Input.Raw;
using Perspex.Interactivity;
using Perspex.Threading;
using Perspex.VisualTree;

namespace Mabiavalon
{
    public enum SizeGrip
    {
        NotApplicable,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft
    }

    public class DockItem : ContentControl
    {
        public const string ThumbPartName = "PART_Thumb";

        private bool _isTemplateThumbWithMouseAfterSeize = false;
        private readonly SerialDisposable _templateSubscriptions = new SerialDisposable();
        private readonly SerialDisposable _rightMouseUpCleanUpDisposable = new SerialDisposable();

        private Thumb _customThumb;
        private Thumb _thumb;
        private bool _seizeDragWithTemplate;
        private Action<DockItem> _dragSeizedContinuation;

        private int _logicalIndex;
        private bool _isDragging;
        private bool _isSiblingDragging;
        private double _y;
        private double _x;
        private bool _isSelected;

        public static readonly DirectProperty<DockItem, int> LogicalIndexProperty =
            PerspexProperty.RegisterDirect<DockItem, int>("LogicalIndex", o => o.LogicalIndex);

        public static readonly DirectProperty<DockItem, bool> IsDraggingProperty =
            PerspexProperty.RegisterDirect<DockItem, bool>("IsDragging", o => o.IsDragging);

        public static readonly DirectProperty<DockItem, bool> IsSiblingDraggingProperty =
            PerspexProperty.RegisterDirect<DockItem, bool>("IsSiblingDragging", o => o.IsSiblingDragging);

        public static readonly DirectProperty<DockItem, double> YProperty =
            PerspexProperty.RegisterDirect<DockItem, double>("Y", o => o.Y,
                (o, v) => o.Y = v);

        public static readonly DirectProperty<DockItem, double> XProperty =
            PerspexProperty.RegisterDirect<DockItem, double>("X", o => o.X,
                (o, v) => o.X = v);

        public static readonly DirectProperty<DockItem, bool> IsSelectedProperty =
            PerspexProperty.RegisterDirect<DockItem, bool>("IsSelected", o => o.IsSelected,
                (o, v) => o.IsSelected = v);

        public static readonly AttachedProperty<SizeGrip> SizeGripProperty =
            PerspexProperty.RegisterAttached<DockItem, Thumb, SizeGrip>("SizeGrip");

        public static readonly AttachedProperty<double> ContentRotateTransformAngleProperty =
            PerspexProperty.RegisterAttached<DockItem, Thumb, double>("ContentRotateTransformAngle");

        public static readonly AttachedProperty<bool> IsCustomThumbProperty =
            PerspexProperty.RegisterAttached<DockItem, Thumb, bool>("IsCustomThumb");

        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<DockItem, RoutedEventArgs>("Click", RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> MouseDownWithinEvent =
            RoutedEvent.Register<DockItem, RoutedEventArgs>("MouseDownWithin", RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> XChangedEvent =
            RoutedEvent.Register<DockItem, RoutedEventArgs>("XChanged", RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> YChangedEvent =
            RoutedEvent.Register<DockItem, RoutedEventArgs>("YChanged", RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> LogicalIndexChangedEvent =
            RoutedEvent.Register<DockItem, RoutedEventArgs>("LogicalIndexChanged", RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> IsDraggingChangedEvent =
            RoutedEvent.Register<DockItem, RoutedEventArgs>("IsDraggingChanged", RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> IsSiblingDraggingChangedEvent =
            RoutedEvent.Register<DockItem, RoutedEventArgs>("IsSiblingDraggingChanged", RoutingStrategies.Bubble);

        static DockItem()
        {
            // StyleOverride
            XProperty.Changed.AddClassHandler<DockItem>(x => x.OnXChanged);
            YProperty.Changed.AddClassHandler<DockItem>(x => x.OnYChanged);
            LogicalIndexProperty.Changed.AddClassHandler<DockItem>(x => x.OnLogicalIndexChanged);
            SizeGripProperty.Changed.AddClassHandler<DockItem>(x => x.OnSizeGripChanged);
            IsDraggingProperty.Changed.AddClassHandler<DockItem>(x => x.OnIsDraggingChanged);
            IsSiblingDraggingProperty.Changed.AddClassHandler<DockItem>(x => x.OnIsSiblingDraggingChanged);
        }

        public DockItem()
        {
            ClickEvent.AddClassHandler<DockItem>(x => x.OnMouseDownWithin(this), handledEventsToo: true);
        }

        public event EventHandler<RoutedEventArgs> Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public event EventHandler<RoutedEventArgs> MouseDownWithin
        {
            add { AddHandler(MouseDownWithinEvent, value); }
            remove { RemoveHandler(MouseDownWithinEvent, value); }
        }

        public event EventHandler<RoutedEventArgs> XChanged
        {
            add { AddHandler(XChangedEvent, value); }
            remove { RemoveHandler(XChangedEvent, value); }
        }

        public event EventHandler<RoutedEventArgs> YChanged
        {
            add { AddHandler(YChangedEvent, value); }
            remove { RemoveHandler(YChangedEvent, value); }
        }

        public event EventHandler<RoutedEventArgs> LogicalIndexChanged
        {
            add { AddHandler(LogicalIndexChangedEvent, value); }
            remove { RemoveHandler(LogicalIndexChangedEvent, value); }
        }

        public event EventHandler<RoutedEventArgs> IsDraggingChanged
        {
            add { AddHandler(IsDraggingChangedEvent, value); }
            remove { RemoveHandler(IsDraggingChangedEvent, value); }
        }

        public event EventHandler<RoutedEventArgs> IsSiblingDraggingChanged
        {
            add { AddHandler(IsSiblingDraggingChangedEvent, value); }
            remove { RemoveHandler(IsSiblingDraggingChangedEvent, value); }
        }

        public int LogicalIndex
        {
            get { return _logicalIndex; }
            private set { SetAndRaise(LogicalIndexProperty, ref _logicalIndex, value); }
        }

        public bool IsDragging
        {
            get { return _isDragging; }
            internal set { SetAndRaise(IsDraggingProperty, ref _isDragging, value); }
        }

        public bool IsSiblingDragging
        {
            get { return _isSiblingDragging; }
            internal set { SetAndRaise(IsSiblingDraggingProperty, ref _isSiblingDragging, value); }
        }

        public double X
        {
            get { return _x; }
            set { SetAndRaise(XProperty, ref _x, value); }
        }

        public double Y
        {
            get { return _y; }
            set { SetAndRaise(YProperty, ref _y, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetAndRaise(IsSelectedProperty, ref _isSelected, value); }
        }

        internal object UnderlyingContent { get; set; }
        internal Point MouseAtDragStart { get; set; }
        internal string PartitionAtDragStart { get; set; }
        internal bool IsDropTargetFound { get; set; }

        public static SizeGrip GetSizeGrip(Control element)
        {
            return element.GetValue(SizeGripProperty);
        }

        public static void SetSizeGrip(Control element, SizeGrip value)
        {
            element.SetValue(SizeGripProperty, value);
        }

        public static double GetContentRotateTransformAngle(Control element)
        {
            return element.GetValue(ContentRotateTransformAngleProperty);
        }

        public static void SetContentRotateTransformAngle(Control element, double value)
        {
            element.SetValue(ContentRotateTransformAngleProperty, value);
        }

        public static bool GetIsCustomThumb(Control element)
        {
            return element.GetValue(IsCustomThumbProperty);
        }

        public static void SetIsCustomThumb(Control element, bool value)
        {
            element.SetValue(IsCustomThumbProperty, value);
        }


        internal void InstigateDrag(Action<DockItem> continuation)
        {
            _dragSeizedContinuation = continuation;
            var thumb = NameScope.GetNameScope(this).Find<Thumb>(ThumbPartName);
            if (thumb != null)
            {
                MouseDevice.Instance.Capture(thumb);
            }
            else
                _seizeDragWithTemplate = true;
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            var thumbSubscriptions = SelectAndSubscribeToThumb();
            _templateSubscriptions.Disposable = thumbSubscriptions.Item2;

            if (_seizeDragWithTemplate && thumbSubscriptions.Item1 != null)
            {
                _isTemplateThumbWithMouseAfterSeize = true;
                //Mouse.AddLostMouseCaptureHandler(this, LostMouseAfterSeizeHandler);
                if (_dragSeizedContinuation != null)
                    _dragSeizedContinuation(this);
                _dragSeizedContinuation = null;

                Dispatcher.UIThread.InvokeAsync(
                    new Action(
                        () =>
                            _thumb.RaiseEvent(new PointerPressedEventArgs(PointerPressedEvent)
                            {
                                MouseButton = MouseButton.Left,
                                ClickCount = 0,
                                Device = MouseDevice.Instance
                            })));
            }
            _seizeDragWithTemplate = false;
        }


        //private void LostMouseAfterSeizeHandler(object sender, MouseEventArgs mouseEventArgs)
        //{
        //    _isTemplateThumbWithMouseAfterSeize = false;
        //    //Mouse.RemoveLostMouseCaptureHandler(this, LostMouseAfterSeizeHandler);
        //}

        private void ThumbOnDragCompleted(object sender, VectorEventArgs e)
        {
            //OnDragCompleted(e);
            MouseAtDragStart = new Point();
        }

        private void ThumbOnDragDelta(object sender, VectorEventArgs e)
        {
            var thumb = (Thumb) sender;

            //var previewEventArgs = new DcokDragDeltaEventArgs(PreviewDragDelta, this, dragDeltaEventArgs);
            //OnPreviewDragDelta(previewEventArgs);
            //if (previewEventArgs.Cancel)
            //    thumb.CancelDrag();
            //if (!previewEventArgs.Handled)
            //{
            //    var eventArgs = new DockDragDeltaEventArgs(DragDelta, this, dragDeltaEventArgs);
            //    OnDragDelta(eventArgs);
            //    if (eventArgs.Cancel)
            //        thumb.CancelDrag();
            //}
        }

        private void ThumbOnDragStarted(object sender, VectorEventArgs e)
        {
            MouseAtDragStart = MouseDevice.Instance.Position;
            //OnDragStarted(new DockDragStartedEventArgs(DragStarted, this, dragStartedEventArgs));
        }

        private void OnXChanged(PerspexPropertyChangedEventArgs e)
        {
            var args = new RoutedEventArgs(XChangedEvent);
            this.RaiseEvent(args);
        }

        private void OnYChanged(PerspexPropertyChangedEventArgs e)
        {
            var args = new RoutedEventArgs(YChangedEvent);
            this.RaiseEvent(args);
        }

        private void OnLogicalIndexChanged(PerspexPropertyChangedEventArgs e)
        {
            var args = new RoutedEventArgs(LogicalIndexChangedEvent);
            this.RaiseEvent(args);
        }

        private void OnSizeGripChanged(PerspexPropertyChangedEventArgs e)
        {
            var thumb = (e.Sender as Thumb);
            if (thumb == null) return;
            thumb.DragDelta += SizeThumbOnDragDelta;
        }

        private void OnIsDraggingChanged(PerspexPropertyChangedEventArgs e)
        {
            var args = new RoutedEventArgs(IsDraggingChangedEvent);
            this.RaiseEvent(args);
        }

        private void OnIsSiblingDraggingChanged(PerspexPropertyChangedEventArgs e)
        {
            var args = new RoutedEventArgs(IsSiblingDraggingChangedEvent);
            this.RaiseEvent(args);
        }

        private Action<RoutedEventArgs> OnMouseDownWithin(DockItem dockItem)
        {
            return args => dockItem.RaiseEvent(new DockItemEventArgs(MouseDownWithinEvent, dockItem));
        }

        private static void ApplyCustomThumbSetting(Thumb thumb)
        {
            var dockItem = thumb.GetVisualAncestors().OfType<DockItem>().FirstOrDefault();
            if (dockItem == null) throw new ApplicationException("Cannot find parent DockItem or custom Thumb");

            var enableCustomThumb = thumb.GetValue(IsCustomThumbProperty);
            dockItem._customThumb = enableCustomThumb ? thumb : null;
            dockItem._templateSubscriptions.Disposable = dockItem.SelectAndSubscribeToThumb().Item2;

            if (dockItem._customThumb != null && dockItem._isTemplateThumbWithMouseAfterSeize)
                Dispatcher.UIThread.InvokeAsync(
                    new Action(
                        () =>
                            dockItem._customThumb.RaiseEvent(new PointerPressedEventArgs(PointerPressedEvent)
                            {
                                ClickCount = 0,
                                Device = MouseDevice.Instance,
                                MouseButton = MouseButton.Left
                            })));
        }

        private Thumb FindCustomThumb()
        {
            return this.GetSelfAndVisualDescendents().OfType<Thumb>().FirstOrDefault(GetIsCustomThumb);
        }

        private void SizeThumbOnDragDelta(object sender, VectorEventArgs e)
        {
            var thumb = (Thumb) sender;
            var dockItem = thumb.GetVisualAncestors().OfType<DockItem>().FirstOrDefault();
            if (dockItem == null) return;

            var sizeGrip = thumb.GetValue(SizeGripProperty);
            var width = dockItem.Width;
            var height = dockItem.Height;
            var x = dockItem.X;
            var y = dockItem.Y;
            switch (sizeGrip)
            {
                case SizeGrip.NotApplicable:
                    break;
                case SizeGrip.Left:
                    width += -e.Vector.X;
                    x += e.Vector.X;
                    break;
                case SizeGrip.TopLeft:
                    width += -e.Vector.X;
                    height += -e.Vector.Y;
                    x += e.Vector.X;
                    y += e.Vector.Y;
                    break;
                case SizeGrip.Top:
                    height += -e.Vector.Y;
                    y += e.Vector.Y;
                    break;
                case SizeGrip.TopRight:
                    height += -e.Vector.Y;
                    width += e.Vector.X;
                    y += e.Vector.Y;
                    break;
                case SizeGrip.Right:
                    width += e.Vector.X;
                    break;
                case SizeGrip.BottomRight:
                    width += e.Vector.X;
                    height += e.Vector.Y;
                    break;
                case SizeGrip.Bottom:
                    height += e.Vector.Y;
                    break;
                case SizeGrip.BottomLeft:
                    height += e.Vector.Y;
                    width += -e.Vector.X;
                    x += e.Vector.X;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            dockItem.SetValue(XProperty, x);
            dockItem.SetValue(YProperty, y);
            dockItem.SetValue(WidthProperty, Math.Max(width, thumb.DesiredSize.Width));
            dockItem.SetValue(HeightProperty, Math.Max(height, thumb.DesiredSize.Height));
        }

        private Tuple<Thumb, IDisposable> SelectAndSubscribeToThumb()
        {
            var templateThumb = NameScope.GetNameScope(this).Find<Thumb>(ThumbPartName);
            templateThumb?.SetValue(IsHitTestVisibleProperty, _customThumb == null);

            _thumb = _customThumb ?? templateThumb;
            if (_thumb != null)
            {
                _thumb.DragStarted += ThumbOnDragStarted;
                _thumb.DragDelta += ThumbOnDragDelta;
                _thumb.DragCompleted += ThumbOnDragCompleted;
            }

            var tidyUpThumb = _thumb;
            var disposable = Disposable.Create(() =>
            {
                if (tidyUpThumb == null) return;
                tidyUpThumb.DragStarted -= ThumbOnDragStarted;
                tidyUpThumb.DragDelta -= ThumbOnDragDelta;
                tidyUpThumb.DragCompleted -= ThumbOnDragCompleted;
            });

            return new Tuple<Thumb, IDisposable>(_thumb, disposable);
        }
    }
}