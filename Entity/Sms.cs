using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Sms
    {
        public long id { get; set; }

        public string cellphone { get; set; }

        public string name { get; set; }

        public DateTime schedule { get; set; }

        public string resource { get; set; }

        public DateTime senddate { get; set; }

        public string message { get; set; }

        public string result { get; set; }

    }
}
