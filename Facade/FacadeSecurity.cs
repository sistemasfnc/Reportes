using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;

namespace Facade
{
    public class FacadeSecurity : IDisposable
    {
        private string sConnection { get; set; }

        public FacadeSecurity(string Connection)
        {
            this.sConnection = Connection;
        }
        
        public List<Security> GetACL(Security oEntity)
        {
            using (ACLDAC oDAC = new ACLDAC(this.sConnection))
            {
                return oDAC.GetAll(oEntity);
            }
        }

        public List<Profile> GetProfiles()
        {
            using (PerfilDAC oDAC = new PerfilDAC(this.sConnection))
            {
                return oDAC.GetAll();
            }
        }

        public void CreateUser(User oEntity)
        {
            using (UsuarioDAC oDAC = new UsuarioDAC(this.sConnection, true))
            {
                if (oEntity.id == 0)
                {
                    oDAC.Insert(oEntity);
                }
                else
                {
                    oDAC.Update(oEntity);
                }                
            }
        }

        public List<Generic> GetGestorUser()
        {
            using (GenericDAC oDAC = new GenericDAC(this.sConnection))
            {
                return oDAC.GetUsers();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
