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
    public class GenericDAC : IDisposable
    {        
        private string ConnectionString { get; set; }
        
        public GenericDAC()
        {

        }

        public GenericDAC(string sConnection)
        {
            this.ConnectionString = sConnection;
        }        

        public List<Generic> GetPlans(bool isProgram = false)
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                dt = this.GetPlansData(isProgram);
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        code = dr["Cod"].ToString(),
                        name = dr["Descrip"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de planes");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetServices()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                dt = this.GetServicesData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        code = dr["Cod"].ToString(),
                        name = dr["Descrip"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de servicios");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetUsers()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                dt = this.GetUsersData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        code = dr["usuactusu"].ToString().ToLower(),
                        name = dr["usuactusu"].ToString().ToLower(),
                    };
                    lGeneric.Add(oEntity);
                }
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de usuarios");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetStatus(int iStatus)
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                dt = this.GetstatusData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        code = dr["es_id"].ToString(),                        
                    };
                    if (iStatus == 2) oEntity.name = dr["es_nombrefacturacion"].ToString();
                    else if (iStatus == 1) oEntity.name = dr["es_nombrecajero"].ToString();
                    else oEntity.name = dr["es_nombre"].ToString();
                    lGeneric.Add(oEntity);
                }
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de estados");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetSupports()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                dt = this.GetSupportsData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        id = Convert.ToInt32(dr["so_id"]),
                        name = dr["so_nombre"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }                
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de soportes");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetReasons()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                dt = this.GetReasonsData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        id = Convert.ToInt32(dr["mo_id"]),
                        name = dr["mo_nombre"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de motivos");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetPendings()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                dt = this.GetPendingData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        id = Convert.ToInt32(dr["pn_id"]),
                        name = dr["pn_nombre"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de pendintes de facturacion");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetCompanies()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                dt = this.GetCompaniesData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        code = dr["Cod"].ToString(),
                        name = dr["Descrip"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de compañias");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetAgreements()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                dt = this.GetAgreementsData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        code = dr["Cod"].ToString(),
                        name = dr["Descrip"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de convenios");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetResponsiveCategories()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                dt = this.GetResponsiveCategoriesData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        code = dr["crg_id"].ToString(),
                        name = dr["crg_nombre"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de categorias del responsable de la glosa");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetResposiveServices(string sCategory)
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                dt = this.GetResposiveServicesData(sCategory);
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        code = dr["srg_id"].ToString(),
                        name = dr["srg_nombre"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de categorias del responsable de la glosa");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }


        private DataTable GetServicesData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT Cod, Descrip FROM Vservicios ORDER BY Cod");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        private DataTable GetPlansData(bool bisProgram = false)
        {
            StringBuilder sQuery = new StringBuilder();
            if (!bisProgram)
            {
                sQuery.Append("SELECT Cod, Descrip FROM Vplanes ORDER BY Cod");
            }
            else
            {
                sQuery.Append("SELECT PLACOD Cod, PLANOM Descrip FROM VPLANPROGRAMAS ORDER BY PLANOM");
            }
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }



        private DataTable GetUsersData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT DISTINCT usuactusu FROM VUsuario");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        private DataTable GetstatusData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT es_id, es_nombre, es_nombrecajero, es_nombrefacturacion FROM estadocargo");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        private DataTable GetSupportsData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT so_id, so_nombre FROM soporte");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        private DataTable GetReasonsData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT mo_id, mo_nombre FROM motivodevolucion");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        private DataTable GetCompaniesData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT Cod, Descrip FROM VEmpresas");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        private DataTable GetAgreementsData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT Cod, Descrip FROM VCONVENIOS");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        private DataTable GetPendingData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT pn_id, pn_nombre FROM pendiente");
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        private DataTable GetResponsiveCategoriesData()
        {
            string squery = "SELECT * FROM categoriaresponsableglosa";
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(squery, null);
            }
        }

        private DataTable GetResposiveServicesData(string iCategory)
        {
            StringBuilder sQuery = new StringBuilder("SELECT * FROM servicioresponsableglosa WHERE 1 = 1");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            if (!string.IsNullOrEmpty(iCategory))
            {
                lParameters.Add(new OracleParameter(":idcategory", iCategory));
                sQuery.Append(" AND srg_crg_id = :idcategory");                
            }
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        public List<Generic> GetRates()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                lGeneric.Add(new Generic() { name = string.Empty, code = string.Empty });
                dt = this.GetRatesData();
                foreach (DataRow dr in dt.Rows)
                {
                    if(Utils.Tools.IsNumeric(dr["empplapla"].ToString()))
                    {
                        oEntity = new Generic()
                        {
                            code = dr["empplaemp"].ToString(),
                            name = dr["empplatar"].ToString(),
                            id = Convert.ToInt32(dr["empplapla"]),
                        };
                        lGeneric.Add(oEntity);
                    }                    
                }
                return lGeneric;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de tarifas por empresa por plan");
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        private DataTable GetRatesData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT empplaemp, empplapla, empplatar FROM VEmpresaPlanTarifa");            
            using (SQLDAC oDAC = new SQLDAC(this.ConnectionString))
            {
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
