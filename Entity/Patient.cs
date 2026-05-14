using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class Patient : User
    {
        public string document { get; set; }

        public int ensuranceid { get; set; }
    }
}
