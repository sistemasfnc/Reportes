using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using EventLog;
using Entity;
using Oracle.ManagedDataAccess.Client;

namespace DAC
{
    public class ACLDAC : IDisposable
    {
        private string ConnectionString { get; set; }

        public ACLDAC(string sConnection)
        {
            this.ConnectionString = sConnection;
        }

        public List<Security> GetAll(Security oEntity)
        {
            List<Security> lSecurity = new List<Security>();
            DataTable dt = new DataTable();
            try
            {
                dt = this.GetData(oEntity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Security()
                    {
                        idprofile = Convert.ToInt32(dt.Rows[i]["pe_id"]),
                        idaccess = Convert.ToInt32(dt.Rows[i]["pm_id"]),
                        profilename = dt.Rows[i]["pe_nombre"].ToString(),
                        accessname = dt.Rows[i]["pm_nombre"].ToString(),
                    };
                    lSecurity.Add(oEntity);
                }
                return lSecurity;
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

        private DataTable GetData(Security oEntity)
        {
            List<OracleParameter> lParameters = new List<OracleParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT pe_id, pm_id, pe_nombre, pm_nombre FROM acl INNER JOIN perfil");
            sQuery.Append(" ON acl_pe_id = pe_id INNER JOIN permiso ON pm_id = acl_pm_id WHERE 1 = 1");
            if (oEntity.idprofile != 0)
            {
                sQuery.Append(" AND pe_id = :idprofile");
                lParameters.Add(new OracleParameter(":idprofile", oEntity.idprofile));
            }
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.ConnectionString;
                oDAC.Connect();
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
