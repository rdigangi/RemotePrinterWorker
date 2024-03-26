using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models
{
    public class RemotePrinters
    {
        [JsonProperty(PropertyName = "remotePRinters")]
        public List<RemotePrinter> remotePRinters { get; set; }
    }
}
