using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using System.Data;
using System.Data.OleDb;
using EventLog;
using System.Data.SqlClient;
using Utils;
using Oracle.ManagedDataAccess.Client;

namespace DAC
{
    public class CostosDAC : IDisposable
    {
        /// <summary>
        /// Cadena de conexión a la base de datos
        /// </summary>
        public string sConnection { get; set; }

        /// <summary>
        /// Método que retorna el listado de empleados activos en novasoft
        /// </summary>
        /// <param name="oEmploy">Objeto empleado con los filtros</param>
        /// <returns>Lista genérica con los empleados</returns>
        public List<Employee> GetEmployees(Employee oEmploy)
        {
            List<Employee> lEmploy = new List<Employee>();
            DataTable dt = new DataTable();
            Cost oCost = null;
            int i = 1;
            try
            {
                dt = this.GeEmployeesData(oEmploy);
                (from DataRow dr in dt.Rows
                 group dr by dr["EMPLEA"] into f
                 select new
                 {
                     Key = f.Key,
                     Elements = f,
                 }).ToList().ForEach(f =>
                 {
                     oEmploy = new Employee()
                     {
                         sdocument = f.Elements.First()["EMPLEA"].ToString().Trim(),
                         slastname = f.Elements.First()["APELLIDO1"].ToString().Trim() + " " + f.Elements.First()["APELLIDO2"].ToString().Trim(),                         
                         sname = f.Elements.First()["NOM"].ToString().Trim(),
                         smaincostcenter = f.Elements.First()["CCOSTO"].ToString().Trim(),                         
                         ihours = Convert.ToInt32(f.Elements.First()["H_MES"]),
                         ssurname = f.Elements.First()["APELLIDO1"].ToString().Trim(),
                         lCost = new List<Cost>(),
                     };
                     (from DataRow dr in f.Elements
                      where dr["EMPLEA"].ToString() == f.Key.ToString()
                      group dr by dr["CENTRO"].ToString() into a
                      select new
                      {
                          Key = a.Key,
                          Elements = a,
                      }).ToList().ForEach(a =>
                      {
                          oCost = new Cost()
                          {
                              dvalue = (a.Elements.First()["PORCENTA"] != DBNull.Value) ? Tools.CastPercent(a.Elements.First()["PORCENTA"].ToString()) : 0, 
                              scode = (a.Elements.First()["CENTRO"] != DBNull.Value) ? a.Elements.First()["CENTRO"].ToString().Trim() : string.Empty,
                              dtotal = (a.Elements.First()["PORCENTA"] != DBNull.Value) ? Tools.CastPercent(a.Elements.First()["PORCENTA"].ToString()) : 0,
                              id = i,
                              istatus = 1,                              
                          };
                          i++;
                          oEmploy.lCost.Add(oCost);
                      });
                     lEmploy.Add(oEmploy);
                 });
                return lEmploy;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de empleados");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
            
        }        

        /// <summary>
        /// Método que obtiene lista genérica con los centros de costos de Servinte
        /// </summary>
        /// <returns>Lista genérica con código y nombre del centro de costo</returns>
        public List<Generic> GetCostList()
        {
            List<Generic> lCost = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oGeneric = null;
            try
            {
                dt = this.GetCostData();
                foreach (DataRow dr in dt.Rows)
                {
                    oGeneric = new Generic()
                    {
                        code = dr["ccocod"].ToString().Trim(),
                        name = dr["cconom"].ToString().Trim(),
                    };
                    lCost.Add(oGeneric);
                }
                return lCost;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de centros de costos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oGeneric = null;
            }
        }

        /// <summary>
        /// Método que obtiene lista genérica con los centros de costos de Servinte
        /// </summary>
        /// <returns>Lista genérica con código y nombre del centro de costo</returns>
        public List<Cost> GetCostList(Cost oCost)
        {
            List<Cost> lCost = new List<Cost>();
            DataTable dt = new DataTable();
            Cost oGeneric = null;
            try
            {
                dt = this.GetCostData(oCost);
                foreach (DataRow dr in dt.Rows)
                {
                    oGeneric = new Cost()
                    {
                        scode = dr["co_centro"].ToString().Trim(),
                        sname = dr["co_nombre"].ToString().Trim(),
                        id = Convert.ToInt32(dr["co_id"]),
                        ctype = Convert.ToChar(dr["co_tipo"]),
                        istatus = Convert.ToInt16(dr["co_inactivo"]),
                    };
                    lCost.Add(oGeneric);
                }
                return lCost;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de centros de costos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oGeneric = null;
            }

        }

        /// <summary>
        /// Método para obtener el listado de procesos de liquidación almacenados
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo para filtros</param>
        /// <returns></returns>
        public List<CostProcess> GetCostProcessList(CostProcess oProcess)
        {
            List<CostProcess> lcostProcesses = new List<CostProcess>();
            DataTable dt = new DataTable();
            Cost oCost = null;
            string sSurname = string.Empty;
            try
            {
                dt = this.GetProcessData(oProcess);
                (from DataRow dr in dt.Rows
                 group dr by Convert.ToInt32(dr["pc_id"]) into f
                 select new
                 {
                     Key = f.Key,
                     Elements = f,
                 }).ToList().ForEach(f =>
                 {
                     oProcess = new CostProcess();
                     sSurname = f.Elements.First()["pc_apellido"].ToString();
                     sSurname = sSurname.Substring(0, sSurname.IndexOf(' '));
                     oProcess.sdocument = f.Elements.First()["pc_documento"].ToString();
                     oProcess.iclosed = Convert.ToInt32(f.Elements.First()["pc_cerrado"]);
                     oProcess.iid = f.Key;
                     oProcess.itype = Convert.ToInt32(f.Elements.First()["pc_liquidapor"]);
                     oProcess.imonth = Convert.ToInt32(f.Elements.First()["pc_mes"]);
                     oProcess.iyear = Convert.ToInt32(f.Elements.First()["pc_ano"]);
                     oProcess.iuser = Convert.ToInt32(f.Elements.First()["pc_us_id"]);
                     oProcess.ihours = Convert.ToInt32(f.Elements.First()["pc_horas"]);
                     oProcess.scostcenter = f.Elements.First()["pc_centro"].ToString();
                     oProcess.oEmployee = new Employee()
                     {
                         sdocument = f.Elements.First()["pc_documento"].ToString(),
                         ihours = Convert.ToInt32(f.Elements.First()["pc_horas"]),
                         slastname = f.Elements.First()["pc_apellido"].ToString(),
                         sname = f.Elements.First()["pc_nombre"].ToString(),
                         smaincostcenter = f.Elements.First()["pc_centro"].ToString(),
                         ssurname = sSurname,
                     };
                     oProcess.lCost = new List<Cost>();
                     (from DataRow dr in f.Elements
                      where Convert.ToInt32(dr["dc_pc_id"]) == f.Key && (dr["co_tipo"].ToString() == "O" || dr["co_tipo"].ToString() == "N")
                      group dr by dr["dc_id"].ToString() into a
                      select new
                      {
                          Key = a.Key,
                          Elements = a,
                      }).ToList().ForEach(a =>
                      {
                          oCost = new Cost()
                          {
                              dvalue = (a.Elements.First()["dc_valor"] != DBNull.Value) ? Convert.ToDecimal(a.Elements.First()["dc_valor"].ToString()) : 0,
                              scode = (a.Elements.First()["dc_centro"] != DBNull.Value) ? a.Elements.First()["dc_centro"].ToString() : string.Empty,
                              id = Convert.ToInt32(a.Elements.First()["dc_id"]),
                              iuser = Convert.ToInt32(a.Elements.First()["dc_us_id"]),
                              ctype = Convert.ToChar(a.Elements.First()["co_tipo"]),
                              dtotal = (a.Elements.First()["dc_total"] != DBNull.Value) ? Convert.ToDecimal(a.Elements.First()["dc_total"].ToString()) : 0,
                              istatus = Convert.ToInt16(a.Elements.First()["dc_liquidapor"]),
                          };
                          oProcess.lCost.Add(oCost);
                      });
                     lcostProcesses.Add(oProcess);
                 });
                return lcostProcesses;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        /// <summary>
        /// Método que obtiene el objeto Proceso Costo asociado a un empleado
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo para filtros</param>
        /// <returns>Objeto proceso costo encontrado</returns>
        public CostProcess GetProcess(CostProcess oProcess)
        {            
            DataTable dt = new DataTable();
            Cost oCost = null;
            try
            {
                dt = this.GetProcessData(oProcess);
                (from DataRow dr in dt.Rows
                 group dr by Convert.ToInt32(dr["pc_id"]) into f
                 select new
                 {
                     Key = f.Key,
                     Elements = f,
                 }).ToList().ForEach(f =>
                 {
                     oProcess.sdocument = f.Elements.First()["pc_documento"].ToString();
                     oProcess.iclosed = Convert.ToInt32(f.Elements.First()["pc_cerrado"]);
                     oProcess.iid = f.Key;
                     oProcess.itype = Convert.ToInt32(f.Elements.First()["pc_liquidapor"]);
                     oProcess.imonth = Convert.ToInt32(f.Elements.First()["pc_mes"]);
                     oProcess.iyear = Convert.ToInt32(f.Elements.First()["pc_ano"]);
                     oProcess.iuser = Convert.ToInt32(f.Elements.First()["pc_us_id"]);
                     oProcess.lCost = new List<Cost>();                     
                     (from DataRow dr in f.Elements
                      where Convert.ToInt32(dr["dc_pc_id"]) == f.Key
                      group dr by dr["dc_id"].ToString() into a
                      select new
                      {
                          Key = a.Key,
                          Elements = a,
                      }).ToList().ForEach(a =>
                      {
                          oCost = new Cost()
                          {
                              dvalue = (a.Elements.First()["dc_valor"] != DBNull.Value) ? Convert.ToDecimal(a.Elements.First()["dc_valor"].ToString()) : 0,
                              scode = (a.Elements.First()["dc_centro"] != DBNull.Value) ? a.Elements.First()["dc_centro"].ToString() : string.Empty,                              
                              id = Convert.ToInt32(a.Elements.First()["dc_id"]),
                              iuser = Convert.ToInt32(a.Elements.First()["dc_us_id"]),
                              ctype = Convert.ToChar(a.Elements.First()["co_tipo"]),
                              dtotal = (a.Elements.First()["dc_total"] != DBNull.Value) ? Convert.ToDecimal(a.Elements.First()["dc_total"].ToString()) : 0,
                              istatus = Convert.ToInt16(a.Elements.First()["dc_liquidapor"]),
                          };                          
                          oProcess.lCost.Add(oCost);
                      });                     
                 });
                return oProcess;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }        

        /// <summary>
        /// Método que obtiene el listado de centros de costo de un empleado
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo para filtro</param>
        /// <returns>Lista genérica objeto costo</returns>
        public List<Cost> GetEmployeeCostList(CostProcess oProcess)
        {
            List<Cost> lCost = new List<Cost>();
            DataTable dt = new DataTable();
            Cost oCost = null;
            try
            {
                dt = this.GetEmployeeCostData(oProcess);
                foreach (DataRow dr in dt.Rows)
                {
                    oCost = new Cost()
                    {
                        dvalue = Convert.ToDecimal(dr["dc_valor"]),
                        scode = dr["dc_centro"].ToString(),
                        id = Convert.ToInt64(dr["dc_id"]),
                        iuser = Convert.ToInt32(dr["dc_us_id"]),
                        dtotal = Convert.ToDecimal(dr["dc_total"]),
                        istatus = Convert.ToInt16(dr["dc_liquidapor"]),
                    };
                    lCost.Add(oCost);
                }
                return lCost;

            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de costos por año y mes");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oCost = null;
            }
        }
        
        /// <summary>
        /// Método que almacena en bd el listado de costos asociados a un empleado
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo</param>
        /// <param name="oEmployee">Objeto empleado</param>
        /// <returns>Entero con el ID del proceso costo creado</returns>
        public int SaveCostList(CostProcess oProcess, Employee oEmployee)
        {
            OracleDAC oDAC = new OracleDAC();            
            try
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                oProcess.iid = this.SaveProcess(oProcess, oEmployee, oDAC);                    
                this.DeleteList(oProcess, oDAC);
                this.SaveList(oProcess, oDAC);
                if (oProcess.iclosed == 1) this.SetRealValues(oProcess, oDAC);
                oDAC.Commit();                
                return oProcess.iid;
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
        /// Método para almacenar el proceso de liquidación con un listado de empelados
        /// </summary>
        /// <param name="lCostProcess">Objeto lista genérica proceso de liquidación</param>
        public void SaveCostList(List<CostProcess> lCostProcess)
        {
            OracleDAC oDAC = new OracleDAC();
            try
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                foreach (CostProcess oProcess in lCostProcess)
                {
                    if (oProcess.iid == 0) oProcess.iid = this.SaveProcess(oProcess, oProcess.oEmployee, oDAC);
                    else this.EditProcess(oProcess, oDAC);
                    this.DeleteList(oProcess, oDAC);
                    this.SaveList(oProcess, oDAC);
                    //this.SetRealValues(oProcess, oDAC);
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
        /// Método para salvar la información de los centros de costo de investigación y de educación
        /// </summary>
        /// <param name="oProcess"></param>
        /// <param name="sType"></param>
        public void SaveSpecialCostList(CostProcess oProcess, string sType)
        {
            OracleDAC oDAC = new OracleDAC();
            try
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                this.EditProcess(oProcess, oDAC);
                this.DeleteList(oProcess, oDAC);
                this.DeleteSpecial(oProcess, oDAC, sType);
                this.SaveList(oProcess, oDAC);
                this.SetRealValues(oProcess, oDAC);
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
        /// Método que realiza el proceso de actualización en la bd el proceso costo de un empleado
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo</param>
        public void EditCostList(CostProcess oProcess)
        {
            OracleDAC oDAC = new OracleDAC();
            try
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.oracleTransaction = oDAC.oracleConnection.BeginTransaction();
                this.EditProcess(oProcess, oDAC);
                this.DeleteList(oProcess, oDAC);
                this.SaveList(oProcess, oDAC);
                if (oProcess.iclosed == 1) this.SetRealValues(oProcess, oDAC);
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
        /// Método que genera lista con la información de los procesos de costos almacenados
        /// </summary>
        /// <param name="oReport">Objeto reporte con los filtros de búsqueda</param>
        /// <returns>Lista genérica proceso costo dealle</returns>
        public List<CostReport> GetCostReport(CostReport oReport)
        {
            List<CostReport> lReport = new List<CostReport>();
            DataTable dt = new DataTable();
            try
            {
                dt = this.GetCostReportData(oReport);
                foreach (DataRow dr in dt.Rows)
                {
                    oReport = new CostReport()
                    {
                        imonth = Convert.ToInt32(dr["pc_mes"]),
                        smonth = Utils.Tools.GetMonthName(Convert.ToInt32(dr["pc_mes"])),
                        iyear = Convert.ToInt32(dr["pc_ano"]),
                        sdocument = dr["pc_documento"].ToString(),
                        scode = dr["dc_centro"].ToString(),
                        dvalue = (Convert.ToInt32(dr["pc_liquidapor"]) == 1) ? Convert.ToDecimal(dr["dc_valor"]) : 0,
                        dtotal = Convert.ToDecimal(dr["pc_completado"]),
                        sstatus = (Convert.ToInt32(dr["pc_cerrado"]) == 0) ? "NO" : "SI",
                        sfirstname = dr["pc_nombre"].ToString(),
                        slastname = dr["pc_apellido"].ToString(),
                        smaincostcenter = dr["pc_centro"].ToString(),  
                        suser = dr["us_usuario"].ToString(),
                    };
                    oReport.dvalue = (Convert.ToInt32(dr["pc_liquidapor"]) == 1) ? Convert.ToDecimal(dr["dc_valor"]) : (Convert.ToDecimal(dr["dc_valor"]) / Convert.ToDecimal(dr["pc_horas"])) * 100;
                    lReport.Add(oReport);
                }
                return lReport;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw;
            }
            finally
            {
                dt.Dispose();
                dt = null;

            }
        }

        /// <summary>
        /// Método que actualiza la tabla proceso costo
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo</param>
        /// <param name="oDAC">Objeto manejador de base de datos</param>
        private void EditProcess(CostProcess oProcess, OracleDAC oDAC)
        {
            string sQuery = "UPDATE procesocosto SET pc_us_id = :user, pc_cerrado = :close, pc_liquidapor = :type, pc_completado = :total WHERE pc_id = :id";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":user", oProcess.iuser));            
            lParameters.Add(new OracleParameter(":type", oProcess.itype));
            lParameters.Add(new OracleParameter(":close", oProcess.iclosed));
            lParameters.Add(new OracleParameter(":total", oProcess.dtotal));
            lParameters.Add(new OracleParameter(":id", oProcess.iid));
            oDAC.ExecuteNonQuery(sQuery, lParameters, false, true);
        }

        /// <summary>
        /// Método que inserta registro en la tabla proceso costo
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo</param>
        /// <param name="oEmployee">Objeto empleado</param>
        /// <param name="oDAC">Objeto manejador de base de datos</param>
        /// <returns>Número entero con el ID creado de la tabla procesocosto</returns>
        private int SaveProcess(CostProcess oProcess, Employee oEmployee, OracleDAC oDAC)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO procesocosto (pc_us_id, pc_documento, pc_mes, pc_ano, pc_cerrado, pc_liquidapor, pc_nombre, pc_apellido, pc_centro, pc_completado, pc_horas)");
            sQuery.Append(" VALUES (:user, :document, :month, :year, :close, :type, :name, :lastname, :costcenter, :total, :hours);  RETURNING pc_id INTO :pc_id;");
            OracleParameter pc_id = new OracleParameter("pc_id", OracleDbType.Int32, ParameterDirection.Output);
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":user", oProcess.iuser));
            lParameters.Add(new OracleParameter(":document", oProcess.sdocument));
            lParameters.Add(new OracleParameter(":month", oProcess.imonth));
            lParameters.Add(new OracleParameter(":year", oProcess.iyear));
            lParameters.Add(new OracleParameter(":close", oProcess.iclosed));
            lParameters.Add(new OracleParameter(":type", oProcess.itype));
            lParameters.Add(new OracleParameter(":name", oEmployee.sname));
            lParameters.Add(new OracleParameter(":lastname", oEmployee.slastname));
            lParameters.Add(new OracleParameter(":costcenter", oEmployee.smaincostcenter));
            lParameters.Add(new OracleParameter(":total", oProcess.dtotal));
            lParameters.Add(new OracleParameter(":hours", oEmployee.ihours));
            lParameters.Add(pc_id);
            oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false, true);
            return Convert.ToInt32(pc_id.Value);            
        }

        /// <summary>
        /// Método que inserta registros en la tabla detalleprocesocosto
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo</param>
        /// <param name="oDAC">Objeto manejador de base de datos</param>
        private void SaveList(CostProcess oProcess, OracleDAC oDAC)
        {
            StringBuilder sQuery = new StringBuilder();
            List<OracleParameter> lParameters = new List<OracleParameter>();
            foreach (Cost oCost in oProcess.lCost)
            {
                sQuery.Append("INSERT INTO detalleprocesocosto (dc_centro, dc_valor, dc_fechaproceso, dc_pc_id, dc_us_id, dc_total, dc_liquidapor) VALUES");
                sQuery.Append("(:cost, :value, GETDATE(), :process, :user, :total, :type)");
                lParameters.Add(new OracleParameter(":cost", oCost.scode));
                lParameters.Add(new OracleParameter(":value", oCost.dvalue));
                lParameters.Add(new OracleParameter(":process", oProcess.iid));
                lParameters.Add(new OracleParameter(":user", (oCost.iuser != 0) ? oCost.iuser : oProcess.iuser));
                lParameters.Add(new OracleParameter(":total", oCost.dtotal));
                lParameters.Add(new OracleParameter(":type", oProcess.itype));
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false, true);
                lParameters.RemoveRange(0, lParameters.Count);
                sQuery.Remove(0, sQuery.Length);
            }
        }

        /// <summary>
        /// Método que agrega los valores de distribución reales a los centros de costo para investigación y educación
        /// </summary>
        /// <param name="oProcess">Objeto proceso costo</param>
        /// <param name="oDAC">Objeto manejador de base de datos</param>
        private void SetRealValues(CostProcess oProcess, OracleDAC oDAC)
        {
            decimal dInve = 0, dEdu = 0, dTotal = 0, dValue = 0;
            string Query = string.Empty;
            StringBuilder sQuery = new StringBuilder("SELECT dc_valor FROM detalleprocesocosto WHERE dc_pc_id = :process AND dc_centro IN ('INVE', 'EDDO') ORDER BY dc_centro DESC");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":process", oProcess.iid));
            lParameters.Add(new OracleParameter(":process1", oProcess.iid));
            lParameters.Add(new OracleParameter(":process2", oProcess.iid));
            using (DataTable dt = oDAC.GetDataTable(sQuery.ToString(), lParameters, false, true))
            {
                if (dt != null)
                {
                    if (dt.Rows.Count == 1)
                    {
                        dInve = Convert.ToDecimal(dt.Rows[0]["dc_valor"]);
                    }
                    else if (dt.Rows.Count == 2)
                    {
                        dInve = Convert.ToDecimal(dt.Rows[0]["dc_valor"]);
                        dEdu = Convert.ToDecimal(dt.Rows[1]["dc_valor"]);
                    }
                }
            }
            if (dInve > 0 || dEdu > 0)
            {
                sQuery = new StringBuilder("SELECT dc_valor, dc_centro, co_tipo, dc_liquidapor, dc_total");
                sQuery.Append(", (SELECT SUM(d1.dc_total) FROM detalleprocesocosto d1, centrocosto c WHERE d1.dc_pc_id = :process1 AND c.co_tipo = 'I' AND c.co_centro = d1.dc_centro) dc_inve");
                sQuery.Append(", (SELECT SUM(d1.dc_total) FROM detalleprocesocosto d1, centrocosto c WHERE d1.dc_pc_id = :process2 AND c.co_tipo = 'E' AND c.co_centro = d1.dc_centro) dc_edu");
                sQuery.Append(" FROM detalleprocesocosto, centrocosto WHERE dc_centro = co_centro AND co_tipo IN ('I', 'E') AND dc_pc_id = :process");
                using (DataTable dt = oDAC.GetDataTable(sQuery.ToString(), lParameters, false, true))
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        lParameters.RemoveRange(0, lParameters.Count);
                        if (dEdu != 0 && dr["co_tipo"].ToString() == "E" && Convert.ToInt32(dr["dc_liquidapor"]) == 1)
                        {
                            dTotal = (Convert.ToDecimal(dr["dc_edu"]) > 0) ? (Convert.ToDecimal(dr["dc_total"]) / Convert.ToDecimal(dr["dc_edu"])) * dEdu : Convert.ToDecimal(dr["dc_total"]);
                            dValue = dTotal;                            
                        }
                        else if (dInve != 0 && dr["co_tipo"].ToString() == "I" && Convert.ToInt32(dr["dc_liquidapor"]) == 1)
                        {
                            dTotal = (Convert.ToDecimal(dr["dc_inve"]) > 0) ? (Convert.ToDecimal(dr["dc_total"]) / Convert.ToDecimal(dr["dc_inve"])) * dInve : Convert.ToDecimal(dr["dc_total"]);
                            dValue = dTotal;
                        }
                        else if (dEdu != 0 && dr["co_tipo"].ToString() == "E" && Convert.ToInt32(dr["dc_liquidapor"]) == 2)
                        {
                            dTotal = (Convert.ToDecimal(dr["dc_edu"]) > 0) ? (Convert.ToDecimal(dr["dc_total"]) / Convert.ToDecimal(dr["dc_edu"])) * dEdu : Convert.ToDecimal(dr["dc_total"]);
                            dValue = oProcess.ihours * (dTotal / 100);                            
                        }
                        else if (dInve != 0 && dr["co_tipo"].ToString() == "I" && Convert.ToInt32(dr["dc_liquidapor"]) == 2)
                        {
                            dTotal = (Convert.ToDecimal(dr["dc_inve"]) > 0) ? (Convert.ToDecimal(dr["dc_total"]) / Convert.ToDecimal(dr["dc_inve"])) * dInve : Convert.ToDecimal(dr["dc_total"]);
                            dValue = oProcess.ihours * (dTotal / 100);
                        }
                        if (dValue != 0)
                        {
                            Query = "UPDATE detalleprocesocosto SET dc_valor = :value, dc_total = :total WHERE dc_centro = :costcenter AND dc_pc_id = :process";
                            lParameters.Add(new OracleParameter(":value", dValue));
                            lParameters.Add(new OracleParameter(":total", dTotal));
                            lParameters.Add(new OracleParameter(":costcenter", dr["dc_centro"].ToString()));
                            lParameters.Add(new OracleParameter(":process", oProcess.iid));
                            oDAC.ExecuteNonQuery(Query, lParameters, false, true);
                        }
                        dTotal = 0;
                    }
                }
            }            
        }

        /// <summary>
        /// Método que elimina los centros de costo
        /// </summary>
        /// <param name="iProcess"></param>
        /// <param name="oDAC"></param>
        private void DeleteList(CostProcess oProcess, OracleDAC oDAC)
        {
            StringBuilder sQuery = new StringBuilder("DELETE FROM detalleprocesocosto WHERE dc_pc_id = :process");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":process", oProcess.iid));
            if (!oProcess.bisrooter)
            {
                sQuery.Append(" AND dc_us_id = :user");
                lParameters.Add(new OracleParameter(":user", oProcess.iuser));
            }
            oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false, true);
            lParameters = null;
            sQuery = null;
        }

        /// <summary>
        /// Método para eliminar el centro de costo especial agregado por el coordinador asistencial
        /// </summary>
        /// <param name="oProcess"></param>
        /// <param name="oDAC"></param>
        /// <param name="sType"></param>
        private void DeleteSpecial(CostProcess oProcess, OracleDAC oDAC, string sType)
        {
            StringBuilder sQuery = new StringBuilder("DELETE FROM detalleprocesocosto WHERE dc_pc_id = :process AND dc_centro = :costcenter");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":process", oProcess.iid));
            lParameters.Add(new OracleParameter(":costcenter", sType));
            oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false, true);
            lParameters = null;
            sQuery = null;
        }

        private DataTable GetEmployeeCostData(CostProcess oProcess)
        {
            using (OracleDAC oDAC = new OracleDAC())
            {
                StringBuilder sQuery = new StringBuilder("SELECT dc.* FROM detalleprocesocosto dc, procesocosto WHERE pc_id = dc_pc_id");
                sQuery.Append(" AND pc_ano = :ano AND pc_mes = :mes");
                List<OracleParameter> lParameters = new List<OracleParameter>();
                lParameters.Add(new OracleParameter(":ano", oProcess.iyear));
                lParameters.Add(new OracleParameter(":mes", oProcess.imonth));
                if (!string.IsNullOrEmpty(oProcess.sdocument))
                {
                    sQuery.Append(" AND pc_documento = :document");
                    lParameters.Add(new OracleParameter(":document", oProcess.sdocument));
                }
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        private DataTable GeEmployeesData(Employee oEmploy)
        {
            using (OLEDBDAC oDAC = new OLEDBDAC(this.sConnection))
            {
                StringBuilder sQuery = new StringBuilder("SELECT EMPLEA, NOM, APELLIDO1, APELLIDO2, EMPLEA.CCOSTO, PORCENTA, (HOR_CON * 4) AS H_MES, DISCCOS.CCOSTO AS CENTRO FROM EMPLEA LEFT JOIN DISCCOS ON DISCCOS = EMPLEA");
                sQuery.Append(" WHERE EST IN (\"A\", \"V\") AND NOT EMPTY(DISCCOS.CCOSTO)");
                List<OleDbParameter> lParameters = new List<OleDbParameter>();
                if (!string.IsNullOrEmpty(oEmploy.smaincostcenter))
                {
                    sQuery.Append(" AND EMPLEA.CCOSTO IN ('" + oEmploy.smaincostcenter + "')");
                    //lParameters.Add(new OleDbParameter("?", oEmploy.smaincostcenter));
                }
                if (!string.IsNullOrEmpty(oEmploy.sdocument))
                {
                    sQuery.Append(" AND EMPLEA.EMPLEA LIKE '%' + ? + '%' ");
                    lParameters.Add(new OleDbParameter("?", oEmploy.sdocument));
                }
                if (!string.IsNullOrEmpty(oEmploy.sname))
                {
                    sQuery.Append(" AND EMPLEA.NOM LIKE '%' + ? + '%' ");
                    lParameters.Add(new OleDbParameter("?", oEmploy.sname.ToUpper()));
                }
                if (!string.IsNullOrEmpty(oEmploy.slastname))
                {
                    sQuery.Append(" AND EMPLEA.APELLIDO1 LIKE '%' + ? + '%' ");
                    lParameters.Add(new OleDbParameter("?", oEmploy.slastname.ToUpper()));
                }
                sQuery.Append(" ORDER BY APELLIDO1");
                return oDAC.GetDataTable(sQuery.ToString(), lParameters, false);
            }
        }

        private DataTable GetProcessData(CostProcess oProcess)
        {
            using (OracleDAC oDAC = new OracleDAC())
            {
                StringBuilder sQuery = new StringBuilder("SELECT * FROM detalleprocesocosto dc, procesocosto, centrocosto WHERE pc_id = dc_pc_id");
                sQuery.Append(" AND dc_centro = co_centro AND pc_ano = :ano AND pc_mes = :mes");                
                List<OracleParameter> lParameters = new List<OracleParameter>();
                lParameters.Add(new OracleParameter(":ano", oProcess.iyear));
                lParameters.Add(new OracleParameter(":mes", oProcess.imonth));
                if (!string.IsNullOrEmpty(oProcess.sdocument))
                {
                    sQuery.Append(" AND pc_documento = :document");
                    lParameters.Add(new OracleParameter(":document", oProcess.sdocument));
                }
                if (!string.IsNullOrEmpty(oProcess.scostcenter))
                {
                    sQuery.Append(" AND pc_centro = :center");
                    lParameters.Add(new OracleParameter(":center", oProcess.scostcenter));
                }
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        private DataTable GetCostData()
        {
            using (OracleDAC oDAC = new OracleDAC())
            {
                string sQuery = "SELECT ccocod, cconom FROM VCostos ORDER BY cconom";
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery, null);
            }
        }

        private DataTable GetCostData(Cost oCost)
        {
            using (OracleDAC oDAC = new OracleDAC())
            {
                StringBuilder sQuery = new StringBuilder("SELECT co_centro, co_nombre, co_id, co_tipo, co_inactivo FROM centrocosto WHERE 1 = 1");
                List<OracleParameter> lparameters = new List<OracleParameter>();
                if (!string.IsNullOrEmpty(oCost.scode))
                {
                    sQuery.Append(" AND co_centro LIKE '%' || :scode  || '%'");
                    lparameters.Add(new OracleParameter(":scode", oCost.scode));
                }
                if (!string.IsNullOrEmpty(oCost.sname))
                {
                    sQuery.Append(" AND co_nombre LIKE '%' || :sname || '%'");
                    lparameters.Add(new OracleParameter(":sname", oCost.sname));
                }
                if (oCost.ctype != '\0')
                {
                    sQuery.Append(" AND co_tipo = :ctype");
                    lparameters.Add(new OracleParameter(":ctype", oCost.ctype));
                }
                sQuery.Append(" ORDER BY co_nombre");
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lparameters, false);
            }
        }

        /// <summary>
        /// Método que obtiene un datatable con la información de costos asignados para generación de reporte
        /// </summary>
        /// <param name="oReport">Objeto reporte</param>
        /// <returns>DataTable con información de los costos asignados</returns>
        private DataTable GetCostReportData(CostReport oReport)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT dc_centro, dc_valor, dc_fechaproceso, pc_documento, pc_mes, pc_ano, pc_cerrado, pc_liquidapor");
            sQuery.Append(", pc_nombre, pc_apellido, pc_centro, pc_completado, pc_cerrado, pc_horas, us_usuario FROM detalleprocesocosto, procesocosto, usuario WHERE pc_id = dc_pc_id");
            sQuery.Append(" AND pc_us_id = us_id");
            using (OracleDAC oDAC = new OracleDAC())
            {
                if (oReport.imonth != 0)
                {
                    sQuery.Append(" AND pc_mes = :month");
                    lParameters.Add(new OracleParameter(":month", oReport.imonth));
                }
                if (oReport.iyear != 0)
                {
                    sQuery.Append(" AND pc_ano = :year");
                    lParameters.Add(new OracleParameter(":year", oReport.iyear));
                }
                if (!string.IsNullOrEmpty(oReport.smaincostcenter))
                {
                    sQuery.Append(" AND pc_centro = :costcenter");
                    lParameters.Add(new OracleParameter(":costcenter", oReport.smaincostcenter));
                }
                if (!string.IsNullOrEmpty(oReport.sdocument))
                {
                    sQuery.Append(" AND pc_documento = :document");
                    lParameters.Add(new OracleParameter(":document", oReport.sdocument));
                }
                if (!string.IsNullOrEmpty(oReport.sstatus))
                {
                    if (oReport.sstatus == "1")
                    {
                        sQuery.Append(" AND pc_completado = 100");
                    }
                    else
                    {
                        sQuery.Append(" AND pc_completado <> 100");
                    }
                }
                if (!string.IsNullOrEmpty(oReport.sfirstname))
                {
                    sQuery.Append(" AND (pc_nombre LIKE '%' || :name || '%' OR pc_apellido LIKE '%' + :name + '%')");
                    lParameters.Add(new OracleParameter(":name", oReport.sfirstname));
                }
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }

        /// <summary>
        /// Método para insertar un centro de costos en la base de datos
        /// </summary>
        /// <param name="oEntity">Objeto Centro de Costo</param>
        public void Insert(Cost oEntity)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO centrocosto (co_centro, co_nombre, co_tipo, co_inactivo)");
            sQuery.Append(" VALUES (:code, :name, :type, :status)");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            OracleDAC oDAC = null;
            try
            {
                lParameters.Add(new OracleParameter(":code", oEntity.scode));
                lParameters.Add(new OracleParameter(":name", oEntity.sname));
                lParameters.Add(new OracleParameter(":type", oEntity.ctype));
                lParameters.Add(new OracleParameter(":status", 1));
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al insertar centro de costos");
            }
            finally
            {
                lParameters = null;
                sQuery = null;
                oDAC.Dispose();
                oDAC = null;
            }
        }

        /// <summary>
        /// Método para editar la información de un centro de costos en la base de datos
        /// </summary>
        /// <param name="oEntity">Objeto Centro de Costo</param>
        public void Edit(Cost oEntity)
        {
            StringBuilder sQuery = new StringBuilder("UPDATE centrocosto SET co_centro = :code, co_nombre = :name, co_tipo = :type");
            sQuery.Append(" WHERE co_id = :id");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            OracleDAC oDAC = null;
            try
            {
                lParameters.Add(new OracleParameter(":code", oEntity.scode));
                lParameters.Add(new OracleParameter(":name", oEntity.sname));
                lParameters.Add(new OracleParameter(":type", oEntity.ctype));
                lParameters.Add(new OracleParameter(":id", oEntity.id));
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al editar el centro de costo");
            }
            finally
            {
                lParameters = null;
                sQuery = null;
                oDAC.Dispose();
                oDAC = null;
            }
        }

        /// <summary>
        /// Método para inactivar un centro de costo en la base de datos
        /// </summary>
        /// <param name="iCost">Id del centro de costo</param>
        public void Delete(int iCost)
        {
            StringBuilder sQuery = new StringBuilder("UPDATE centrocosto SET co_inactivo = 1");
            sQuery.Append(" WHERE co_id = :id");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            OracleDAC oDAC = null;
            try
            {
                lParameters.Add(new OracleParameter(":id", iCost));
                oDAC = new OracleDAC();
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al inactivar el centro de costo");
            }
            finally
            {
                lParameters = null;
                sQuery = null;
                oDAC.Dispose();
                oDAC = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
