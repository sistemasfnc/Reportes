using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Support
    {
        public int id { get; set; }

        public string observation { get; set; }

        public int idcharge { get; set; }

        public string response { get; set; }

        public string name { get; set; }

        public string code { get; set; }
    }
}
