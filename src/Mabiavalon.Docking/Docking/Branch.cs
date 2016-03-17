using Perspex;
using Perspex.Controls;
using Perspex.Controls.Presenters;
using Perspex.Controls.Primitives;
using Perspex.Data;

namespace Mabiavalon.Docking
{
    /// <summary>
    /// This control is used to determine where a control is in the docking tree.
    /// <remarks>
    /// Each ContentPresenter in this control can either be another Branch or a "Leaf"
    /// </remarks>
    /// </summary>
    public class Branch : TemplatedControl
    {
        public static readonly StyledProperty<Orientation> OrientationProperty =
            PerspexProperty.Register<Branch, Orientation>("Orientation");

        public static readonly StyledProperty<object> FirstItemProperty =
            PerspexProperty.Register<Branch, object>("FirstItem");

        public static readonly StyledProperty<GridLength> FirstItemLengthProperty =
            PerspexProperty.Register<Branch, GridLength>("FirstItemLenth", 
                new GridLength(0.49999, GridUnitType.Star), defaultBindingMode: BindingMode.TwoWay); 

        public static readonly StyledProperty<object> SecondItemProperty =
            PerspexProperty.Register<Branch, object>("SecondItem"); 

        public static readonly StyledProperty<GridLength> SecondItemLengthProperty =
           PerspexProperty.Register<Branch, GridLength>("SecondItemLenth", 
               new GridLength(0.50001, GridUnitType.Star), defaultBindingMode: BindingMode.TwoWay);

        public Branch()
        {
            // Style
        }

        public Orientation Orientation
        {
            get { return GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        } 

        public object FirstItem
        {
            get { return GetValue(FirstItemProperty); }
            set { SetValue(FirstItemProperty, value); }
        }

        public GridLength FirstItemLength
        {
            get { return GetValue(FirstItemLengthProperty); }
            set { SetValue(FirstItemLengthProperty, value); }
        }

        public object SecondItem
        {
            get { return GetValue(SecondItemProperty); }
            set { SetValue(SecondItemProperty, value); }
        }

        public GridLength SecondItemLength
        {
            get { return GetValue(SecondItemLengthProperty); }
            set { SetValue(SecondItemLengthProperty, value); }
        }

        internal ContentPresenter FirstContentPresenter { get; private set; }
        internal ContentPresenter SecondContentPresenter { get; private set; }

        /// <summary>
        /// Gets the proportional size of the first item, between 0 and 1, where 1 would represent the entire size of the branch.
        /// </summary>
        /// <returns></returns>
        public double GetFirstProportion()
        {
            return (1/(FirstItemLength.Value + SecondItemLength.Value))*FirstItemLength.Value;
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            FirstContentPresenter = e.NameScope.Find<ContentPresenter>("PART_FirstContentPresenter");
            SecondContentPresenter = e.NameScope.Find<ContentPresenter>("PART_SecondContentPresenter");
        } 
    }
}