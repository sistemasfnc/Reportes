using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;

namespace Facade
{
    public class FacadeSanitas : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Sanitas> GetProcedures()
        {
            using (SanitasDAC oDAC = new SanitasDAC())
            {
                return oDAC.GetProcedures();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lSanitas"></param>
        public void GenerateExcelFile(List<Sanitas> lSanitas)
        {
            using (ExcelDAC oDAC = new ExcelDAC())
            {
                oDAC.GenerateFile(lSanitas);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
