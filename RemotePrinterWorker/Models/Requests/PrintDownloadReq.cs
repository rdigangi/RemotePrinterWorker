using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models.Requests
{
    public class PrintDownloadReq
    {
        public string cmd { get; set; }
        public Payload payload { get; set; }
    }

}
