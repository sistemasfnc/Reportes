using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;

namespace Facade
{
    public class FacadeSMS : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        public void InsertLog(Sms oEntity)
        {
            using (LogDAC oDAC = new LogDAC())
            {
                oDAC.InsertLog(oEntity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Sms> GetSchedule()
        {
            using (SMSDAC oDAC = new SMSDAC())
            {
                return oDAC.GetScheduleList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {            
            GC.SuppressFinalize(this);
        }
    }
}
