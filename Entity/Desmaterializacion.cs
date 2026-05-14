using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Desmaterializacion
    {
        public string sfuenterelacion { get; set; }

        public string srelacion { get; set; }

        public string sfactura { get; set; }

        public string sfuentefactura { get; set; }

        public string sfechafactura { get; set; }

        public string singreso { get; set; }

        public string scups { get; set; }

        public string sservicio { get; set; }

        public string scantidad { get; set; }

        public string svalorunitario { get; set; }

        public string svalorfacturado { get; set; }

        public string scopago { get; set; }

        public string sautorizacion { get; set; }

        public string sdiagnostico { get; set; }

        public string stipodocumento { get; set; }

        public string sdocumento { get; set; }

        public string sprimernombre { get; set; }

        public string ssegundonombre { get; set; }

        public string sprimerapellido { get; set; }

        public string ssegundoapellido { get; set; }

        public string sfechaingreso { get; set; }

        public string sfechaegreso { get; set; }

        public string scausaexterna { get; set; }

        public string sarchivo { get; set; }

        public string stiponota { get; set; }

        public string svalornota { get; set; }

        public string snotadebito { get; set; }

        public string svalordebito { get; set; }

        public string sarchivonc { get; set; }

        public string sarchivond { get; set; }

        public string sepisodio { get; set; }

        public bool bmultiple { get; set; }
    }
}
