using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models.Responses
{
    public class PrintDownloadResp
    {
        public string cmd { get; set; }
        public string status { get; set; }
        public Payload payload { get; set; }
    }
}
