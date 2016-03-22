using Perspex.Controls;
using Perspex.Markup.Xaml;

namespace Mabiavalon.Theme
{
    public class DockControl : UserControl
    {
        public DockControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            PerspexXamlLoader.Load(this);
        }
    }
}
