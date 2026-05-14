using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using Config;
using Entity;
using EventLog;

namespace DAC
{
    public class ExcelDAC : IDisposable
    {
        public ExcelDAC()
        {
           
        }
        
        public void GenerateFile(List<Sanitas> lSanitas)
        {
            if (!this.ValidateTable()) this.CreateFile();
            this.CreateRows(lSanitas);
        }

        private void CreateRows(List<Sanitas> lSanitas)
        {
            StringBuilder sQuery = new StringBuilder();
            List<OleDbParameter> lParameters = null;
            OLEDBDAC oDAC = new OLEDBDAC(Config.Configuration.GetStringValue("SanitasConnection"));
            foreach (Sanitas oEntity in lSanitas)
            {
                sQuery.Append("INSERT INTO [table1$] (F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15");
                sQuery.Append(", F16, F17, F18, F19, F20, F21, F22, F23, F24, F25, F26, F27, F28, F29, F30)");
                sQuery.Append(" VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
                try
                {
                    lParameters = this.GetParameters(oEntity);
                    oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters, true);
                }
                catch (Exception ex)
                {
                    LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);                    
                }                
                sQuery.Remove(0, sQuery.Length);                
            }
            oDAC.Dispose();
            oDAC = null;
        }

        private List<OleDbParameter> GetParameters(Sanitas oEntity)
        {
            List<OleDbParameter> lParameters = new List<OleDbParameter>();
            lParameters.Add(new OleDbParameter("?", oEntity.counter));
            lParameters.Add(new OleDbParameter("?", oEntity.branch));
            lParameters.Add(new OleDbParameter("?", oEntity.typebranch));
            lParameters.Add(new OleDbParameter("?", oEntity.idbranch));
            lParameters.Add(new OleDbParameter("?", oEntity.insurance));
            lParameters.Add(new OleDbParameter("?", oEntity.patientname));
            lParameters.Add(new OleDbParameter("?", oEntity.patientdoctype));
            lParameters.Add(new OleDbParameter("?", oEntity.patientdocument));
            lParameters.Add(new OleDbParameter("?", oEntity.gender));
            lParameters.Add(new OleDbParameter("?", oEntity.birthday.ToString("dd/MM/yyyy")));
            lParameters.Add(new OleDbParameter("?", oEntity.cellphone));
            lParameters.Add(new OleDbParameter("?", oEntity.phone));
            lParameters.Add(new OleDbParameter("?", oEntity.email));
            lParameters.Add(new OleDbParameter("?", oEntity.servicedate.ToString("dd/MM/yyyy")));
            lParameters.Add(new OleDbParameter("?", oEntity.servicesource));
            lParameters.Add(new OleDbParameter("?", oEntity.servicecode));
            lParameters.Add(new OleDbParameter("?", oEntity.servicename));
            lParameters.Add(new OleDbParameter("?", oEntity.qty));
            lParameters.Add(new OleDbParameter("?", oEntity.priority));
            lParameters.Add(new OleDbParameter("?", oEntity.requestdate.ToString("dd/MM/yyyy")));
            lParameters.Add(new OleDbParameter("?", oEntity.observation));
            lParameters.Add(new OleDbParameter("?", oEntity.justification));
            lParameters.Add(new OleDbParameter("?", oEntity.cie10code));
            lParameters.Add(new OleDbParameter("?", oEntity.cie10name));
            lParameters.Add(new OleDbParameter("?", oEntity.etiology));
            lParameters.Add(new OleDbParameter("?", oEntity.professional));
            lParameters.Add(new OleDbParameter("?", oEntity.speciality));
            lParameters.Add(new OleDbParameter("?", oEntity.profdoctype));
            lParameters.Add(new OleDbParameter("?", oEntity.profdocument));
            lParameters.Add(new OleDbParameter("?", oEntity.profphone));            
            return lParameters;
        }


        private bool ValidateTable()
        {
            using (OLEDBDAC oDAC = new OLEDBDAC(Config.Configuration.GetStringValue("SanitasConnection")))
            {
                return oDAC.TableExists("table1$");                
            }
        }

        private void CreateFile()
        {
            using (OLEDBDAC oDAC = new OLEDBDAC(Config.Configuration.GetStringValue("SanitasConnection")))
            {
                StringBuilder sQuery = new StringBuilder("CREATE TABLE [table1] ([counter] INT, branch VARCHAR, typebranch VARCHAR, idbranch VARCHAR");
                sQuery.Append(", insurance VARCHAR, patientname VARCHAR, patientdoctype VARCHAR, patientdocument VARCHAR, gender VARCHAR, birthday DATE, cellphone VARCHAR, phone VARCHAR");
                sQuery.Append(", email VARCHAR, servicedate DATE, servicesource VARCHAR, servicecode VARCHAR, servicename VARCHAR, qty INT, priority VARCHAR");
                sQuery.Append(", requestdate DATE, observation VARCHAR, justification VARCHAR, cie10code VARCHAR, cie10name VARCHAR, etiology VARCHAR, professional VARCHAR");
                sQuery.Append(", speciality VARCHAR, profdoctype VARCHAR, profdocument VARCHAR, profphone VARCHAR);");
                oDAC.ExecuteNonQuery(sQuery.ToString(), null, false);
                sQuery = null;
            }
        }

        public void Dispose()
        {            
            GC.SuppressFinalize(this);            
        }
    }
}
