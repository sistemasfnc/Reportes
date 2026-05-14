using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using System.Data;
using System.Data.SqlClient;
using EventLog;
using Oracle.ManagedDataAccess.Client;

namespace DAC
{
    public class GlosaDAC : IDisposable
    {
        /// <summary>
        /// Cadena de conexión a la base de datos
        /// </summary>
        private string sConnection { get; set; }

        /// <summary>
        /// Método de inserta los conceptos de la glosa
        /// </summary>
        /// <param name="oEntity">Objeto que contiene los conceptos de la glosa</param>
        /// <param name="oDAC">Objeto de conexión a la base de datos</param>
        private void InsertConcept(ConceptoGlosa oEntity, OracleDAC oDAC)
        {
            //Declaración de variables
            List<OracleParameter> lParamenters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("INSERT INTO conceptosporglosa VALUES (:idcomment, :idconcept, :value, :observations)");
            //Inserta en la base de datos el concepto por glosa
            ///Asigna parámetros de query
            lParamenters.Add(new OracleParameter(":idcomment", oEntity.idcomment));
            lParamenters.Add(new OracleParameter(":idconcept", oEntity.idconcept));
            lParamenters.Add(new OracleParameter(":value", oEntity.conceptvalue));
            lParamenters.Add(new OracleParameter(":observations", oEntity.conceptobservations));
            //Ejecuta query
            oDAC.ExecuteNonQuery(sQuery.ToString(), lParamenters, false, true);            
        }

        /// <summary>
        /// Elimina todos los conceptos por glosa
        /// </summary>
        /// <param name="iComment">Id de la glosa</param>
        /// <param name="oDAC">Objeto manejador de la base de datos</param>
        /// <param name="isUpdate"></param>
        private void DeleteConcept(int iComment, OracleDAC oDAC, bool isUpdate = false)
        {
            //Definición de variables
            List<OracleParameter> lParameters = new List<OracleParameter>();
            string sQuery = "DELETE FROM conceptosporglosa WHERE cpg_gl_id = :gl_id";
            lParameters.Add(new OracleParameter(":gl_id", iComment));
            if (isUpdate)
            {
                sQuery += " AND cpg_cg_id IN (SELECT cg_id FROM conceptoglosa WHERE cg_grupo = '9')";
            }
            //Borra conceptos por glosa
            oDAC.ExecuteNonQuery(sQuery, lParameters, false, true);
        }

        /// <summary>
        /// Método que obtiene el listado de conceptos por glosa de la base de datos
        /// </summary>
        /// <returns>Data Table</returns>
        private DataTable GetConceptData(string sGroup = "")
        {
            //Declaración de variables
            string sQuery = "SELECT * FROM conceptoglosa WHERE cg_activo = 1";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            //Abre el objeto de base de datos
            using (OracleDAC oDAC = new OracleDAC())
            {
                if (!string.IsNullOrEmpty(sGroup))
                {
                    sQuery += " AND cg_grupo = :grupo";
                    lParameters.Add(new OracleParameter(":grupo", sGroup));
                }
                //Ejecuta query
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery, lParameters);
            }
        }

        /// <summary>
        /// Obtiene datatable con glosas de la base de datos
        /// </summary>
        /// <param name="oEntity">Objeto glosa contiene los filtros de la consulta</param>
        /// <returns>DataTable con glosas</returns>
        private DataTable GetCommentsData(Glosa oEntity)
        {
            //Declaración de variables
            List<OracleParameter> lParameters = new List<OracleParameter>();
            //StringBuilder sQuery = new StringBuilder("SELECT glosa.*, cg.cg_id, cg.cg_nombre, cg.cg_codigo, cg.cg_grupo, rg_aceptado, rg_observacion, cg1.cg_codigo rg_codigo, cg1.cg_nombre rg_nombre, cpg_valor, cpg_observacion");
            //sQuery.Append(" FROM glosa, conceptosporglosa, conceptoglosa cg, respuestaglosa, conceptoglosa cg1 WHERE gl_id = cpg_gl_id AND cpg_cg_id = cg.cg_id AND rg_gl_id = gl_id AND rg_cg_id = cg.cg_id AND rg_id = cg1.cg_id");
            StringBuilder sQuery = new StringBuilder("SELECT glosa.*, cg.cg_id, cg.cg_nombre, cg.cg_codigo, cg.cg_grupo, rg_aceptado, rg_observacion, cg1.cg_codigo rg_codigo, cg1.cg_nombre rg_nombre, cpg_valor, cpg_observacion");
            sQuery.Append(" FROM glosa INNER JOIN conceptosporglosa ON gl_id = cpg_gl_id INNER JOIN");
            sQuery.Append(" conceptoglosa cg ON cpg_cg_id = cg.cg_id LEFT JOIN respuestaglosa ON rg_gl_id = gl_id AND rg_cg_id = cg.cg_id LEFT JOIN conceptoglosa cg1 ON rg_id = cg1.cg_id ");
            sQuery.Append(" WHERE 1 = 1");
            //Arma la consulta
            if (!string.IsNullOrEmpty(oEntity.invoice))
            {
                sQuery.Append(" AND gl_factura = :gl_factura");
                lParameters.Add(new OracleParameter(":gl_factura", oEntity.invoice));
            }
            if (oEntity.type != 0)
            {
                sQuery.Append(" AND gl_tipo = :gl_tipo");
                lParameters.Add(new OracleParameter(":gl_tipo", oEntity.type));
            }
            if (oEntity.initialdate.HasValue)
            {
                sQuery.Append(" AND gl_fechatramite >= TO_DATE(:initial, 'YYYY-MM-DD')");
                lParameters.Add(new OracleParameter(":initial", oEntity.initialdate.Value.ToString("yyyy-MM-dd")));
            }
            if (oEntity.finaldate.HasValue)
            {
                sQuery.Append(" AND gl_fechatramite <= TO_DATE(:final, 'YYYY-MM-DD')");
                lParameters.Add(new OracleParameter(":final", oEntity.finaldate.Value.ToString("yyyy-MM-dd")));
            }
            if (oEntity.id != 0)
            {
                sQuery.Append(" AND gl_id = :gl_id");
                lParameters.Add(new OracleParameter(":gl_id", oEntity.id));
            }
            if (!string.IsNullOrEmpty(oEntity.company))
            {
                sQuery.Append(" AND gl_empresa = :gl_empresa");
                lParameters.Add(new OracleParameter(":gl_empresa", oEntity.company));
            }
            using (OracleDAC oDAC = new OracleDAC())
            {
                //Returna DataTable
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// Método que ejecuta la consulta en base de datos para obtener las respuestas de los conceptos por glosa
        /// </summary>
        /// <param name="oResponse">Objeto que contiene los filtros de la respuesta de la glosa</param>
        /// <returns></returns>
        private DataTable GetConceptResponseData(RespuestaGlosa oResponse)
        {
            //Declaración de variables
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT rg_id, rg_observacion, rg_aceptado, cg_nombre, rg_cg_id, rg_gl_id, rg_srg_id, rg_responsable, srg_crg_id FROM respuestaglosa, conceptoglosa, glosa, servicioresponsableglosa");
            sQuery.Append(" WHERE rg_gl_id = gl_id AND rg_id = cg_id AND rg_srg_id = srg_id");
            //Arma la consulta
            if (oResponse.idcomment != 0)
            {
                sQuery.Append(" AND rg_gl_id = :idcomment");
                lParameters.Add(new OracleParameter(":idcomment", oResponse.idcomment));
            }
            if (oResponse.idresponse != 0)
            {
                sQuery.Append(" AND rg_id = :idresponse");
                lParameters.Add(new OracleParameter(":idresponse", oResponse.idresponse));
            }
            if (oResponse.idconcept != 0)
            {
                sQuery.Append(" AND rg_cg_id = :idconcept");
                lParameters.Add(new OracleParameter(":idconcept", oResponse.idconcept));
            }
            using (OracleDAC oDAC = new OracleDAC())
            {
                //Returna DataTable
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// Método que retorna lista genérica de conceptos de glosa
        /// </summary>
        /// <param name="sGroup">Grup del concepto</param>
        /// <returns>Lista genérica de conceptos</returns>
        public List<ConceptoGlosa> GetConcepts(string sGroup = "")
        {
            //Declaración de variables
            List<ConceptoGlosa> lConcepto = new List<ConceptoGlosa>();
            DataTable dt = new DataTable();
            ConceptoGlosa oEntity = null;
            try
            {
                //Obtiene datatable de conceptos
                dt = this.GetConceptData(sGroup);
                //Recorre datatable de conceptos
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //Asigna valores al objeto concepto glosa
                    oEntity = new ConceptoGlosa()
                    {
                        conceptcode = dt.Rows[i]["cg_codigo"].ToString(),
                        idconcept = Convert.ToInt32(dt.Rows[i]["cg_id"]),
                        conceptname = dt.Rows[i]["cg_nombre"].ToString(),                        
                    };
                    //Agrega concepto a la lista
                    lConcepto.Add(oEntity);
                }
                return lConcepto;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de conceptos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        /// <summary>
        /// Método que inserta la glosa en la base de datos
        /// </summary>
        /// <param name="oEntity">Objeto que contiene los datos de la glosa</param>
        /// <returns>ID de la glosa</returns>
        public int Insert(Glosa oEntity)
        {
            //Declaracón de variables
            List<OracleParameter> lParamenters = new List<OracleParameter>();
            OracleDAC oDAC = new OracleDAC();                        
            StringBuilder sQuery = new StringBuilder("INSERT INTO glosa (gl_factura, gl_ingreso, gl_tipo, gl_fechatramite, gl_fecharespuesta, gl_observaciones, gl_empresa, gl_valor, gl_aceptado)");
            sQuery.Append(" VALUES (:gl_factura, :gl_ingreso, :gl_tipo, :gl_fechatramite, :gl_fecharespuesta, :gl_observaciones, :gl_empresa, :gl_valor, :gl_aceptado) RETURNING gl_id INTO :gl_id");
            try
            {
                OracleParameter gl_id = new OracleParameter("gl_id", OracleDbType.Int32, ParameterDirection.ReturnValue);
                lParamenters.Add(gl_id);
                //Asigna parámetros a la consulta
                lParamenters.Add(new OracleParameter(":gl_factura", oEntity.invoice));
                lParamenters.Add(new OracleParameter(":gl_ingreso", oEntity.charge));
                lParamenters.Add(new OracleParameter(":gl_tipo", oEntity.type));
                lParamenters.Add(new OracleParameter(":gl_fechatramite", oEntity.transactdate));
                //lParamenters.Add(new OracleParameter(":gl_fecharespuesta", oEntity.responsedate));
                lParamenters.Add(new OracleParameter(":gl_fecharespuesta", DBNull.Value));
                lParamenters.Add(new OracleParameter(":gl_observaciones", oEntity.observations));
                lParamenters.Add(new OracleParameter(":gl_empresa", oEntity.company));
                lParamenters.Add(new OracleParameter(":gl_valor", oEntity.value));
                lParamenters.Add(new OracleParameter(":gl_aceptado", oEntity.accepted));
                //Abre conexión a la base de datos
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                //Inicia transacción
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                //Ejecuta query para insertar glosa
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParamenters, false, true);
                oEntity.id = Convert.ToInt32(gl_id.Value.ToString());                
                //Recorre listado con los conceptos de la glosa
                for (int i = 0; i < oEntity.lConcept.Count; i++)
                {
                    //Inserta el concepto de la glosa
                    oEntity.lConcept[i].idcomment = oEntity.id;
                    this.InsertConcept(oEntity.lConcept[i], oDAC);
                }
                //Guarda los cambios
                oDAC.Commit();
                return oEntity.id;
            }
            catch (Exception ex)
            {
                //En caso de error se aborta la transacción y se devuelven los cambios
                if (oDAC.oracleConnection.State == ConnectionState.Open) oDAC.RollBack();
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al ingresar la información de la glosa y sus conceptos");                
            }
            finally
            {
                oDAC.Dispose();
                oDAC = null;
                sQuery = null;
                lParamenters = null;
            }            
        }                 

        /// <summary>
        /// Método que actualiza la información de la glosa
        /// </summary>
        /// <param name="oEntity">Objeto que contiene la información de la glosa</param>
        public void Update(Glosa oEntity, bool isUpdate = true)
        {
            //Declaracón de variables
            List<OracleParameter> lParamenters = new List<OracleParameter>();
            OracleDAC oDAC = new OracleDAC();
            StringBuilder sQuery = new StringBuilder("UPDATE glosa SET gl_aceptado = :gl_aceptado");            
            lParamenters.Add(new OracleParameter(":gl_aceptado", oEntity.accepted));     
            if (!string.IsNullOrEmpty(oEntity.invoice))
            {
                sQuery.Append(", gl_factura = :gl_factura");
                lParamenters.Add(new OracleParameter(":gl_factura", oEntity.invoice));
            }
            if (oEntity.type != 0)
            {
                sQuery.Append(", gl_tipo = :gl_tipo");
                lParamenters.Add(new OracleParameter(":gl_tipo", oEntity.type));
            }
            if (oEntity.transactdate.Year > 1900)
            {
                sQuery.Append(", gl_fechatramite = :gl_fechatramite");
                lParamenters.Add(new OracleParameter(":gl_fechatramite", oEntity.transactdate));
            }
            if (!string.IsNullOrEmpty(oEntity.observations))
            {
                sQuery.Append(", gl_observaciones = :gl_observaciones");
                lParamenters.Add(new OracleParameter(":gl_observaciones", oEntity.observations));
            }
            if (oEntity.responsedate.HasValue)
            {
                sQuery.Append(", gl_fecharespuesta = :gl_fecharespuesta");
                lParamenters.Add(new OracleParameter(":gl_fecharespuesta", oEntity.responsedate));
            }
            if (!string.IsNullOrEmpty(oEntity.charge))
            {
                sQuery.Append(", gl_ingreso = :gl_ingreso");
                lParamenters.Add(new OracleParameter(":gl_ingreso", oEntity.charge));
            }
            if (oEntity.value != 0)
            {
                sQuery.Append(", gl_valor = :gl_valor");
                lParamenters.Add(new OracleParameter(":gl_valor", oEntity.value));
            }
            if (!string.IsNullOrEmpty(oEntity.analysis))
            {
                sQuery.Append(", gl_analisis = :gl_analisis");
                lParamenters.Add(new OracleParameter(":gl_analisis", oEntity.analysis));
            }
            sQuery.Append(" WHERE gl_id = :gl_id");
            lParamenters.Add(new OracleParameter(":gl_id", oEntity.id));
            try
            {
                //Abre conexión a la base de datos
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                //Inicia transacción
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                //Ejecuta query para insertar glosa
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParamenters, true, false);
                //Elimina los conceptos de la glosa
                if (isUpdate)
                    this.DeleteConcept(oEntity.id, oDAC, false);
                //Recorre listado con los conceptos de la glosa
                for (int i = 0; i < oEntity.lConcept.Count; i++)
                {
                    //Inserta el concepto de la glosa
                    oEntity.lConcept[i].idcomment = oEntity.id;
                    this.InsertConcept(oEntity.lConcept[i], oDAC);
                }
                //Guarda los cambios
                oDAC.Commit();
            }
            catch (Exception ex)
            {
                //En caso de error se aborta la transacción y se devuelven los cambios
                if (oDAC.oracleConnection.State == ConnectionState.Open) oDAC.RollBack();
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al ingresar la información de la glosa y sus conceptos");
            }
            finally
            {
                oDAC.Dispose();
                oDAC = null;
                sQuery = null;
                lParamenters = null;
            }
        }

        /// <summary>
        /// Método que retorna el listado de glosas
        /// </summary>
        /// <param name="oEntity">Objeto que contiene filtros de información para la glosa</param>
        /// <returns>lista genérica de glosas</returns>
        public List<Glosa> GetComments(Glosa oEntity)
        {
            //Declaración de variables
            List<Glosa> lGlosa = new List<Glosa>();
            DataTable dt = new DataTable();
            ConceptoGlosa oConcepto = null;
            try
            {
                //Obtiene datatable con las glosas
                dt = this.GetCommentsData(oEntity);
                //Recorre datatable
                (from DataRow dr in dt.Rows group dr by Convert.ToInt32(dr["gl_id"]) into f
                 select new
                 {
                     Key = f.Key,
                     Elements = f,
                 }).ToList().ForEach(f =>
                 {
                     oEntity = new Glosa()
                     {
                         invoice = f.Elements.First()["gl_factura"].ToString(),
                         transactdate = Convert.ToDateTime(f.Elements.First()["gl_fechatramite"]),
                         responsedate = (f.Elements.First()["gl_fecharespuesta"] != DBNull.Value) ? Convert.ToDateTime(f.Elements.First()["gl_fecharespuesta"]) : (DateTime?)null,
                         observations = f.Elements.First()["gl_observaciones"].ToString(),
                         type = Convert.ToInt32(f.Elements.First()["gl_tipo"]),
                         company = f.Elements.First()["gl_empresa"].ToString(),
                         value = (f.Elements.First()["gl_valor"] != DBNull.Value) ? Convert.ToDecimal(f.Elements.First()["gl_valor"]) : 0,
                         accepted = (f.Elements.First()["gl_aceptado"] != DBNull.Value) ? Convert.ToDecimal(f.Elements.First()["gl_aceptado"]) : 0,
                         id = f.Key,
                         analysis = f.Elements.First()["gl_analisis"].ToString(),
                         lConcept = new List<ConceptoGlosa>(),
                     };
                     (from DataRow dr in f.Elements where Convert.ToInt32(dr["gl_id"]) == f.Key group dr by dr["cg_id"].ToString() into a
                      select new
                      {
                          Key = a.Key,
                          Elements = a,
                      }).ToList().ForEach(a =>
                      {
                          oConcepto = new ConceptoGlosa()
                          {
                              idcomment = f.Key,
                              idconcept = Convert.ToInt32(a.Elements.First()["cg_id"]),
                              conceptname = a.Elements.First()["cg_nombre"].ToString(),
                              conceptcode = a.Elements.First()["cg_codigo"].ToString(),
                              conceptgroup = a.Elements.First()["cg_grupo"].ToString(),
                              conceptobservations = a.Elements.First()["cpg_observacion"].ToString(),
                              conceptvalue = Convert.ToDecimal(a.Elements.First()["cpg_valor"]),
                              oResponse = new RespuestaGlosa()
                              {
                                  acceptedvalue = (a.Elements.First()["rg_aceptado"] != DBNull.Value) ? Convert.ToInt32(a.Elements.First()["rg_aceptado"]) : 0,
                                  observations = a.Elements.First()["rg_observacion"].ToString(),    
                                  responsecode = a.Elements.First()["rg_codigo"].ToString(),
                                  responsename = a.Elements.First()["rg_nombre"].ToString(),
                              }
                          };
                          oEntity.lConcept.Add(oConcepto);
                    });
                     lGlosa.Add(oEntity);
                });
                return lGlosa;
            }
            catch (Exception ex)
            {
                //Registra errores
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de glosas");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oConcepto = null;
                oEntity = null;
            }
        }

        /// <summary>
        /// Método que obtiene la información de la vista de glosas
        /// </summary>
        /// <param name="oEntity">Objeto glosa que contiene los parámetros para el filtro</param>
        /// <returns></returns>
        public DataTable GetCommentViewData(Glosa oEntity)
        {
            //Definción de variables
            StringBuilder sQuery = new StringBuilder("SELECT * FROM VGlosa WHERE 1 = 1");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            //Filtros según valores
            if (!string.IsNullOrEmpty(oEntity.invoice))
            {
                sQuery.Append(" AND invoice = :gl_factura");
                lParameters.Add(new OracleParameter(":gl_factura", oEntity.invoice));
            }
            if (oEntity.type != 0)
            {
                sQuery.Append(" AND [type] = :gl_tipo");
                lParameters.Add(new OracleParameter(":gl_tipo", oEntity.type));
            }
            if (oEntity.initialdate.HasValue)
            {
                sQuery.Append(" AND transactdate >= TO_DATE(:initial, 'YYYY-MM-DD')");
                lParameters.Add(new OracleParameter(":initial", oEntity.initialdate.Value.ToString("yyyy-MM-dd")));
            }
            if (oEntity.finaldate.HasValue)
            {
                sQuery.Append(" AND transactdate <= TO_DATE(:final, 'YYYY-MM-DD')");
                lParameters.Add(new OracleParameter(":final", oEntity.finaldate.Value.ToString("yyyy-MM-dd")));
            }
            if (oEntity.id != 0)
            {
                sQuery.Append(" AND id = :gl_id");
                lParameters.Add(new OracleParameter(":gl_id", oEntity.id));
            }
            if (!string.IsNullOrEmpty(oEntity.company))
            {
                sQuery.Append(" AND company = :gl_empresa");
                lParameters.Add(new OracleParameter(":gl_empresa", oEntity.company));
            }
            //Abre objeto de conexión a la base de datos
            using (OracleDAC oDAC = new OracleDAC())
            {
                //Ejecuta consulta y obtiene datatable
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// Obtiene una glosa
        /// </summary>
        /// <param name="oEntity">Objeto glosa con filtros</param>
        /// <returns>Retorna objeto glosa</returns>
        public Glosa GetComment(Glosa oEntity)
        {
            List<Glosa> lGlosa = this.GetComments(oEntity);
            if (lGlosa.Count > 0)
            {
                oEntity = lGlosa[0];
            }
            return oEntity;
        }

        /// <summary>
        /// Método que obtiene el listado de conceptos por glosa
        /// </summary>
        /// <param name="iComment">Id de la glosa</param>
        /// <returns></returns>
        public List<ConceptoGlosa> GetCoceptsByComment(int iComment)
        {
            //Definición de parámetros
            List<ConceptoGlosa> lConcepts = new List<ConceptoGlosa>();
            List<OracleParameter> lParameters = new List<OracleParameter>();
            OracleDAC oDAC = new OracleDAC();
            DataTable dt = new DataTable();
            ConceptoGlosa oConcept = null;
            StringBuilder sQuery = new StringBuilder("SELECT cg_id, cg_grupo, cg_codigo, cg_nombre, cpg_gl_id, cpg_valor, cpg_observacion FROM conceptoglosa INNER JOIN conceptosporglosa ON cg_id = cpg_cg_id");            
            if (iComment != 0)
            {
                sQuery.Append(" AND cpg_gl_id = :gl_id");
                lParameters.Add(new OracleParameter(":gl_id", iComment));
            }
            try
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                dt = oDAC.GetDataTable(sQuery.ToString(), lParameters);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oConcept = new ConceptoGlosa()
                    {
                        conceptcode = dt.Rows[i]["cg_codigo"].ToString(),
                        idcomment = Convert.ToInt32(dt.Rows[i]["cpg_gl_id"]),
                        idconcept = Convert.ToInt32(dt.Rows[i]["cg_id"]),
                        conceptobservations = dt.Rows[i]["cpg_observacion"].ToString(),
                        conceptvalue = Convert.ToDecimal(dt.Rows[i]["cpg_valor"]),
                        conceptname = dt.Rows[i]["cg_nombre"].ToString(),
                        conceptgroup = dt.Rows[i]["cg_grupo"].ToString(),
                    };
                    lConcepts.Add(oConcept);
                }
                return lConcepts;
            }
            catch (Exception ex)
            {
                //Registra errores
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de glosas");
            }
        }

        /// <summary>
        /// Método que obtiene el listado de respuestas por concepto de la glosa
        /// </summary>
        /// <param name="oResponse">Objeto que contiene los filtros de la respuesta de la glosa</param>
        /// <returns></returns>
        public List<RespuestaGlosa> GetConceptResponse(RespuestaGlosa oResponse)
        {
            //Declaración de variables
            List<RespuestaGlosa> lResponse = new List<RespuestaGlosa>();
            DataTable dt = new DataTable();            
            try
            {
                dt = this.GetConceptResponseData(oResponse);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oResponse = new RespuestaGlosa()
                    {
                        idcomment = Convert.ToInt32(dt.Rows[i]["rg_gl_id"]),
                        idconcept = Convert.ToInt32(dt.Rows[i]["rg_cg_id"]),
                        idresponse = Convert.ToInt32(dt.Rows[i]["rg_id"]),
                        observations = dt.Rows[i]["rg_observacion"].ToString(),
                        acceptedvalue = Convert.ToInt32(dt.Rows[i]["rg_aceptado"]), 
                        idservice = Convert.ToInt32(dt.Rows[i]["rg_srg_id"]),
                        responsible = dt.Rows[i]["rg_responsable"].ToString(),
                        idcategory = Convert.ToInt32(dt.Rows[i]["srg_crg_id"]),
                    };
                    lResponse.Add(oResponse);
                }
                return lResponse;
            }
            catch (Exception ex)
            {
                //Registra errores
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de respuestas por concepto por glosa");
            }            
        }
                        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oResponse"></param>
        public void InsertConceptResponse(RespuestaGlosa oResponse)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            string squery = "INSERT INTO respuestaglosa (rg_gl_id, rg_cg_id, rg_id, rg_observacion, rg_aceptado, rg_srg_id, rg_responsable) VALUES (:rg_gl_id, :rg_cg_id, :rg_id, :rg_observacion, :rg_aceptado, :rg_srg_id, :rg_responsable)";
            using (OracleDAC oDAC = new OracleDAC())
            {
                lParameters.Add(new OracleParameter(":rg_gl_id", oResponse.idcomment));
                lParameters.Add(new OracleParameter(":rg_cg_id", oResponse.idconcept));
                lParameters.Add(new OracleParameter(":rg_id", oResponse.idresponse));
                lParameters.Add(new OracleParameter(":rg_observacion", oResponse.observations));
                lParameters.Add(new OracleParameter(":rg_aceptado", oResponse.acceptedvalue));
                lParameters.Add(new OracleParameter(":rg_srg_id", oResponse.idservice));
                lParameters.Add(new OracleParameter(":rg_responsable", oResponse.responsible));
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(squery, lParameters);
                lParameters = null;
            }
        }

        /// <summary>
        /// Método que elimina las respuestas de concepto por glosa
        /// </summary>
        /// <param name="oResponse">Objeto con los parámetros del filtro de borrado</param>
        public void DeleteResponses(RespuestaGlosa oResponse)
        {
            //Definición de variables
            List<OracleParameter> lParameters = new List<OracleParameter>();
            string squery = "DELETE FROM respuestaglosa WHERE rg_gl_id = :idcomment AND rg_cg_id = :idconcept";
            using (OracleDAC oDAC = new OracleDAC())
            {
                lParameters.Add(new OracleParameter(":idcomment", oResponse.idcomment));
                lParameters.Add(new OracleParameter(":idconcept", oResponse.idconcept));
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(squery, lParameters);
                lParameters = null;
            }
        }

        /// <summary>
        /// Constructor del objeto
        /// </summary>
        /// <param name="connection">Cadena de conexón a la base de datos</param>
        public GlosaDAC(string connection)
        {
            //Asigna cadena de conexión
            this.sConnection = connection;
        }

        /// <summary>
        /// Método Dispose libera objetos de la memoria
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
