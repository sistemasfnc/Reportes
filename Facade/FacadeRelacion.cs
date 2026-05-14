using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;
using System.Data;

namespace Facade
{
    public class FacadeRelacion : IDisposable
    {

        /// <summary>
        /// Cadena de conexión a la base de datos
        /// </summary>
        private string sConnection { get; set; }

        /// <summary>
        /// Constructor de la clase, asigna cadena de conexión al objeto
        /// </summary>
        /// <param name="Connection">Cadena de conexión</param>
        public FacadeRelacion(string Connection)
        {
            this.sConnection = Connection;
        }

        /// <summary>
        /// Fachada para obtener las facturas pertenecientes a una relación de envío
        /// </summary>
        /// <param name="oEntity">Objeto factura</param>
        /// <returns>Lsita genérica con las facturas</returns>
        public List<Invoice> GetInvoicesByRelation(Invoice oEntity)
        {
            using (RelacionDAC oRelacion = new RelacionDAC(this.sConnection))
            {
                return oRelacion.GetInvoicesByRelation(oEntity);
            }
        }

        /// <summary>
        /// Fachada de método que inserta las relaciones de envío en la base de datos
        /// </summary>
        /// <param name="oEntity"></param>
        public void CreateRelation(RelacionEnvio oEntity)
        {
            using (RelacionDAC oRelacion = new RelacionDAC(this.sConnection))
            {
                oRelacion.CreateRelationship(oEntity);
            }
        }

        /// <summary>
        /// Fachada de método que obtiene las relaciones de envío de la base de datos
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        /// <returns>Lista genérica con las relaciones de envío encontradas</returns>
        public List<RelacionEnvio> GetRelationships(RelacionEnvio oEntity)
        {
            using (RelacionDAC oDAC = new RelacionDAC(this.sConnection))
            {
                return oDAC.GetRelationships(oEntity);
            }
        }

        /// <summary>
        /// Fachada de método que almacena los nuevos estados de las facturas de la relación de envío
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        /// <param name="iType">Tipo de update para la relación de envío (1 para envío, 2 para tramitado)</param>
        public void SaveInvoices(RelacionEnvio oRelacion, int iType)
        {
            using (RelacionDAC oDAC = new RelacionDAC(this.sConnection))
            {
                oDAC.SaveInvoices(oRelacion, iType);
            }
        }

        /// <summary>
        /// Fachada de método para obtener el listado de observaciones para una relación de envío
        /// </summary>
        /// <param name="iRelacion">Id de la relación de envío</param>
        /// <returns>Lista genérica con las observaciones para una relación de envío</returns>
        public List<RelacionLog> GetLog(long iRelacion)
        {
            using (RelacionDAC oDAC = new RelacionDAC(this.sConnection))
            {
                return oDAC.GetLog(iRelacion);
            }
        }

        /// <summary>
        /// Fachada de método para actualizar el estado de la relación de envío
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        public void UpdateRelationship(RelacionEnvio oRelacion)
        {
            using (RelacionDAC oDAC = new RelacionDAC(this.sConnection))
            {
                oDAC.UpdateRelationship(oRelacion);
            }
        }

        /// <summary>
        /// Fachada de método para obtener las relaciones de envío y cuentas de facturas para generación de documento de mensajería
        /// </summary>
        /// <param name="sRelationships">Listado de números de relaciones de envío separados por ',</param>
        /// <returns></returns>
        public DataTable GenerateTemplate(string sRelationships)
        {
            using (RelacionDAC oDAC = new RelacionDAC(this.sConnection))
            {
                return oDAC.GenerateTemplate(sRelationships);
            }
        }

        /// <summary>
        /// Facahda de método para generar el reporte de facturas y relaciones de envío
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío con los filtros de búsqueda</param>
        /// <returns>DataTable con las relaciones de envío y facturas encontradas</returns>
        public DataTable GetRelationshipsReport(RelacionEnvio oRelacion)
        {
            using (RelacionDAC oDAC = new RelacionDAC(this.sConnection))
            {
                return oDAC.GetRelationshipsReport(oRelacion);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
