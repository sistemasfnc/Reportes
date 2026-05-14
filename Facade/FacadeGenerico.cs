using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using DAC;

namespace Facade
{
    public class FacadeGenerico : IDisposable
    {
        private string ConnectionString { get; set; }
        
        public FacadeGenerico()
        {

        }

        public FacadeGenerico(string sConnection)
        {
            this.ConnectionString = sConnection;
        }

        public List<Generic> GetList(string type, int iStatus = 0)
        {
            using (GenericDAC oDAC = new GenericDAC(this.ConnectionString))
            {
                switch (type)
                {
                    case "plan":
                        return oDAC.GetPlans();
                    case "service":
                        return oDAC.GetServices();
                    case "user":
                        return oDAC.GetUsers();
                    case "status":
                        return oDAC.GetStatus(iStatus);
                    case "support":
                        return oDAC.GetSupports();
                    case "reason":
                        return oDAC.GetReasons();
                    case "company":
                        return oDAC.GetCompanies();
                    case "pending":
                        return oDAC.GetPendings();
                    case "category":
                        return oDAC.GetResponsiveCategories();
                    case "services":
                        return oDAC.GetResposiveServices(iStatus.ToString());
                    case "rate":
                        return oDAC.GetRates();
                    case "planprograms":
                        return oDAC.GetPlans(true);
                    case "convenios":
                        return oDAC.GetAgreements();
                    default:
                        return null;
                }                
            }            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
