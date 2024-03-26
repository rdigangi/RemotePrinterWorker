using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models
{
    public class Pdl
    {
        public string ver_sdp { get; set; }
        public bool flag_scm { get; set; }
        public string operatore { get; set; }
        public string dt_cont { get; set; }
        public float imp_cassa { get; set; }
        public string id_cassa { get; set; }
        public string term_id { get; set; }
        public long id_operazione { get; set; }
        public int num_opez { get; set; }
        public string pdl { get; set; }
        public string fraz { get; set; }
    }
}
