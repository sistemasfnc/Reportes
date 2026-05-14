using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using System.Data;
using System.Data.OleDb;
using Utils;
using EventLog;

namespace DAC
{
    public class SMSDAC : IDisposable
    {        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Sms> GetScheduleList()
        {
            List<Sms> lSms = new List<Sms>();
            DataTable dt = new DataTable();
            Sms oEntity = new Sms();
            try
            {
                dt = this.GetData();
                if (dt.Rows.Count > 0) dt = dt.AsEnumerable().GroupBy(x => new { Dapuntefch = x["Dapuntefch"].ToString(), Cpacprinom = x["Cpacprinom"].ToString(), ncelular = x["ncelular"].ToString() }).Select(y => y.First()).CopyToDataTable();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["ncelular"].ToString().Trim() != string.Empty && dt.Rows[i]["Cpacprinom"].ToString() != string.Empty)
                    {
                        oEntity = new Sms()
                        {
                            cellphone = Tools.FormatPhone(dt.Rows[i]["ncelular"].ToString().Trim()),
                            name = dt.Rows[i]["Cpacprinom"].ToString().Trim(),
                            schedule = Tools.FormatDate(dt.Rows[i]["Dapuntefch"].ToString(), dt.Rows[i]["Choraini"].ToString())
                        };
                        lSms.Add(oEntity);
                    }
                }
                return lSms;
            }
            catch (ApplicationException)
            {
                throw new ApplicationException("Error al obtener el listado de pacientes de la agenda");
            }
            catch(Exception ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de pacientes de la agenda");
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DataTable GetData()
        {
            DateTime iDate = DateTime.Now.AddDays(1);
            StringBuilder sQuery = new StringBuilder("SELECT Cpacprinom, ncelular, Dapuntefch, Choraini FROM Hicpaciente");
            sQuery.Append(" INNER JOIN Agenda ON Hicpaciente.Cregidpac = Agenda.Cregidpac");
            sQuery.Append(" INNER JOIN Agenelem ON Agenelem.Celecod = Agenda.Celecod");
            //sQuery.Append(" WHERE Dapuntefch = ?");
            //sQuery.Append(" WHERE Agenda.Ctipelecod IN ('01', '12') AND Agenda.Celecod <> 'MED LKLK' AND NOT EMPTY(Cpacprinom) AND ncelular <> 0 AND (Cespecialidad = 'A' OR EMPTY(Cespecialidad)) AND Dapuntefch = ");
            //sQuery.Append(" WHERE Agenda.Celecod <> 'MED LKLK' AND NOT EMPTY(Cpacprinom) AND ncelular <> 0 AND Dapuntefch = ");
            sQuery.Append(" WHERE EMPTY(Agenda.Celecodrel) AND ncelular <> 0 AND Lenviarmsm = .T. AND mapuntedes NOT LIKE '(JUNTA MEDICA)%' AND Dapuntefch = ");
            sQuery.Append(Tools.FormatFoxDate(iDate));
            //sQuery.Append(" AND Agenelem.ccatcod IN ('COAD', 'PRFP')");
            sQuery.Append(" GROUP BY Cpacprinom, ncelular, Dapuntefch, Choraini");
            List<OleDbParameter> lParameters = new List<OleDbParameter>();
            //lParameters.Add(new OleDbParameter("?", Tools.FormatFoxDate(iDate)));
            using (OLEDBDAC oDAC = new OLEDBDAC())
            {
                return oDAC.GetDataTable(sQuery.ToString(), lParameters, false);                
            }
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
