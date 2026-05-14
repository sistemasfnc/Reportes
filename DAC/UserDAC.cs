using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using System.Data;
using System.Data.SqlClient;
using EventLog;

namespace DAC
{
    public class UserDAC : IDisposable
    {

        private SQLDAC oDAC { get; set; }

        public UserDAC()
        {
            this.oDAC = new SQLDAC();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public User Get(User oEntity)
        {
            List<User> lUser = this.GetAll(oEntity);
            return (lUser.Count > 0) ? lUser[0] : oEntity;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<User> GetAll(User oEntity)
        {
            List<User> lUser = new List<User>();
            DataTable dt = new DataTable();
            try
            {
                dt = this.GetData(oEntity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oEntity = new User()
                    {
                        id = Convert.ToInt32(dt.Rows[i]["id"]),
                        username = dt.Rows[i]["username"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        lastname = dt.Rows[i]["lastname"].ToString(),
                        lastlogin = Convert.ToDateTime(dt.Rows[i]["lastlogin"]),
                        email = dt.Rows[i]["email"].ToString(),                        
                    };
                    lUser.Add(oEntity);
                }
                return lUser;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                throw new ApplicationException("Ha ocurrido un error al obtener los usuarios");
            }
            catch(NullReferenceException ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                throw new ApplicationException("Ha ocurrido un error al obtener los usuarios");
            }
            catch (Exception)
            {
                throw new ApplicationException("Ha ocurrido un error al obtener los usuarios");
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
        private DataTable GetData(User oEntity)
        {
            StringBuilder sQuery = new StringBuilder("SELECT * FROM users WHERE id IS NOT NULL");
            List<SqlParameter> lParameters = new List<SqlParameter>();
            if (oEntity.id != 0)
            {
                sQuery.Append(" AND id = @id");
                lParameters.Add(new SqlParameter("@id", oEntity.id));
            }
            if (!string.IsNullOrEmpty(oEntity.username))
            {
                sQuery.Append(" AND username = @username");
                lParameters.Add(new SqlParameter("@username", oEntity.username));
            }
            if (!string.IsNullOrEmpty(oEntity.password))
            {
                sQuery.Append(" AND password = @password");
                lParameters.Add(new SqlParameter("@password", oEntity.password));
            }
            return this.oDAC.GetDataTable(sQuery.ToString(), lParameters);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lastlogin"></param>
        public void UpdateLastLogin(int id, DateTime lastlogin)
        {
            List<SqlParameter> lParameters = new List<SqlParameter>();
            string sQuery = "UPDATE users SET lastlogin = @lastlogin WHERE id = @id";
            lParameters.Add(new SqlParameter("@lastlogin", lastlogin));
            lParameters.Add(new SqlParameter("@id", id));
            try
            {
                this.oDAC.ExecuteNonQuery(sQuery, lParameters);
            }
            catch (Exception)
            {                
                throw new ApplicationException("Error al actualizar el ultimo ingreso del usuario");
            }
            finally
            {
                lParameters = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {            
            this.oDAC.Dispose();
            this.oDAC = null;
            GC.SuppressFinalize(this);
        }
    }
}
