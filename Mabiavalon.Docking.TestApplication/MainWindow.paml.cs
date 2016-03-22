using Perspex.Controls;
using Perspex.Markup.Xaml;

namespace Mabiavalon.Docking.TestApplication
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            App.AttachDevTools(this);
        }

        private void InitializeComponent()
        {
            PerspexXamlLoader.Load(this);
        }
    }
}
