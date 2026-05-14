using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using Entity;
using Config;
using EventLog;
using Utils;

namespace Trazabilidad
{
    public partial class EstadoCargo : System.Web.UI.Page
    {
        private List<Cargo> lCharges
        {
            get
            {
                return (Session["lCharges"] != null) ? Session["lCharges"] as List<Cargo> : new List<Cargo>();
            }
            set
            {
                Session["lCharges"] = value;
            }
        }        
        
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.chargestatusreport))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
                this.EnableUserControls();
                this.BindGrid();
            }
        }

        protected void gvCargos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvCargos.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        private void EnableUserControls()
        {
            if (this.oUser.idprofile == (int)ProfileEnum.cashier)
            {                
                this.ddlUsuario.Enabled = false;                
            }
            else
            {
                //this.btnEnviar.Visible = this.imbEnviar.Visible = this.imbGuardar.Visible = false;
            }            
        }

        private void LoadControls()
        {
            this.txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlPlan.DataSource = oFacade.GetList("plan");
                this.ddlPlan.DataTextField = "name";
                this.ddlPlan.DataValueField = "name";
                this.ddlPlan.DataBind();
                this.ddlServicio.DataSource = oFacade.GetList("service");
                this.ddlServicio.DataTextField = "name";
                this.ddlServicio.DataValueField = "name";
                this.ddlServicio.DataBind();
                this.ddlUsuario.DataSource = oFacade.GetList("user");
                this.ddlUsuario.DataTextField = "code";
                this.ddlUsuario.DataValueField = "code";
                this.ddlUsuario.DataBind();
                this.ddlEstado.DataSource = oFacade.GetList("status", this.oUser.idprofile);
                this.ddlEstado.DataTextField = "name";
                this.ddlEstado.DataValueField = "code";
                this.ddlEstado.DataBind();
                this.ddlEmpresa.DataSource = oFacade.GetList("company");
                this.ddlEmpresa.DataTextField = "name";
                this.ddlEmpresa.DataValueField = "name";
                this.ddlEmpresa.DataBind();
                this.ddlUsuario.SelectedValue = this.ddlPlan.SelectedValue = this.ddlServicio.SelectedValue = this.ddlEstado.SelectedValue = this.ddlEmpresa.SelectedValue = string.Empty;
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

        private void BindGrid()
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            Cargo oCargo = null;
            try
            {
                oCargo = new Cargo()
                {
                    idadmission = this.txtIngreso.Text,
                    initialdate = (!string.IsNullOrEmpty(this.txtFechaInicio.Text)) ? Convert.ToDateTime(this.txtFechaInicio.Text) : new DateTime(),
                    finaldate = (!string.IsNullOrEmpty(this.txtFechaFin.Text)) ? Convert.ToDateTime(this.txtFechaFin.Text) : new DateTime(),
                    plan = this.ddlPlan.SelectedValue,
                    service = this.ddlServicio.SelectedValue,
                    company = this.ddlEmpresa.SelectedValue,
                    status = (!string.IsNullOrEmpty(this.ddlEstado.SelectedValue)) ? Convert.ToInt32(this.ddlEstado.SelectedValue) : 10,
                };
                if (this.ddlUsuario.Enabled) oCargo.user = this.ddlUsuario.SelectedValue;
                else oCargo.user = string.Join(",", this.oUser.otheruser.ToArray());
                this.gvCargos.DataKeyNames = new string[] { "idadmission", "status", "id", "service", "plan", "value", "company", "eps" };
                this.lCharges = oFacade.GetListReport(oCargo);
                this.gvCargos.DataSource = this.lCharges;
                this.gvCargos.DataBind();
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

        protected string ShowStatus(int iStatus)
        {
            if (this.oUser.idprofile == (int)ProfileEnum.cashier) return Tools.GetStatus(iStatus);
            else if (this.oUser.idprofile == (int)ProfileEnum.invoicingaux) return Tools.GetStatus(iStatus, true);
            else return Tools.GetStatus(iStatus, false);
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtIngreso.Text = string.Empty;
            this.txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.ddlPlan.SelectedValue = this.ddlEmpresa.SelectedValue = this.ddlServicio.SelectedValue = this.ddlEstado.SelectedValue = string.Empty;
            this.EnableUserControls();
            this.BindGrid();
        }

        protected void imbExportar_Click(object sender, ImageClickEventArgs e)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"window.open('ExportarCargos.aspx');", true);
        }
    }
}