using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models
{
    public class GetAvailableDocumentsRepsonse
    {
        public Data data { get; set; }
        public List<Error> errors { get; set; }
    }
}
