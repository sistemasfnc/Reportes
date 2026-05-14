using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    /// <summary>
    /// Objeto glosa, almacena la información de las glosas
    /// </summary>
    public class Glosa
    {
        /// <summary>
        /// Id autonumérico
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Número de factura
        /// </summary>
        public string invoice { get; set; }

        /// <summary>
        /// Número de ingreso
        /// </summary>
        public string charge { get; set; }

        /// <summary>
        /// Tipo de glosa. 1 = Devolución, 2 = Glosa
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// Fecha de trámite
        /// </summary>
        public DateTime transactdate { get; set; }

        /// <summary>
        /// Fecha de respuesta
        /// </summary>
        public DateTime? responsedate { get; set; }

        /// <summary>
        /// Observaciones
        /// </summary>
        public string observations { get; set; }
        
        /// <summary>
        /// Lista genérica que contiene los conceptos por glosa
        /// </summary>
        public List<ConceptoGlosa> lConcept { get; set; }

        /// <summary>
        /// Filtro fecha inicial
        /// </summary>
        public DateTime? initialdate { get; set; }

        /// <summary>
        /// Filtro fecha final
        /// </summary>
        public DateTime? finaldate { get; set; }
        
        /// <summary>
        /// Multicompañía
        /// </summary>
        public string company { get; set; }

        /// <summary>
        /// Valor de la glosa
        /// </summary>
        public decimal value { get; set; }

        /// <summary>
        /// Valor aceptado de la glosa
        /// </summary>
        public decimal accepted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string analysis { get; set; }
    }

    /// <summary>
    /// Objeto que almacena los conceptos por glosa
    /// </summary>
   [Serializable]
    public class ConceptoGlosa
    {
        /// <summary>
        /// Id de la glosa
        /// </summary>
        public int idcomment { get; set; }

        /// <summary>
        /// Id del concepto
        /// </summary>
        public int idconcept { get; set; }

        /// <summary>
        /// Nombre del concepto
        /// </summary>
        public string conceptname { get; set; }

        /// <summary>
        /// Código del concepto
        /// </summary>
        public string conceptcode { get; set; }

        /// <summary>
        /// Código del grupo del concepto
        /// </summary>
        public string conceptgroup { get; set; }

        /// <summary>
        /// Valor de aceptacion del concepto
        /// </summary>
        public decimal conceptvalue { get; set; }

        /// <summary>
        /// Observaciones del concepto
        /// </summary>
        public string conceptobservations { get; set; }

        /// <summary>
        /// Respuesta de la glosa
        /// </summary>
        public RespuestaGlosa oResponse { get; set; }
    }

    /// <summary>
    /// Objeto que maneja las respuestas de la glosa
    /// </summary>
    [Serializable]
    public class RespuestaGlosa
    {
        /// <summary>
        /// Id respuesta de la glosa
        /// </summary>
        public int idresponse { get; set; }

        /// <summary>
        /// Id de la glosa
        /// </summary>
        public int idcomment { get; set; }

        /// <summary>
        /// Id del concepto
        /// </summary>
        public int idconcept { get; set; }

        /// <summary>
        /// Nombre del concepto
        /// </summary>
        public string conceptname { get; set; }

        /// <summary>
        /// Nombre de la respuesta
        /// </summary>
        public string responsename { get; set; }

        /// <summary>
        /// Valor de glosa aceptado en la respuesta
        /// </summary>
        public int acceptedvalue { get; set; }

        /// <summary>
        /// Observaciones de la respuesta
        /// </summary>
        public string observations { get; set; }

        /// <summary>
        /// Código de la respuesta
        /// </summary>
        public string responsecode { get; set; }

        /// <summary>
        /// Id del servicio responsable
        /// </summary>
        public int idservice { get; set; }

        /// <summary>
        /// Ide de la categoría responsable
        /// </summary>
        public int idcategory { get; set; }

        /// <summary>
        /// Responsable de la glosa
        /// </summary>
        public string responsible { get; set; }
    }
}
