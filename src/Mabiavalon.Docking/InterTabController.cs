using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex;
using Perspex.Controls;

namespace Mabiavalon
{
    public class InterTabController : Control
    {
        private double _horizontalPopoutGrace;
        private double _verticalPopoutGrace;
        private bool _moveWindowWithSolitaryTabs;

        public static readonly DirectProperty<InterTabController, double> HorizontalPopoutGraceProperty =
            PerspexProperty.RegisterDirect<InterTabController, double>("HorizontalPopoutGrace", o => o.HorizontalPopoutGrace,
                (o, v) => o.HorizontalPopoutGrace = v);

        public static readonly DirectProperty<InterTabController, double> VerticalPopoutGraceProperty =
            PerspexProperty.RegisterDirect<InterTabController, double>("VerticalPopoutGrace", o => o.VerticalPopoutGrace,
                (o, v) => o.VerticalPopoutGrace = v); 

        public static readonly DirectProperty<InterTabController, bool> MoveWindowWithSolitaryTabsProperty =
            PerspexProperty.RegisterDirect<InterTabController, bool>("MoveWindowWithSolitaryTabs", o => o.MoveWindowWithSolitaryTabs,
                (o, v) => o.MoveWindowWithSolitaryTabs = v);

        public static readonly StyledProperty<IInterTabClient> InterTabClientProperty =
            PerspexProperty.Register<InterTabController, IInterTabClient>("InterTabClient", defaultValue: new DefaultInterTabClient());

        //public static readonly StyledProperty<object> PartitionProperty =
        //    PerspexProperty.Register<InterTabController, object>("Partition");

        public InterTabController()
        {
            HorizontalPopoutGrace = 8;
            VerticalPopoutGrace = 8;
            MoveWindowWithSolitaryTabs = true;
        }

        public double HorizontalPopoutGrace
        {
            get { return _horizontalPopoutGrace; }
            set { SetAndRaise(HorizontalPopoutGraceProperty, ref _horizontalPopoutGrace, value); }
        }

        public double VerticalPopoutGrace
        {
            get { return _verticalPopoutGrace; }
            set { SetAndRaise(VerticalPopoutGraceProperty, ref _verticalPopoutGrace, value); }
        }

        public bool MoveWindowWithSolitaryTabs
        {
            get { return _moveWindowWithSolitaryTabs; }
            set { SetAndRaise(MoveWindowWithSolitaryTabsProperty, ref _moveWindowWithSolitaryTabs, value); }
        }

        public IInterTabClient InterTabClient
        {
            get { return GetValue(InterTabClientProperty); }
            set { SetValue(InterTabClientProperty, value); }
        }

        //public object Partition
        //{
        //    get { return GetValue(PartitionProperty); }
        //    set { SetValue(PartitionProperty, value); }
        //}

        /// <summary>
        /// The partition allows on or more tab environments in a single application.  Only tabs which have a tab controller
        /// with a common partition will be allowed to have tabs dragged between them.  <c>null</c> is a valid partition (i.e global).
        /// </summary>
        public string Partition { get; set; }
    }
}
