using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Baul
    {
        public int iid { get; set; }

        public string saccess { get; set; }

        public string suser { get; set; }

        public string spassword { get; set; }

        public string srol { get; set; }

        public string sdetail { get; set; }

        public string screatedby { get; set; }

        public string smodifiedby { get; set; }
    }
}
