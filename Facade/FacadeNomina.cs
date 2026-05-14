using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;

namespace Facade
{
    public class FacadeNomina : IDisposable
    {
        private string sConnection { get; set; }
        
        public FacadeNomina(string sConnection)
        {
            this.sConnection = sConnection;
        }
        
        public List<Nomina> GetList(Nomina oEntity)
        {
            using (NominaDAC oDAC = new NominaDAC(this.sConnection))
            {
                return oDAC.GetList(oEntity);
            }
        }

        public List<NominaStatus> GetStatusList(NominaStatus oEntity)
        {
            using (NominaDAC oDAC = new NominaDAC(this.sConnection))
            {
                return oDAC.GetStatusList(oEntity);
            }
        }

        public void InsertStatus(NominaStatus oEntity)
        {
            using (NominaDAC oDAC = new NominaDAC(this.sConnection))
            {
                oDAC.InsertStatus(oEntity);
            }
        }

        public List<Generic> GetEnsurance()
        {
            using (NominaDAC oDAC = new NominaDAC(this.sConnection))
            {
                return oDAC.GetEnsurance();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
