﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex.Controls;

namespace Mabiavalon
{
    /// <summary>
    /// Implementors should provide mechanisms for providing new windows and closing old windows.
    /// </summary>

    public interface IInterTabClient
    {
        /// <summary>
        /// Provide a new host window so a tab can be teared from an existing window into a new window.
        /// </summary>
        /// <param name="interTabClient"></param>
        /// <param name="partition">Provides the partition where the drag operation was initiated.</param>
        /// <param name="source">The source control where a dragging operation was initiated.</param>
        /// <returns></returns>
        INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, DockControl source);

        /// <summary>
        /// Called when a tab has been emptied, and thus typically a window needs closing.
        /// </summary>
        /// <param name="dockControl"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        TabEmptiedResponse TabEmptiedHandler(DockControl dockControl, Window window);
    }
}
