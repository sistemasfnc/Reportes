using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using Utils;
using EventLog;
using System.Data;
using System.Data.OleDb;

namespace DAC
{
    public class NominaDAC : IDisposable
    {
        private string sConnection { get; set; }
        
        public NominaDAC(string sConnection)
        {
            this.sConnection = sConnection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<Nomina> GetList(Nomina oEntity)
        {
            List<Nomina> lNomina = new List<Nomina>();
            DataTable dt = new DataTable();
            try
            {
                dt = this.GetData(oEntity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new Nomina()
                    {
                        document = dt.Rows[i]["document"].ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        inccode = dt.Rows[i]["inccode"].ToString(),
                        incdays = Convert.ToInt32(dt.Rows[i]["incdays"]),
                        incvalue = Convert.ToInt32(dt.Rows[i]["incvalue"]),
                        costcenter = dt.Rows[i]["costcenter"].ToString(),
                        incnum = dt.Rows[i]["incnum"].ToString(),
                        eps = dt.Rows[i]["eps"].ToString(),
                        date = Convert.ToDateTime(dt.Rows[i]["datehis"]),
                        incdate = Convert.ToDateTime(dt.Rows[i]["incdate"]),
                        incfdate = Convert.ToDateTime(dt.Rows[i]["incdate"]).AddDays(Convert.ToInt32(dt.Rows[i]["incdays"]) - 1),
                        inccodename = Tools.GetInability(dt.Rows[i]["inccode"].ToString()),
                        //status = Convert.ToInt32(dt.Rows[i]["status"]),
                        //statusdate = (dt.Rows[i]["statusdate"] != DBNull.Value) ? Convert.ToDateTime(dt.Rows[i]["statusdate"]) : new DateTime(1910, 1, 1),
                    };
                    lNomina.Add(oEntity);
                }
                return lNomina;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Nomina", "DAC", ex);
                throw new ApplicationException("Ha ocurrido un error al obtener los datos de nomina");
            }
            catch (NullReferenceException ex)
            {
                LogError.WriteError("Nomina", "DAC", ex);
                throw new ApplicationException("Ha ocurrido un error al obtener los datos de nomina");
            }
            catch (Exception)
            {
                throw new ApplicationException("Ha ocurrido un error al obtener los datos de nomina");
            }       
            finally
            {
                dt.Dispose();
                dt = null;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<NominaStatus> GetStatusList(NominaStatus oEntity)
        {
            List<NominaStatus> lNomina = new List<NominaStatus>();
            DataTable dt = new DataTable();
            try
            {
                dt = this.GetStatusData(oEntity);                
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new NominaStatus()
                    {
                        status = Convert.ToInt32(dr["status"]),
                        statusdate = Convert.ToDateTime(dr["statusdate"]),
                        document = dr["document"].ToString(),
                        inccode = dr["inccode"].ToString(),
                        incdays = Convert.ToInt32(dr["incdays"]),
                        incnum = dr["incnum"].ToString(),
                        incdate = Convert.ToDateTime(dr["incdate"]),
                        incfdate = Convert.ToDateTime(dr["incdate"]).AddDays(Convert.ToInt32(dr["incdays"]) - 1),
                        observations = dr["observ"].ToString(),
                        value = (dr["value"] != DBNull.Value) ? Convert.ToInt32(dr["value"]) :  0,
                        diagnosis = dr["diagnosis"].ToString(),
                    };
                    lNomina.Add(oEntity);
                }
                return lNomina;
            }
            catch (Exception)
            {                
                throw new ApplicationException("Error al obtener el listado de estados de incapacidades");                
            }
            finally
            {
                dt.Dispose();
                dt = null;
            }
        }

        public List<Generic> GetEnsurance()
        {
            List<Generic> lGeneric = new List<Generic>();
            DataTable dt = new DataTable();
            Generic oEntity = null;
            try
            {
                dt = this.GetEnsuranceData();
                foreach (DataRow dr in dt.Rows)
                {
                    oEntity = new Generic()
                    {
                        name = dr["NOMBRE"].ToString(),
                        code = dr["TAB_EMP"].ToString(),
                    };
                    lGeneric.Add(oEntity);
                }
                return lGeneric;
            }
            catch (Exception)
            {
                throw new ApplicationException("Error al obtener el listado de EPS");                
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
        /// <param name="oEntity"></param>
        public void InsertStatus(NominaStatus oEntity)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO INCESTADO (DOCUMENT, INCDATE, INCNUM, INCCODE, INCDAYS, STATUS, STATUSDATE, OBSERV, DIAGNOSIS, VALUE) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
            OLEDBDAC oDAC = new OLEDBDAC(this.sConnection);
            List<OleDbParameter> lParameters = new List<OleDbParameter>();
            lParameters.Add(new OleDbParameter("?", oEntity.document));
            lParameters.Add(new OleDbParameter("?", oEntity.incdate));
            lParameters.Add(new OleDbParameter("?", oEntity.incnum));
            lParameters.Add(new OleDbParameter("?", oEntity.inccode));
            lParameters.Add(new OleDbParameter("?", oEntity.incdays));
            lParameters.Add(new OleDbParameter("?", oEntity.status));
            lParameters.Add(new OleDbParameter("?", oEntity.statusdate));
            lParameters.Add(new OleDbParameter("?", oEntity.observations));
            lParameters.Add(new OleDbParameter("?", oEntity.diagnosis));
            lParameters.Add(new OleDbParameter("?", oEntity.value));
            try
            {
                oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, false);
            }
            catch (Exception)
            {
                throw new ApplicationException("Error al almacenar estado de la incapacidad");
            }
            finally
            {
                oDAC.Dispose();
                oDAC = null;
                lParameters = null;
            }
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        private DataTable GetData(Nomina oEntity)
        {
            List<OleDbParameter> lParameters = new List<OleDbParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT DISTINCT EMPLEA.EMPLEA AS document, EMPLEA.NOMBRES AS name, TAB_EMP.NOMBRE AS eps, LIQHIS.CODCONHIS AS inccode");
            sQuery.Append(", LIQHIS.CANCONHIS AS incdays, LIQHIS.VALCONHIS AS incvalue, LIQHIS.FECHAHIS AS datehis, EMPLEA.CCOSTO AS costcenter");
            sQuery.Append(", HIS_INC.FEC_INC AS incdate, HIS_INC.NUM_INC AS incnum");
            //sQuery.Append(", (SELECT STATUS FROM INCESTADO WHERE DOCUMENT = EMPLEA AND INCDATE = FEC_INC AND INCNUM = HIS_INC.NUM_INC AND INCDAYS = CANCONHIS AND INCCODE = CODCONHIS GROUP BY STATUS, STATUSDATE HAVING STATUSDATE = MAX(STATUSDATE)) AS status");
            //sQuery.Append(", (SELECT MAX(STATUSDATE) FROM INCESTADO WHERE DOCUMENT = EMPLEA AND INCDATE = FEC_INC AND INCNUM = HIS_INC.NUM_INC AND INCDAYS = CANCONHIS AND INCCODE = CODCONHIS GROUP BY STATUSDATE) AS statusdate");
            sQuery.Append("  FROM EMPLEA, TAB_EMP, LIQHIS, HIS_INC WHERE");
            sQuery.Append(" TAB_EMP.TAB_EMP = EMPLEA.EPS AND LIQHIS.LIQHIS = EMPLEA.EMPLEA AND LIQHIS.LIQHIS = HIS_INC.HIS_INC AND");
            sQuery.Append(" HIS_INC.DIA_INC = LIQHIS.CANCONHIS AND HIS_INC.FEC_PER = LIQHIS.FECHAHIS AND HIS_INC.COD_INC = CODCONHIS AND");
            sQuery.Append(" LIQHIS.CODCONHIS IN ('1112', '1113') AND LIQHIS.FECHAHIS >= {^2017-12-01}");
            if (!string.IsNullOrEmpty(oEntity.document))
            {
                sQuery.Append(" AND EMPLEA.EMPLEA = ?");
                lParameters.Add(new OleDbParameter("?", oEntity.document));
            }
            if (!string.IsNullOrEmpty(oEntity.inccode))
            {
                sQuery.Append(" AND LIQHIS.CODCONHIS = ?");
                lParameters.Add(new OleDbParameter("?", oEntity.inccode));
            }
            if (!string.IsNullOrEmpty(oEntity.eps))
            {
                sQuery.Append(" AND TAB_EMP.TAB_EMP = ?");
                lParameters.Add(new OleDbParameter("?", oEntity.eps));
            }
            if (oEntity.initialdate.Year > 1)
            {
                sQuery.Append(" AND LIQHIS.FECHAHIS >= ");
                sQuery.Append(Tools.FormatFoxDate(oEntity.initialdate));
            }
            if (oEntity.finaldate.Year > 1)
            {
                sQuery.Append(" AND LIQHIS.FECHAHIS <= ");
                sQuery.Append(Tools.FormatFoxDate(oEntity.finaldate));
            }
            if (oEntity.initialdate.Year == 1)
            {
                DateTime initialDate = new DateTime(2016, 1, 1);
                sQuery.Append(" AND LIQHIS.FECHAHIS >= ");
                sQuery.Append(Tools.FormatFoxDate(initialDate));
            }
            sQuery.Append(" ORDER BY FECHAHIS DESC");
            using (OLEDBDAC oDAC = new OLEDBDAC(this.sConnection))
            {
                return oDAC.GetDataTable(sQuery.ToString(), lParameters, false);
            }
        }

        private DataTable GetStatusData(NominaStatus oEntity)
        {
            List<OleDbParameter> lParameters = new List<OleDbParameter>();
            StringBuilder sQuery = new StringBuilder("SELECT * FROM INCESTADO WHERE 1 = 1");
            if (!string.IsNullOrEmpty(oEntity.document))
            {
                sQuery.Append(" AND DOCUMENT = ?");
                lParameters.Add(new OleDbParameter("?", oEntity.document));
            }
            if (oEntity.incdate.Year != 1)
            {
                sQuery.Append(" AND INCDATE = ");
                sQuery.Append(Tools.FormatFoxDate(oEntity.incdate));
            }
            if (!string.IsNullOrEmpty(oEntity.incnum))
            {
                sQuery.Append(" AND INCNUM = ? ");
                lParameters.Add(new OleDbParameter("?", oEntity.incnum));
            }
            if (oEntity.incdays != 0)
            {
                sQuery.Append(" AND INCDAYS = ? ");
                lParameters.Add(new OleDbParameter("?", oEntity.incdays));
            }
            if (!string.IsNullOrEmpty(oEntity.inccode))
            {
                sQuery.Append(" AND INCCODE = ? ");
                lParameters.Add(new OleDbParameter("?", oEntity.inccode));
            }
            sQuery.Append(" ORDER BY STATUSDATE DESC");
            using (OLEDBDAC oDAC = new OLEDBDAC(this.sConnection))
            {
                return oDAC.GetDataTable(sQuery.ToString(), lParameters, false);
            }
        }


        private DataTable GetEnsuranceData()
        {
            string sQuery = "SELECT TAB_EMP, NOMBRE FROM TAB_EMP WHERE TIP_EMP = 'S'";
            using (OLEDBDAC oDAC = new OLEDBDAC(this.sConnection))
            {
                return oDAC.GetDataTable(sQuery, null, false);
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
