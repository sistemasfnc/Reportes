using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;

namespace Facade
{
    public class FacadeUser : IDisposable
    {

        private string sConnectionString { get; set; }

        public FacadeUser(string sConnection = "")
        {
            if (!string.IsNullOrEmpty(sConnection)) this.sConnectionString = sConnection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public User Get(User oEntity)
        {
            if (string.IsNullOrEmpty(this.sConnectionString))
            {
                using (UserDAC oDAC = new UserDAC())
                {
                    return oDAC.Get(oEntity);
                }
            }
            else
            {
                using (UsuarioDAC oDAC = new UsuarioDAC(this.sConnectionString))
                {
                    return oDAC.Get(oEntity);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iprofile"></param>
        /// <returns></returns>
        public List<Security> GetPermissionsByProfile(int iprofile)
        {
            using (UsuarioDAC oDAC = new UsuarioDAC(this.sConnectionString))
            {
                return oDAC.GetPermissionsByProfile(iprofile);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lastlogin"></param>
        public void  UpdateLastLogin(int id, DateTime lastlogin)
        {
            if (string.IsNullOrEmpty(this.sConnectionString))
            {
                using (UserDAC oDAC = new UserDAC())
                {
                    oDAC.UpdateLastLogin(id, lastlogin);
                }
            }
            else
            {
                using (UsuarioDAC oDAC = new UsuarioDAC(this.sConnectionString))
                {
                    oDAC.UpdateLastLogin(id, lastlogin);
                }
            }                        
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public List<User> GetAll(User oEntity)
        {
            if (string.IsNullOrEmpty(this.sConnectionString))
            {
                using (UserDAC oDAC = new UserDAC())
                {
                    return oDAC.GetAll(oEntity);
                }
            }
            else
            {
                using (UsuarioDAC oDAC = new UsuarioDAC(this.sConnectionString))
                {
                    return oDAC.GetAll(oEntity);
                }
            }                        
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {            
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
