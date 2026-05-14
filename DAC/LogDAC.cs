using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Entity;
using EventLog;

namespace DAC
{
    public class LogDAC : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        public void InsertLog(Sms oEntity)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO smslog (cellphone, senddate, message, result, appdate)");
            sQuery.Append(" VALUES (@cellphone, @senddate, @message, @result, @appdate)");
            List<SqlParameter> lParameters = new List<SqlParameter>();
            lParameters.Add(new SqlParameter("@cellphone", oEntity.cellphone));
            lParameters.Add(new SqlParameter("@senddate", oEntity.senddate));
            lParameters.Add(new SqlParameter("@message", oEntity.message));
            lParameters.Add(new SqlParameter("@result", oEntity.result));
            lParameters.Add(new SqlParameter("@appdate", oEntity.schedule.AddMinutes(10)));
            using (SQLDAC oDAC = new SQLDAC())
            {
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);
                sQuery = null;
                lParameters = null;
            }            
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
