using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Entity;
using Config;
using EventLog;
using Oracle.ManagedDataAccess.Client;
using Utils;

namespace DAC
{
    public class BaulDAC : IDisposable
    {

        public string sconnection { get; set; }

        private string skey { get; set; }

        public BaulDAC()
        {
            this.skey = "@2k+*21";
        }

        public List<Baul> GetBauls(Baul baul)
        {
            StringBuilder stringBuilder = new StringBuilder("SELECT * FROM BAUL WHERE BA_ID IS NOT NULL");
            List<OracleParameter> oracleParameters = new List<OracleParameter>();
            DataTable dataTable = new DataTable();
            List<Baul> lbauls = new List<Baul>();
            OracleDAC oracle = new OracleDAC();
            try
            {
                if (!string.IsNullOrEmpty(baul.suser))
                {
                    stringBuilder.Append(" AND BA_USUARIO LIKE '%' || :BA_USUARIO || '%' ");
                    oracleParameters.Add(new OracleParameter("BA_USUARIO", baul.suser));
                }
                if (!string.IsNullOrEmpty(baul.saccess))
                {
                    stringBuilder.Append(" AND BA_ACCESO LIKE '%' || :BA_ACCESO || '%' ");
                    oracleParameters.Add(new OracleParameter("BA_ACCESO", baul.saccess));
                }
                if (!string.IsNullOrEmpty(baul.srol))
                {
                    stringBuilder.Append(" AND BA_ROL LIKE '%' || :BA_ROL || '%' ");
                    oracleParameters.Add(new OracleParameter("BA_ROL", baul.srol));
                }
                oracle.sConnection = this.sconnection;
                oracle.Connect();
                dataTable = oracle.GetDataTable(stringBuilder.ToString(), oracleParameters);
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    baul = new Baul()
                    {
                        iid = Convert.ToInt32(dataRow["BA_ID"]),
                        saccess = dataRow["BA_ACCESO"].ToString(),
                        suser = dataRow["BA_USUARIO"].ToString(),
                        sdetail = dataRow["BA_DETALLE"].ToString(),
                        spassword = StringCipher.Decrypt(dataRow["BA_PASSWORD"].ToString(), this.skey),
                        srol = dataRow["BA_ROL"].ToString(),
                    };
                    lbauls.Add(baul);
                }
                return lbauls;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex); 
                throw new ApplicationException("Error al obtener el listado de entradas");
            }
            finally
            {
                stringBuilder = null;
                oracleParameters = null;
                dataTable.Dispose();
                dataTable = null;
                oracle.Dispose();
                oracle = null;
            }
        }

        public void Insert(Baul baul)
        {
            string squery = "INSERT INTO BAUL (BA_ACCESO, BA_USUARIO, BA_PASSWORD, BA_ROL, BA_DETALLE, BA_CREADOPOR, BA_MODIFICADOPOR) VALUES (:BA_ACCESO, :BA_USUARIO, :BA_PASSWORD, :BA_ROL, :BA_DETALLE, :BA_CREADOPOR, :BA_MODIFICADOPOR)";
            List<OracleParameter> oracleParameters = new List<OracleParameter>();
            using (OracleDAC oDAC = new OracleDAC())
            {
                oracleParameters.Add(new OracleParameter("BA_ACCESO", baul.saccess));
                oracleParameters.Add(new OracleParameter("BA_USUARIO", baul.suser));
                oracleParameters.Add(new OracleParameter("BA_PASSWORD", StringCipher.Encrypt(baul.spassword, this.skey)));
                oracleParameters.Add(new OracleParameter("BA_ROL", baul.srol));
                oracleParameters.Add(new OracleParameter("BA_DETALLE", baul.sdetail));
                oracleParameters.Add(new OracleParameter("BA_CREADOPOR", baul.screatedby));
                oracleParameters.Add(new OracleParameter("BA_MODIFICADOPOR", baul.smodifiedby));
                oDAC.sConnection = this.sconnection;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(squery, oracleParameters);
            }
            oracleParameters = null;
        }

        public void Update(Baul baul)
        {
            StringBuilder stringBuilder = new StringBuilder("UPDATE BAUL SET BA_FECHAACTUALIZACION = SYSDATE, BA_MODIFICADOPOR = :BA_MODIFICADOPOR");
            List<OracleParameter> oracleParameters = new List<OracleParameter>();
            oracleParameters.Add(new OracleParameter("BA_MODIFICADOPOR", baul.smodifiedby));
            if (!string.IsNullOrEmpty(baul.suser))
            {
                stringBuilder.Append(", BA_USUARIO = :BA_USUARIO");
                oracleParameters.Add(new OracleParameter("BA_USUARIO", baul.suser));
            }
            if (!string.IsNullOrEmpty(baul.saccess))
            {
                stringBuilder.Append(", BA_ACCESO = :BA_ACCESO");
                oracleParameters.Add(new OracleParameter("BA_ACCESO", baul.saccess));
            }
            if (!string.IsNullOrEmpty(baul.srol))
            {
                stringBuilder.Append(", BA_ROL = :BA_ROL");
                oracleParameters.Add(new OracleParameter("BA_ROL", baul.srol));
            }
            if (!string.IsNullOrEmpty(baul.sdetail))
            {
                stringBuilder.Append(", BA_DETALLE = :BA_DETALLE");
                oracleParameters.Add(new OracleParameter("BA_DETALLE", baul.sdetail));
            }
            stringBuilder.Append(" WHERE BA_ID = :BA_ID");
            oracleParameters.Add(new OracleParameter("BA_ID", baul.iid));
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.sconnection;
                oDAC.Connect();
                oDAC.ExecuteNonQuery(stringBuilder.ToString(), oracleParameters);
            }
            oracleParameters = null;
            stringBuilder = null;
        }

        public void Delete(int iid)
        {
            string stringBuilder = "DELETE FROM BAUL WHERE BA_ID = :BA_ID";
            List<OracleParameter> oracleParameters = new List<OracleParameter>();
            using (OracleDAC oDAC = new OracleDAC())
            {
                oDAC.sConnection = this.sconnection;
                oDAC.Connect();
                oracleParameters.Add(new OracleParameter("BA_ID", iid));
                oDAC.ExecuteNonQuery(stringBuilder.ToString(), oracleParameters);
            }
            oracleParameters = null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
