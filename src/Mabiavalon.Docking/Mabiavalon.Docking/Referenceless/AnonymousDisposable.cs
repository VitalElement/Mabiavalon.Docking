using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mabiavalon.Referenceless
{
    internal sealed class AnonymousDisposable : ICancelable, IDisposable
    {
        private volatile Action _dispose;

        public bool IsDisposed
        {
            get
            {
                return this._dispose == null;
            }
        }

        public AnonymousDisposable(Action dispose)
        {
            this._dispose = dispose;
        }

        public void Dispose()
        {
            var action = Interlocked.Exchange<Action>(ref _dispose, (Action)null);
            if (action == null)
                return;
            action();
        }
    }
}
