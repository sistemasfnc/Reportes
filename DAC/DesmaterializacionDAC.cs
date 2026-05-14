using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using Config;
using EventLog;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.IO;

namespace DAC
{
    public class DesmaterializacionDAC : IDisposable
    {
        public string sconnection { get; set; }

        public List<RelacionEnvio> GetRelationships(RelacionEnvio relacionEnvio)
        {
            List<RelacionEnvio> lrelacionEnvios = new List<RelacionEnvio>();
            DataTable dataTable = new DataTable();
            try
            {
                dataTable = this.GetRelationshipData(relacionEnvio);
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    relacionEnvio = new RelacionEnvio()
                    {
                        snumero = dataRow["ENVENCDOC"].ToString(),
                        dtfecha = Convert.ToDateTime(dataRow["ENVENCFEC"]),
                        sestado = dataRow["ENVENCEST"].ToString(),
                        iid = Convert.ToInt32(dataRow["ENVENCCSE"]),
                        sempresa = dataRow["ENVENCNIT"].ToString(),
                    };
                    lrelacionEnvios.Add(relacionEnvio);
                }
                return lrelacionEnvios;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de relaciones de envío");
            }
            finally
            {
                dataTable.Dispose();
                dataTable = null;
            }
        }

        private DataTable GetRelationshipData(RelacionEnvio relacionEnvio)
        {
            StringBuilder stringBuilder = new StringBuilder("SELECT * FROM VRADICACIONENVIO WHERE ENVENCNIT IN (" + relacionEnvio.sempresa + ") ORDER BY ENVENCFRA DESC");
            //List<OracleParameter> loracleParameters = new List<OracleParameter>();
            using (OracleDAC oracle = new OracleDAC())
            {
                //loracleParameters.Add(new OracleParameter("ENVENCNIT", relacionEnvio.sempresa));
                oracle.sConnection = this.sconnection;
                oracle.Connect();
                return oracle.GetDataTable(stringBuilder.ToString(), null);
            }
        }

        public void PutUploadFile(string srelationship, string sfile)
        {
            this.DeleteFileforUpload(srelationship);
            this.FileforUpload(srelationship, sfile);
        }

        public List<Generic> GetFilesForUpload(string ssource)
        {
            string squery = "SELECT AC_RELACION, AC_ARCHIVO FROM ARCHIVOCARGACOMPENSAR WHERE AC_CARGADO = 0";
            DataTable dataTable = new DataTable();
            OracleDAC oracle = new OracleDAC();
            Generic generic = null;
            List<Generic> lFiles = new List<Generic>();
            string[] apaths = null;
            try
            {
                oracle.sConnection = this.sconnection;
                oracle.Connect();
                dataTable = oracle.GetDataTable(squery, null);
                
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    apaths = new string[] { ssource, dataRow["AC_RELACION"].ToString(), dataRow["AC_ARCHIVO"].ToString() };
                    generic = new Generic()
                    {
                        code = dataRow["AC_RELACION"].ToString(),
                        name = Path.Combine(apaths),
                    };
                    lFiles.Add(generic);
                }
                return lFiles;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al eliminar la relación de envío");
            }
            finally
            {
                oracle.Dispose();
                dataTable.Dispose();
                oracle = null;
                dataTable = null;
                apaths = null;
            }
            
        }


        private void DeleteFileforUpload(string srelationship)
        {            
            List<OracleParameter> oracleParameters = new List<OracleParameter>();
            OracleDAC oracleDAC = new OracleDAC();
            string squery = "DELETE FROM ARCHIVOCARGACOMPENSAR WHERE AC_RELACION = :AC_RELACION";
            try
            {
                oracleDAC.sConnection = this.sconnection;
                oracleDAC.Connect();
                oracleParameters.Add(new OracleParameter("AC_RELACION", srelationship));
                oracleDAC.ExecuteNonQuery(squery, oracleParameters);
            }
            catch (Exception ex)
            {

                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al eliminar la relación de envío");
            }
            finally
            {
                oracleParameters = null;
                oracleDAC.Dispose();
                oracleDAC = null;
            }
        }

        public void UpdateFileStatus(List<Generic> lFiles)
        {
            OracleDAC oracleDAC = new OracleDAC();
            string squery = "UPDATE ARCHIVOCARGACOMPENSAR SET AC_CARGADO = 1 WHERE AC_RELACION = :AC_RELACION";
            List<OracleParameter> oracleParameters = new List<OracleParameter>();
            try
            {
                oracleDAC.sConnection = this.sconnection;                
                oracleDAC.Connect();
                oracleDAC.oracleTransaction = oracleDAC.oracleConnection.BeginTransaction();
                foreach (var item in lFiles)
                {
                    if (item.id == 1)
                    {
                        oracleParameters.Add(new OracleParameter("AC_RELACION", item.code));
                        oracleDAC.ExecuteNonQuery(squery, oracleParameters);
                        oracleParameters.RemoveAt(0);
                    }
                }
                oracleDAC.Commit();
            }
            catch (Exception ex)
            {
                if (oracleDAC.oracleConnection.State == ConnectionState.Open)
                {
                    oracleDAC.RollBack();
                }
                LogError.WriteError("Facturacion", "DAC", ex);
                throw;
            }
            finally
            {
                oracleDAC.Dispose();
                oracleDAC = null;
                oracleParameters = null;
            }
        }

        public List<Generic> GetSupports(string sepisode)
        {
            DataTable dataTable = new DataTable();
            List<Generic> lgeneric = new List<Generic>();
            try
            {
                dataTable = this.GetSupportsData(sepisode);
                foreach (DataRow dr in dataTable.Rows)
                {
                    var generic = new Generic()
                    {
                        code = dr["TIPO"].ToString(),
                        name = dr["ARCHIVO"].ToString(),
                        date = Convert.ToDateTime(dr["FECHA"]),
                    };
                    lgeneric.Add(generic);
                }
                return lgeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener los soportes de la factura");
            }
            finally
            {
                dataTable.Dispose();
                dataTable = null;
            }
        }


        private DataTable GetSupportsData(string sepisode)
        {
            string squery = "SELECT ARCHIVO, TIPO, FECHA FROM VSOPORTESFACTURA WHERE EPISODIO = :EPISODIO";
            List<OracleParameter> oracleParameters = new List<OracleParameter>();
            using (OracleDAC oracle = new OracleDAC())
            {
                oracle.sConnection = this.sconnection;
                oracle.Connect();
                oracleParameters.Add(new OracleParameter("EPISODIO", sepisode));
                return oracle.GetDataTable(squery, oracleParameters);
            }
        }

        private void FileforUpload(string srelationship, string sfile)
        {
            string squery = "INSERT INTO ARCHIVOCARGACOMPENSAR (AC_RELACION, AC_ARCHIVO, AC_CARGADO) VALUES (:AC_RELACION, :AC_ARCHIVO, 0)";
            List<OracleParameter> oracleParameters = new List<OracleParameter>();
            OracleDAC oracleDAC = new OracleDAC();
            try
            {
                oracleDAC.sConnection = this.sconnection;
                oracleDAC.Connect();
                oracleParameters.Add(new OracleParameter("AC_RELACION", srelationship));
                oracleParameters.Add(new OracleParameter("AC_ARCHIVO", sfile));
                oracleDAC.ExecuteNonQuery(squery, oracleParameters);
            }
            catch (Exception ex)
            {

                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al insertar el archivo para cagar");
            }
            finally
            {
                oracleParameters = null;
                oracleDAC.Dispose();
                oracleDAC = null;
            }
        }

        public List<Desmaterializacion> GetInvoicesDetail(string srelacion, string ssource)
        {
            List<Desmaterializacion> ldesmaterializacion = new List<Desmaterializacion>();
            DataTable dataTable = new DataTable();            
            try
            {
                dataTable = this.GetInvoicesData(srelacion, ssource);
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    var desmaterializacion = new Desmaterializacion()
                    {
                        sarchivo = dataRow["ARCHIVO"].ToString(),
                        sautorizacion = dataRow["AUTORIZACION"].ToString(),
                        scantidad = dataRow["CANTIDAD"].ToString(),
                        scausaexterna = dataRow["CAUSAEXTERNA"].ToString(),
                        scopago = dataRow["COPAGO"].ToString(),
                        scups = dataRow["CUPS"].ToString(),
                        sdiagnostico = dataRow["DIAGNOSTICO"].ToString(),
                        sdocumento = dataRow["DOCUMENTO"].ToString(),
                        sfactura = dataRow["FACTURA"].ToString(),
                        sfechaegreso = dataRow["FECHAEGRESO"].ToString(),
                        sfechafactura = dataRow["FECHAFACTURA"].ToString(),
                        sfechaingreso = dataRow["FECHAINGRESO"].ToString(),
                        //sfuentefactura = dataRow["FUENTE"].ToString(),
                        sfuentefactura = "SETT",
                        singreso = dataRow["INGRESO"].ToString(),
                        sprimerapellido = dataRow["PRIMERAPELLIDO"].ToString(),
                        sprimernombre = dataRow["PRIMERNOMBRE"].ToString(),
                        ssegundoapellido = dataRow["SEGUNDOAPELLIDO"].ToString(),
                        ssegundonombre = dataRow["SEGUNDONOMBRE"].ToString(),
                        sservicio = dataRow["SERVICIO"].ToString(),
                        stipodocumento = dataRow["TIPODOCUMENTO"].ToString(),
                        svalorfacturado = dataRow["VALORFACTURADO"].ToString(),
                        svalorunitario = dataRow["VALORUNITARIO"].ToString(),
                        stiponota = dataRow["TIPONOTA"].ToString(),
                        svalornota = dataRow["VAlORNOTA"].ToString(),
                        sarchivonc = dataRow["ARCHIVONC"].ToString(),
                        sarchivond = dataRow["ARCHIVOND"].ToString(),
                        snotadebito = dataRow["NOTADEBITO"].ToString(),
                        svalordebito = dataRow["VALORDEBITO"].ToString(),
                        sepisodio = dataRow["EPISODIO"].ToString(),
                        bmultiple = (Convert.ToInt32(dataRow["MULTIPLE"]) > 1),
                    };
                    ldesmaterializacion.Add(desmaterializacion);
                }
                return ldesmaterializacion;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de motivos por cargo");
            }
            finally
            {
                dataTable.Dispose();
                dataTable = null;
            }
        }

        private DataTable GetInvoicesData(string srelacion, string ssource)
        {
            StringBuilder stringBuilder = new StringBuilder("SELECT * FROM VDETALLERELACIONENVIO WHERE FUENTERELACION = :FUENTERELACION AND RELACION = :RELACION");            
            List<OracleParameter> loracleParameters = new List<OracleParameter>();
            using (OracleDAC oracle = new OracleDAC())
            {
                loracleParameters.Add(new OracleParameter("FUENTERELACION", ssource));
                loracleParameters.Add(new OracleParameter("RELACION", srelacion));
                oracle.sConnection = this.sconnection;
                oracle.Connect();
                return oracle.GetDataTable(stringBuilder.ToString(), loracleParameters);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
