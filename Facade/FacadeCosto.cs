using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;

namespace Facade
{
    public class FacadeCosto : IDisposable
    {
        public string sConnection { get; set; }

        public FacadeCosto(string sConnectionString)
        {
            this.sConnection = sConnectionString;
        }

        public List<Employee> GetEmployees(Employee oEmploy)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetEmployees(oEmploy);
            }
        }

        public List<Generic> GetCosts()
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetCostList();
            }
        }

        public List<Cost> GetCosts(Cost oCost)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetCostList(oCost);
            }
        }

        public List<Cost> GetEmployeeCostList(CostProcess oProcess)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetEmployeeCostList(oProcess);
            }
        }

        public int Save(CostProcess oProcess, Employee oEmployee)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                if (oProcess.iid == 0)
                {
                    return oDAC.SaveCostList(oProcess, oEmployee);
                }
                else
                {
                    oDAC.EditCostList(oProcess);
                    return 0;
                }

            }
        }

        public void Save(CostProcess oProcess)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.EditCostList(oProcess);
            }
        }

        public CostProcess GetProcess(CostProcess oProcess)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetProcess(oProcess);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oReport"></param>
        /// <returns></returns>
        public List<CostReport> GetCostReport(CostReport oReport)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetCostReport(oReport);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        public void Insert(Cost oEntity)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Insert(oEntity);
            }
        }

        /// <summary>
        /// Método para inactivar un centro de costo en la base de datos
        /// </summary>
        /// <param name="iCost">Id del centro de costo</param>
        public void Delete(int iCost)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Delete(iCost);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        public void Edit(Cost oEntity)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.Edit(oEntity);
            }
        }

        public List<CostProcess> GetCostProcessList(CostProcess oProcess)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                return oDAC.GetCostProcessList(oProcess);
            }
        }

        public void SaveCostList(List<CostProcess> lCostProcess)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.SaveCostList(lCostProcess);
            }
        }

        public void SaveSpecialCostList(CostProcess oProcess, string sType)
        {
            using (CostosDAC oDAC = new CostosDAC())
            {
                oDAC.sConnection = this.sConnection;
                oDAC.SaveSpecialCostList(oProcess, sType);
            }
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
