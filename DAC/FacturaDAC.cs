using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using Config;
using EventLog;
using System.Data;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;

namespace DAC
{
    public class FacturaDAC : IDisposable
    {
        private string sConnection { get; set; }

        public FacturaDAC(string Connection)
        {
            this.sConnection = Connection;
        }

        public List<Invoice> GetInvoiceList(Invoice oEntity)
        {
            DataTable dt = new DataTable();
            List<Invoice> lInvoice = new List<Invoice>();
            try
            {
                dt = this.GetAll(oEntity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Invoice()
                    {
                        invoice = dt.Rows[i]["Factura"].ToString(),
                        eps = dt.Rows[i]["EPS"].ToString(),
                        status = dt.Rows[i]["Estado"].ToString(),
                        invoicedate = Convert.ToDateTime(dt.Rows[i]["FechaFactura"]),
                        locateddate = (dt.Rows[i]["FechaRadicado"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["FechaRadicado"]) : (Nullable<DateTime>)null,
                        value = Convert.ToDouble(dt.Rows[i]["Valor"]),
                        source = dt.Rows[i]["Fuente"].ToString(),
                        user = dt.Rows[i]["Usuario"].ToString(),
                        dbstatus = dt.Rows[i]["EstadoBase"].ToString(),
                        observations = dt.Rows[i]["Pendientes"].ToString(),     
                    };
                    lInvoice.Add(oEntity);
                }
                return lInvoice;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de facturas");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }
        
        private DataTable GetAll(Invoice oEntity)
        {
            StringBuilder sQuery = new StringBuilder("SELECT * FROM VFactura WHERE 1 = 1");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            if (!string.IsNullOrEmpty(oEntity.invoice))
            {
                sQuery.Append(" AND Factura = :invoice");
                lParameters.Add(new OracleParameter(":invoice", oEntity.invoice));                
            }
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FechaFactura, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":finaldate", oEntity.finaldate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FechaFactura, 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (!string.IsNullOrEmpty(oEntity.eps))
            {
                sQuery.Append(" AND UPPER(EPS) LIKE UPPER('%' || :eps || '%')");
                lParameters.Add(new OracleParameter(":eps", oEntity.eps));
            }
            if (!string.IsNullOrEmpty(oEntity.status))
            {
                sQuery.Append(" AND Estado = :status");
                lParameters.Add(new OracleParameter(":status", oEntity.status));                
            }
            if (!string.IsNullOrEmpty(oEntity.source))
            {
                sQuery.Append(" AND Empresa = :source");
                lParameters.Add(new OracleParameter(":source", oEntity.source));
            }
            sQuery.Append(" ORDER BY FechaFactura DESC");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        public List<Support> GetPendings(string invoice)
        {
            List<Support> lSupport = new List<Support>();
            DataTable dt = new DataTable();
            Support oEntity = null;
            try
            {
                dt = this.GetPendingData(invoice);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Support()
                    {
                        id = Convert.ToInt32(dt.Rows[i]["pf_pn_id"]),
                        code = invoice,
                        observation = dt.Rows[i]["pf_observacion"].ToString(),
                        name = dt.Rows[i]["pn_nombre"].ToString(),
                    };
                    lSupport.Add(oEntity);
                }
                return lSupport;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de pendientes por factura");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de pendientes por factura");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        private DataTable GetPendingData(string invoice)
        {
            StringBuilder sQuery = new StringBuilder("SELECT pendientefactura.*, pn_nombre FROM pendiente, pendientefactura WHERE pf_pn_id = pn_id AND pf_factura = :idinvoice");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idinvoice", invoice));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        public void InsertPending(List<Support> lEntity)
        {
            OracleDAC oDAC = new OracleDAC();
            try
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                this.DeletePendings(lEntity[0].code, oDAC);
                for (int i = 0; i < lEntity.Count; i++)
                {
                    this.InsertPending(lEntity[i], oDAC);
                }
                oDAC.Commit();
            }
            catch (Exception ex)
            {
                if (oDAC.oracleTransaction != null) oDAC.RollBack();
                LogError.WriteError("Facturacion", "DAC", ex);
                throw;
            }
            finally
            {
                oDAC.Dispose();
                oDAC = null;
            }
        }

        private void InsertPending(Support oEntity, OracleDAC oDAC)
        {
            string sQuery = "INSERT INTO pendientefactura (pf_factura, pf_pn_id, pf_observacion) VALUES (:invoice, :idpending, :observation)";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":invoice", oEntity.code));
            lParameters.Add(new OracleParameter(":idpending", oEntity.id));
            lParameters.Add(new OracleParameter(":observation", oEntity.observation));            
            oDAC.ExecuteNonQuery(sQuery, lParameters, false, true);
            lParameters = null;
        }

        private void DeletePendings(string invoice, OracleDAC oDAC)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":invoice", invoice));
            oDAC.ExecuteNonQuery("DELETE FROM pendientefactura WHERE pf_factura = :invoice", lParameters, false, true);
            lParameters = null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
