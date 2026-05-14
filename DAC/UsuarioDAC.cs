using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using EventLog;
using Entity;
using Oracle.ManagedDataAccess.Client;

namespace DAC
{
    public class UsuarioDAC : IDisposable
    {
        private OracleDAC oDAC { get; set; }        

        private List<Security> lSecurity { get; set; }

        public UsuarioDAC()
        {
            this.oDAC = new OracleDAC();
        }

        public UsuarioDAC(string sConnection)
        {
            this.oDAC = new OracleDAC();
            this.oDAC.sConnection = sConnection;
            ACLDAC oACL = new ACLDAC(sConnection);
            try
            {
                oDAC.Connect();
                this.lSecurity = oACL.GetAll(new Security());
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Error al inicializar el objeto de usuarios");
            }
            finally
            {
                oACL.Dispose();
                oACL = null;
            }
        }

        public UsuarioDAC(string sConnection, bool flag)
        {
            this.oDAC = new OracleDAC();            
            this.oDAC.sConnection = sConnection;
            this.oDAC.Connect();
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
            string sUser = string.Empty;
            CostUser oCost = null;
            try
            {
                dt = this.GetData(oEntity);
                (from DataRow dr in dt.Rows
                 group dr by Convert.ToInt32(dr["us_id"]) into f
                 select new
                 {
                     Key = f.Key,
                     Elements = f,
                 }).ToList().ForEach(f =>
                {
                    oEntity = new User()
                    {
                        id = f.Key,
                        username = f.Elements.First()["us_usuario"].ToString(),
                        firstname = f.Elements.First()["us_nombre"].ToString(),
                        lastname = f.Elements.First()["us_apellido"].ToString(),
                        lastlogin = (f.Elements.First()["us_ultimologin"] != DBNull.Value) ? Convert.ToDateTime(f.Elements.First()["us_ultimologin"]) : new DateTime(),
                        email = f.Elements.First()["us_email"].ToString().Trim(),
                        idprofile = Convert.ToInt32(f.Elements.First()["us_pe_id"]),
                        profilename = f.Elements.First()["pe_nombre"].ToString(),
                        spagina = f.Elements.First()["pe_pagina"].ToString(),
                        lSecurity = this.lSecurity.FindAll(x => x.idprofile == Convert.ToInt32(f.Elements.First()["us_pe_id"])),
                        otheruser = new List<string>(),
                        lCost = new List<CostUser>(),
                    };
                    (from DataRow dr in f.Elements
                     where Convert.ToInt32(dr["us_id"]) == f.Key
                     group dr by dr["ug_nombre"].ToString() into a
                     select new
                     {
                         Key = a.Key,
                         Elements = a,
                     }).ToList().ForEach(a =>
                    {

                        sUser = a.Elements.First()["ug_nombre"].ToString();
                        oEntity.otheruser.Add(sUser);
                    });
                    (from DataRow dr in f.Elements
                     where Convert.ToInt32(dr["us_id"]) == f.Key
                     group dr by dr["uc_centro"].ToString() into b
                     select new
                     {
                         Key = b.Key,
                         Elements = b,
                     }).ToList().ForEach(b =>
                     {
                         oCost = new CostUser()
                         {
                             iuser = f.Key,
                             scode = b.Elements.First()["uc_centro"].ToString(),
                         };                        
                         oEntity.lCost.Add(oCost);
                     });
                    lUser.Add(oEntity);
                });                  
                return lUser;
            }
            catch (InvalidCastException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
                throw new ApplicationException("Ha ocurrido un error al obtener los usuarios");
            }
            catch(NullReferenceException ex)
            {
                LogError.WriteError("Facturacion", "DAC", ex);
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
        /// <param name="iprofile"></param>
        /// <returns></returns>
        public List<Security> GetPermissionsByProfile(int iprofile)
        {
            return this.lSecurity.FindAll(x => x.idprofile == iprofile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        private DataTable GetData(User oEntity)
        {
            StringBuilder sQuery = new StringBuilder("SELECT usuario.*, pe_nombre, ug_nombre, uc_centro, pe_pagina FROM usuario INNER JOIN perfil ON us_pe_id = pe_id LEFT JOIN usuariogestor ON us_id = ug_us_id");
            sQuery.Append(" LEFT JOIN usuariocentrocosto ON uc_us_id = us_id WHERE 1 = 1");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            if (oEntity.id != 0)
            {
                sQuery.Append(" AND us_id = :id");
                lParameters.Add(new OracleParameter(":id", oEntity.id));
            }
            if (!string.IsNullOrEmpty(oEntity.username))
            {
                sQuery.Append(" AND us_usuario = :username");
                lParameters.Add(new OracleParameter(":username", oEntity.username));
            }
            if (!string.IsNullOrEmpty(oEntity.password))
            {
                sQuery.Append(" AND us_password = :password");
                lParameters.Add(new OracleParameter(":password", oEntity.password));
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
            List<OracleParameter> lParameters = new List<OracleParameter>();
            string sQuery = "UPDATE usuario SET us_ultimologin = :lastlogin WHERE us_id = :id";
            lParameters.Add(new OracleParameter(":lastlogin", lastlogin));
            lParameters.Add(new OracleParameter(":id", id));
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

        public void Insert(User oEntity)
        {
            StringBuilder sQuery = new StringBuilder("INSERT INTO usuario (us_usuario, us_nombre, us_apellido, us_email, us_pe_id, us_ultimologin) VALUES");
            sQuery.Append("(:username, :firstname, :lastname, :email, :idprofile, SYSDATE) RETURNING us_id INTO :us_id");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            OracleParameter us_id = new OracleParameter("us_id", OracleDbType.Decimal, ParameterDirection.ReturnValue);
            try
            {
                lParameters.Add(us_id);
                lParameters.Add(new OracleParameter(":username", oEntity.username));
                lParameters.Add(new OracleParameter(":firstname", oEntity.firstname));
                lParameters.Add(new OracleParameter(":lastname", oEntity.lastname));
                lParameters.Add(new OracleParameter(":email", oEntity.email));
                lParameters.Add(new OracleParameter(":idprofile", oEntity.idprofile));                
                this.oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);                
                oEntity.id = Convert.ToInt32(us_id.Value.ToString());
                this.DeleteUsers(oEntity.id);
                this.InsertUsers(oEntity);
                this.DeleteCost(oEntity.id);
                this.InsertCosts(oEntity);
            }
            catch (Exception ex)
            {                
                throw new ApplicationException("Error al ingresar la informacion del usuario en la base de datos");
            }
            finally
            {
                sQuery = null;
                lParameters = null;
            }
        }

        private void DeleteUsers(int id)
        {
            string Query = "DELETE FROM usuariogestor WHERE ug_us_id = :iduser";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter("iduser", id));
            this.oDAC.ExecuteNonQuery(Query, lParameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        private void InsertUsers(User oEntity)
        {
            string sQuery = "INSERT INTO usuariogestor (ug_us_id, ug_nombre) VALUES (:iduser, :gestor)";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            for (int i = 0; i < oEntity.otheruser.Count; i++)
            {                
                if (!string.IsNullOrEmpty(oEntity.otheruser[i]))
                {
                    lParameters.Add(new OracleParameter("iduser", oEntity.id));
                    lParameters.Add(new OracleParameter("gestor", oEntity.otheruser[i]));
                    this.oDAC.ExecuteNonQuery(sQuery, lParameters);
                    lParameters.RemoveRange(0, lParameters.Count);
                }
                
            }
        }

        /// <summary>
        /// Inserta los centros de costo para un usuario
        /// </summary>
        /// <param name="oEntity">Objeto usuario</param>
        private void InsertCosts(User oEntity)
        {
            string sQuery = "INSERT INTO usuariocentrocosto (uc_us_id, uc_centro) VALUES (:iduser, :codigo)";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            for (int i = 0; i < oEntity.lCost.Count; i++)
            {
                lParameters.Add(new OracleParameter(":iduser", oEntity.id));
                lParameters.Add(new OracleParameter(":codigo", oEntity.lCost[i].scode));
                this.oDAC.ExecuteNonQuery(sQuery, lParameters);
                lParameters.RemoveRange(0, lParameters.Count);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        private void DeleteCost(int id)
        {
            string Query = "DELETE FROM usuariocentrocosto WHERE uc_us_id = :iduser";
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter("iduser", id));
            this.oDAC.ExecuteNonQuery(Query, lParameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        public void Update(User oEntity)
        {
            StringBuilder sQuery = new StringBuilder("UPDATE usuario SET us_pe_id = :idprofile");
            List<OracleParameter> lParameters = new List<OracleParameter>();
            lParameters.Add(new OracleParameter(":idprofile", oEntity.idprofile));
            if (!string.IsNullOrEmpty(oEntity.firstname))
            {
                lParameters.Add(new OracleParameter(":firstname", oEntity.firstname));
                sQuery.Append(", us_nombre = :firstname");
            }
            if (!string.IsNullOrEmpty(oEntity.username))
            {
                lParameters.Add(new OracleParameter(":username", oEntity.username));
                sQuery.Append(", us_usuario = :username");
            }
            if (!string.IsNullOrEmpty(oEntity.lastname))
            {
                lParameters.Add(new OracleParameter(":lastname", oEntity.lastname));
                sQuery.Append(", us_apellido = :lastname");
            }
            if (!string.IsNullOrEmpty(oEntity.email))
            {
                lParameters.Add(new OracleParameter(":email", oEntity.email));
                sQuery.Append(", us_email = :email");
            }
            if (!string.IsNullOrEmpty(oEntity.password))
            {
                lParameters.Add(new OracleParameter(":password", oEntity.password));
                sQuery.Append(", us_password = :password");
            }
            if (!string.IsNullOrEmpty(oEntity.costcenter))
            {
                lParameters.Add(new OracleParameter(":costcenter", oEntity.costcenter));
                sQuery.Append(", us_centrocostos = :costcenter");
            }
            sQuery.Append(" WHERE us_id = :id");
            lParameters.Add(new OracleParameter(":id", oEntity.id));
            try
            {
                this.oDAC.ExecuteNonQuery(sQuery.ToString(), lParameters);
                this.DeleteUsers(oEntity.id);
                this.InsertUsers(oEntity);
                this.DeleteCost(oEntity.id);
                this.InsertCosts(oEntity);
            }
            catch (Exception)
            {                
                throw new ApplicationException("Error al actualizar la informacion del usuario en la base de datos");
            }
            finally
            {
                sQuery = null;
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
            GC.Collect();
        }
    }
}
