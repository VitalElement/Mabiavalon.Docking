using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Perspex;
using Perspex.Controls;
using Perspex.Controls.Primitives;
using Perspex.Input;
using Perspex.Interactivity;

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
        private bool _isCustomThumb;

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
            PerspexProperty.RegisterAttached<DockItem, Control, SizeGrip>("SizeGrip");

        public static readonly AttachedProperty<double> ContentRotateTransformAngleProperty =
            PerspexProperty.RegisterAttached<DockItem, Control, double>("ContentRotateTransformAngle");

        public static readonly AttachedProperty<bool> IsCustomThumbProperty =
            PerspexProperty.RegisterAttached<DockItem, Control, bool>("IsCustomThumb");

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

        private Action<RoutedEventArgs> OnMouseDownWithin(DockItem dockItem)
        {
            return args => dockItem.RaiseEvent(new DockItemEventArgs(MouseDownWithinEvent, dockItem));
        }

        
    }
}
