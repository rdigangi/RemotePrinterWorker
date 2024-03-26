using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models.Requests
{
    public class Payload
    {
        public string file { get; set; }
        public string printerName { get; set; }
    }
}
