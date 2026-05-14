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

namespace Trazabilidad
{
    public partial class RecibirDevolucion : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        private int idcgharge
        {
            get { return (ViewState["id"] != null) ? Convert.ToInt32(ViewState["id"]) : 0; }
            set { ViewState["id"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.returnreception))
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
            this.txtEmpresa.Text = this.txtIngreso.Text = string.Empty;
            this.txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.ddlPlan.SelectedValue = this.ddlServicio.SelectedValue = string.Empty;
            this.BindGrid();
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
                this.ddlPlan.SelectedValue = this.ddlServicio.SelectedValue = string.Empty;                
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
                    status = (int)ChargeStatus.returned,
                };
                oCargo.user = string.Join(",", this.oUser.otheruser.ToArray());
                this.gvCargos.DataKeyNames = new string[] { "id", "idadmission" };
                this.gvCargos.DataSource = oFacade.GetCharges(oCargo, true);
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
                oCargo = null;
            }
        }

        protected void imbRecibir_Click(object sender, ImageClickEventArgs e)
        {
            List<Cargo> lCargo = new List<Cargo>();
            Cargo oEntity = null;
            foreach (GridViewRow gr in this.gvCargos.Rows)
            {
                if ((gr.FindControl("chkEnviar") as CheckBox).Checked)
                {
                    oEntity = new Cargo()
                    {
                        id = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]),
                        status = (int)ChargeStatus.recievedreturned,
                        iduser = this.oUser.id,
                        lastuser = this.oUser.username
                    };
                    lCargo.Add(oEntity);
                }
            }
            if (lCargo.Count == 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Debe seleccionar por lo menos un cargo');", true);
            }
            else
            {
                this.UpdateChargeStatus(lCargo);
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Cargos recibidos correctamente');", true);
                Response.Redirect("~/RecibirDevolucion.aspx");
            }
        }

        private void UpdateChargeStatus(List<Cargo> lCargo)
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                oFacade.ChangeStatus(lCargo);
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

        private void BindReasonGrid()
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvSoportes.DataKeyNames = new string[] { "id" };
                this.gvSoportes.DataSource = oFacade.GetReasons(this.idcgharge);
                this.gvSoportes.DataBind();
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

        protected void gvCargos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Ver")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.idcgharge = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]);
                this.BindReasonGrid();
                this.mpeValidar.Show();
            }
        }
    }
}