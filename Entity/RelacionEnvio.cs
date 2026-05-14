using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    /// <summary>
    /// Objeto que almacena las relaciones de envío
    /// </summary>
    [Serializable]
    public class RelacionEnvio
    {
        /// <summary>
        /// Identificador del registro
        /// </summary>
        public long iid { get; set; }

        /// <summary>
        /// Número de la relación de envío
        /// </summary>
        public string snumero { get; set; }

        /// <summary>
        /// Fecha de la relación de envío
        /// </summary>
        public DateTime dtfecha { get; set; }

        /// <summary>
        /// Estado de la relación de envío: S = Sin Enviar, P = Con Pendientes, E = Enviada Total
        /// </summary>
        public char cestado { get; set; }

        /// <summary>
        /// Detalle de facturas enviadas a la relación de envío
        /// </summary>
        public List<DetalleRelacion> lDetalle { get; set; }

        /// <summary>
        /// Usuario que crea la relación
        /// </summary>
        public int iusuario { get; set; }

        /// <summary>
        /// Fecha inicial para filtro de búsqueda
        /// </summary>
        public DateTime dtfechainicial { get; set; }

        /// <summary>
        /// Fecha final para filtro de búsqueda
        /// </summary>
        public DateTime dtfechafinal { get; set; }

        /// <summary>
        /// Registro de la relación de envío
        /// </summary>
        public RelacionLog oLog { get; set; }

        /// <summary>
        /// Fecha recepción de envío
        /// </summary>
        public DateTime dtfecharecepcion { get; set; }

        /// <summary>
        /// Objeto detalle relación de envío
        /// </summary>
        public DetalleRelacion oDetalle { get; set; }

        /// <summary>
        /// String aseguradora que se le hizo la relación de envío
        /// </summary>
        public string sempresa { get; set; }

        /// <summary>
        /// String estado de la radicación en Servinte
        /// </summary>
        public string sestado { get; set; }

    }

    /// <summary>
    /// Objeto que almacena el detalle de las relaciones de envío
    /// </summary>
    [Serializable]
    public class DetalleRelacion
    {
        /// <summary>
        /// Identificador de la relación
        /// </summary>
        public int iid { get; set; }

        /// <summary>
        /// Número de factura
        /// </summary>
        public int ifactura { get; set; }

        /// <summary>
        /// Identificador de la fuente       
        /// </summary>
        public string sfuente { get; set; }

        /// <summary>
        /// Indica si la factura fue asignada
        /// </summary>
        public short iasignado { get; set; }

        /// <summary>
        /// Indica la fecha de asignación de la factura
        /// </summary>
        public DateTime dtfechaasignado { get; set; }

        /// <summary>
        /// Indica si la factura fue enviada
        /// </summary>
        public short ienviado { get; set; }

        /// <summary>
        /// Indica la fecha de envío de la factura
        /// </summary>
        public DateTime dtfechaenviado { get; set; }

        /// <summary>
        /// Indica si la factura fue recibida
        /// </summary>
        public short irecibido { get; set; }

        /// <summary>
        /// Indica la fecha de recepción de la factura
        /// </summary>
        public DateTime dtrecibido { get; set; }

        /// <summary>
        /// Indica el usuario que tramita la relación
        /// </summary>
        public int iusuariologistica { get; set; }

        /// <summary>
        /// Indica el usuario que recibe la relación en facturación
        /// </summary>
        public int isuariofacturacion { get; set; }
    }

    [Serializable]
    public class RelacionLog
    {
        /// <summary>
        /// Id de la relación de envío
        /// </summary>
        public long iid { get; set; }

        /// <summary>
        /// Fecha de la observación
        /// </summary>
        public DateTime dtfecha { get; set; }

        /// <summary>
        /// Texto de la observación
        /// </summary>
        public string sobservacion { get; set; }

        /// <summary>
        /// Id usuario que realiza la observación
        /// </summary>
        public int iuser { get; set; }

        /// <summary>
        /// Nombre del usuario que envía la relación
        /// </summary>
        public string susuario { get; set; }
    }
        

}
