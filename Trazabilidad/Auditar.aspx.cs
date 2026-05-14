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
    public partial class Auditar : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        private List<Support> lSupport
        {
            get { return (ViewState["lSupport"] != null) ? ViewState["lSupport"] as List<Support> : new List<Support>(); }
            set { ViewState["lSupport"] = value; }
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
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.entryreturn))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
                this.BindGrid();
                this.BindReasonGrid();
            }
        }

        protected void checkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (GridViewRow gr in this.gvCargos.Rows)
            {
                (gr.FindControl("chkEnviar") as CheckBox).Checked = (sender as CheckBox).Checked;
            }
        }

        protected void gvCargos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Devolver")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.idcgharge = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]);
                this.mpeValidar.Show();
            }
            else if (e.CommandName == "Pendiente")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.idcgharge = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]);
                using (FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion")))
                {
                    this.lSupport = oFacade.GetPendings(this.idcgharge);
                }
                this.BindPendingGrid();
                this.mpePendiente.Show();                
            }
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtIngreso.Text = this.txtDocumento.Text = string.Empty;
            this.txtFechaFin.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.ddlUsuario.SelectedValue = this.ddlPlan.SelectedValue = this.ddlServicio.SelectedValue = this.ddlEmpresa.SelectedValue = string.Empty;
            this.BindGrid();
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
                Response.Redirect("~/Auditar.aspx");
            }
        }

        protected void imbFacturar_Click(object sender, ImageClickEventArgs e)
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
                        status = (int)ChargeStatus.readytoinvoice,
                        iduser = this.oUser.id,
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
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Cargos enviados correctamente');", true);
                Response.Redirect("~/Auditar.aspx");
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
                this.ddlEmpresa.DataSource = oFacade.GetList("company");
                this.ddlEmpresa.DataTextField = "name";
                this.ddlEmpresa.DataValueField = "name";
                this.ddlEmpresa.DataBind();
                this.ddlUsuario.SelectedValue = this.ddlPlan.SelectedValue = this.ddlServicio.SelectedValue = this.ddlEmpresa.SelectedValue = string.Empty;
                this.imbGuardar.Visible = Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.sendreturn);
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
                    user = this.ddlUsuario.SelectedValue,
                    status = (int)ChargeStatus.recieved,
                    company = this.ddlEmpresa.SelectedValue,
                    service = this.ddlServicio.SelectedValue,
                    plan = this.ddlPlan.SelectedValue,
                    patientdocument = this.txtDocumento.Text,
                };
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
        
        private void UpdateStatus()
        {
            List<Cargo> lCargo = new List<Cargo>();
            Cargo oCargo = new Cargo() { id = this.idcgharge, status = (int)ChargeStatus.returned, iduser = this.oUser.id, lastuser = this.oUser.username };
            lCargo.Add(oCargo);
            this.UpdateChargeStatus(lCargo);
        }

        private void BindPendingGrid()
        {
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvPendientes.DataSource = oFacade.GetList("pending");
                this.gvPendientes.DataBind();
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

        protected void chkMotivo_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            GridViewRow gr = chk.NamingContainer as GridViewRow;
            (gr.FindControl("txtObservacion") as TextBox).Enabled = chk.Checked;
            this.mpeValidar.Show();
        }

        protected bool CheckSupport(object id)
        {
            if (this.lSupport.Count > 0)
            {
                return (this.lSupport.Find(x => x.id == Convert.ToInt32(id)) != null);
            }
            return false;
        }

        protected bool ViewObservation(object id)
        {
            if (this.lSupport.Count > 0)
            {
                Support oEntity = this.lSupport.Find(x => x.id == Convert.ToInt32(id));
                if (oEntity != null) return (oEntity.id == 7);
            }
            return false;
        }

        protected string GetObservation(object id)
        {
            if (this.lSupport.Count > 0)
            {
                Support oEntity = this.lSupport.Find(x => x.id == Convert.ToInt32(id));
                return (oEntity != null) ? oEntity.observation : string.Empty;
            }
            return string.Empty;
        }

        protected void chkSoporte_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            GridViewRow gr = chk.NamingContainer as GridViewRow;
            int id = Convert.ToInt32(this.gvSoportes.DataKeys[gr.RowIndex]["id"]);
            (gr.FindControl("txtObservacion") as TextBox).Visible = ((id == 7) && chk.Checked);
            this.mpePendiente.Show();
        }

        protected void imbSave_Click(object sender, ImageClickEventArgs e)
        {
            List<Cargo> lCargo = new List<Cargo>();
            Cargo oEntity = new Cargo()
            {
                id = this.idcgharge,
                status = (int)ChargeStatus.invoicedpending,
                iduser = this.oUser.id,
            };
            lCargo.Add(oEntity);
            try
            {
                this.UpdateChargeStatus(lCargo);
                this.InsertPendings();
                this.mpePendiente.Hide();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", "alert('Pendientes almacenados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw; 
            }
            finally
            {
                lCargo = null;
                oEntity = null;
            }            
        }

        private void InsertPendings()
        {
            List<Support> lSupport = new List<Support>();
            Support oEntity = null;
            foreach (GridViewRow gr in this.gvPendientes.Rows)
            {
                if ((gr.FindControl("chkSoporte") as CheckBox).Checked)
                {
                    oEntity = new Support()
                    {
                        idcharge = this.idcgharge,
                        observation = (gr.FindControl("txtObservacion") as TextBox).Text,
                        id = Convert.ToInt32(this.gvSoportes.DataKeys[gr.RowIndex]["id"]),
                    };
                    lSupport.Add(oEntity);                    
                }
            }
            using (FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion")))
            {
                oFacade.InsertPending(lSupport);
            }
        }

        protected void imbCancel_Click(object sender, ImageClickEventArgs e)
        {
            this.mpePendiente.Hide();
        }
    }
}