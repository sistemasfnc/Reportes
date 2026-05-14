using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using EventLog;
using System.Data;
using System.Data.OleDb;
using Utils;

namespace DAC
{
    public class SanitasDAC : IDisposable
    {        
        public List<Sanitas> GetProcedures()
        {
            List<Sanitas> lSanitas = new List<Sanitas>();
            DataTable dt = new DataTable();
            Sanitas oEntity = null;            
            try
            {
                dt = this.GetData();
                for (int i = 0; i < dt.Rows.Count; i++)
                {                    
                    oEntity = new Sanitas()
                    {
                        counter = i + 1,
                        branch = "FUNDACION NEUMOLOGICA COLOMBIANA",
                        typebranch = "NIT",
                        idbranch = "800180553-4",
                        insurance = "EPS SANITAS",
                        patientname = dt.Rows[i]["cpacprinom"].ToString().Trim() + " " + dt.Rows[i]["cpacpriape"].ToString().Trim(),
                        patientdoctype = (string.IsNullOrEmpty(dt.Rows[i]["ctipidcod"].ToString())) ? "CC" : dt.Rows[i]["ctipidcod"].ToString(),
                        patientdocument = dt.Rows[i]["cpacidnro"].ToString(),
                        gender = (Convert.ToInt32(dt.Rows[i]["nPacSexo"]) == 1) ? 'M' : 'F',
                        birthday = Convert.ToDateTime(dt.Rows[i]["dPacNacim"]),
                        cellphone = dt.Rows[i]["ncelular"].ToString(),
                        phone = dt.Rows[i]["cpactelefono"].ToString(),
                        email = dt.Rows[i]["cpacemail"].ToString(),
                        servicedate = Convert.ToDateTime(dt.Rows[i]["dRegHic"]),
                        servicesource = "ENFERMEDAD GENERAL",
                        servicecode = dt.Rows[i]["cCupsCod"].ToString(),
                        servicename = (string.IsNullOrEmpty(dt.Rows[i]["cCupsDes"].ToString().Trim())) ? dt.Rows[i]["csrvDes"].ToString() : dt.Rows[i]["cCupsDes"].ToString(), 
                        //servicename = dt.Rows[i]["csrvDes"].ToString(), 
                        qty = Convert.ToInt32(dt.Rows[i]["nsesiones"]),
                        priority = "NO PRIORITARIO",
                        requestdate = Convert.ToDateTime(dt.Rows[i]["dtsrvsol"]),
                        observation = dt.Rows[i]["mrecomenda"].ToString(),
                        justification = dt.Rows[i]["mjustifica"].ToString(),
                        cie10code = dt.Rows[i]["cDiagCod"].ToString(),
                        cie10name = dt.Rows[i]["cDiagDes"].ToString(),                        
                        etiology = dt.Rows[i]["mDiagDes"].ToString(),
                        professional = dt.Rows[i]["cprofnom"].ToString() + " " + dt.Rows[i]["cprofape"].ToString(),
                        speciality = dt.Rows[i]["cEspDes"].ToString(),
                        profdoctype = (string.IsNullOrEmpty(dt.Rows[i]["cprofcc"].ToString())) ? string.Empty : "CC",
                        profdocument = dt.Rows[i]["cprofcc"].ToString(),
                        profphone = dt.Rows[i]["cproftele1"].ToString(),
                    };
                    lSanitas.Add(oEntity);
                }
                return lSanitas;
            }
            catch (ApplicationException)
            {
                throw new ApplicationException("Error al obtener el listado de procedimientos Sanitas");
            }
            catch (Exception ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                throw new ApplicationException("Error al obtener el listado de procedimientos Sanitas");
            }
            finally
            {
                oEntity = null;
                dt.Dispose();
                dt = null;
            }                        
        }

        private DataTable GetData()
        {
            DateTime iDate = DateTime.Now.AddDays(-1);
            StringBuilder sQuery = new StringBuilder("SELECT HicDetServicio.*, HicPaciente.nPacSexo, HicPaciente.dPacNacim, srvServicios.cSrvDes");
            sQuery.Append(", srvServicios.cSrvDesCor, srvServicios.cCncCod, srvServicios.cCupsCod, srvServicios.cCupsDes, srvServicios.mJustifica, srvConcepto.cCncDes");
            sQuery.Append(", cpacprinom, cpacpriape, cpacidnro, cpactelefono, ctipidcod, cpacemail, HicDetDiag.*, RipsDiag.cDiagDes, cEspDes");
            sQuery.Append(", cprofnom, cprofape, cprofcc, cproftele1, ncelular, HicPaciente.cpacidnro");
            sQuery.Append(" FROM HicDetServicio INNER JOIN HicPaciente ON HicDetServicio.cRegIdPac = HicPaciente.cRegIdPac");
            sQuery.Append(" INNER JOIN srvServicios ON srvServicios.cSrvCod = HicDetServicio.cSrvCod");
            sQuery.Append(" INNER JOIN srvConcepto ON srvConcepto.cCncCod = srvServicios.cCncCod");
            sQuery.Append(" INNER JOIN HicDetDiag ON HicDetDiag.cclave = HicDetServicio.cclave");
            sQuery.Append(" INNER JOIN RipsDiag ON RipsDiag.cDiagCod = HicDetDiag.cDiagCod");
            sQuery.Append(" INNER JOIN hicprofesional ON hicprofesional.cregidpro = HicDetServicio.cregidpro");
            sQuery.Append(" INNER JOIN RIPsEspe ON RIPsEspe.cEspCod = hicprofesional.cEspCod");
            sQuery.Append(" WHERE HicDetServicio.dRegHic BETWEEN {^2015-02-24} AND {^2015-06-30}");
            //sQuery.Append(Tools.FormatFoxDate(iDate));
            sQuery.Append(" AND HicDetServicio.cEmpCod = '00029' AND nposic = 1 AND srvServicios.cCncCod <> '9999' ORDER BY HicPaciente.cRegIdPac");
            using (OLEDBDAC oDAC = new OLEDBDAC())
            {
                return oDAC.GetDataTable(sQuery.ToString(), null, false);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
