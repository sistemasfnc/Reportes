using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using DAC;

namespace Facade
{
    public class FacadeDesmaterializacion : IDisposable
    {
        private string sconnection;
        public FacadeDesmaterializacion(string sconnectionstring)
        {
            this.sconnection = sconnectionstring;
        }

        public List<RelacionEnvio> GetRelationships(RelacionEnvio relacionEnvio)
        {
            using (DesmaterializacionDAC desmaterializacion = new DesmaterializacionDAC())
            {
                desmaterializacion.sconnection = this.sconnection;
                return desmaterializacion.GetRelationships(relacionEnvio);
            }
        }

        public List<Desmaterializacion> GetInvoicesDetail(string srelacion, string ssource)
        {
            using (DesmaterializacionDAC desmaterializacion = new DesmaterializacionDAC())
            {
                desmaterializacion.sconnection = this.sconnection;
                return desmaterializacion.GetInvoicesDetail(srelacion, ssource);
            }
        }

        public void PutUploadFile(string srelationship, string sfile)
        {
            using (DesmaterializacionDAC desmaterializacion = new DesmaterializacionDAC())
            {
                desmaterializacion.sconnection = this.sconnection;
                desmaterializacion.PutUploadFile(srelationship, sfile);
            }
        }

        public List<Generic> GetFilesForUpload(string ssource)
        {
            using (DesmaterializacionDAC desmaterializacion = new DesmaterializacionDAC())
            {
                desmaterializacion.sconnection = this.sconnection;
                return desmaterializacion.GetFilesForUpload(ssource);
            }
        }

        public void UpdateFileStatus(List<Generic> lFiles)
        {
            using (DesmaterializacionDAC desmaterializacion = new DesmaterializacionDAC())
            {
                desmaterializacion.sconnection = this.sconnection;
                desmaterializacion.UpdateFileStatus(lFiles);
            }
        }

        public List<Generic> GetInvoiceSupports(string sepisode)
        {
            using (DesmaterializacionDAC desmaterializacion = new DesmaterializacionDAC())
            {
                desmaterializacion.sconnection = this.sconnection;
                return desmaterializacion.GetSupports(sepisode);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
