using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models
{
    public class EtcdRoot
    {
        public string action { get; set; }
        public EtcdNode node { get; set; }
    }
}
