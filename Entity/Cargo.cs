using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Cargo
    {
        public string idadmission { get; set; }

        public int companytype { get; set; }

        public string user { get; set; }

        public string service { get; set; }

        public int? status { get; set; }

        public decimal value { get; set; }

        public decimal adding { get; set; }

        public string plan { get; set; }

        public DateTime date { get; set; }

        public decimal surplus { get; set; }

        public DateTime initialdate { get; set; }

        public DateTime finaldate { get; set; }

        public string company { get; set; }

        public int id { get; set; }

        public int iduser { get; set; }

        public DateTime fcharge { get; set; }

        public List<Support> lSupport { get; set; }

        public List<Support> lReason { get; set; }

        public string lastuser { get; set; }

        public List<RegistroCargo> lLog { get; set; }

        public string invoice { get; set; }

        public string eps { get; set; }

        public decimal invoicevalue { get; set; }

        public int? canceled { get; set; }

        public string invoiced { get; set; }

        public string authorization { get; set; }

        public string notuser { get; set; }

        public DateTime invoicedate { get; set; }

        public string costcenter { get; set; }

        public string subcenter { get; set; }

        public string costname { get; set; }

        public string subcentername { get; set; }

        public string patientname { get; set; }

        public string patientdocument { get; set; }

        public string patientsurname { get; set; }

        public string patientfullname { get; set; }

        public string documenttype { get; set; }

        public string sattentiontype { get; set; }

        public string scharge { get; set; }

        public int iline { get; set; }

        public int iqty { get; set; }

        public string ssource { get; set; }

        public int iidprofile { get; set; }
    }

    [Serializable]
    public class Devolucion
    {
        public string idadmission { get; set; }

        public string user { get; set; }

        public string eps { get; set; }

        public DateTime date { get; set; }

        public DateTime? returndate { get; set; }

        public DateTime? recievedate { get; set; }

        public DateTime? returnsenddate { get; set; }
        
        public DateTime? senddate { get; set; }

        public DateTime? centraldate { get; set; }

        public string centraluser { get; set; }

        public DateTime? readytoinvoicedate { get; set; }

        public string reasontext { get; set; }

        public string centralrecieveuser { get; set; }

        public string authorization { get; set; }

        public string company { get; set; }
    }

}
