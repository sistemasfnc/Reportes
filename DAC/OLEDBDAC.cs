using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using Config;
using EventLog;

namespace DAC
{
    public class OLEDBDAC : IDisposable
    {
        private OleDbConnection oConnection { get; set; }
        private bool isConnected { get; set; }
        private string sConnection { get; set; }

        public OLEDBDAC()
        {
            //this.sConnection = @"Provider=VFPOLEDB.1;Data Source=\\eir\Data\aghis.dbc;";
            this.sConnection = Config.Configuration.GetStringValue("FoxConnection");
        }

        public OLEDBDAC(string sConnection)
        {
            this.sConnection = sConnection;
        }

        private bool Connect()
        {            
            try
            {
                this.oConnection = new OleDbConnection(this.sConnection);
                this.oConnection.Open();
                return true;
            }
            catch (Exception ex)
            {                
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                return false;
            }            
        }

        public DataTable GetDataTable(string CommandText, List<OleDbParameter> lParamenters, bool type)
        {
            DataSet ds = new DataSet();
            OleDbDataAdapter da = null;
            try
            {
                if (this.Connect())
                {
                    da = new OleDbDataAdapter(CommandText, this.oConnection);
                    if (type) da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    if (lParamenters != null)
                    {
                        for (int i = 0; i < lParamenters.Count; i++)
                        {
                            da.SelectCommand.Parameters.Add(lParamenters[i]);
                        }
                    }
                    da.Fill(ds);
                                        
                }
                return (ds.Tables.Count > 0) ? ds.Tables[0] : new DataTable();
            }
            catch (OleDbException ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                //throw new ApplicationException("Error al obtener la tabla de datos");
                throw;
            }
            finally
            {
                if (da != null)
                {
                    da.SelectCommand.Parameters.Clear();
                    da.Dispose();
                    da = null;
                }                
            }            
        }

        public object GetScalar(string CommandText, List<OleDbParameter> lParameters)
        {
            object Scalar = null;
            OleDbCommand sc = null;
            try
            {
                if (this.Connect())
                {
                    sc = new OleDbCommand(CommandText, this.oConnection);
                    if (lParameters != null)
                    {
                        for (int i = 0; i < lParameters.Count; i++)
                        {
                            sc.Parameters.Add(lParameters[i]);
                        }
                    }                                    
                }
                Scalar = sc.ExecuteScalar();
                return Scalar;
            }
            catch (OleDbException ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                throw new ApplicationException("Error al obtener el valor escalar");
            }
            finally
            {
                if (sc != null)
                {
                    sc.Parameters.Clear();
                    sc.Dispose();
                    sc = null;
                }                
            }
        }

        public void ExecuteNonQuery(string CommandText, List<OleDbParameter> parameters, bool aux, bool type = false)
        {
            OleDbCommand sc = null;
            try
            {
                if (this.Connect())
                {
                    sc = new OleDbCommand(CommandText, this.oConnection);
                    if (type) sc.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        foreach (OleDbParameter parameter in parameters)
                        {
                            sc.Parameters.Add(parameter);
                        }
                    }
                    sc.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                throw;
            }
            finally
            {
                if (sc != null)
                {
                    sc.Parameters.Clear();
                    sc.Dispose();
                    sc = null;
                }                
            }
        }

        public bool TableExists(string sTable)
        {
            DataTable dt = null;
            bool aux = false;
            try
            {
                if (this.Connect())
                {
                    dt = this.oConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    for (int i = 0; i < dt.Rows.Count && !aux; i++)
                    {
                        aux = (dt.Rows[i]["TABLE_NAME"].ToString() == sTable);
                    }                    
                }
                return aux;
            }
            catch (OleDbException ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                throw;
            }
        }

        public void Dispose()
        {
            
            if (this.oConnection != null)
            {
                if (this.oConnection.State == ConnectionState.Open) this.oConnection.Close();
                this.oConnection.Dispose();
            }
            this.oConnection = null;
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
