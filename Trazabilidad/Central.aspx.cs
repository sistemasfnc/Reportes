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
    public partial class Central : System.Web.UI.Page
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
            }
        }

        protected void checkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (GridViewRow gr in this.gvCargos.Rows)
            {
                (gr.FindControl("chkEnviar") as CheckBox).Checked = (sender as CheckBox).Checked;
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

        protected void imbRecibir_Click(object sender, ImageClickEventArgs e)
        {
            this.UpdateChargeStatus(this.GetSelected((int)ChargeStatus.recieved));
        }

        protected void imbDevolver_Click(object sender, ImageClickEventArgs e)
        {
            this.UpdateChargeStatus(this.GetSelected((int)ChargeStatus.incomplete));
        }

        protected void gvCargos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.idcgharge = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]);
                this.idadmission = this.gvCargos.DataKeys[gr.RowIndex]["idadmission"].ToString();
                this.oEntity = new Cargo()
                {
                    idadmission = this.gvCargos.DataKeys[gr.RowIndex]["idadmission"].ToString(),
                    id = this.idcgharge,
                };
                using (FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion")))
                {
                    this.lSupport = oFacade.GetSupports(this.idcgharge);
                }
                this.imbEnviar.Visible = this.imbGuardar.Visible = (Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["status"]) <= (int)ChargeStatus.saved);
                this.BindSupportGrid();
                this.mpeValidar.Show();
            }
            else if (e.CommandName == "Ver")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.idcgharge = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]);
                this.BindReasonGrid();
                this.mpeMotivos.Show();
            }
        }

        private List<Cargo> GetSelected(int iStatus)
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
                        iduser = this.oUser.id,
                        status = iStatus,
                        lastuser = this.oUser.username
                    };
                    lCargo.Add(oEntity);
                }
            }
            return lCargo;
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
                    status = (int)ChargeStatus.dispatched,
                    notuser = string.Join(",", this.oUser.otheruser.ToArray()),
                    patientdocument = this.txtDocumento.Text,
                    authorization = this.txtAutorizacion.Text,
                };
                this.gvCargos.DataKeyNames = new string[] { "id", "idadmission" };
                this.gvCargos.DataSource = oFacade.GetCharges(oCargo, true);
                this.gvCargos.DataBind();
                this.imbRecibir.Visible = (this.gvCargos.Rows.Count > 0);                
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
            if (lCargo.Count > 0)
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
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Cargos guardados correctamente');", true);
                Response.Redirect("~/Central.aspx");
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Debe seleccionar por lo menos un cargo');", true);
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

        private void BindSupportGrid()
        {
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvSoportes.DataKeyNames = new string[] { "id" };
                this.gvSoportes.DataSource = oFacade.GetList("support");
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

        private void RememberOldValues()
        {
            ArrayList categoryIDList = new ArrayList();
            int index = -1;
            foreach (GridViewRow row in this.gvCargos.Rows)
            {
                index = Convert.ToInt32(this.gvCargos.DataKeys[row.RowIndex].Value);
                bool result = ((CheckBox)row.FindControl("chkEnviar")).Checked;
                if (this.CheckedItems != null)
                    categoryIDList = this.CheckedItems;
                if (result)
                {
                    if (!categoryIDList.Contains(index))
                        categoryIDList.Add(index);
                }
                else
                    categoryIDList.Remove(index);
            }
            if (categoryIDList != null && categoryIDList.Count > 0)
                this.CheckedItems = categoryIDList;
        }

        private void RePopulateValues()
        {
            ArrayList categoryIDList = this.CheckedItems;
            if (categoryIDList != null && categoryIDList.Count > 0)
            {
                foreach (GridViewRow row in this.gvCargos.Rows)
                {
                    int index = Convert.ToInt32(this.gvCargos.DataKeys[row.RowIndex].Value);
                    if (categoryIDList.Contains(index))
                    {
                        CheckBox myCheckBox = (CheckBox)row.FindControl("chkEnviar");
                        myCheckBox.Checked = true;
                    }
                }
            }
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
                if (oEntity != null) return (oEntity.id == 7 || oEntity.id == 8);
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

        protected void imbEnviar_Click(object sender, ImageClickEventArgs e)
        {
            this.SaveSupport((int)ChargeStatus.recieved);
        }

        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            this.SaveSupport((int)ChargeStatus.recieved);
        }

        protected void chkSoporte_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            GridViewRow gr = chk.NamingContainer as GridViewRow;
            int id = Convert.ToInt32(this.gvSoportes.DataKeys[gr.RowIndex]["id"]);
            (gr.FindControl("txtObservacion") as TextBox).Visible = ((id == 7 || id == 8) && chk.Checked);
            this.mpeValidar.Show();
        }

        private void SaveSupport(int status)
        {
            this.gvCargos.AllowPaging = false;
            this.RePopulateValues();
            if (this.ValidateGrid())
            {
                this.SaveCharge(status);
                this.DeleteCharges();
                foreach (GridViewRow gr in this.gvSoportes.Rows)
                {
                    if ((gr.FindControl("chkSoporte") as CheckBox).Checked)
                    {
                        Support oEntity = new Support()
                        {
                            idcharge = this.idcgharge,
                            observation = (gr.FindControl("txtObservacion") as TextBox).Text,
                            id = Convert.ToInt32(this.gvSoportes.DataKeys[gr.RowIndex]["id"]),
                        };
                        using (FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion")))
                        {
                            oFacade.InsertSupports(oEntity);
                        }
                    }
                }
                ScriptManager.RegisterStartupScript(this, this.GetType(), "", @"alert('Soportes agregados correctamente');", true);
                this.gvCargos.AllowPaging = true;
                this.RememberOldValues();
                this.BindGrid();
                this.RePopulateValues();
                this.mpeValidar.Hide();
                //Response.Redirect("~/Listado.aspx");
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Debe seleccionar por lo menos un soporte');", true);
                this.mpeValidar.Show();
            }
        }

        private bool ValidateGrid()
        {
            bool flag = false;
            foreach (GridViewRow gr in this.gvSoportes.Rows)
            {
                if ((gr.FindControl("chkSoporte") as CheckBox).Checked)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        private void SaveCharge(int status)
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.oEntity.status = status;
                this.oEntity.iduser = this.oUser.id;
                this.oEntity.lastuser = this.oUser.username;
                oFacade.CreateCharge(this.oEntity);
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

        private void DeleteCharges()
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                oFacade.DeleteSupports(this.idcgharge);
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

        protected void imbFacturar_Click(object sender, ImageClickEventArgs e)
        {
            List<Cargo> lCargo = this.GetSelected((int)ChargeStatus.readytoinvoice);
            this.UpdateChargeStatus(lCargo);            
        }

        private void BindReasonGrid()
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvMotivos.DataKeyNames = new string[] { "id" };
                this.gvMotivos.DataSource = oFacade.GetReasons(this.idcgharge);
                this.gvMotivos.DataBind();
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

        protected void gvCargos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {            
            this.gvCargos.PageIndex = e.NewPageIndex;
            this.RememberOldValues();
            this.BindGrid();
            this.RePopulateValues();
        }

        protected void imbTramite_Click(object sender, ImageClickEventArgs e)
        {
            this.UpdateChargeStatus(this.GetSelected((int)ChargeStatus.intreatment));
        }

        protected void imbTramite_Click1(object sender, ImageClickEventArgs e)
        {

        }
    }
}