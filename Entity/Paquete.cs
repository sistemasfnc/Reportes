using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Paquete
    {
        public string scodigo { get; set; }

        public string sconcepto { get; set; }

        public string sservicio { get; set; }

        public string scentro { get; set; }

        public string starifa { get; set; }

        public int icantidad { get; set; }

        public int ivalor { get; set; }

        public string snombre { get; set; }

        public string sunidad { get; set; }

        public int ivalorpaquete { get; set; }
    }
}
