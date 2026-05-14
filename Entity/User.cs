using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class User
    {
        public int id { get; set; }

        public string username { get; set; }

        public string firstname { get; set; }

        public string lastname { get; set; }

        public string password { get; set; }

        public DateTime lastlogin { get; set; }

        public string email { get; set; }

        public int idprofile { get; set; }

        public string profilename { get; set; }

        public List<string> otheruser { get; set; }

        public List<Security> lSecurity { get; set; }

        public string costcenter { get; set; }

        public List<CostUser> lCost { get; set; }

        public string spagina { get; set; }
    }
}
