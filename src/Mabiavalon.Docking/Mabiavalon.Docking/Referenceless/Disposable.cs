using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mabiavalon.Referenceless
{
    internal static class Disposable
    {
        public static IDisposable Empty
        {
            get
            {
                return (IDisposable)DefaultDisposable.Instance;
            }
        }

        public static IDisposable Create(Action dispose)
        {
            if (dispose == null)
                throw new ArgumentNullException("dispose");
            else
                return (IDisposable)new AnonymousDisposable(dispose);
        }
    }
}
