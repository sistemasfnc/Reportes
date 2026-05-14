using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;

namespace Facade
{
    public class FacadeFactura : IDisposable
    {
        private string sConnection { get; set; }
        
        public FacadeFactura(string Connection)
        {
            this.sConnection = Connection;
        }

        public List<Invoice> GetList(Invoice oEntity)
        {
            using (FacturaDAC oFactura = new FacturaDAC(this.sConnection))
            {
                return oFactura.GetInvoiceList(oEntity);
            }
        }

        public void InsertPending(List<Support> lEntity)
        {
            using (FacturaDAC oFactura = new FacturaDAC(this.sConnection))
            {
                oFactura.InsertPending(lEntity);
            }
        }

        public List<Support> GetPendings(string invoice)
        {
            using (FacturaDAC oFactura = new FacturaDAC(this.sConnection))
            {
                return oFactura.GetPendings(invoice);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
