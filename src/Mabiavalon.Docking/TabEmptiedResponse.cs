using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mabiavalon
{
    public enum TabEmptiedResponse
    {
        /// <summary>
        /// Allow the Window to be closed automatically.
        /// </summary>
        CloseWindowOrLayoutBranch,
        /// <summary>
        /// The window will not be closed by the <see cref="DockControl"/>, probably meaning the implementor will close the window manually
        /// </summary>
        DoNothing
    }
}
