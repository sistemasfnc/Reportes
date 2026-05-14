using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Entity;
using Config;
using EventLog;
using Oracle.ManagedDataAccess.Client;
using System.Runtime.InteropServices;

namespace DAC
{
    public class RelacionDAC : IDisposable
    {
        /// <summary>
        /// Cadena de conexión a la base de datos
        /// </summary>
        private string ConnectionString { get; set; }

        /// <summary>
        /// Constructor de la clase, recibe la cadena de conexión a la base de datos y la asigna a la propiedad privada
        /// </summary>
        /// <param name="sConnection">Cadena de conexión</param>
        public RelacionDAC(string sConnection)
        {
            this.ConnectionString = sConnection;
        }

        /// <summary>
        /// Obtiene las facturas pertenecientes a una relación de envío (utiliza la BD de Gestor)
        /// </summary>
        /// <param name="oInvoice">Objeto que contiene los parámetros para filtrar la consulta</param>
        /// <returns>Lista genérica con las relaciones de envío</returns>
        public List<Invoice> GetInvoicesByRelation(Invoice oInvoice)
        {
            List<Invoice> lInvoices = new List<Invoice>();
            DataTable dt = new DataTable();            
            try
            {
                dt = this.GetInvoices(oInvoice);
                foreach (DataRow dr in dt.Rows)
                {
                    oInvoice = new Invoice()
                    {
                        invoice = dr["Factura"].ToString(),
                        eps = dr["EPS"].ToString(),
                        status = dr["Estado"].ToString(),
                        invoicedate = Convert.ToDateTime(dr["FechaFactura"]),
                        locateddate = (dr["FechaRadicado"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaRadicado"]) : (Nullable<DateTime>)null,
                        value = Convert.ToDouble(dr["Valor"]),
                        source = dr["Fuente"].ToString(),
                        user = dr["Usuario"].ToString(),
                        dbstatus = (dr["re_estado"] != DBNull.Value) ? dr["re_estado"].ToString() : string.Empty,
                        observations = dr["Radicado"].ToString(),
                        senddate = (dr["re_fecha"] != DBNull.Value) ? Convert.ToDateTime(dr["re_fecha"]) : (Nullable<DateTime>)null,
                    };
                    lInvoices.Add(oInvoice);
                }
                return lInvoices;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de relaciones de envío");
            }     
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }


        /// <summary>
        /// Consulta en la base de datos para obtener las relaciones de envío
        /// </summary>
        /// <param name="oEntity">Objeto factura para filtros de búsqueda</param>
        /// <returns>Data Table con las relaciones de envío seleccionadas</returns>
        private DataTable GetInvoices(Invoice oEntity)
        {
            StringBuilder sQuery = new StringBuilder("SELECT DISTINCT VFacturasRadicadas.*, re_id, re_fecha, re_estado FROM VFacturasRadicadas");
            sQuery.Append(" LEFT JOIN relacionenvio ON re_numero = Radicado WHERE 1 = 1");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            using (OracleDAC oDAC = new OracleDAC())
            {
                if (!string.IsNullOrEmpty(oEntity.observations))
                {
                    sQuery.Append(" AND Radicado = :radicado");
                    lParameters.Add(new OracleParameter(":radicado", oEntity.observations));
                }
                if (oEntity.initialdate.Year > 1)
                {
                    lParameters.Add(new OracleParameter(":initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                    sQuery.Append(" AND FechaFactura >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
                }
                if (oEntity.finaldate.Year > 1)
                {
                    lParameters.Add(new OracleParameter(":finaldate", oEntity.finaldate.ToString("yyyy-MM-dd")));
                    sQuery.Append(" AND FechaFactura <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
                }
                if (!string.IsNullOrEmpty(oEntity.eps))
                {
                    sQuery.Append(" AND UPPER(EPS) LIKE UPPER('%' || :eps || '%')");
                    lParameters.Add(new OracleParameter(":eps", oEntity.eps));
                }
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }


        /// <summary>
        /// Método que crea la relación de envío en la base de datos
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        public void CreateRelationship(RelacionEnvio oRelacion)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO relacionenvio (re_numero, re_fecha, re_estado) VALUES (:numero, :fecha, :estado) RETURNING re_id INTO :re_id");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            OracleParameter re_id = new OracleParameter("re_id", OracleDbType.Int32, ParameterDirection.ReturnValue);
            int iRelacion = 0;
            object oValue = null;
            using (OracleDAC oDAC = new OracleDAC())
            {
                lParameters.Add(new OracleParameter(":numero", oRelacion.snumero));
                lParameters.Add(new OracleParameter(":fecha", oRelacion.dtfecha));
                lParameters.Add(new OracleParameter(":estado", oRelacion.cestado));
                lParameters.Add(re_id);
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);
                oValue = re_id.Value.ToString();
                if (oValue != null)
                {
                    iRelacion = Convert.ToInt32(oValue.ToString());
                    this.CreateRelatiohsipDetail(oRelacion.lDetalle, iRelacion, oRelacion.iusuario);
                }
            }
            
        }

        /// <summary>
        /// Método que inserta el detalle de la relación de envío en la base de datos
        /// </summary>
        /// <param name="lDetalle">Lista genérica que contiene el detalle de las relaciones de envío</param>
        /// <param name="iRelacion">Id. de la relación de envío</param>
        /// <param name="iUsuario">Id. del usuario que envía la relación</param>
        private void CreateRelatiohsipDetail(List<DetalleRelacion> lDetalle, int iRelacion, int iUsuario)
        {            
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();               
                int[] ids = new int[lDetalle.Count];
                int[] invoices = new int[lDetalle.Count];
                string[] sources = new string[lDetalle.Count];
                short[] assigned = new short[lDetalle.Count];                
                int[] users = new int[lDetalle.Count];                
                int j = 0;
                foreach (DetalleRelacion item in lDetalle)
                {
                    ids[j] = iRelacion;
                    invoices[j] = item.ifactura;
                    sources[j] = item.sfuente;
                    assigned[j] = 1;                                        
                    users[j] = iUsuario;
                    j++;
                }                
                OracleParameter id = new OracleParameter();
                id.OracleDbType = OracleDbType.Int32;
                id.Value = ids;
                OracleParameter invoice = new OracleParameter();
                invoice.OracleDbType = OracleDbType.Int32;
                invoice.Value = invoices;
                OracleParameter source = new OracleParameter();
                source.OracleDbType = OracleDbType.Varchar2;
                source.Value = sources;
                OracleParameter assignedp = new OracleParameter();
                assignedp.OracleDbType = OracleDbType.Int16;
                assignedp.Value = assigned;
                OracleParameter user = new OracleParameter();
                user.OracleDbType = OracleDbType.Int32;
                user.Value = users;                
                OracleCommand cmd = oDAC.oracleConnection.CreateCommand();
                cmd.CommandText = "INSERT INTO DETALLERELACION (dr_re_id, dr_factura, dr_fuente, dr_asignado, dr_fechaasignado, dr_us_id) VALUES (:1, :2, :3, :4, SYSDATE, :5)";
                cmd.ArrayBindCount = ids.Length;
                cmd.Parameters.Add(id);
                cmd.Parameters.Add(invoice);
                cmd.Parameters.Add(source);
                cmd.Parameters.Add(assignedp);
                cmd.Parameters.Add(user);
                cmd.ExecuteNonQuery();
                
            }            
        }

        /// <summary>
        /// Método que obtiene el listado de relaciones de envío que se encuentran en trámite en la base de datos
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        /// <returns>Lista genérica que contiene las relaciones de envío</returns>
        public List<RelacionEnvio> GetRelationships(RelacionEnvio oRelacion)
        {
            List<RelacionEnvio> lRelacion = new List<RelacionEnvio>();
            DataTable dt = new DataTable();
            DetalleRelacion oDetalle = null;
            try
            {
                dt = this.GetRelationshipsDataTable(oRelacion);
                (from DataRow dr in dt.Rows
                 group dr by Convert.ToInt32(dr["re_id"]) into f
                 select new
                 {
                     Key = f.Key,
                     Elements = f,
                 }).ToList().ForEach(f =>
                 {
                     oRelacion = new RelacionEnvio()
                     {
                         iid = f.Key,
                         snumero = f.Elements.First()["re_numero"].ToString(),
                         dtfecha = Convert.ToDateTime(f.Elements.First()["re_fecha"]),
                         lDetalle = new List<DetalleRelacion>(),
                         cestado = Convert.ToChar(f.Elements.First()["re_estado"]),
                     };
                     (from DataRow dr in f.Elements where Convert.ToInt32(dr["dr_re_id"]) == f.Key group dr by dr["dr_factura"].ToString() into a
                        select new
                        {
                            Key = a.Key,
                            Elements = a,
                        }).ToList().ForEach(a =>
                        {
                            oDetalle = new DetalleRelacion()
                            {
                                ifactura = Convert.ToInt32(a.Elements.First()["dr_factura"]),
                                sfuente = a.Elements.First()["dr_fuente"].ToString(),
                                iid = f.Key,
                                dtfechaasignado = (a.Elements.First()["dr_fechaasignado"] != DBNull.Value) ? Convert.ToDateTime(a.Elements.First()["dr_fechaasignado"]) : new DateTime(1900, 1, 1),
                                dtfechaenviado = (a.Elements.First()["dr_fechaenviado"] != DBNull.Value) ? Convert.ToDateTime(a.Elements.First()["dr_fechaenviado"]) : new DateTime(1900, 1, 1),
                                iasignado = Convert.ToInt16(a.Elements.First()["dr_asignado"]),
                                ienviado = Convert.ToInt16(a.Elements.First()["dr_enviado"]),
                                dtrecibido = (a.Elements.First()["dr_fecharecibido"] != DBNull.Value) ? Convert.ToDateTime(a.Elements.First()["dr_fecharecibido"]) : new DateTime(1900, 1, 1),
                                irecibido = Convert.ToInt16(a.Elements.First()["dr_recibido"]),
                            };
                            oRelacion.lDetalle.Add(oDetalle);
                    });
                    lRelacion.Add(oRelacion);
                 });
                 return lRelacion;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de las relaciones de envío");                
            }
            finally
            {
                dt.Dispose();
                dt = null;                
            }
        }

        /// <summary>
        /// Método para obtener el listado de observaciones para una relación de envío
        /// </summary>
        /// <param name="iRelacion">Id de la relación</param>
        /// <returns>Lista genérica con las observaciones para una relación de envío</returns>
        public List<RelacionLog> GetLog(long iRelacion)
        {
            StringBuilder sQuery = new StringBuilder("SELECT relacionobservacion.*, us_usuario FROM relacionobservacion, usuario WHERE us_id = ro_us_id AND ro_re_id = :id");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            DataTable dt = new DataTable();
            OracleDAC oDAC = new OracleDAC();
            List<RelacionLog> lLog = new List<RelacionLog>();
            RelacionLog oEntity = null;
            try
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                lParameters.Add(new OracleParameter(":id", iRelacion));
                dt = oDAC.GetDataTable(sQuery.ToString(), lParameters);
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new RelacionLog()
                    {
                        iid = iRelacion,
                        dtfecha = Convert.ToDateTime(dr["ro_fecha"]),
                        iuser = Convert.ToInt32(dr["ro_us_id"]),
                        sobservacion = dr["ro_observacion"].ToString(),
                        susuario = dr["us_usuario"].ToString(),
                    };
                    lLog.Add(oEntity);
                }
                return lLog;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de observaciones para la relación de envío");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
                oDAC.Dispose();
                oDAC = null;
                lParameters = null;
                sQuery = null;
            }
        }

        /// <summary>
        /// Método que obtiene el Data Table de las relaciones de envío, ejecuta la consulta en las tablas acorde a los filtros
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        /// <returns>Data Table con la información encontrada</returns>
        private DataTable GetRelationshipsDataTable(RelacionEnvio oRelacion)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT * FROM relacionenvio, detallerelacion WHERE re_id = dr_re_id");
            using (OracleDAC oDAC = new OracleDAC())
            {
                if (!string.IsNullOrEmpty(oRelacion.snumero))
                {
                    lParameters.Add(new OracleParameter(":relacion", oRelacion.snumero));
                    sQuery.Append(" AND re_numero = :relacion");
                }
                if (oRelacion.cestado != '\0')
                {
                    lParameters.Add(new OracleParameter(":estado", oRelacion.cestado));
                    sQuery.Append(" AND re_estado = :estado");
                }
                if (oRelacion.dtfechainicial.Year > 1)
                {
                    lParameters.Add(new OracleParameter(":initialdate", oRelacion.dtfechainicial.ToString("yyyy-MM-dd")));
                    sQuery.Append(" AND TO_DATE(TO_CHAR(re_fecha, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
                }
                if (oRelacion.dtfechafinal.Year > 1)
                {
                    lParameters.Add(new OracleParameter(":finaldate", oRelacion.dtfechafinal.ToString("yyyy-MM-dd")));
                    sQuery.Append(" AND TO_DATE(TO_CHAR(re_fecha, 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
                }
                sQuery.Append(" ORDER BY re_fecha DESC");
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
                
            }
        }

        /// <summary>
        /// Método que genera el Data Table para hacer el bulk copy y crear el detalle de las relaciones de envío
        /// </summary>
        /// <param name="lDetalle">Lista genérica que contiene el detalle de las relaciones de envío</param>
        /// <param name="iRelacion">Id. de la relación de envío</param>
        /// <param name="iUsuario">Id. del usuario que envía la relación</param>
        /// <returns>Data Table con el detalle de las relaciones de envío</returns>
        private DataTable GetRelationshipTable(List<DetalleRelacion> lDetalle, int iRelacion, int iUsuario)
        {
            DataTable dt = new DataTable();
            object[] values = new object[12] { "dr_re_id", "dr_factura", "dr_fuente", "dr_asignado", "dr_fechaasignado", "dr_enviado", "dr_fechaenviado", "dr_recibido", "dr_fecharecibido", "dr_us_id", "dr_us_id_enviado", "dr_us_id_recibido" };
            try
            {
                for (int i = 0; i < values.Length; i++)
                {
                    dt.Columns.Add(values[i].ToString());
                }
                for (int i = 0; i < lDetalle.Count; i++)
                {
                    values = new object[12]
                    {
                        iRelacion,
                        lDetalle[i].ifactura,
                        lDetalle[i].sfuente,
                        1,
                        DateTime.Now,
                        0,
                        DBNull.Value,
                        0,
                        DBNull.Value,
                        iUsuario,
                        0,
                        0
                    };
                    dt.Rows.Add(values);
                }
                return dt;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al generar el data table detalle relaciones de envío");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                values = null;
            }
        }

        /// <summary>
        /// Método que almacena los nuevos estados de las facturas de la relación de envío
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        /// <param name="iType">Tipo de update para la relación de envío (1 para envío, 2 para tramitado)</param>
        public void SaveInvoices(RelacionEnvio oRelacion, int iType)
        {
            OracleDAC oDAC = new OracleDAC();
            StringBuilder sQuery = new StringBuilder();
            List<OracleParameter> lParameters = new List<OracleParameter>();
            try
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                //Inicia transacción
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                foreach (DetalleRelacion oDetalle in oRelacion.lDetalle)
                {
                    sQuery.Append("UPDATE detallerelacion SET ");
                    if (iType == 1)
                    {
                        sQuery.Append(" dr_enviado = :enviado, dr_fechaenviado = :fechaenviado, dr_us_id_enviado = :usuario");
                        lParameters.Add(new OracleParameter(":enviado", oDetalle.ienviado));
                        lParameters.Add(new OracleParameter(":fechaenviado", oDetalle.dtfechaenviado));
                        lParameters.Add(new OracleParameter(":usuario", oDetalle.iusuariologistica));
                    }
                    else
                    {
                        sQuery.Append(" dr_recibido = :recibido, dr_fecharecibido = :fecharecibido, dr_us_id_recibido = :usuario");
                        lParameters.Add(new OracleParameter(":recibido", oDetalle.irecibido));
                        lParameters.Add(new OracleParameter(":fecharecibido", oDetalle.dtrecibido));
                        lParameters.Add(new OracleParameter(":usuario", oDetalle.isuariofacturacion));
                    }
                    sQuery.Append(" WHERE dr_factura = :factura AND dr_re_id = :id");
                    lParameters.Add(new OracleParameter(":factura", oDetalle.ifactura));
                    lParameters.Add(new OracleParameter(":id", oRelacion.iid));
                    oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false, true);
                    sQuery.Remove(0, sQuery.Length);
                    lParameters = new List<OracleParameter>();
                }
                sQuery.Append("UPDATE relacionenvio SET re_estado = :estado");
                if (oRelacion.cestado == 'T')
                {
                    sQuery.Append(", re_fechacompleto = SYSDATE, re_us_completo = :usuario");
                    lParameters.Add(new OracleParameter(":usuario", oRelacion.iusuario));
                }
                sQuery.Append(" WHERE re_id = :id");
                lParameters.Add(new OracleParameter(":estado", oRelacion.cestado));
                lParameters.Add(new OracleParameter(":id", oRelacion.iid));
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false, true);
                if (oRelacion.oLog != null)
                {
                    this.InsertObservation(oRelacion.oLog, oDAC);
                }
                oDAC.Commit();
            }
            catch (Exception ex)
            {
                //En caso de error se aborta la transacción y se devuelven los cambios
                if (oDAC.oracleConnection.State == ConnectionState.Open) oDAC.RollBack();
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al ingresar la información de las faturas para la relación de envio");
            }
            finally
            {
                oDAC.Dispose();
                oDAC = null;
            }
        }

        /// <summary>
        /// Método que almacena en la base de datos la observacion agregada al envío de facturas de la relación
        /// </summary>
        /// <param name="oEntity">Objeto log de relación de envío</param>
        /// <param name="oDAC">Objeto conexión a base de datos</param>
        private void InsertObservation(RelacionLog oEntity, OracleDAC oDAC)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO relacionobservacion (ro_re_id, ro_observacion, ro_fecha, ro_us_id) VALUES ");
            sQuery.Append("(:id, :observacion, SYSDATE, :usuario)");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":id", oEntity.iid));
            lParameters.Add(new OracleParameter(":observacion", oEntity.sobservacion));
            lParameters.Add(new OracleParameter(":usuario", oEntity.iuser));                        
            oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false, true);
            lParameters = null;
            sQuery = null;
        }
               
        /// <summary>
        /// Método que libera la memoria
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();            
        }

        /// <summary>
        /// Método para actualizar el estado de la relación de envío
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        public void UpdateRelationship(RelacionEnvio oRelacion)
        {
            StringBuilder sQuery = new StringBuilder("UPDATE relacionenvio SET re_estado = :estado");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            if (oRelacion.dtfecharecepcion.Year != 1900)
            {
                sQuery.Append(", re_fecharecibido = :fecharecibido");
                lParameters.Add(new OracleParameter(":fecharecibido", oRelacion.dtfecharecepcion));
            }
            sQuery.Append(" WHERE re_id = :id");
            lParameters.Add(new OracleParameter(":estado", oRelacion.cestado));
            lParameters.Add(new OracleParameter(":id", oRelacion.iid));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);
            }
            sQuery = null;
            lParameters = null;
        }

        /// <summary>
        /// Método para generar el reporte de facturas y relaciones de envío
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío con los filtros de búsqueda</param>
        /// <returns>DataTable con las relaciones de envío y facturas encontradas</returns>
        public DataTable GetRelationshipsReport(RelacionEnvio oRelacion)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT * FROM VRelacionesFacturas WHERE 1 = 1");
            using (OracleDAC oDAC = new OracleDAC())
            {
                if (!string.IsNullOrEmpty(oRelacion.snumero))
                {
                    sQuery.Append(" AND Numero = :numero");
                    lParameters.Add(new OracleParameter(":numero", oRelacion.snumero));
                }
                if (oRelacion.cestado != '\0')
                {
                    lParameters.Add(new OracleParameter(":estado", oRelacion.cestado));
                    sQuery.Append(" AND Estado = :estado");
                }
                if (oRelacion.dtfechainicial.Year > 1)
                {
                    lParameters.Add(new OracleParameter(":initialdate", oRelacion.dtfechainicial.ToString("yyyy-MM-dd")));
                    sQuery.Append(" AND TO_DATE(TO_CHAR(Fecha, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
                }
                if (oRelacion.dtfechafinal.Year > 1)
                {
                    lParameters.Add(new OracleParameter(":finaldate", oRelacion.dtfechafinal.ToString("yyyy-MM-dd")));
                    sQuery.Append(" AND TO_DATE(TO_CHAR(Fecha, 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
                }
                if (oRelacion.oDetalle.iasignado != 2)
                {
                    lParameters.Add(new OracleParameter(":asignado", oRelacion.oDetalle.iasignado));
                    sQuery.Append(" AND iAsgnado = :asignado");
                }
                if (oRelacion.oDetalle.ienviado != 2)
                {
                    lParameters.Add(new OracleParameter(":enviado", oRelacion.oDetalle.ienviado));
                    sQuery.Append(" AND iEnviado = :enviado");
                }
                if (oRelacion.oDetalle.irecibido != 2)
                {
                    lParameters.Add(new OracleParameter(":recibido", oRelacion.oDetalle.irecibido));
                    sQuery.Append(" AND iRecibido = :recibido");
                }
                if (oRelacion.oDetalle.ifactura != 0)
                {
                    lParameters.Add(new OracleParameter(":factura", oRelacion.oDetalle.ifactura));
                    sQuery.Append(" AND Factura = :factura");
                }
                sQuery.Append(" ORDER BY Fecha DESC");
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
                
            }
        }

        /// <summary>
        /// Método para obtener las relaciones de envío y cuentas de facturas para generación de documento de mensajería
        /// </summary>
        /// <param name="sRelationships">Listado de números de relaciones de envío separados por ',</param>
        /// <returns></returns>
        public DataTable GenerateTemplate(string sRelationships)
        {
            StringBuilder sQuery = new StringBuilder("SELECT re_fecha Fecha, re_numero Numero, Empresa, Direccion, us_usuario Usuario, COUNT(1) Cantidad");
            sQuery.Append(" FROM relacionenvio INNER JOIN VFacturasRadicadas ON re_numero = Radicado INNER JOIN detallerelacion ON re_id = dr_re_id INNER JOIN usuario ON us_id = dr_us_id");            
            sQuery.Append(" WHERE re_id IN (");
            sQuery.Append(sRelationships);
            sQuery.Append(") GROUP BY re_fecha, re_numero, Empresa, Direccion, us_usuario");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }
    }
}
