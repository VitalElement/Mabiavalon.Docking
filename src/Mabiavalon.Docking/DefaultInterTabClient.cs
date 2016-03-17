using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex.Controls;
using Perspex.LogicalTree;
using Perspex.Threading;
using Perspex.VisualTree;

namespace Mabiavalon
{
    public class DefaultInterTabClient : IInterTabClient
    {
        public virtual INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, DockControl source)
        {
            if (source == null) throw new ArgumentNullException("source");
            var sourceWindow = source.GetSelfAndVisualAncestors().OfType<Window>().First();
            if (sourceWindow == null) throw new ApplicationException("Unable to ascrtain source window.");

            var newHost = (Window)Activator.CreateInstance(sourceWindow.GetType());

            Dispatcher.UIThread.InvokeAsync(new Action(() => { }), DispatcherPriority.DataBind);

            var newDockControl = newHost.GetSelfAndLogicalAncestors().OfType<DockControl>().FirstOrDefault();
            if (newDockControl == null) throw new ApplicationException("Unable to ascrtain tab control.");

            return new NewTabHost<Window>(newHost, newDockControl);

        }

        public virtual TabEmptiedResponse TabEmptiedHandler(DockControl dockControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}
