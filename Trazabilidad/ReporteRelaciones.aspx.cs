using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using Entity;
using EventLog;
using System.Data;
using Config;
using Utils;
using OfficeOpenXml;

namespace Trazabilidad
{
    public partial class ReporteRelaciones : System.Web.UI.Page
    {

        /// <summary>
        /// Usuario logueado en el sistema
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Data Table que contiene el resultado de la consulta
        /// </summary>
        private DataTable dtRelaciones
        {            
            get { return (ViewState["dtRelaciones"] != null) ? ViewState["dtRelaciones"] as DataTable : new DataTable(); }
            set { ViewState["dtRelaciones"] = value; }
        }


        /// <summary>
        /// Método que llena la grilla de relaciones de envío
        /// </summary>
        private void BindGrid()
        {
            FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion"));
            RelacionEnvio oRelacion = new RelacionEnvio();            
            try
            {
                oRelacion.oDetalle = new DetalleRelacion();
                oRelacion.snumero = this.txtRelacion.Text;
                oRelacion.oDetalle.ifactura = (!string.IsNullOrEmpty(this.txtFactura.Text)) ? Convert.ToInt32(this.txtFactura.Text) : 0;
                oRelacion.dtfechainicial = (!string.IsNullOrEmpty(this.txtFechaInicio.Text)) ? Convert.ToDateTime(this.txtFechaInicio.Text) : new DateTime();
                oRelacion.dtfechafinal = (!string.IsNullOrEmpty(this.txtFechaFin.Text)) ? Convert.ToDateTime(this.txtFechaFin.Text) : new DateTime();
                oRelacion.oDetalle.ienviado = (!string.IsNullOrEmpty(this.ddlEnviado.SelectedValue)) ? Convert.ToInt16(this.ddlEnviado.SelectedValue) : (short)2;
                oRelacion.oDetalle.irecibido = (!string.IsNullOrEmpty(this.ddlRecibido.SelectedValue)) ? Convert.ToInt16(this.ddlRecibido.SelectedValue) : (short)2;
                oRelacion.oDetalle.iasignado = (!string.IsNullOrEmpty(this.ddlAsignado.SelectedValue)) ? Convert.ToInt16(this.ddlAsignado.SelectedValue) : (short)2;
                if (!string.IsNullOrEmpty(this.ddlEstado.SelectedValue)) oRelacion.cestado = Convert.ToChar(this.ddlEstado.SelectedValue);
                this.dtRelaciones = oFacade.GetRelationshipsReport(oRelacion);
                this.gvRelaciones.DataSource = dtRelaciones;
                this.gvRelaciones.DataBind();
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
                oRelacion = null;
            }
        }
        
        /// <summary>
        /// Evento cargar página, carga los controles iniciales
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.relationshipreport))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                if (!this.IsPostBack)
                {
                    this.LoadControls();
                    this.BindGrid();
                }
            }
        }

        /// <summary>
        /// Evento botón filtrar grilla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón limpiar filtros de búsqueda de la grilla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtFactura.Text = this.txtRelacion.Text = string.Empty;
            this.ddlEstado.SelectedValue = this.ddlRecibido.SelectedValue = this.ddlEnviado.SelectedValue = this.ddlAsignado.SelectedValue = string.Empty;
            this.txtFechaFin.Text = this.txtFechaInicio.Text = string.Empty;
            this.BindGrid();
        }

        /// <summary>
        /// Evento paginador de la grilla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvRelaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvRelaciones.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void imbExportar_Click(object sender, ImageClickEventArgs e)
        {
            ExcelPackage oExcel = new ExcelPackage();
            ExcelWorksheet ws = oExcel.Workbook.Worksheets.Add("Relaciones");
            ws.Cells["A1"].LoadFromDataTable(this.dtRelaciones, true);
            Response.AddHeader("content-disposition", "attachment; filename=relacionesenvio.xlsx");
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

        private void LoadControls()
        {
            this.txtFechaInicio.Text = new DateTime(DateTime.Now.Year, 1, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).ToString("dd/MM/yyyy");
        }
    }
}