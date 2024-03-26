using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models.Exceptions
{
    public class WecomException : Exception
    {
        public string errMessage { get; private set; }

        public WecomException(string m)
        {
            errMessage = m;
        }
    }
}
