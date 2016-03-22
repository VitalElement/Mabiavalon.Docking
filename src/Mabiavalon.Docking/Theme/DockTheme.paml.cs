namespace Mabiavalon.Theme
{
    using Perspex.Markup.Xaml;
    using Perspex.Styling;

    public class DockTheme : Styles
    {
        public DockTheme()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            PerspexXamlLoader.Load(this);
        }
    }
}
