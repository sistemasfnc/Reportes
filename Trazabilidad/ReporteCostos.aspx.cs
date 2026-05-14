using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Entity;
using Facade;
using Utils;
using Config;
using OfficeOpenXml;
using EventLog;

namespace Trazabilidad
{
    public partial class ReporteCostos : System.Web.UI.Page
    {
        /// <summary>
        /// Propiedad de la clase objeto que almacena el usuario en sesión
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Propiedad de la página que almacena en el viewstate la lista de distribución de costos del reporte 
        /// </summary>
        private List<CostReport> lReport
        {
            get { return (ViewState["lReport"] != null) ? (ViewState["lReport"] as List<CostReport>) : new List<CostReport>(); }
            set { ViewState["lReport"] = value; }
        }

        /// <summary>
        /// Evento de la página Cargar
        /// </summary>
        /// <param name="sender">Objeto página</param>
        /// <param name="e">Argumentos</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.costreport))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
                this.BindGrid();
            }
        }

        /// <summary>
        /// Evento botón limpiar la búsqueda
        /// </summary>
        /// <param name="sender">Objeto botón cancelar</param>
        /// <param name="e">Argumentos</param>
        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtDocumento.Text = this.txtNombre.Text = string.Empty;
            this.ddlAno.ClearSelection();
            this.ddlMes.SelectedValue = "0";
            this.ddlCompleto.SelectedValue = string.Empty;
            this.ddlCentro.SelectedValue = string.Empty;            
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón aplicar filtros de búsqueda
        /// </summary>
        /// <param name="sender">Objeto botón buscar</param>
        /// <param name="e">Argumentos</param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        /// <summary>
        /// Evento paginación de la grilla
        /// </summary>
        /// <param name="sender">Objeto grilla</param>
        /// <param name="e">Argumentos</param>
        protected void gvCostos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvCostos.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón exportar excel resultado grilla
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void imbExportar_Click(object sender, ImageClickEventArgs e)
        {
            ExcelPackage oExcel = new ExcelPackage();
            ExcelWorksheet ws = oExcel.Workbook.Worksheets.Add("Costos");
            System.Data.DataTable dt = Tools.ToDataTable(this.lReport);
            ws.Cells["A1"].LoadFromDataTable(dt, true);
            Response.AddHeader("content-disposition", "attachment; filename=distribucioncostos.xlsx");
            Response.Charset = string.Empty;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Clear();
            Response.Buffer = true;
            Response.ContentEncoding = System.Text.Encoding.Default;
            Response.BinaryWrite(oExcel.GetAsByteArray());
            Response.End();
            oExcel.Dispose();
            oExcel = null;
        }

        /// <summary>
        /// Método que asigna los valores iniciales a los controles
        /// </summary>
        private void LoadControls()
        {
            this.ddlAno.DataSource = Tools.GetYears();
            this.ddlAno.DataValueField = "Key";
            this.ddlAno.DataTextField = "Value";
            this.ddlAno.DataBind();
            this.ddlMes.DataSource = Tools.GetMonths();
            this.ddlMes.DataValueField = "Key";
            this.ddlMes.DataTextField = "Value";
            this.ddlMes.DataBind();
            this.ddlMes.Items.Add(new ListItem(string.Empty, "0"));
            this.ddlMes.SelectedValue = "0";
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlCentro.DataSource = oFacade.GetCosts();
                this.ddlCentro.DataTextField = "name";
                this.ddlCentro.DataValueField = "code";
                this.ddlCentro.DataBind();
                this.ddlCentro.SelectedValue = string.Empty;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        /// <summary>
        /// Método que carga la información de la grilla
        /// </summary>
        private void BindGrid()
        {
            CostReport oReport = null;
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {                
                oReport = new CostReport()
                {
                    imonth = Convert.ToInt32(this.ddlMes.SelectedValue),
                    iyear = Convert.ToInt32(this.ddlAno.SelectedValue),
                    smaincostcenter = this.ddlCentro.SelectedValue,
                    sdocument = this.txtDocumento.Text,
                    sfirstname = this.txtNombre.Text,
                    sstatus = this.ddlCompleto.SelectedValue,
                };
                this.lReport = oFacade.GetCostReport(oReport);
                this.gvCostos.DataSource = this.lReport;
                this.gvCostos.DataBind();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oReport = null;
                oFacade.Dispose();
                oFacade = null;
            }
        }        
    }
}