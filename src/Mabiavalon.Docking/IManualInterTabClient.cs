using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mabiavalon
{
    public interface IManualInterTabClient : IInterTabClient
    {
        void Add(object item);
        void Remove(object item);
    }
}
