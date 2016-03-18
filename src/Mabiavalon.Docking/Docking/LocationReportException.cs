using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mabiavalon.Docking
{
    public class LocationReportException : Exception
    {
        public LocationReportException()
        {

        }

        public LocationReportException(string message) : base(message)
        {

        }

        public LocationReportException(string message, Exception exception) : base(message, exception)
        {

        }
    }
}
