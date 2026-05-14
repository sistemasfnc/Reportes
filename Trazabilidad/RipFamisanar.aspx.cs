using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using Entity;
using Utils;
using EventLog;
using Config;
using System.Data;
using OfficeOpenXml;

namespace Trazabilidad
{
    public partial class RipFamisanar : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {                
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.ripsreport))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
            }
        }

        private void LoadControls()
        {
            DateTime initial = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            this.txtFechaInicio.Text = initial.ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            DateTime initial = Convert.ToDateTime(this.txtFechaInicio.Text);
            DateTime final = Convert.ToDateTime(this.txtFechaFin.Text);
            ExcelPackage oExcel = null;
            using (FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion")))
            {
                DataTable dt = oFacade.GetFamisanar(initial, final);
                oExcel = new ExcelPackage();
                ExcelWorksheet ws = oExcel.Workbook.Worksheets.Add("Rips Famisanar");
                ws.Cells["A1"].LoadFromDataTable(dt, true);
                Response.AddHeader("content-disposition", "attachment; filename=ripsfamisanar.xlsx");
                Response.Charset = string.Empty;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Clear();
                Response.Buffer = true;
                Response.ContentEncoding = System.Text.Encoding.Default;
                Response.BinaryWrite(oExcel.GetAsByteArray());
                Response.End();
                dt.Dispose();
                dt = null;
                oExcel.Dispose();
                oExcel = null;
            }
        }
    }
}