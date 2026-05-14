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
using System.Text;
using EventLog;

namespace Trazabilidad
{
    public partial class DistribuirCostos : System.Web.UI.Page
    {
        /// <summary>
        /// Objeto que guarda al usuario logueado en el sistema
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }       

        /// <summary>
        /// Evento cargar página
        /// </summary>
        /// <param name="sender">Objeto página</param>
        /// <param name="e">Argumentos</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.listemployee))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
                this.BindGrid();
            }
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtApellido.Text = this.txtDocumento.Text = this.txtNombre.Text = string.Empty;
            this.ddlCentro.SelectedValue = (!this.ddlCentro.Enabled) ? this.oUser.costcenter : string.Empty;
            this.BindGrid();
        }

        protected void gvEmpleados_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvEmpleados.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void gvEmpleados_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
            if (e.CommandName == "Assign")
            {
                string sDocument = this.gvEmpleados.DataKeys[gr.RowIndex]["sdocument"].ToString();
                //ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "window.open('AsignarCentros.aspx?document=" + sDocument + "');", true);
                ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "OpenWindow('" + sDocument + "');", true);
            }
        }

        /// <summary>
        /// Método que retorna la lista separadas por (,) de centro de costos seleccionados en el filtro
        /// </summary>
        /// <returns>Cadena de caracteres con la lista</returns>
        private string GetCostCenterList()
        {
            if (this.ddlCentro.Enabled)
            {
                return this.ddlCentro.SelectedValue;
            }
            string[] aText = new string[this.oUser.lCost.Count];
            for (int i = 0; i < aText.Length; i++)
            {
                aText[i] = this.oUser.lCost[i].scode;
            }
            return string.Join("','", aText);
        }

        /// <summary>
        /// Método que llena la grilla de empleados
        /// </summary>
        private void BindGrid()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("DBNovasoft"));
            Employee oEmploy = null;
            try
            {
                oEmploy = new Employee()
                {
                    sdocument = this.txtDocumento.Text,
                    sname = this.txtNombre.Text,
                    slastname = this.txtApellido.Text,
                    //smaincostcenter = this.GetCostCenterList(),
                };
                this.gvEmpleados.DataSource = oFacade.GetEmployees(oEmploy);
                this.gvEmpleados.DataKeyNames = new string[] { "sdocument" };
                this.gvEmpleados.DataBind();
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
        /// Método que carga los controles iniciales
        /// </summary>
        private void LoadControls()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlCentro.DataSource = oFacade.GetCosts();
                this.ddlCentro.DataTextField = "name";
                this.ddlCentro.DataValueField = "code";
                this.ddlCentro.DataBind();                
                this.ddlCentro.Enabled = Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.filteremployee);
                this.ddlCentro.SelectedValue = string.Empty;
                //this.ddlCentro.SelectedValue = (!this.ddlCentro.Enabled) ? this.oUser.costcenter : string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }        
    }
}