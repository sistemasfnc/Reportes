using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace DAC
{
    public class CitasDAC : IDisposable
    {
        public string sqlConnection { get; set; }

        public DataTable GetData(DateTime dTInitial, DateTime dTFinal)
        {
            List<SqlParameter> lParameters = new List<SqlParameter>();
            lParameters.Add(new SqlParameter("@initial", dTInitial.ToString("yyyyMMdd")));
            lParameters.Add(new SqlParameter("@final", dTFinal.ToString("yyyyMMdd")));
            StringBuilder sQuery = new StringBuilder("SELECT * FROM [VRecordatorioCitas] WHERE CAST((SUBSTRING(FECHA, 8, 4) + SUBSTRING(FECHA, 4, 3) + '-' + SUBSTRING(FECHA, 2, 2)) AS DATE) BETWEEN @initial AND @final");
            using (SQLDAC oDAC = new DAC.SQLDAC(this.sqlConnection))
            {
                return oDAC.GetDataTable(sQuery.ToString(), lParameters);
            }
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
