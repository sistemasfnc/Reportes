using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Cost
    {
        public string scode { get; set; }

        public string sname { get; set; }                

        public decimal dvalue { get; set; }

        public long id { get; set; }

        public int iuser { get; set; }

        public char ctype { get; set; }

        public short istatus { get; set; }

        public decimal dtotal { get; set; }
    }

    [Serializable]
    public class CostProcess
    {
        public int imonth { get; set; }

        public int iyear { get; set; }

        public int iuser { get; set; }

        public int iclosed { get; set; }

        public List<Cost> lCost { get; set; }

        public string sdocument { get; set; }

        public int itype { get; set; }

        public int iid { get; set; }

        public decimal dtotal { get; set; }

        public int ihours { get; set; }

        public bool bisrooter { get; set; }

        public Employee oEmployee { get; set; }

        public string scostcenter { get; set; }
    }

    [Serializable]
    public class CostUser
    {
        public int iuser { get; set; }

        public string scode { get; set; }
    }

    [Serializable]
    public class CostReport
    {
        public int imonth { get; set; }

        public int iyear { get; set; }

        public int iuser { get; set; }

        public string suser { get; set; }

        public string scode { get; set; }

        public decimal dvalue { get; set; }

        public string sdocument { get; set; }

        public string sfirstname { get; set; }

        public string slastname { get; set; }

        public string smaincostcenter { get; set; }

        public string smonth { get; set; }

        public decimal dtotal { get; set; }

        public string sstatus { get; set; }

        public string sname
        {
            get { return sfirstname.Trim() + " " + slastname.Trim(); }
        }
    }
}
