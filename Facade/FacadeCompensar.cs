using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using DAC;
using System.Data;

namespace Facade
{
    public class FacadeCompensar : IDisposable
    {
        public string sConnection { get; set; }

        public List<PFP> GetCompensarList()
        {
            using (CompensarDAC oDAC = new CompensarDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetPFPList();
            }
        }

        public DataTable GetSanitasList()
        {
            using (CompensarDAC oDAC = new CompensarDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetSanitasFiles();
            }
        }

        public void BulkData(DataTable dt)
        {
            using (CompensarDAC oDAC = new CompensarDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Insert(dt);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
