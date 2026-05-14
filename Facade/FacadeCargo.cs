using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;
using System.Data;
using FNCDAC;


namespace Facade
{
    public class FacadeCargo : IDisposable
    {
        private string sConnection { get; set; }

        public FacadeCargo(string Connection)
        {
            this.sConnection = Connection;
        }

        public List<Cargo> GetCharges(Cargo oEntity, bool view = false)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetList(oEntity, view);
            }
        }

        public void ChangeStatus(List<Cargo> lEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                oDAC.UpdateStatus(lEntity);
            }
        }
         
        public void DeleteSupports(int id)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                oDAC.DeleteSupports(id);
            }
        }

        public void InsertSupports(Support oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                oDAC.InsertSupports(oEntity);
            }
        }

        public int CreateCharge(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                if (oEntity.id == 0)
                {
                    return oDAC.Insert(oEntity);
                }
                else
                {
                    oDAC.Edit(oEntity);
                    return oEntity.id;
                }
            }
        }

        public List<Support> GetSupports(int idcharge)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetSupports(idcharge);
            }
        }

        public void InsertReasons(List<Support> lEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                oDAC.InsertReasons(lEntity);
            }
        }

        public List<Support> GetReasons(int idcharge, bool aux = true)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return (aux) ? oDAC.GetReasons(idcharge) : oDAC.GetReasonsLog(idcharge);
            }
        }

        public List<Cargo> GetChargesLog(string idadmission, string company)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetChargesLog(idadmission, company);
            }
        }

        public List<Cargo> GetSurplusList(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetSurplusList(oEntity);
            }
        }

        public void UpdateReasonsResponse(List<Support> lEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                oDAC.UpdateReasonsResponse(lEntity);
            }
        }

        public List<Cargo> GetChargesInvoice(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetChargesInvoice(oEntity);
            }
        }

        public List<Cargo> GetListReport(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetListReport(oEntity);
            }
        }

        public List<Devolucion> GetListReturn(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetListReturn(oEntity);
            }
        }

        public List<Devolucion> GetListStatus(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetListStatus(oEntity);
            }
        }

        public List<Support> GetPendings(int idcharge)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetPendings(idcharge);
            }
        }

        public void InsertPending(List<Support> lEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                oDAC.InsertPending(lEntity);
            }
        }

        public List<Cargo> GetPayment(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetPayment(oEntity);
            }
        }

        public List<Cargo> GetPhamacyCharges(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetPharmacyCharges(oEntity);
            }
        }

        public DataTable GetFamisanar(DateTime initialdate, DateTime enddate)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                return oDAC.GetFamisanar(initialdate, enddate);
            }
        }

        public List<Generic> GetGenericTable()
        {
            List<Generic> lgeneric = new List<Generic>();
            DataTable dataTable = new DataTable();
            ServinteOracle servinteOracle = null;
            Generic generic = null;
            try
            {
                servinteOracle = new ServinteOracle();
                servinteOracle.sconnection = this.sConnection;
                dataTable = servinteOracle.GetGenericTable();
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    generic = new Generic()
                    {
                        code = dataRow["CODIGO"].ToString(),
                        name = dataRow["NOMBRE"].ToString(),
                        table = dataRow["TABLA"].ToString(),

                    };
                    lgeneric.Add(generic);
                }
                return lgeneric;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                servinteOracle.Dispose();
                servinteOracle = null;
                generic = null;
                dataTable.Dispose();
                dataTable = null;
            }
        }

        public List<Paquete> GetProductRates()
        {
            List<Paquete> lpaquete = new List<Paquete>();
            DataTable dataTable = new DataTable();
            ServinteOracle servinteOracle = null;
            Paquete paquete = null;
            try
            {
                servinteOracle = new ServinteOracle();
                servinteOracle.sconnection = this.sConnection;
                dataTable = servinteOracle.GetProductRates();
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    paquete = new Paquete()
                    {
                        scentro = dataRow["PROTARCCO"].ToString(),
                        starifa = dataRow["PROTARTAR"].ToString(),
                        sconcepto = dataRow["PROTARCON"].ToString(),
                        ivalor = Convert.ToInt32(dataRow["PROTARVAL"]),
                        sservicio = dataRow["PROTARPRO"].ToString(),
                    };
                    lpaquete.Add(paquete);
                }
                return lpaquete;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataTable.Dispose();
                dataTable = null;
                servinteOracle.Dispose();
                servinteOracle = null;
                paquete = null;
            }
        }

        public List<Paquete> GetPackages()
        {
            List<Paquete> lpaquete = new List<Paquete>();
            DataTable dataTable = new DataTable();
            ServinteOracle servinteOracle = null;
            Paquete paquete = null;
            try
            {
                servinteOracle = new ServinteOracle();
                servinteOracle.sconnection = this.sConnection;
                dataTable = servinteOracle.GetPackages();
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    paquete = new Paquete()
                    {
                        scentro = dataRow["CENTRO"].ToString(),
                        starifa = dataRow["TARIFA"].ToString(),
                        sconcepto = dataRow["CONCEPTO"].ToString(),
                        icantidad = Convert.ToInt32(dataRow["CANTIDAD"]),
                        ivalor = Convert.ToInt32(dataRow["VALOR"]),
                        scodigo = dataRow["PAQUETE"].ToString(),
                        sservicio = dataRow["SERVICIO"].ToString(),
                        snombre = dataRow["NOMBRE"].ToString(),
                        sunidad = dataRow["UNIDAD"].ToString(),
                        ivalorpaquete = Convert.ToInt32(dataRow["VALORPAQUETE"]),
                    };
                    lpaquete.Add(paquete);                    
                }
                return lpaquete;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataTable.Dispose();
                dataTable = null;
                servinteOracle.Dispose();
                servinteOracle = null;
                paquete = null;
            }
        }

        public void UpdatePharmacyEstatus(Cargo oEntity)
        {
            using (CargosDAC oDAC = new CargosDAC(this.sConnection))
            {
                oDAC.UpdatePharmacyEstatus(oEntity);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
