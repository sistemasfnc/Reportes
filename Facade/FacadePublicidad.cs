using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;

namespace Facade
{
    public class FacadePublicidad : IDisposable
    {
        public List<Generic> GetList()
        {
            using (PublcidadDAC oDAC = new PublcidadDAC())
            {
                return oDAC.GetList();
            }
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
