using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Entity;
using Config;
using EventLog;
using Oracle.ManagedDataAccess.Client;
using System.Security.Cryptography;

namespace DAC
{
    public class CargosDAC : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private string ConnectionString { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sConnection"></param>
        public CargosDAC(string sConnection)
        {
            this.ConnectionString = sConnection;
        }

        #region Métodos públicos

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idadmission"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<Cargo> GetChargesLog(string idadmission, string company)
        {
            List<Cargo> lCargo = new List<Cargo>();
            DataTable dt = new DataTable();
            Cargo oCargo = null;
            RegistroCargo oLog = null;
            try
            {
                dt = this.GetChargesLogData(idadmission, company);
                (from DataRow dr in dt.Rows
                 group dr by Convert.ToInt32(dr["id"]) into f
                 select new
                 {
                     Key = f.Key,
                     Elements = f,
                 }).ToList().ForEach(f =>
                 {
                     oCargo = new Cargo()
                     {
                         id = f.Key,
                         date = Convert.ToDateTime(f.Elements.First()["Fecha"]),
                         eps = f.Elements.First()["EPS"].ToString().Trim(),
                         idadmission = f.Elements.First()["Ingreso"].ToString(),
                         plan = f.Elements.First()["Plan"].ToString().Trim(),
                         service = f.Elements.First()["Servicio"].ToString(),
                         adding = Convert.ToDecimal(f.Elements.First()["Excedente"]),
                         surplus = (f.Elements.First()["Abono"] != DBNull.Value) ? Convert.ToDecimal(f.Elements.First()["Abono"]) : 0,
                         value = Convert.ToDecimal(f.Elements.First()["Total"]),
                         company = company,
                         status = (f.Elements.First()["ca_es_id"] != DBNull.Value) ? Convert.ToInt32(f.Elements.First()["ca_es_id"]) : 0,
                         user = f.Elements.First()["usuario"].ToString().ToLower(),
                         lLog = new List<RegistroCargo>(),
                         invoice = f.Elements.First()["Factura"].ToString(),
                         invoicevalue = (f.Elements.First()["ValorFacturado"] != DBNull.Value) ? Convert.ToDecimal(f.Elements.First()["ValorFacturado"]) : 0,
                         canceled = (f.Elements.First()["Anulado"] != DBNull.Value) ? Convert.ToInt32(f.Elements.First()["Anulado"]) : 0,
                         authorization = f.Elements.First()["Autorizacion"].ToString(),
                     };
                     (from DataRow dr in f.Elements
                      where Convert.ToInt32(dr["id"]) == f.Key
                      group dr by dr["cl_id"].ToString() into a
                      select new
                      {
                          Key = a.Key,
                          Elements = a,
                      }).ToList().ForEach(a =>
                      {
                          oLog = new RegistroCargo()
                          {
                              date = (a.Elements.First()["cl_fecha"] != DBNull.Value) ? Convert.ToDateTime(a.Elements.First()["cl_fecha"]) : new DateTime(),
                              idadmission = f.Key,
                              status = a.Elements.First()["es_nombre"].ToString(),
                              user = a.Elements.First()["us_usuario"].ToString(),
                              idstatus = (a.Elements.First()["cl_es_id"] != DBNull.Value) ? Convert.ToInt32(a.Elements.First()["cl_es_id"]) : 0,
                          };
                          oCargo.lLog.Add(oLog);
                      });
                     lCargo.Add(oCargo);
                 });
                return lCargo;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de motivos por cargo");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<Cargo> GetChargesInvoice(Cargo oEntity)
        {
            List<Cargo> lCargo = new List<Cargo>();
            DataTable dt = new DataTable();
            try
            {
                dt = this.GetChargesInvoiceData(oEntity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Cargo()
                    {
                        date = Convert.ToDateTime(dt.Rows[i]["FechaIngreso"]),
                        invoicedate = Convert.ToDateTime(dt.Rows[i]["FechaFactura"]),
                        idadmission = dt.Rows[i]["Ingreso"].ToString(),
                        plan = dt.Rows[i]["Plan"].ToString().Trim(),
                        service = dt.Rows[i]["Servicio"].ToString(),
                        value = Convert.ToDecimal(dt.Rows[i]["ValorIngreso"]),
                        invoicevalue = Convert.ToDecimal(dt.Rows[i]["ValorFactura"]),
                        eps = dt.Rows[i]["EPS"].ToString().Trim(),
                        company = dt.Rows[i]["Empresa"].ToString(),
                        notuser = dt.Rows[i]["Origen"].ToString(),
                        invoice = dt.Rows[i]["Factura"].ToString(),
                        costcenter = dt.Rows[i]["CodigoCentro"].ToString(),
                        subcenter = dt.Rows[i]["CodigoSubcentro"].ToString(),
                        costname = dt.Rows[i]["Centro"].ToString(),
                        subcentername = dt.Rows[i]["Subcentro"].ToString(),
                    };
                    lCargo.Add(oEntity);
                }
                return lCargo;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<Cargo> GetSurplusList(Cargo oEntity)
        {
            List<Cargo> lCargo = new List<Cargo>();
            DataTable dt = new DataTable();
            try
            {
                dt = this.GetSurplusData(oEntity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Cargo()
                    {
                        user = dt.Rows[i]["usuario"].ToString().ToLower(),
                        date = Convert.ToDateTime(dt.Rows[i]["Fecha"]),
                        idadmission = dt.Rows[i]["Ingreso"].ToString(),
                        plan = dt.Rows[i]["Plan"].ToString().Trim(),
                        service = dt.Rows[i]["Servicio"].ToString(),
                        adding = Convert.ToDecimal(dt.Rows[i]["Excedente"]),
                        surplus = (dt.Rows[i]["Valor"] != DBNull.Value) ? -Convert.ToDecimal(dt.Rows[i]["Valor"]) : 0,
                        value = (dt.Rows[i]["Valor"] != DBNull.Value) ? -Convert.ToDecimal(dt.Rows[i]["Valor"]) : 0,
                        eps = dt.Rows[i]["EPS"].ToString().Trim(),
                        company = dt.Rows[i]["Empresa"].ToString(),
                        costcenter = dt.Rows[i]["CodigoCentro"].ToString(),
                        subcenter = dt.Rows[i]["CodigoSubcentro"].ToString(),
                        costname = dt.Rows[i]["Centro"].ToString(),
                        subcentername = dt.Rows[i]["Subcentro"].ToString(),
                        documenttype = dt.Rows[i]["TipoDocumento"].ToString(),
                    };
                    lCargo.Add(oEntity);
                }
                return lCargo;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lSupport"></param>
        public void UpdateReasonsResponse(List<Support> lSupport)
        {
            OracleDAC oDAC = new OracleDAC();
            oDAC.sConnection = this.ConnectionString;
            oDAC.Connect();
            string Query = "UPDATE motivocargo SET mc_respuesta = :response WHERE mc_mo_id = :idsupport AND mc_ca_id = :idcharge";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            for (int i = 0; i < lSupport.Count; i++)
            {
                lParameters.Add(new OracleParameter(":response", lSupport[i].response));
                lParameters.Add(new OracleParameter(":idsupport", lSupport[i].id));
                lParameters.Add(new OracleParameter(":idcharge", lSupport[i].idcharge));
                oDAC.ExecuteNonQuery(Query, lParameters);
                lParameters.RemoveRange(0, lParameters.Count);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<Devolucion> GetListReturn(Cargo oEntity)
        {
            DataTable dt = new DataTable();
            List<Devolucion> lReturn = new List<Devolucion>();
            Devolucion oReturn = null;
            try
            {
                dt = this.GetReturns(oEntity);
                foreach (DataRow dr in dt.Rows)
                {
                    oReturn = new Devolucion()
                    {
                        idadmission = dr["Ingreso"].ToString(),
                        user = dr["Usuario"].ToString(),
                        eps = dr["EPS"].ToString(),
                        date = (dr["FechaCargo"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaCargo"]) : new DateTime(),
                        centraluser = dr["UsuarioAuditor"].ToString(),
                        readytoinvoicedate = (dr["FechaListoFacturar"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaListoFacturar"].ToString()) : (Nullable<DateTime>)null,
                        centraldate = (dr["FechaRecibidoCentral"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaRecibidoCentral"].ToString()) : (Nullable<DateTime>)null,
                        senddate = (dr["FechaEnviadoACentral"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaEnviadoACentral"].ToString()) : (Nullable<DateTime>)null,
                        returnsenddate = (dr["FechaDevueltoaCentral"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaDevueltoaCentral"].ToString()) : (Nullable<DateTime>)null,
                        returndate = (dr["FechaDevolucion"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaDevolucion"].ToString()) : (Nullable<DateTime>)null,
                        recievedate = (dr["FechaReciboDevolucion"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaReciboDevolucion"].ToString()) : (Nullable<DateTime>)null,
                        reasontext = dr["MotivoDevolucion"].ToString(),
                    };
                    lReturn.Add(oReturn);
                }
                return lReturn;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public int Insert(Cargo oEntity)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO cargo (ca_ingreso, ca_es_id, ca_servicio, ca_plan, ca_empresa, ca_valor, ca_eps, ca_documento, ca_apellido");
            sQuery.Append(", ca_nombre, ca_excedente, ca_usuario, ca_codigosubcentro, ca_centro, ca_subcentro, ca_abono, ca_autorizacion, ca_codigocentro, ca_ultimafecha, ca_ultimousuario, ca_tipodocumento) VALUES (:idadmission, :status, :service, :plan, :company, :value");
            sQuery.Append(", :eps, :documento, :apellido, :nombre, :excedente, :usuario, :codigosubcentro, :centro, :subcentro, :abono, :autorizacion, :codigocentro, SYSDATE, :usuario, :tipodocumento) RETURNING ca_id INTO :ca_id");
            OracleParameter ca_id = new OracleParameter("ca_id", OracleDbType.Int32, ParameterDirection.ReturnValue);
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(ca_id);
            lParameters.Add(new OracleParameter(":idadmission", oEntity.idadmission));
            lParameters.Add(new OracleParameter(":status", oEntity.status));
            lParameters.Add(new OracleParameter(":service", oEntity.service));
            lParameters.Add(new OracleParameter(":plan", oEntity.plan));
            lParameters.Add(new OracleParameter(":company", oEntity.company));
            lParameters.Add(new OracleParameter(":value", oEntity.value));
            lParameters.Add(new OracleParameter(":eps", oEntity.eps));
            lParameters.Add(new OracleParameter(":documento", oEntity.patientdocument));
            lParameters.Add(new OracleParameter(":apellido", oEntity.patientsurname));
            lParameters.Add(new OracleParameter(":nombre", oEntity.patientname));
            lParameters.Add(new OracleParameter(":excedente", oEntity.surplus));
            lParameters.Add(new OracleParameter(":usuario", oEntity.user));
            lParameters.Add(new OracleParameter(":codigosubcentro", oEntity.subcenter));
            lParameters.Add(new OracleParameter(":centro", oEntity.costname));
            lParameters.Add(new OracleParameter(":subcentro", oEntity.subcentername));
            lParameters.Add(new OracleParameter(":abono", oEntity.adding));
            lParameters.Add(new OracleParameter(":autorizacion", oEntity.authorization));
            lParameters.Add(new OracleParameter(":codigocentro", oEntity.costcenter));
            lParameters.Add(new OracleParameter(":usuario", oEntity.lastuser));
            lParameters.Add(new OracleParameter(":tipodocumento", oEntity.documenttype));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);
                oEntity.id = Convert.ToInt32(ca_id.Value.ToString());
                this.InsertLog(oEntity);
                lParameters = null;
                return oEntity.id;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        public void Edit(Cargo oEntity)
        {
            string sQuery = "UPDATE cargo set ca_es_id = :status, ca_ultimousuario = :usuario, ca_ultimafecha = SYSDATE WHERE ca_id = :id";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":status", oEntity.status));
            lParameters.Add(new OracleParameter(":usuario", oEntity.lastuser));
            lParameters.Add(new OracleParameter(":id", oEntity.id));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery, lParameters);
                this.InsertLog(oEntity);
            }
            lParameters = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        public void DeleteSupports(int idcharge)
        {
            string sQuery = "DELETE FROM soportecargo WHERE sc_ca_id = :idcharge";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idcharge", idcharge));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery, lParameters, false);
            }
            lParameters = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        public void InsertSupports(Support oEntity)
        {
            string sQuery = "INSERT INTO soportecargo (sc_ca_id, sc_so_id, sc_observacion) VALUES (:idcharge, :idsupport, :observation)";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idcharge", oEntity.idcharge));
            lParameters.Add(new OracleParameter(":idsupport", oEntity.id));
            lParameters.Add(new OracleParameter(":observation", oEntity.observation));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery, lParameters, false);
            }
            lParameters = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lEntity"></param>
        public void InsertReasons(List<Support> lEntity)
        {
            OracleDAC oDAC = new OracleDAC();
            try
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                this.DeleteReasons(lEntity[0].idcharge, oDAC);
                for (int i = 0; i < lEntity.Count; i++)
                {
                    this.InsertReason(lEntity[i], oDAC);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<Cargo> GetPayment(Cargo oEntity)
        {
            List<Cargo> lCargo = new List<Cargo>();
            DataTable dt = new DataTable();
            try
            {
                dt = this.GetPaymentData(oEntity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Cargo()
                    {
                        user = (dt.Rows[i]["usuario"] != DBNull.Value) ? dt.Rows[i]["usuario"].ToString().ToLower() : string.Empty,
                        date = (dt.Rows[i]["Fecha"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["Fecha"]) : new DateTime(),
                        idadmission = (dt.Rows[i]["Ingreso"] != DBNull.Value) ? dt.Rows[i]["Ingreso"].ToString() : string.Empty,
                        plan = (dt.Rows[i]["Plan"] != DBNull.Value) ? dt.Rows[i]["Plan"].ToString().Trim() : string.Empty,
                        service = (dt.Rows[i]["Servicio"] != DBNull.Value) ? dt.Rows[i]["Servicio"].ToString() : string.Empty,                                                
                        value = (dt.Rows[i]["Total"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[i]["Total"]) : 0,
                        eps = (dt.Rows[i]["EPS"] != DBNull.Value) ? dt.Rows[i]["EPS"].ToString().Trim() : string.Empty,
                        fcharge = (dt.Rows[i]["FechaDetalle"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["FechaDetalle"]) : new DateTime(),                        
                        authorization = (dt.Rows[i]["Autorizacion"] != DBNull.Value) ? dt.Rows[i]["Autorizacion"].ToString() : string.Empty,
                        costcenter = (dt.Rows[i]["CodigoCentro"] != DBNull.Value) ? dt.Rows[i]["CodigoCentro"].ToString() : string.Empty,
                        subcenter = (dt.Rows[i]["CodigoSubcentro"] != DBNull.Value) ? dt.Rows[i]["CodigoSubcentro"].ToString() : string.Empty,
                        costname = (dt.Rows[i]["Centro"] != DBNull.Value) ? dt.Rows[i]["Centro"].ToString() : string.Empty,
                        subcentername = (dt.Rows[i]["Subcentro"] != DBNull.Value) ? dt.Rows[i]["Subcentro"].ToString() : string.Empty,
                        patientname = (dt.Rows[i]["Nombre"] != DBNull.Value) ? dt.Rows[i]["Nombre"].ToString() : string.Empty,
                        patientsurname = (dt.Rows[i]["Apellido"] != DBNull.Value) ? dt.Rows[i]["Apellido"].ToString() : string.Empty,
                        patientdocument = (dt.Rows[i]["Documento"] != DBNull.Value) ? dt.Rows[i]["Documento"].ToString() : string.Empty,
                        documenttype = (dt.Rows[i]["TipoDocumento"] != DBNull.Value) ? dt.Rows[i]["TipoDocumento"].ToString() : string.Empty,
                    };
                    lCargo.Add(oEntity);
                }
                return lCargo;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de abonos");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de abonos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<Devolucion> GetListStatus(Cargo oEntity)
        {
            DataTable dt = new DataTable();
            List<Devolucion> lReturn = new List<Devolucion>();
            Devolucion oReturn = null;
            try
            {
                dt = this.GetStatusLogData(oEntity);
                foreach (DataRow dr in dt.Rows)
                {
                    oReturn = new Devolucion()
                    {
                        idadmission = dr["Ingreso"].ToString(),
                        user = dr["Usuario"].ToString(),
                        eps = dr["EPS"].ToString(),
                        date = (dr["FechaCargo"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaCargo"]) : new DateTime(),
                        centraluser = dr["UsuarioAuditor"].ToString(),
                        readytoinvoicedate = (dr["FechaListoFacturar"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaListoFacturar"].ToString()) : (Nullable<DateTime>)null,
                        centraldate = (dr["FechaRecibidoCentral"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaRecibidoCentral"].ToString()) : (Nullable<DateTime>)null,
                        senddate = (dr["FechaEnviadoACentral"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaEnviadoACentral"].ToString()) : (Nullable<DateTime>)null,
                        returnsenddate = (dr["FechaDevueltoaCentral"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaDevueltoaCentral"].ToString()) : (Nullable<DateTime>)null,
                        returndate = (dr["FechaDevolucion"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaDevolucion"].ToString()) : (Nullable<DateTime>)null,
                        recievedate = (dr["FechaReciboDevolucion"] != DBNull.Value) ? Convert.ToDateTime(dr["FechaReciboDevolucion"].ToString()) : (Nullable<DateTime>)null,
                        authorization = dr["Autorizacion"].ToString(),
                        centralrecieveuser = dr["UsuarioCentral"].ToString(),
                        company = dr["Empresa"].ToString(),
                        reasontext = dr["MotivoDevolucion"].ToString(),
                    };
                    lReturn.Add(oReturn);
                }
                return lReturn;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }
        
        private DataTable FilterDataTable(DataTable dt, Cargo oEntity)
        {
            DataTable dataTable = new DataTable();
            if (oEntity.initialdate.Year > 1)
            {
                
                //Si el perfil es cajero de RHB se debe tener en cuenta la fecha del cargo y no del ingreso
                if (oEntity.iidprofile == 10)
                {
                    if (dt.AsEnumerable().Count(row => row.Field<DateTime>("FechaDetalle") >= oEntity.initialdate) > 0)
                    {
                        dataTable = dt.AsEnumerable().Where(row => row.Field<DateTime>("FechaDetalle") >= oEntity.initialdate).CopyToDataTable();
                    }                    
                }
                else
                {
                    if (dt.AsEnumerable().Count(row => row.Field<DateTime>("Fecha") >= oEntity.initialdate) > 0)
                    {
                        dataTable = dt.AsEnumerable().Where(row => row.Field<DateTime>("Fecha") >= oEntity.initialdate).CopyToDataTable();
                    }                        
                }                
            }
            if (oEntity.finaldate.Year > 1)
            {                
                //Si el perfil es cajero de RHB se debe tener en cuenta la fecha del cargo y no del ingreso
                if (oEntity.iidprofile == 10)
                {
                    if (dt.AsEnumerable().Count(row => row.Field<DateTime>("FechaDetalle") <= oEntity.initialdate) > 0)
                    {
                        dataTable = dt.AsEnumerable().Where(row => row.Field<DateTime>("FechaDetalle") <= oEntity.finaldate).CopyToDataTable();
                    }                        
                }
                else
                {
                    if (dt.AsEnumerable().Count(row => row.Field<DateTime>("Fecha") <= oEntity.initialdate) > 0)
                    {
                        dataTable = dt.AsEnumerable().Where(row => row.Field<DateTime>("Fecha") <= oEntity.finaldate).CopyToDataTable();
                    }
                }                    
            }
            /*if (oEntity.status.HasValue)
            {
                if (oEntity.status < 2)
                {
                    dataTable = dt.AsEnumerable().Where(row => (row.Field<int>("ca_es_id") >= 0 && row.Field<int>("ca_es_id") <= 3)).CopyToDataTable();                    
                }
                else if (oEntity.status > 1 && oEntity.status < 10)
                {
                    dataTable = dt.AsEnumerable().Where(row => row.Field<int>("ca_es_id") == oEntity.status).CopyToDataTable();                    
                }            
            }*/
            return dataTable;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public List<Cargo> GetList(Cargo oEntity, bool view = false)
        {
            int idprofile = oEntity.iidprofile;
            List<Cargo> lCargo = new List<Cargo>();
            DataTable dt = new DataTable();
            StringBuilder sName = new StringBuilder();
            try
            {
                dt = (!view) ? this.GetData(oEntity, view) : this.GetData(oEntity);
                if (!view && dt.Rows.Count > 0)
                {
                    dt = this.FilterDataTable(dt, oEntity);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Cargo()
                    {
                        user = (dt.Rows[i]["usuario"] != DBNull.Value) ? dt.Rows[i]["usuario"].ToString().ToLower() : string.Empty,
                        date = (dt.Rows[i]["Fecha"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["Fecha"]) : new DateTime(),
                        idadmission = (dt.Rows[i]["Ingreso"] != DBNull.Value) ? dt.Rows[i]["Ingreso"].ToString() : string.Empty,
                        plan = (dt.Rows[i]["Plan"] != DBNull.Value) ? dt.Rows[i]["Plan"].ToString().Trim() : string.Empty,
                        service = (dt.Rows[i]["Servicio"] != DBNull.Value) ? dt.Rows[i]["Servicio"].ToString() : string.Empty,
                        adding = (dt.Rows[i]["Excedente"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[i]["Excedente"]) : 0,
                        surplus = (dt.Rows[i]["Abono"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[i]["Abono"]) : 0,
                        value = (dt.Rows[i]["Total"] != DBNull.Value) ?  Convert.ToDecimal(dt.Rows[i]["Total"]) : 0,
                        eps = (dt.Rows[i]["EPS"] != DBNull.Value) ? dt.Rows[i]["EPS"].ToString().Trim() : string.Empty,
                        status = (dt.Rows[i]["ca_es_id"] != DBNull.Value) ? Convert.ToInt32(dt.Rows[i]["ca_es_id"]) : 0,
                        id = (dt.Rows[i]["ca_id"] != DBNull.Value) ? Convert.ToInt32(dt.Rows[i]["ca_id"]) : 0,
                        fcharge = (dt.Rows[i]["FechaCargo"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["FechaCargo"]) : new DateTime(),
                        lastuser = (dt.Rows[i]["UltimoUsuario"] != DBNull.Value) ? dt.Rows[i]["UltimoUsuario"].ToString() : string.Empty,
                        company = (dt.Rows[i]["Empresa"] != DBNull.Value) ? dt.Rows[i]["Empresa"].ToString() : string.Empty,
                        authorization = (dt.Rows[i]["Autorizacion"] != DBNull.Value) ? dt.Rows[i]["Autorizacion"].ToString() : string.Empty,
                        costcenter = (dt.Rows[i]["CodigoCentro"] != DBNull.Value) ? dt.Rows[i]["CodigoCentro"].ToString() : string.Empty,
                        subcenter = (dt.Rows[i]["CodigoSubcentro"] != DBNull.Value) ?  dt.Rows[i]["CodigoSubcentro"].ToString() : string.Empty,
                        costname = (dt.Rows[i]["Centro"] != DBNull.Value) ? dt.Rows[i]["Centro"].ToString() : string.Empty,
                        subcentername = (dt.Rows[i]["Subcentro"] != DBNull.Value) ?  dt.Rows[i]["Subcentro"].ToString() : string.Empty,
                        patientname = (dt.Rows[i]["Nombre"] != DBNull.Value) ? dt.Rows[i]["Nombre"].ToString() : string.Empty,
                        patientsurname = (dt.Rows[i]["Apellido"] != DBNull.Value) ? dt.Rows[i]["Apellido"].ToString() : string.Empty,
                        patientdocument = (dt.Rows[i]["Documento"] != DBNull.Value) ? dt.Rows[i]["Documento"].ToString() : string.Empty,                                                
                        documenttype = (dt.Rows[i]["TipoDocumento"] != DBNull.Value) ? dt.Rows[i]["TipoDocumento"].ToString() : string.Empty,
                        iline = (dt.Rows[i]["Linea"] != DBNull.Value) ? Convert.ToInt32(dt.Rows[i]["Linea"]) : 0,
                        scharge = (dt.Rows[i]["Cargo"] != DBNull.Value) ? dt.Rows[i]["Cargo"].ToString() : string.Empty,
                        iqty = (dt.Rows[i]["Cantidad"] != DBNull.Value) ? Convert.ToInt32(dt.Rows[i]["Cantidad"]) : 0,
                        ssource = (dt.Rows[i]["Origen"] != DBNull.Value) ? dt.Rows[i]["Origen"].ToString() : string.Empty,                           
                    };
                    sName.Append(oEntity.patientname);
                    sName.Append(" ");
                    sName.Append(oEntity.patientsurname);
                    oEntity.patientfullname = sName.ToString();
                    sName.Remove(0, sName.Length);
                    //Si el perfil es cajero de RHB se debe tener en cuenta la fecha del cargo y no del ingreso
                    if (idprofile == 10)
                    {
                        oEntity.fcharge = (dt.Rows[i]["FechaDetalle"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["FechaDetalle"]) : new DateTime();
                    }
                    lCargo.Add(oEntity);
                }
                return lCargo;
            }
            catch(InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
                sName = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<Cargo> GetListReport(Cargo oEntity)
        {
            List<Cargo> lCargo = new List<Cargo>();
            DataTable dt = new DataTable();
            //DataTable dtFacturas = new DataTable();
            StringBuilder sName = new StringBuilder();
            Cargo oCargo = oEntity;
            try
            {
                //dtFacturas = this.GetInvoicesByCharge(oCargo);
                dt = this.GetReport(oEntity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Cargo()
                    {
                        user = (dt.Rows[i]["usuario"] != DBNull.Value) ? dt.Rows[i]["usuario"].ToString().ToLower() : string.Empty,
                        date = (dt.Rows[i]["Fecha"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["Fecha"]) : new DateTime(),
                        idadmission = (dt.Rows[i]["Ingreso"] != DBNull.Value) ? dt.Rows[i]["Ingreso"].ToString() : string.Empty,
                        plan = (dt.Rows[i]["Plan"] != DBNull.Value) ? dt.Rows[i]["Plan"].ToString().Trim() : string.Empty,
                        service = (dt.Rows[i]["Servicio"] != DBNull.Value) ? dt.Rows[i]["Servicio"].ToString() : string.Empty,
                        adding = (dt.Rows[i]["Excedente"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[i]["Excedente"]) : 0,
                        surplus = (dt.Rows[i]["Abono"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[i]["Abono"]) : 0,
                        value = (dt.Rows[i]["Total"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[i]["Total"]) : 0,
                        eps = (dt.Rows[i]["EPS"] != DBNull.Value) ? dt.Rows[i]["EPS"].ToString().Trim() : string.Empty,
                        status = (dt.Rows[i]["ca_es_id"] != DBNull.Value) ? Convert.ToInt32(dt.Rows[i]["ca_es_id"]) : 0,
                        id = (dt.Rows[i]["ca_id"] != DBNull.Value) ? Convert.ToInt32(dt.Rows[i]["ca_id"]) : 0,
                        fcharge = (dt.Rows[i]["FechaCargo"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["FechaCargo"]) : new DateTime(),
                        lastuser = (dt.Rows[i]["UltimoUsuario"] != DBNull.Value) ? dt.Rows[i]["UltimoUsuario"].ToString() : string.Empty,
                        company = (dt.Rows[i]["Empresa"] != DBNull.Value) ? dt.Rows[i]["Empresa"].ToString() : string.Empty,
                        authorization = (dt.Rows[i]["Autorizacion"] != DBNull.Value) ? dt.Rows[i]["Autorizacion"].ToString() : string.Empty,
                        costcenter = (dt.Rows[i]["CodigoCentro"] != DBNull.Value) ? dt.Rows[i]["CodigoCentro"].ToString() : string.Empty,
                        subcenter = (dt.Rows[i]["CodigoSubcentro"] != DBNull.Value) ? dt.Rows[i]["CodigoSubcentro"].ToString() : string.Empty,
                        costname = (dt.Rows[i]["Centro"] != DBNull.Value) ? dt.Rows[i]["Centro"].ToString() : string.Empty,
                        subcentername = (dt.Rows[i]["Subcentro"] != DBNull.Value) ? dt.Rows[i]["Subcentro"].ToString() : string.Empty,
                        patientname = (dt.Rows[i]["Nombre"] != DBNull.Value) ? dt.Rows[i]["Nombre"].ToString() : string.Empty,
                        patientsurname = (dt.Rows[i]["Apellido"] != DBNull.Value) ? dt.Rows[i]["Apellido"].ToString() : string.Empty,
                        patientdocument = (dt.Rows[i]["Documento"] != DBNull.Value) ? dt.Rows[i]["Documento"].ToString() : string.Empty,
                        invoiced = (dt.Rows[i]["Facturado"] != DBNull.Value) ? dt.Rows[i]["Facturado"].ToString() : string.Empty,
                        documenttype = (dt.Rows[i]["TipoDocumento"] != DBNull.Value) ? dt.Rows[i]["TipoDocumento"].ToString() : string.Empty,
                    };
                    sName.Append(oEntity.patientname);
                    sName.Append(" ");
                    sName.Append(oEntity.patientsurname);
                    oEntity.patientfullname = sName.ToString();
                    sName.Remove(0, sName.Length);
                    lCargo.Add(oEntity);
                }                                  
                return lCargo;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
                sName = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lCargo"></param>
        /// <param name="oCargo"></param>
        /// <returns></returns>
        private List<Cargo> FilterList(List<Cargo> lCargo, Cargo oCargo)
        {
            List<Cargo> lResult = lCargo.Where(x => x.date >= oCargo.initialdate && x.date <= oCargo.finaldate).ToList();
            if (!string.IsNullOrEmpty(oCargo.eps))
            {
                lResult = lResult.Where(x => x.eps.ToUpper().Contains(oCargo.eps.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(oCargo.service))
            {
                lResult = lResult.Where(x => x.service.ToUpper().Contains(oCargo.service.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(oCargo.company))
            {
                lResult = lResult.Where(x => x.company.ToUpper().Contains(oCargo.company.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(oCargo.plan))
            {
                lResult = lResult.Where(x => x.plan.ToUpper().Contains(oCargo.plan.ToUpper())).ToList();
            }           
            if (oCargo.status < 2)
            {
                lResult = lResult.Where(x => x.status == 1 || x.status == 0).ToList();                
            }
            else if (oCargo.status > 1 && oCargo.status < 10)
            {
                lResult = lResult.Where(x => x.status == oCargo.status).ToList();
            }
            return lResult.OrderBy(x => x.idadmission).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lAdmission"></param>
        public void UpdateStatus(List<Cargo> lAdmission)
        {
            OracleDAC oDAC = new OracleDAC();
            try
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();                
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                for (int i = 0; i < lAdmission.Count; i++)
                {
                    if (lAdmission[i].id == 0)
                    {
                        lAdmission[i].id = this.Insert(lAdmission[i], oDAC);
                    }
                    else
                    {
                        this.Edit(lAdmission[i], oDAC);
                    }
                    this.InsertLog(lAdmission[i], oDAC);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lEntity"></param>
        public void InsertPending(List<Support> lEntity)
        {
            OracleDAC oDAC = new OracleDAC();
            try
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                this.DeletePendings(lEntity[0].idcharge, oDAC);
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

        #endregion

        #region Métodos privados

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        private DataTable GetData(Cargo oEntity)
        {
            string[] lUser = null;            
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT DISTINCT ca_usuario usuario, ca_fecha Fecha, ca_ingreso Ingreso, ca_plan \"Plan\", ca_documento Documento");
            sQuery.Append(", ca_tipodocumento TipoDocumento, ca_apellido Apellido, ca_nombre Nombre, ca_servicio Servicio, ca_excedente Excedente, ca_cantidad Cantidad, ca_valor Total");
            sQuery.Append(", ca_es_id, ca_id, ca_ultimafecha FechaCargo, ca_ultimousuario UltimoUsuario, ca_abono Abono, ca_empresa Empresa, ca_codigosubcentro CodigoSubcentro, ca_origen Origen");
            sQuery.Append(", ca_codigocentro AS CodigoCentro, ca_centro Centro, ca_subcentro subcentro, ca_eps EPS, ca_autorizacion Autorizacion");
            sQuery.Append(", ca_tipodocumento TipoDocumento, ca_cargo Cargo, ca_linea Linea FROM cargo WHERE ca_id IS NOT NULL");
            if (!string.IsNullOrEmpty(oEntity.user))
            {
                if (oEntity.user.Contains(','))
                {
                    sQuery.Append(" AND (");
                    lUser = oEntity.user.Split(',');
                    for (int i = 0; i < lUser.Length; i++)
                    {
                        sQuery.Append("ca_usuario = '");
                        sQuery.Append(lUser[i]);
                        sQuery.Append("'");
                        if (i < lUser.Length - 1)
                        {
                            sQuery.Append(" OR ");
                        }
                    }
                    sQuery.Append(")");
                }
                else
                {
                    lParameters.Add(new OracleParameter(":usuario", oEntity.user));
                    sQuery.Append(" AND ca_usuario = :usuario");
                }
            }
            if (!string.IsNullOrEmpty(oEntity.idadmission))
            {
                lParameters.Add(new OracleParameter(":idadmission", oEntity.idadmission));
                sQuery.Append(" AND ca_ingreso = :idadmission");
            }
            if (!string.IsNullOrEmpty(oEntity.service))
            {
                lParameters.Add(new OracleParameter(":service", oEntity.service));
                sQuery.Append(" AND ca_servicio = :service");
            }
            if (!string.IsNullOrEmpty(oEntity.company))
            {
                lParameters.Add(new OracleParameter(":company", oEntity.company));
                sQuery.Append(" AND ca_empresa = :company");
            }
            if (!string.IsNullOrEmpty(oEntity.eps))
            {
                lParameters.Add(new OracleParameter(":eps", oEntity.eps));
                sQuery.Append(" AND ca_eps LIKE '%' || :eps || '%' ");                
            }
            if (!string.IsNullOrEmpty(oEntity.plan))
            {
                lParameters.Add(new OracleParameter(":plan", oEntity.plan));
                sQuery.Append(" AND ca_plan = :plan");
            }
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(ca_ultimafecha, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":finaldate", oEntity.finaldate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(ca_ultimafecha, 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (oEntity.status == 3)
            {
                sQuery.Append(" AND ca_es_id IN (2, 3)");
            }
            else if (oEntity.status == (int)ChargeStatus.recieved)
            {
                //lParameters.Add(new OracleParameter(":status", oEntity.status));
                sQuery.Append(" AND ca_es_id IN (4, 8)");
            }
            else if (oEntity.status == (int)ChargeStatus.returned)
            {
                sQuery.Append(" AND ca_es_id IN (6, 11)");
            }
            else if (oEntity.status > 1 && oEntity.status < 10)
            {
                lParameters.Add(new OracleParameter(":estado", oEntity.status));
                sQuery.Append(" AND ca_es_id = :estado");
            }                 
            if (!string.IsNullOrEmpty(oEntity.notuser))
            {
                sQuery.Append(" AND (");
                lUser = oEntity.notuser.Split(',');
                for (int i = 0; i < lUser.Length; i++)
                {
                    sQuery.Append("ca_usuario <> '");
                    sQuery.Append(lUser[i]);
                    sQuery.Append("'");
                    if (i < lUser.Length - 1)
                    {
                        sQuery.Append(" OR ");
                    }
                }                
                sQuery.Append(")");
            }
            if (!string.IsNullOrEmpty(oEntity.authorization))
            {
                lParameters.Add(new OracleParameter(":authorization", oEntity.authorization));
                sQuery.Append(" AND ca_autorizacion = :authorization");
            }
            if (!string.IsNullOrEmpty(oEntity.patientdocument))
            {
                lParameters.Add(new OracleParameter(":document", oEntity.patientdocument));
                sQuery.Append(" AND ca_documento = :document");
            }
            if (!string.IsNullOrEmpty(oEntity.costcenter))
            {
                sQuery.Append(oEntity.costcenter);
            }
            sQuery.Append(" ORDER BY ca_ingreso DESC");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            };
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        private DataTable GetReport(Cargo oEntity)
        {
            string[] lUser = null;
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT * FROM VEstadoCargo WHERE 1 = 1");
            if (!string.IsNullOrEmpty(oEntity.user))
            {
                if (oEntity.user.Contains(','))
                {
                    sQuery.Append(" AND (");
                    lUser = oEntity.user.Split(',');
                    for (int i = 0; i < lUser.Length; i++)
                    {
                        sQuery.Append("Usuario = '");
                        sQuery.Append(lUser[i]);
                        sQuery.Append("'");
                        if (i < lUser.Length - 1)
                        {
                            sQuery.Append(" OR ");
                        }
                    }
                    sQuery.Append(")");
                }
                else
                {
                    lParameters.Add(new OracleParameter(":usuario", oEntity.user));
                    sQuery.Append(" AND Usuario = :usuario");
                }
            }
            if (!string.IsNullOrEmpty(oEntity.idadmission))
            {
                lParameters.Add(new OracleParameter(":idadmission", oEntity.idadmission));
                sQuery.Append(" AND Ingreso = :idadmission");
            }
            if (!string.IsNullOrEmpty(oEntity.eps))
            {
                lParameters.Add(new OracleParameter(":eps", oEntity.eps));
                sQuery.Append(" AND EPS LIKE '%' || :eps || '%' ");
            }
            if (!string.IsNullOrEmpty(oEntity.service))
            {
                lParameters.Add(new OracleParameter(":service", oEntity.service));
                sQuery.Append(" AND Servicio = :service");
            }
            if (!string.IsNullOrEmpty(oEntity.company))
            {
                lParameters.Add(new OracleParameter(":company", oEntity.company));
                sQuery.Append(" AND Empresa = :company");
            }
            if (!string.IsNullOrEmpty(oEntity.plan))
            {
                lParameters.Add(new OracleParameter(":plan", oEntity.plan));
                sQuery.Append(" AND \"Plan\" = :plan");
            }
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(Fecha, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":finaldate", oEntity.finaldate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(Fecha, 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (oEntity.status < 2)
            {
                sQuery.Append(" AND (ca_es_id IS NULL or ca_es_id = 1 OR ca_es_id = 0)");
            }
            else if (oEntity.status > 1 && oEntity.status < 10)
            {
                lParameters.Add(new OracleParameter(":status", oEntity.status));
                sQuery.Append(" AND ca_es_id = :status");
            }            
            sQuery.Append(" ORDER BY Fecha DESC");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        private DataTable GetData(Cargo oEntity, bool view)
        {        
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT \"Fecha Ingreso\" Fecha, \"Numero Ingreso\" Ingreso, Autorizacion, EPS, \"Plan\", \"Tipo Identificacion\" TipoDocumento, \"Numero Identificacion\" Documento");
            sQuery.Append(", Apellido, Nombre, Servicio, Reconocido, Excedente, Cantidad, \"Valor Total\" Total, Origen, \"Ingresado por\" usuario, NVL(ca_es_id, 0) ca_es_id, ca_id");
            sQuery.Append(", lg.cl_fecha AS FechaCargo, lg.us_usuario AS UltimoUsuario, Abono, \"Compañia\" AS Empresa, CodigoSubcentro, CodigoCentro, Centro, Subcentro, Cargo, Linea, FechaDetalle");            
            sQuery.Append(" FROM VCargos LEFT JOIN cargo ON \"Numero Ingreso\" = ca_ingreso AND VCargos.Cargo = ca_cargo AND ca_linea = Linea AND \"Compañia\" = ca_empresa");
            sQuery.Append(" LEFT JOIN ");
            sQuery.Append(" (SELECT us_usuario, cl_ca_id, cl_fecha, ROW_NUMBER() OVER(PARTITION BY cl_ca_id ORDER BY cl_fecha DESC) RowNumber FROM cargolog, usuario WHERE cl_us_id = us_id) lg");
            sQuery.Append(" ON lg.cl_ca_id = ca_id AND lg.RowNumber = 1 WHERE 1 = 1");
            if (!string.IsNullOrEmpty(oEntity.user))
            {
                //lParameters.Add(new OracleParameter(":usuario", oEntity.user));
                sQuery.Append(" AND (\"Ingresado por\" IN ('" + oEntity.user + "') OR \"Ingresado por\" = 'admon')");
            }
            if (!string.IsNullOrEmpty(oEntity.idadmission))
            {
                lParameters.Add(new OracleParameter(":idadmission", oEntity.idadmission));
                sQuery.Append(" AND \"Numero Ingreso\" = :idadmission");
            }
            if (!string.IsNullOrEmpty(oEntity.eps))
            {
                lParameters.Add(new OracleParameter(":eps", oEntity.eps));
                sQuery.Append(" AND EPS LIKE '%' || :eps || '%' ");
            }
            if (!string.IsNullOrEmpty(oEntity.service))
            {
                lParameters.Add(new OracleParameter(":service", oEntity.service));
                sQuery.Append(" AND Servicio = :service");
            }
            if (!string.IsNullOrEmpty(oEntity.company))
            {
                lParameters.Add(new OracleParameter(":company", oEntity.company));
                sQuery.Append(" AND \"Compañia\" = :company");
            }
            if (!string.IsNullOrEmpty(oEntity.plan))
            {
                lParameters.Add(new OracleParameter(":plan", oEntity.plan));
                sQuery.Append(" AND \"Plan\" = :plan");
            }            
            if (oEntity.status.HasValue)
            {
                if (oEntity.status < 2)
                {
                    sQuery.Append(" AND (ca_es_id IS NULL OR ca_es_id IN (1, 2, 3, 10, 11))");
                }
                else if (oEntity.status > 1 && oEntity.status < 13)
                {
                    lParameters.Add(new OracleParameter(":status", oEntity.status));
                    sQuery.Append(" AND ca_es_id = :status");
                }            
            }
            if (!string.IsNullOrEmpty(oEntity.invoiced))
            {
                lParameters.Add(new OracleParameter(":invoiced", oEntity.invoiced));
                sQuery.Append(" AND Facturado = :invoiced");
            }
            if (!string.IsNullOrEmpty(oEntity.authorization))
            {
                lParameters.Add(new OracleParameter(":authorization", oEntity.authorization));
                sQuery.Append(" AND Autorizacion = :authorization");
            }
            if (!string.IsNullOrEmpty(oEntity.patientdocument))
            {
                lParameters.Add(new OracleParameter(":document", oEntity.patientdocument));
                sQuery.Append(" AND \"Numero Identificacion\" = :document");
            }
            if (oEntity.canceled.HasValue)
            {
                lParameters.Add(new OracleParameter(":canceled", oEntity.canceled));
                sQuery.Append(" AND Anulado = :canceled");
            }   
            if (!string.IsNullOrEmpty(oEntity.costcenter))
            {                
                sQuery.Append(oEntity.costcenter);                
            }
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(\"Fecha Ingreso\", 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":finaldate", oEntity.finaldate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(\"Fecha Ingreso\", 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (oEntity.iidprofile == 10)
            {
                sQuery.Append(" ORDER BY FechaDetalle DESC, Cargo DESC");
            }
            else
            {
                sQuery.Append(" ORDER BY Ingreso DESC");
            }                
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            };
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <param name="oDAC"></param>
        private void InsertLog(Cargo oEntity, OracleDAC oDAC = null)
        {
            string sQuery = "INSERT INTO cargolog (cl_ca_id, cl_es_id, cl_fecha, cl_us_id) VALUES (:idcharge, :status, SYSDATE, :iduser)";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idcharge", oEntity.id));
            lParameters.Add(new OracleParameter(":status", oEntity.status));
            lParameters.Add(new OracleParameter(":iduser", oEntity.iduser));
            if (oDAC == null)
            {
                using (OracleDAC oSQL = new OracleDAC())
                {
                    oSQL.sConnection = this.ConnectionString;
                    oSQL.Connect();
                    oSQL.ExecuteNonQuery(sQuery, lParameters);
                }
            }
            else
            {
                oDAC.ExecuteNonQuery(sQuery, lParameters, false, true);
            }
            lParameters = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <param name="oDAC"></param>
        /// <returns></returns>
        private int Insert(Cargo oEntity, OracleDAC oDAC)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO cargo (ca_ingreso, ca_es_id, ca_servicio, ca_plan, ca_empresa, ca_valor, ca_eps, ca_documento, ca_apellido");
            sQuery.Append(", ca_nombre, ca_excedente, ca_usuario, ca_codigosubcentro, ca_centro, ca_subcentro, ca_abono, ca_autorizacion, ca_codigocentro, ca_ultimafecha");
            sQuery.Append(", ca_ultimousuario, ca_tipodocumento, ca_cargo, ca_linea, ca_cantidad, ca_origen, ca_fecha)");
            sQuery.Append("  VALUES (:idadmission, :status, :service, :plan, :company, :value, :eps, :documento, :apellido, :nombre, :excedente, :usuario, :codigosubcentro, :centro, :subcentro");
            sQuery.Append(", :abono, :autorizacion, :codigocentro, SYSDATE, :usuario, :tipodocumento, :cargo, :linea, :cantidad, :origen, :ca_fecha) RETURNING ca_id INTO :ca_id");
            OracleParameter ca_id = new OracleParameter("ca_id", OracleDbType.Int32, ParameterDirection.ReturnValue);
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(ca_id);
            lParameters.Add(new OracleParameter(":idadmission", oEntity.idadmission));
            lParameters.Add(new OracleParameter(":status", oEntity.status));
            lParameters.Add(new OracleParameter(":service", oEntity.service));
            lParameters.Add(new OracleParameter(":plan", oEntity.plan));
            lParameters.Add(new OracleParameter(":company", oEntity.company));
            lParameters.Add(new OracleParameter(":value", oEntity.value));
            lParameters.Add(new OracleParameter(":eps", oEntity.eps));
            lParameters.Add(new OracleParameter(":documento", oEntity.patientdocument));
            lParameters.Add(new OracleParameter(":apellido", oEntity.patientsurname));
            lParameters.Add(new OracleParameter(":nombre", oEntity.patientname));
            lParameters.Add(new OracleParameter(":excedente", oEntity.surplus));
            lParameters.Add(new OracleParameter(":usuario", oEntity.user));
            lParameters.Add(new OracleParameter(":codigosubcentro", oEntity.subcenter));
            lParameters.Add(new OracleParameter(":centro", oEntity.costname));
            lParameters.Add(new OracleParameter(":subcentro", oEntity.subcentername));
            lParameters.Add(new OracleParameter(":abono", oEntity.adding));
            lParameters.Add(new OracleParameter(":autorizacion", oEntity.authorization));
            lParameters.Add(new OracleParameter(":codigocentro", oEntity.costcenter));
            lParameters.Add(new OracleParameter(":usuario", oEntity.lastuser));
            lParameters.Add(new OracleParameter(":tipodocumento", oEntity.documenttype));
            lParameters.Add(new OracleParameter(":cargo", oEntity.scharge));
            lParameters.Add(new OracleParameter(":linea", oEntity.iline));
            lParameters.Add(new OracleParameter(":cantidad", oEntity.iqty));
            lParameters.Add(new OracleParameter(":origen", oEntity.ssource));
            lParameters.Add(new OracleParameter(":ca_fecha", oEntity.date));
            oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false, true);
            return Convert.ToInt32(ca_id.Value.ToString());            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <param name="oDAC"></param>
        private void Edit(Cargo oEntity, OracleDAC oDAC)
        {
            string sQuery = "UPDATE cargo set ca_es_id = :status WHERE ca_id = :idcharge";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":status", oEntity.status));
            lParameters.Add(new OracleParameter(":idcharge", oEntity.id));
            oDAC.ExecuteNonQuery(sQuery, lParameters, false, true);
            lParameters = null;
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <param name="oDAC"></param>
        private void InsertReason(Support oEntity, OracleDAC oDAC)
        {
            string sQuery = "INSERT INTO motivocargo (mc_ca_id, mc_mo_id, mc_observacion) VALUES (:idcharge, :idreason, :observation)";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idcharge", oEntity.idcharge));
            lParameters.Add(new OracleParameter(":idreason", oEntity.id));
            lParameters.Add(new OracleParameter(":observation", oEntity.observation));            
            oDAC.ExecuteNonQuery(sQuery, lParameters, false, true);            
            lParameters = null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <param name="oDAC"></param>
        private void InsertPending(Support oEntity, OracleDAC oDAC)
        {
            string sQuery = "INSERT INTO pendientecargo (pc_ca_id, pc_pn_id, pc_observacion) VALUES (:idcharge, :idpending, :observation)";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idcharge", oEntity.idcharge));
            lParameters.Add(new OracleParameter(":idpending", oEntity.id));
            lParameters.Add(new OracleParameter(":observation", oEntity.observation));            
            oDAC.ExecuteNonQuery(sQuery, lParameters, false, true);            
            lParameters = null;
        }

        private void DeletePendings(int idcharge, OracleDAC oDAC)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idcharge", idcharge));
            oDAC.ExecuteNonQuery("DELETE FROM pendientecargo WHERE pc_ca_id = :idcharge", lParameters, false, true);
            lParameters = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        public List<Support> GetSupports(int idcharge)
        {
            List<Support> lSupport = new List<Support>();
            DataTable dt = new DataTable();
            Support oEntity = null;
            try
            {
                dt = this.GetSupportData(idcharge);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Support()
                    {
                        id = Convert.ToInt32(dt.Rows[i]["sc_so_id"]),
                        idcharge = idcharge,
                        observation = dt.Rows[i]["sc_observacion"].ToString(),
                        name = dt.Rows[i]["so_nombre"].ToString(),
                    };
                    lSupport.Add(oEntity);
                }
                return lSupport;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos");
            }
            finally
            {
                dt.Dispose();
                dt = null;                
            }
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        public List<Support> GetReasons(int idcharge)
        {
            List<Support> lSupport = new List<Support>();
            DataTable dt = new DataTable();
            Support oEntity = null;
            try
            {
                dt = this.GetReasonData(idcharge);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Support()
                    {
                        id = Convert.ToInt32(dt.Rows[i]["mc_mo_id"]),
                        idcharge = idcharge,
                        observation = dt.Rows[i]["mc_observacion"].ToString(),
                        response = dt.Rows[i]["mc_respuesta"].ToString(),
                        name = dt.Rows[i]["mo_nombre"].ToString(),
                    };
                    lSupport.Add(oEntity);
                }
                return lSupport;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de motivos por cargo");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de motivos por cargo");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        public List<Support> GetPendings(int idcharge)
        {
            List<Support> lSupport = new List<Support>();
            DataTable dt = new DataTable();
            Support oEntity = null;
            try
            {
                dt = this.GetPendingData(idcharge);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Support()
                    {
                        id = Convert.ToInt32(dt.Rows[i]["pc_pn_id"]),
                        idcharge = idcharge,
                        observation = dt.Rows[i]["pc_observacion"].ToString(),                        
                        name = dt.Rows[i]["pn_nombre"].ToString(),
                    };
                    lSupport.Add(oEntity);
                }
                return lSupport;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de pendientes por cargo");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de pendientes por cargo");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        public List<Support> GetReasonsLog(int idcharge)
        {
            List<Support> lSupport = new List<Support>();
            DataTable dt = new DataTable();
            Support oEntity = null;
            try
            {
                dt = this.GetReasonDataLog(idcharge);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Support()
                    {
                        id = Convert.ToInt32(dt.Rows[i]["ml_mo_id"]),
                        idcharge = idcharge,
                        observation = dt.Rows[i]["ml_observacion"].ToString(),
                        response = dt.Rows[i]["ml_respuesta"].ToString(),
                        name = dt.Rows[i]["mo_nombre"].ToString(),
                    };
                    lSupport.Add(oEntity);
                }
                return lSupport;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de motivos por cargo");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de motivos por cargo");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        private DataTable GetSupportData(int idcharge)
        {
            StringBuilder sQuery = new StringBuilder("SELECT soportecargo.*, so_nombre FROM soportecargo, soporte WHERE so_id = sc_so_id AND sc_ca_id = :idcharge");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idcharge", idcharge));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        private DataTable GetReasonData(int idcharge)
        {            
            StringBuilder sQuery = new StringBuilder("SELECT motivocargo.*, mo_nombre FROM motivocargo, motivodevolucion WHERE mc_mo_id = mo_id AND mc_ca_id = :idcharge");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idcharge", idcharge));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        private DataTable GetPendingData(int idcharge)
        {
            StringBuilder sQuery = new StringBuilder("SELECT pendientecargo.*, pn_nombre FROM pendiente, pendientecargo WHERE pc_pn_id = pn_id AND pc_ca_id = :idcharge");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter("idcharge", idcharge));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <returns></returns>
        private DataTable GetReasonDataLog(int idcharge)
        {
            StringBuilder sQuery = new StringBuilder("SELECT motivocargolog.*, mo_nombre FROM motivocargolog, motivodevolucion WHERE ml_mo_id = mo_id AND ml_ca_id = :idcharge");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter("idcharge", idcharge));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idcharge"></param>
        /// <param name="oDAC"></param>
        private void DeleteReasons(int idcharge, OracleDAC oDAC)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter("idcharge", idcharge));
            oDAC.ExecuteNonQuery("DELETE FROM motivocargo WHERE mc_ca_id = :idcharge", lParameters, false, true);
            lParameters = null;
        }               

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        private DataTable GetChargesInvoiceData(Cargo oEntity)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT * FROM CargosyAbonosFacturados WHERE 1 = 1");
            if (!string.IsNullOrEmpty(oEntity.idadmission))
            {
                lParameters.Add(new OracleParameter("idadmission", oEntity.idadmission));
                sQuery.Append(" AND Ingreso = :idadmission");
            }
            if (!string.IsNullOrEmpty(oEntity.service))
            {
                lParameters.Add(new OracleParameter("service", oEntity.service));
                sQuery.Append(" AND Servicio = :service");
            }
            if (!string.IsNullOrEmpty(oEntity.company))
            {
                lParameters.Add(new OracleParameter("company", oEntity.company));
                sQuery.Append(" AND Empresa = :company");
            }
            if (!string.IsNullOrEmpty(oEntity.plan))
            {
                lParameters.Add(new OracleParameter("plan", oEntity.plan));
                sQuery.Append(" AND \"Plan\" = :plan");
            }
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter("initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FechaFactura, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter("finaldate", oEntity.finaldate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FechaFactura, 'YYYY-MM-DD'), 'YYYY-MM-DD') AS DATE <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (!string.IsNullOrEmpty(oEntity.notuser))
            {
                lParameters.Add(new OracleParameter("source", oEntity.notuser));
                sQuery.Append(" AND Origen = :source");
            }
            if (!string.IsNullOrEmpty(oEntity.invoice))
            {
                lParameters.Add(new OracleParameter("invoice", oEntity.invoice));
                sQuery.Append(" AND Factura = :invoice");
            }
            sQuery.Append(" ORDER BY FechaFactura DESC");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idadmission"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        private DataTable GetChargesLogData(string idadmission, string company)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT \"Fecha Ingreso\" Fecha, \"Numero Ingreso\" Ingreso, Autorizacion, EPS, \"Plan\", \"Tipo Identificacion\" TipoDocumento, \"Numero Identificacion\" Documento");
            sQuery.Append(", Apellido, Nombre, Servicio, Reconocido, Excedente, Cantidad, \"Valor Total\" Total, Origen, \"Ingresado por\" usuario, ca_es_id, NVL(ca_id, 0) id");
            sQuery.Append(", Abono, es_nombre, cl_es_id, us_usuario, Factura, cl_fecha, cl_id, SUM(ValorFacturado) AS ValorFacturado, Anulado FROM VTrazabilidad LEFT JOIN");
            sQuery.Append(" cargo ON \"Numero Ingreso\" = ca_ingreso AND ca_cargo = Cargo AND ca_linea = Linea");
            //sQuery.Append(" cargo ON \"Numero Ingreso\" = ca_ingreso");
            //sQuery.Append(" AND Servicio = ca_servicio AND \"Valor Total\" = ca_valor");
            sQuery.Append(" LEFT JOIN cargolog ON ca_id = cl_ca_id LEFT JOIN estadocargo ON cl_es_id = es_id LEFT JOIN usuario ON cl_us_id = us_id");
            sQuery.Append(" LEFT JOIN VFacturas ON \"Numero Ingreso\" = Ingreso AND Empresa = \"Compañia\"");
            sQuery.Append(" WHERE \"Numero Ingreso\" = :idadmission AND \"Compañia\" = :company");
            sQuery.Append(" GROUP BY \"Fecha Ingreso\", \"Numero Ingreso\", Autorizacion, EPS, \"Plan\", \"Tipo Identificacion\", \"Numero Identificacion\", Apellido, Nombre, Servicio");
            sQuery.Append(", Reconocido, Excedente, Cantidad, \"Valor Total\", Origen, \"Ingresado por\", ca_es_id, ca_id, Abono, es_nombre, cl_es_id, us_usuario, Factura, cl_fecha, cl_id, Anulado ORDER BY cl_fecha DESC");
            lParameters.Add(new OracleParameter("idadmission", idadmission));
            lParameters.Add(new OracleParameter("company", company));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        private DataTable GetSurplusData(Cargo oEntity)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT \"Ingresado por\" AS usuario, \"Numero Ingreso\" AS Ingreso, Servicio, \"Compañia\" AS Empresa, \"Plan\"");
            sQuery.Append(", EPS AS EPS, \"Valor Total\" AS Valor, Excedente AS Excedente, \"Fecha Ingreso\" AS Fecha, CodigoSubcentro, CodigoCentro, Centro, Subcentro, \"Tipo Identificacion\" TipoDocumento FROM VAbonos WHERE 1 = 1");
            if (!string.IsNullOrEmpty(oEntity.user))
            {
                lParameters.Add(new OracleParameter(":usuario", oEntity.user));
                sQuery.Append(" AND \"Ingresado por\" = :usuario");
            }
            if (!string.IsNullOrEmpty(oEntity.idadmission))
            {
                lParameters.Add(new OracleParameter(":idadmission", oEntity.idadmission));
                sQuery.Append(" AND \"Numero Ingreso\" = :idadmission");
            }
            if (!string.IsNullOrEmpty(oEntity.service))
            {
                lParameters.Add(new OracleParameter(":service", oEntity.service));
                sQuery.Append(" AND \"Servicio\" = :service");
            }
            if (!string.IsNullOrEmpty(oEntity.company))
            {
                lParameters.Add(new OracleParameter(":company", oEntity.company));
                sQuery.Append(" AND \"Compañia\" = :company");
            }
            if (!string.IsNullOrEmpty(oEntity.plan))
            {
                lParameters.Add(new OracleParameter(":plan", oEntity.plan));
                sQuery.Append(" AND \"Plan\" = :plan");
            }
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(\"Fecha Ingreso\", 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":finaldate", oEntity.finaldate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(\"Fecha Ingreso\", 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (!string.IsNullOrEmpty(oEntity.invoiced))
            {
                lParameters.Add(new OracleParameter(":invoiced", oEntity.invoiced));
                sQuery.Append(" AND Facturado = :invoiced");
            }
            sQuery.Append(" ORDER BY \"Numero Ingreso\" DESC");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }               

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        private DataTable GetStatusLogData(Cargo oEntity)
        {
            StringBuilder sQuery = new StringBuilder("SELECT * FROM VLogEstadoCargo WHERE 1 = 1");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter("initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FechaCargo, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter("finaldate", oEntity.finaldate.ToString("yyyyMMdd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FechaCargo, 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (!string.IsNullOrEmpty(oEntity.idadmission))
            {
                lParameters.Add(new OracleParameter("idadmission", oEntity.idadmission));
                sQuery.Append(" AND Ingreso = :idadmission");
            }
            if (!string.IsNullOrEmpty(oEntity.user))
            {
                lParameters.Add(new OracleParameter("usuario", oEntity.user));
                sQuery.Append(" AND Usuario = :usuario");
            }
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        private DataTable GetReturns(Cargo oEntity)
        {
            StringBuilder sQuery = new StringBuilder("SELECT * FROM VCargosDevueltos WHERE 1 = 1");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":initialdate", oEntity.initialdate.ToString("yyyyMMdd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FechaCargo, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= :initialdate");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":finaldate", oEntity.finaldate.ToString("yyyyMMdd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FechaCargo, 'YYYY-MM-DD'), 'YYYY-MM-DD') <= :finaldate");
            }
            if (!string.IsNullOrEmpty(oEntity.idadmission))
            {
                lParameters.Add(new OracleParameter(":idadmission", oEntity.idadmission));
                sQuery.Append(" AND Ingreso = :idadmission");
            }
            if (!string.IsNullOrEmpty(oEntity.user))
            {
                lParameters.Add(new OracleParameter(":usuario", oEntity.user));
                sQuery.Append(" AND Usuario = :usuario");
            }
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        private DataTable GetPaymentData(Cargo oEntity)
        {
            string[] lUser = null;
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT \"Fecha Ingreso\" Fecha, \"Numero Ingreso\" Ingreso, Autorizacion, EPS, \"Plan\", \"Tipo Identificacion\" TipoDocumento, \"Numero Identificacion\" Documento");
            sQuery.Append(", Apellido, Nombre, Servicio, Reconocido, Excedente, Cantidad, \"Valor Total\" Total, Origen, \"Ingresado por\" usuario, CodigoSubcentro, CodigoCentro, Centro, Subcentro, FechaDetalle");
            sQuery.Append(" FROM VAbonos WHERE FechaDetalle > \"Fecha Ingreso\"");
            if (!string.IsNullOrEmpty(oEntity.user))
            {
                if (oEntity.user.Contains(','))
                {
                    sQuery.Append(" AND (");
                    lUser = oEntity.user.Split(',');
                    for (int i = 0; i < lUser.Length; i++)
                    {
                        sQuery.Append("\"Ingresado por\" = '");
                        sQuery.Append(lUser[i]);
                        sQuery.Append("'");
                        if (i < lUser.Length - 1)
                        {
                            sQuery.Append(" OR ");
                        }
                    }
                    sQuery.Append(")");
                }
                else
                {
                    lParameters.Add(new OracleParameter(":usuario", oEntity.user));
                    sQuery.Append(" AND \"Ingresado por\" = :usuario");
                }
            }
            if (!string.IsNullOrEmpty(oEntity.idadmission))
            {
                lParameters.Add(new OracleParameter(":idadmission", oEntity.idadmission));
                sQuery.Append(" AND [Numero Ingreso] = :idadmission");
            }
            if (!string.IsNullOrEmpty(oEntity.eps))
            {
                lParameters.Add(new OracleParameter(":eps", oEntity.eps));
                sQuery.Append(" AND EPS LIKE '%' || :eps || '%' ");
            }
            if (!string.IsNullOrEmpty(oEntity.service))
            {
                lParameters.Add(new OracleParameter(":service", oEntity.service));
                sQuery.Append(" AND Servicio = :service");
            }
            if (!string.IsNullOrEmpty(oEntity.company))
            {
                lParameters.Add(new OracleParameter(":company", oEntity.company));
                sQuery.Append(" AND \"Compañia\" = :company");
            }
            if (!string.IsNullOrEmpty(oEntity.plan))
            {
                lParameters.Add(new OracleParameter(":plan", oEntity.plan));
                sQuery.Append(" AND \"Plan\" = :plan");
            }
            if (oEntity.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":initialdate", oEntity.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(\"Fecha Ingreso\", 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (oEntity.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":finaldate", oEntity.finaldate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(\"Fecha Ingreso\", 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (!string.IsNullOrEmpty(oEntity.invoiced))
            {
                lParameters.Add(new OracleParameter(":invoiced", oEntity.invoiced));
                sQuery.Append(" AND Facturado = :invoiced");
            }
            if (!string.IsNullOrEmpty(oEntity.authorization))
            {
                lParameters.Add(new OracleParameter(":authorization", oEntity.authorization));
                sQuery.Append(" AND Autorizacion = :authorization");
            }
            if (!string.IsNullOrEmpty(oEntity.patientdocument))
            {
                lParameters.Add(new OracleParameter(":document", oEntity.patientdocument));
                sQuery.Append(" AND [Numero Identificacion] = :document");
            }
            if (oEntity.canceled.HasValue)
            {
                lParameters.Add(new OracleParameter(":canceled", oEntity.canceled));
                sQuery.Append(" AND Anulado = :canceled");
            }
            if (!string.IsNullOrEmpty(oEntity.costcenter))
            {
                sQuery.Append(oEntity.costcenter);
            }
            sQuery.Append(" ORDER BY [Numero Ingreso] DESC");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            };
        }

        public DataTable GetFamisanar(DateTime initialdate, DateTime enddate)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT carfacdoc, ips, pactid, pacide, cardettot, total FROM VRIPSFAMISANAR");
            sQuery.Append("  WHERE TO_DATE(TO_CHAR(MOVFEC, 'YYYY-MM-DD'), 'YYYY-MM-DD') BETWEEN TO_DATE(:BEGINING, 'YYYY-MM-DD') AND TO_DATE(:END, 'YYYY-MM-DD')");
            lParameters.Add(new OracleParameter(":BEGINING", initialdate.ToString("yyyy-MM-dd")));
            lParameters.Add(new OracleParameter(":END", enddate.ToString("yyyy-MM-dd")));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            };
        }


        #endregion

        #region Métodos para el desarrollo para la Farmacia


        private DataTable GetPharmacyChargesData(Cargo cargo)
        {
            StringBuilder sQuery = new StringBuilder("SELECT * FROM VCARGOSVENTAFARMACIA WHERE 1 = 1");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            if (!string.IsNullOrEmpty(cargo.idadmission))
            {
                sQuery.Append(" AND INGRESO = :Ingreso");
                lParameters.Add(new OracleParameter("Ingreso", cargo.idadmission));
            }
            if (!string.IsNullOrEmpty(cargo.patientdocument))
            {
                sQuery.Append(" AND DOCUMENTO LIKE '%' || :Documento || '%'");
                lParameters.Add(new OracleParameter("Documento", cargo.patientdocument));
            }
            if (!string.IsNullOrEmpty(cargo.patientfullname))
            {
                sQuery.Append(" AND PACIENTE LIKE '%' || :Paciente || '%'");
                lParameters.Add(new OracleParameter("Paciente", cargo.patientfullname));
            }
            if (cargo.initialdate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":initialdate", cargo.initialdate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FECHA, 'YYYY-MM-DD'), 'YYYY-MM-DD') >= TO_DATE(:initialdate, 'YYYY-MM-DD')");
            }
            if (cargo.finaldate.Year > 1)
            {
                lParameters.Add(new OracleParameter(":finaldate", cargo.finaldate.ToString("yyyy-MM-dd")));
                sQuery.Append(" AND TO_DATE(TO_CHAR(FECHA, 'YYYY-MM-DD'), 'YYYY-MM-DD') <= TO_DATE(:finaldate, 'YYYY-MM-DD')");
            }
            if (!string.IsNullOrEmpty(cargo.sattentiontype))
            {
                sQuery.Append(" AND CF_ESTADO = :Estado");
                lParameters.Add(new OracleParameter("Estado", cargo.sattentiontype));
            }
            if (!string.IsNullOrEmpty(cargo.service))
            {
                sQuery.Append(" AND NOMARTICULO LIKE '%' || :Servicio || '%'");
                lParameters.Add(new OracleParameter("Servicio", cargo.service));
            }
            if (!string.IsNullOrEmpty(cargo.lastuser))
            {
                sQuery.Append(" AND USUARIO IN ('" + cargo.lastuser + "')");
                //lParameters.Add(new OracleParameter("Usuario", cargo.lastuser));
            }
            sQuery.Append(" ORDER BY Fecha DESC");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        public List<Cargo> GetPharmacyCharges(Cargo cargo)
        {
            DataTable dataTable = new DataTable();
            List<Cargo> pharmacyCharges = new List<Cargo>();            
            try
            {
                dataTable = this.GetPharmacyChargesData(cargo);
                foreach (DataRow item in dataTable.Rows)
                {
                    cargo = new Cargo()
                    {
                        patientdocument = item["DOCUMENTO"].ToString(),
                        date = Convert.ToDateTime(item["FECHA"]),
                        id = (item["CF_ID"] != DBNull.Value) ? Convert.ToInt32(item["CF_ID"]) : 0,
                        scharge = item["CARGO"].ToString(),
                        patientfullname = item["PACIENTE"].ToString(),
                        iqty = Convert.ToInt32(item["CANTIDAD"]),
                        ssource = item["CODARTICULO"].ToString(),
                        service = item["NOMARTICULO"].ToString(),
                        costcenter = item["CODCENTRO"].ToString(),
                        costname = item["CENTRO"].ToString(),
                        subcenter = item["CODBODEGA"].ToString(),
                        subcentername = item["BODEGA"].ToString(),
                        sattentiontype = item["CF_ESTADO"].ToString(),
                        idadmission = item["INGRESO"].ToString(),
                    };
                    pharmacyCharges.Add(cargo);
                }
                return pharmacyCharges;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de cargos de farmacia");
            }
            finally
            {
                dataTable.Dispose();
                dataTable = null;
            }
        }

        public void UpdatePharmacyEstatus(Cargo cargo)
        {
            StringBuilder sb = new StringBuilder();
            List<OracleParameter> parameters = new List<OracleParameter>();
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                if (cargo.id == 0)
                {
                    sb.Append("INSERT INTO CARGOFARMACIA (CF_CARGO, CF_ESTADO) VALUES (:Cargo, :Estado)");
                    parameters.Add(new OracleParameter("Cargo", cargo.scharge));
                    
                }
                else
                {
                    sb.Append("UPDATE CARGOFARMACIA SET Estado = :Estado WHERE CF_ID = :Id");
                    parameters.Add(new OracleParameter("Id", cargo.id));
                }
                parameters.Add(new OracleParameter("Estado", cargo.sattentiontype));
                oDAC.ExecuteNonQuery(sb.ToString(), parameters);
            }
            sb = null;
            parameters = null;
        }

        #endregion

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
