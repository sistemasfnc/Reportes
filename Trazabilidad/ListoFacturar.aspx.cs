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
using System.Collections;

namespace Trazabilidad
{
    public partial class ListoFacturar : System.Web.UI.Page
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

        private string idadmission
        {
            get { return (ViewState["idadmission"] != null) ? ViewState["idadmission"].ToString() : string.Empty; }
            set { ViewState["idadmission"] = value; }
        }

        private Cargo oEntity
        {
            get { return (ViewState["oEntity"] != null) ? ViewState["oEntity"] as Cargo : new Cargo(); }
            set { ViewState["oEntity"] = value; }
        }

        private List<Support> lSupport
        {
            get { return (ViewState["lSupport"] != null) ? ViewState["lSupport"] as List<Support> : new List<Support>(); }
            set { ViewState["lSupport"] = value; }
        }

        private ArrayList CheckedItems
        {
            get { return (ViewState["CheckedItems"] == null) ? null : (ArrayList)ViewState["CheckedItems"]; }
            set { ViewState["CheckedItems"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.entryreception))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
                this.BindGrid();
                this.BindReasonGrid();            
            }
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtIngreso.Text = this.txtDocumento.Text = this.txtAutorizacion.Text = string.Empty;
            this.txtFechaFin.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.ddlUsuario.SelectedValue = string.Empty;
            this.BindGrid();
        }

        protected void gvCargos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.idcgharge = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]);
                this.mpeValidar.Show();
            }
        }

        private void LoadControls()
        {
            this.txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlUsuario.DataSource = oFacade.GetList("user");
                this.ddlUsuario.DataTextField = "code";
                this.ddlUsuario.DataValueField = "code";
                this.ddlUsuario.DataBind();
                this.ddlUsuario.SelectedValue = string.Empty;
                this.ddlPlan.DataSource = oFacade.GetList("plan");
                this.ddlPlan.DataTextField = "name";
                this.ddlPlan.DataValueField = "name";
                this.ddlPlan.DataBind();
                this.ddlServicio.DataSource = oFacade.GetList("service");
                this.ddlServicio.DataTextField = "name";
                this.ddlServicio.DataValueField = "name";
                this.ddlServicio.DataBind();
                this.ddlPlan.SelectedValue = this.ddlPlan.SelectedValue = string.Empty;
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

        private void UpdateStatus()
        {
            List<Cargo> lCargo = new List<Cargo>();
            Cargo oCargo = new Cargo() { id = this.idcgharge, status = (int)ChargeStatus.returned, iduser = this.oUser.id, lastuser = this.oUser.username };
            lCargo.Add(oCargo);
            this.UpdateChargeStatus(lCargo);
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
                    user = string.Join(",", this.GetUsers()),
                    plan = this.ddlPlan.SelectedValue,
                    eps = this.txtEPS.Text,
                    service = this.ddlServicio.SelectedValue,
                    status = (int)ChargeStatus.readytoinvoice,
                    notuser = string.Join(",", this.oUser.otheruser.ToArray()),
                    patientdocument = this.txtDocumento.Text,
                    authorization = this.txtAutorizacion.Text,
                };
                this.gvCargos.DataKeyNames = new string[] { "id", "idadmission" };
                this.gvCargos.DataSource = oFacade.GetCharges(oCargo);
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

        private string[] GetUsers()
        {
            List<string> lUsers = new List<string>();
            foreach (ListItem item in this.ddlUsuario.Items)
            {
                if (item.Selected)
                {
                    lUsers.Add(item.Value);
                }
            }
            return lUsers.ToArray();
        }

        protected void chkMotivo_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            GridViewRow gr = chk.NamingContainer as GridViewRow;
            (gr.FindControl("txtObservacion") as TextBox).Enabled = chk.Checked;
            this.mpeValidar.Show();
        }

        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            List<Support> lEntity = new List<Support>();
            Support oEntity = null;
            foreach (GridViewRow gr in this.gvSoportes.Rows)
            {
                if ((gr.FindControl("chkMotivo") as CheckBox).Checked)
                {
                    oEntity = new Support()
                    {
                        idcharge = this.idcgharge,
                        observation = (gr.FindControl("txtObservacion") as TextBox).Text,
                        id = Convert.ToInt32(this.gvSoportes.DataKeys[gr.RowIndex]["id"]),
                    };
                    lEntity.Add(oEntity);
                }
            }
            if (lEntity.Count == 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Debe seleccionar por lo menos un motivo');", true);
                this.mpeValidar.Show();
            }
            else
            {
                this.SaveReasons(lEntity);
                this.UpdateStatus();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Cargo devuelto al cajero correctamente');", true);
                this.BindGrid();
                //Response.Redirect("~/ListoFacturar.aspx");
            }
        }

        private void SaveReasons(List<Support> lEntity)
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                oFacade.InsertReasons(lEntity);
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
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvSoportes.DataKeyNames = new string[] { "id" };
                this.gvSoportes.DataSource = oFacade.GetList("reason");
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
    }
}