using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using System.Data;
using System.Data.OleDb;

namespace DAC
{
    public class PublcidadDAC : IDisposable
    {        

        public List<Generic> GetList()
        {
            DataTable dt = new DataTable();
            List<Generic> lGeneric = new List<Generic>();
            Generic oEntity = null;
            try
            {
                dt = this.GetData();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Generic()
                    {
                        code = dt.Rows[i]["Cpacemail"].ToString(),
                        name = dt.Rows[i]["cpacprinom"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                return lGeneric;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }

        private DataTable GetData()
        {
            using (OLEDBDAC oDAC = new OLEDBDAC())
            {
                return oDAC.GetDataTable("SELECT Cpacemail, cpacprinom FROM hicpaciente WHERE NOT EMPTY(Cpacemail)", null, false);
            }
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
