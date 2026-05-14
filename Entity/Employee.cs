using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Employee
    {
        public string sdocument { get; set; }

        public string sname { get; set; }

        public string slastname { get; set; }

        public string smaincostcenter { get; set; }

        public List<Cost> lCost { get; set; }

        public List<Cost> lCostTotal { get; set; }

        public int ihours { get; set; }

        public string ssurname { get; set; }
    }
}
