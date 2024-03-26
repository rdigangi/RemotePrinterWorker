using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models
{
    public class Validator
    {
        public bool result { get; set; }
        public List<string> errorMessages { get; set; }

        public Validator()
        {
            errorMessages = new List<string>();
        }
    }
}
