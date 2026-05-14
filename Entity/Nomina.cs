using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class NominaStatus
    {
        public string document { get; set; }

        public string inccode { get; set; }

        public DateTime incdate { get; set; }

        public DateTime incfdate { get; set; }

        public string incnum { get; set; }

        public int incdays { get; set; }

        public int status { get; set; }

        public DateTime statusdate { get; set; }

        public DateTime initialdate { get; set; }

        public DateTime finaldate { get; set; }

        public string inccodename { get; set; }

        public string observations { get; set; }

        public int value { get; set; }

        public string diagnosis { get; set; }
    }
    
    [Serializable]
    public class Nomina : NominaStatus
    {       
        public string name { get; set; }

        public string eps { get; set; }
       
        public int incvalue { get; set; }

        public DateTime date { get; set; }

        public string costcenter { get; set; }                
    }    
}
