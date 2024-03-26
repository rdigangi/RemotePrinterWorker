using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models
{
    public class EtcdNode
    {
        public string key { get; set; }
        public bool dir { get; set; }
        public int modifiedIndex { get; set; }
        public int createdIndex { get; set; }
        public string value { get; set; }
        public List<EtcdNode> nodes { get; set; }
    }
}
