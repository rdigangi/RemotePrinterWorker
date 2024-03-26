using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models
{
    public class Document
    {
        public string url { get; set; }
        public string id { get; set; }
        public string filename { get; set; }
        public string description { get; set; }
        public string repository { get; set; }

        public Document()
        {
        }

        public Document(string link)
        {
            url = link;
        }
    }
}
