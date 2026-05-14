using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using DAC;

namespace Facade
{
    public class FacadeBaul : IDisposable
    {

        public string sconnection { get; set; }

        public FacadeBaul(string sconnectionstring)
        {
            this.sconnection = sconnectionstring;
        }

        public void Insert(Baul baul)
        {
            using (BaulDAC baulDAC = new BaulDAC())
            {
                baulDAC.sconnection = this.sconnection;
                baulDAC.Insert(baul);
            }
        }
        public void Update(Baul baul)
        {
            using (BaulDAC baulDAC = new BaulDAC())
            {
                baulDAC.sconnection = this.sconnection;
                baulDAC.Update(baul);
            }
        }

        public void Delete(int iid)
        {
            using (BaulDAC baulDAC = new BaulDAC())
            {
                baulDAC.sconnection = this.sconnection;
                baulDAC.Delete(iid);
            }
        }

        public List<Baul> GetBauls(Baul baul)
        {
            using (BaulDAC baulDAC = new BaulDAC())
            {
                baulDAC.sconnection = this.sconnection;
                return baulDAC.GetBauls(baul);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
