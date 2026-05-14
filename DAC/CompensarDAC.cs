using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using System.Data;
using System.Data.SqlClient;
using Utils;

namespace DAC
{
    public class CompensarDAC : IDisposable
    {
        public string sConnection { get; set; }

        public List<PFP> lPFP { get; set; }

        private DataTable GetServinteData()
        {
            string sQuery = "SELECT * FROM VPFPCompensar";
            using (SQLDAC oDAC = new SQLDAC(this.sConnection))
            {
                return oDAC.GetDataTable(sQuery, null);
            }
        }
        
        private void GetServinteList()
        {
            DataTable dt = new DataTable();
            PFP oEntity = null;
            try
            {
                dt = this.GetServinteData();
                this.lPFP = new List<PFP>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    oEntity = new PFP()
                    {
                        autorizacion = dt.Rows[i]["Autorizacion"].ToString(),
                        nit = dt.Rows[i]["Nit"].ToString(),
                        nuip = dt.Rows[i]["Nuip"].ToString(),
                        cups = dt.Rows[i]["Cups"].ToString(),
                        fecha = Convert.ToDateTime(dt.Rows[i]["Fecha"]),
                        estado = dt.Rows[i]["Estado"].ToString(),
                        tiposervicio = dt.Rows[i]["TipoServicio"].ToString(),
                        eps = dt.Rows[i]["Eps"].ToString(),
                        documento = dt.Rows[i]["Documento"].ToString(),
                        tipodocumento = Tools.GetDocumentType(dt.Rows[i]["TipoDocumento"].ToString()),
                        origen = dt.Rows[i]["Origen"].ToString(),
                    };
                    this.lPFP.Add(oEntity);
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            finally
            {
                dt.Dispose();
                dt = null;
                oEntity = null;
            }
        }

        private PFP GetEntity(DataRow dr)
        {
            return this.lPFP.Find(x => x.documento == dr["Documento"].ToString() && x.fecha == Convert.ToDateTime(dr["Fecha"]));
        }

        public List<PFP> GetPFPList()
        {
            DataTable dt = new DataTable();
            List<PFP> lEntity = new List<PFP>();
            int i = 0;
            PFP oEntity = null;
            try
            {
                this.GetServinteList();
                dt = this.GetPFPData();
                foreach (DataRow dr in dt.Rows)
	            {
                    oEntity = this.GetEntity(dr);
                    if (oEntity != null)
                    {
                        lEntity.Add(oEntity);
                        lEntity[i].archivo = dr["archivo"].ToString();
                        i++;
                    }                    
	            }
                return lEntity;
            }
            catch (Exception)
            {                
                throw;
            }
            finally
            {
                oEntity = null;
                dt.Dispose();
                dt = null;
            }
        }

        private DataTable GetPFPData()
        {
            StringBuilder sQuery = new StringBuilder("SELECT IDENTIFICACION Documento, CAST(FECHA_PRUEBA AS DATE) Fecha, NOMBRE_ARCHIVO archivo FROM InspiraPdfPfp LEFT JOIN CompensarEnviados ON archivo = NOMBRE_ARCHIVO");
            sQuery.Append(" WHERE ESTADO = 'Aprobado' AND YEAR(fecha_creacion) = YEAR(GETDATE()) AND NOM_EMPRESA LIKE '%COMPENSAR%' AND ID IS NULL");
            using (SQLDAC oDAC = new SQLDAC(this.sConnection))
            {
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }            
        }

        public void Insert(DataTable dt)
        {
            using (SQLDAC oDAC = new SQLDAC(this.sConnection))
            {
                oDAC.BulkData("CompensarEnviados", dt);
            }
        }

        public DataTable GetSanitasFiles()
        {
            StringBuilder sQuery = new StringBuilder("SELECT IDENTIFICACION Documento, CAST(FECHA_PRUEBA AS DATE) Fecha, NOMBRE_ARCHIVO archivo FROM InspiraPdfPfp");
            sQuery.Append(" WHERE ESTADO = 'Aprobado' AND fecha_creacion = CAST(DATEADD(DAY, -1, GETDATE()) AS DATE) AND NOM_EMPRESA LIKE '%SANITAS%'");
            //sQuery.Append(" WHERE ESTADO = 'Aprobado' AND fecha_creacion BETWEEN '20170524' AND '20170605' AND NOM_EMPRESA LIKE '%SANITAS%'");
            using (SQLDAC oDAC = new SQLDAC(this.sConnection))
            {
                return oDAC.GetDataTable(sQuery.ToString(), null);
            }
        }

        public void Dispose()
        {
            this.lPFP = null;
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
