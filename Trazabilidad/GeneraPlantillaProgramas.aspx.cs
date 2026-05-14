using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FNCEntity;
using FNCUtils;
using System.IO;
using System.Text;
using EventLog;
using Facade;
using Config;
using FNCDAC;
using OfficeOpenXml;
using System.Data;
using Entity;
using FNCFacade;
using AjaxControlToolkit;

namespace Trazabilidad
{
    public partial class GeneraPlantillaProgramas : System.Web.UI.Page
    {
        /// <summary>
        /// Usuario logueado
        /// </summary>
        private Entity.User oUser
        {
            get { return Session["oUser"] as Entity.User; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Utils.Tools.HaveAccess(this.oUser.lSecurity, (int)Entity.Permissions.createadmission))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
            }
        }

        private void LoadControls()
        {
            FacadeGenerico facadeGenerico = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlAgreement.DataValueField = "code";
                this.ddlAgreement.DataTextField = "name";
                this.ddlAgreement.DataSource = facadeGenerico.GetList("convenios");
                this.ddlAgreement.DataBind();
                this.txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
                this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                throw;
            }
            finally
            {
                facadeGenerico.Dispose();
                facadeGenerico = null;
            }
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            DateTime initial = Convert.ToDateTime(this.txtFechaInicio.Text);
            DateTime final = Convert.ToDateTime(this.txtFechaFin.Text);
            ExcelPackage oExcel = null;
            FacadeStatistics facadeStatistics = new FacadeStatistics();
            DataTable dataTable = new DataTable();
            try
            {
                facadeStatistics.sConnection = Configuration.GetStringValue("ServinteIntegra");
                bool besvaloracion = (this.ddlValoracion.SelectedValue == "Si");
                dataTable = facadeStatistics.GetProgramsData(initial, final, this.ddlPlan.SelectedValue, this.ddlAgreement.SelectedValue, besvaloracion);
                oExcel = new ExcelPackage();
                ExcelWorksheet ws = oExcel.Workbook.Worksheets.Add("Plantilla");
                ws.Cells["A1"].LoadFromDataTable(dataTable, true);
                Response.AddHeader("content-disposition", "attachment; filename=planillaprogramas.xlsx");
                Response.Charset = string.Empty;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Clear();
                Response.Buffer = true;
                Response.ContentEncoding = System.Text.Encoding.Default;
                Response.BinaryWrite(oExcel.GetAsByteArray());
                Response.End();               
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                throw;
            }
            finally
            {
                dataTable.Dispose();
                dataTable = null;
                oExcel.Dispose();
                oExcel = null;
            }
        }
    }
}