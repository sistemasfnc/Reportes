using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class RegistroCargo
    {
        public string status { get; set; }

        public DateTime date { get; set; }

        public string user { get; set; }

        public int idadmission { get; set; }

        public int idstatus { get; set; }
    }
}
