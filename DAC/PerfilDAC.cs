using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using System.Data;
using System.Data.SqlClient;

namespace DAC
{
    public class PerfilDAC : IDisposable
    {

        private string sConnection { get; set; }

        public PerfilDAC(string Connection)
        {
            this.sConnection = Connection;
        }
        
        public List<Profile> GetAll()
        {
            List<Profile> lProfile = new List<Profile>();
            DataTable dt = new DataTable();
            Profile oProfile = null;
            try
            {
                dt = this.GetData();
                foreach (DataRow dr in dt.Rows)
                {
                    oProfile = new Profile()
                    {
                        idprofile = Convert.ToInt32(dr["pe_id"]),
                        name = dr["pe_nombre"].ToString(),
                    };
                    lProfile.Add(oProfile);
                }
                return lProfile;
            }
            catch (Exception)
            {
                
                throw;
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oProfile = null;
            }            
        }
        
        private DataTable GetData()
        {
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Connect();
                return oDAC.GetDataTable("SELECT * FROM perfil UNION ALL SELECT 0, '', '' FROM DUAL", null);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
