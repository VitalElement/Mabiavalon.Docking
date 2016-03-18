using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex.Controls;

namespace Mabiavalon
{
    public class DefaultInterLayoutClient : IInterLayoutClient
    {
        public INewTabHost<Control> GetNewHost(object partition, DockControl source)
        {
            return new NewTabHost<Control>(source, source);
        }
    }
}
