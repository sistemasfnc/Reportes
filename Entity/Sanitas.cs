using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Sanitas
    {
        public int counter { get; set; }

        public string branch { get; set; }

        public string typebranch { get; set; }

        public string idbranch { get; set; }

        public string insurance { get; set; }

        public string patientname { get; set; }

        public string patientdoctype { get; set; }

        public string patientdocument { get; set; }

        public char gender { get; set; }

        public DateTime birthday { get; set; }

        public string cellphone { get; set; }

        public string phone { get; set; }

        public string email { get; set; }
        
        public DateTime servicedate { get; set; }

        public string servicesource { get; set; }

        public string servicecode { get; set; }

        public string servicename { get; set; }

        public int qty { get; set; }

        public string priority { get; set; }

        public DateTime requestdate { get; set; }

        public string observation { get; set; }

        public string justification { get; set; }

        public string cie10code { get; set; }

        public string cie10name { get; set; }        

        public string etiology { get; set; }

        public string professional { get; set; }

        public string speciality { get; set; }

        public string profdoctype { get; set; }

        public string profdocument { get; set; }

        public string profphone { get; set; }
    }
}
