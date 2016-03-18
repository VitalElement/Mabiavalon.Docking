using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex;
using Perspex.Markup.Xaml.Templates;

namespace Mabiavalon
{
    /// <summary>
    /// A <see cref="DockItem"/> that is good for Tool Windows
    /// </summary>
    public class HeaderedDockItem : DockItem
    {
        public static readonly StyledProperty<object> HeaderContentProperty =
            PerspexProperty.Register<HeaderedDockItem, object>("HeaderContent");

        public static readonly StyledProperty<string> HeaderContentStringFormatProperty =
            PerspexProperty.Register<HeaderedDockItem, string>("HeaderContentStringFormat");

        public static readonly StyledProperty<Template> HeaderContentTemplateProperty =
            PerspexProperty.Register<HeaderedDockItem, Template>("HeaderContentTemplate");

        static HeaderedDockItem()
        {
            // Style
        }

        public object HeaderContent
        {
            get { return GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value);}
        }

        public string HeaderContentStringFormat
        {
            get { return GetValue(HeaderContentStringFormatProperty); }
            set { SetValue(HeaderContentStringFormatProperty, value); }
        }

        public Template HeaderContentTemplate
        {
            get { return GetValue(HeaderContentTemplateProperty); }
            set { SetValue(HeaderContentTemplateProperty, value);}
        }
    }
}
