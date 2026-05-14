using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Invoice
    {
        public string invoice { get; set; }

        public DateTime initialdate { get; set; }

        public DateTime finaldate { get; set; }

        public DateTime invoicedate { get; set; }

        public string user { get; set; }

        public string source { get; set; }

        public double value { get; set; }

        public string eps { get; set; }

        public string status { get; set; }

        public DateTime? locateddate { get; set; }

        public string observations { get; set; }

        public string dbstatus { get; set; }

        public DateTime? senddate { get; set; }
    }
}
