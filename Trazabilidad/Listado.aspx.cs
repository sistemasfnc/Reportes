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
using System.Collections;

namespace Trazabilidad
{
    public partial class Listado : System.Web.UI.Page
    {
        #region Propiedades privadas

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

        private string service
        {
            get { return (ViewState["service"] != null) ? ViewState["service"].ToString() : string.Empty; }
            set { ViewState["service"] = value; }
        }

        private string plan
        {
            get { return (ViewState["plan"] != null) ? ViewState["plan"].ToString() : string.Empty; }
            set { ViewState["plan"] = value; }
        }

        private string company
        {
            get { return (ViewState["company"] != null) ? ViewState["company"].ToString() : string.Empty; }
            set { ViewState["company"] = value; }
        }

        private string eps
        {
            get { return (ViewState["eps"] != null) ? ViewState["eps"].ToString() : string.Empty; }
            set { ViewState["eps"] = value; }
        }

        private decimal value
        {
            get { return (ViewState["value"] != null) ? Convert.ToDecimal(ViewState["value"]) : 0; }
            set { ViewState["value"] = value; }
        }

        private Cargo oEntity
        {
            get { return (ViewState["oEntity"] != null) ? ViewState["oEntity"] as Cargo : new Cargo(); }
            set { ViewState["oEntity"] = value; }
        }

        private User oUser
        {
            get { return Session["oUser"] as User; }
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

        #endregion

        #region Eventos Controles

        protected void Page_Load(object sender, EventArgs e)
        {            
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.entrylist))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
                this.EnableUserControls();
                this.BindGrid();
            }
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtIngreso.Text = this.txtDocumento.Text = this.txtAutorizacion.Text = string.Empty;
            this.txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.ddlPlan.SelectedValue = this.ddlEmpresa.SelectedValue = this.ddlServicio.SelectedValue = this.ddlEstado.SelectedValue = string.Empty;
            this.EnableUserControls();
            this.BindGrid();
        }

        protected void checkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (GridViewRow gr in this.gvCargos.Rows)
            {
                (gr.FindControl("chkEnviar") as CheckBox).Checked = (sender as CheckBox).Checked;
            }
        }

        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            CheckBox chk = null;
            string idadmission = string.Empty;
            //int iStatus = 0;
            List<Cargo> lAdmission = new List<Cargo>();
            bool flag = true;
            Cargo oEntity = null;
            foreach (GridViewRow gr in this.gvCargos.Rows)
            {
                chk = gr.FindControl("chkEnviar") as CheckBox;
                if (chk.Checked)
                {                    
                    oEntity = new Cargo()
                    {                        
                        status = (int)Entity.ChargeStatus.dispatched,
                        id = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]),
                        iduser = this.oUser.id,
                        adding = (this.gvCargos.DataKeys[gr.RowIndex]["adding"] != null) ? Convert.ToDecimal(this.gvCargos.DataKeys[gr.RowIndex]["adding"]) : 0,
                        idadmission = this.gvCargos.DataKeys[gr.RowIndex]["idadmission"].ToString(),
                        value = Convert.ToDecimal(this.gvCargos.DataKeys[gr.RowIndex]["value"]),
                        service = this.gvCargos.DataKeys[gr.RowIndex]["service"].ToString(),
                        plan = this.gvCargos.DataKeys[gr.RowIndex]["plan"].ToString(),
                        company = this.gvCargos.DataKeys[gr.RowIndex]["company"].ToString(),
                        eps = this.gvCargos.DataKeys[gr.RowIndex]["eps"].ToString(),
                        date = Convert.ToDateTime(this.gvCargos.DataKeys[gr.RowIndex]["date"]),
                        costcenter = this.gvCargos.DataKeys[gr.RowIndex]["costcenter"].ToString(),
                        subcenter = this.gvCargos.DataKeys[gr.RowIndex]["subcenter"].ToString(),
                        costname = this.gvCargos.DataKeys[gr.RowIndex]["costname"].ToString(),
                        subcentername = this.gvCargos.DataKeys[gr.RowIndex]["subcentername"].ToString(),
                        authorization = this.gvCargos.DataKeys[gr.RowIndex]["authorization"].ToString(),
                        patientdocument = this.gvCargos.DataKeys[gr.RowIndex]["patientdocument"].ToString(),
                        patientname = this.gvCargos.DataKeys[gr.RowIndex]["patientname"].ToString(),
                        patientsurname = this.gvCargos.DataKeys[gr.RowIndex]["patientsurname"].ToString(),
                        surplus = (this.gvCargos.DataKeys[gr.RowIndex]["surplus"] != null) ? Convert.ToDecimal(this.gvCargos.DataKeys[gr.RowIndex]["surplus"]) : 0,
                        user = this.gvCargos.DataKeys[gr.RowIndex]["user"].ToString(),       
                        lastuser = this.oUser.username,
                        documenttype = this.gvCargos.DataKeys[gr.RowIndex]["documenttype"].ToString(),
                        iqty = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["iqty"]),
                        iline = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["iline"]),
                        scharge = this.gvCargos.DataKeys[gr.RowIndex]["scharge"].ToString(),
                        ssource = this.gvCargos.DataKeys[gr.RowIndex]["ssource"].ToString(),
                    };                    
                    lAdmission.Add(oEntity);
                }
            }
            if (flag)
            {
                this.SendAdmissions(lAdmission);
                Response.Redirect("~/Listado.aspx");
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Debe seleccionar por lo menos un cargo');", true);
            }
        }
        
        protected void btnEnviar_Click1(object sender, ImageClickEventArgs e)
        {
            btnEnviar_Click(sender, null);
        }     
  
        protected bool EnableControl(object status)
        {
            return ((int)status < (int)ChargeStatus.dispatched);
        }

        protected string ShowColor(object status)
        {
            return ((int)status < (int)ChargeStatus.dispatched) ? "White" : "Green";
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
                    adding = (this.gvCargos.DataKeys[gr.RowIndex]["adding"] != null) ? Convert.ToDecimal(this.gvCargos.DataKeys[gr.RowIndex]["adding"]) : 0,
                    idadmission = this.gvCargos.DataKeys[gr.RowIndex]["idadmission"].ToString(),
                    value = Convert.ToDecimal(this.gvCargos.DataKeys[gr.RowIndex]["value"]),
                    service = this.gvCargos.DataKeys[gr.RowIndex]["service"].ToString(),
                    plan = this.gvCargos.DataKeys[gr.RowIndex]["plan"].ToString(),
                    company = this.gvCargos.DataKeys[gr.RowIndex]["company"].ToString(),
                    eps = this.gvCargos.DataKeys[gr.RowIndex]["eps"].ToString(),
                    date = Convert.ToDateTime(this.gvCargos.DataKeys[gr.RowIndex]["date"]),
                    costcenter = this.gvCargos.DataKeys[gr.RowIndex]["costcenter"].ToString(),
                    subcenter = this.gvCargos.DataKeys[gr.RowIndex]["subcenter"].ToString(),
                    costname = this.gvCargos.DataKeys[gr.RowIndex]["costname"].ToString(),
                    subcentername = this.gvCargos.DataKeys[gr.RowIndex]["subcentername"].ToString(),
                    authorization = this.gvCargos.DataKeys[gr.RowIndex]["authorization"].ToString(),
                    patientdocument = this.gvCargos.DataKeys[gr.RowIndex]["patientdocument"].ToString(),
                    patientname = this.gvCargos.DataKeys[gr.RowIndex]["patientname"].ToString(),
                    patientsurname = this.gvCargos.DataKeys[gr.RowIndex]["patientsurname"].ToString(),
                    surplus = (this.gvCargos.DataKeys[gr.RowIndex]["surplus"] != null) ? Convert.ToDecimal(this.gvCargos.DataKeys[gr.RowIndex]["surplus"]) : 0,
                    user = this.gvCargos.DataKeys[gr.RowIndex]["user"].ToString(),
                    lastuser = this.oUser.username,
                    id = this.idcgharge,
                    documenttype = this.gvCargos.DataKeys[gr.RowIndex]["documenttype"].ToString(),
                    iqty = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["iqty"]),
                    iline = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["iline"]),
                    scharge = this.gvCargos.DataKeys[gr.RowIndex]["scharge"].ToString(),
                    ssource = this.gvCargos.DataKeys[gr.RowIndex]["ssource"].ToString(),
                };                                                                                
                using (FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion")))
                {
                    this.lSupport = oFacade.GetSupports(this.idcgharge);
                }
                this.imbEnviar.Visible = this.imbGuardar.Visible = (Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["status"]) <= (int)ChargeStatus.saved);
                this.BindSupportGrid();
                this.mpeValidar.Show();
            }
        }

        protected void chkSoporte_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;            
            GridViewRow gr = chk.NamingContainer as GridViewRow;
            int id = Convert.ToInt32(this.gvSoportes.DataKeys[gr.RowIndex]["id"]);
            (gr.FindControl("txtObservacion") as TextBox).Visible = ((id == 7 || id == 8) && chk.Checked);
            this.mpeValidar.Show();
        }

        protected void imbEnviar_Click(object sender, ImageClickEventArgs e)
        {
            this.SaveSupport((int)ChargeStatus.dispatched);
        }

        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            this.SaveSupport((int)ChargeStatus.saved);
        }

        #endregion

        private void SendAdmissions(List<Cargo> lAdmission)
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                oFacade.ChangeStatus(lAdmission);
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Cargos enviados con exito');", true);
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
            Response.Redirect("~/Listado.aspx");
        }

        private void EnableUserControls()
        {
            if (this.oUser.idprofile == (int)ProfileEnum.cashier)
            {
                //this.ddlUsuario.SelectedValue = this.oUser.otheruser.ToLower();
                this.ddlUsuario.Enabled = false;
                //this.ddlEstado.SelectedValue = ((int)ChargeStatus.incomplete).ToString();
                //this.ddlEstado.Enabled = false;
            }
            else
            {
                this.btnEnviar.Visible = this.imbEnviar.Visible = this.imbGuardar.Visible = false;                
            }
            this.imbEnviar.Visible = this.btnEnviar.Visible = Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.sendcentral);
            this.imbGuardar.Visible = Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.supportsave);
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
                    status = (!string.IsNullOrEmpty(this.ddlEstado.SelectedValue)) ? Convert.ToInt32(this.ddlEstado.SelectedValue) : 0,
                    eps = this.txtEPS.Text,
                    user = (Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.rhbcharges)) ? string.Empty : string.Join("','", this.oUser.otheruser.ToArray()),
                    patientdocument = this.txtDocumento.Text,
                    authorization = this.txtAutorizacion.Text,
                    canceled = 0,
                    invoiced = "N",
                    //costcenter = " AND CodigoCentro NOT IN ('1100', '3500')",
                    iidprofile = this.oUser.idprofile,
                };
                if (Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.rhbcharges))
                {
                    oCargo.service = "933500";
                }
                //if (this.ddlUsuario.Enabled) oCargo.user = this.ddlUsuario.SelectedValue;
                //else 
                //oCargo.user = string.Join(",", this.oUser.otheruser.ToArray());
                this.gvCargos.DataKeyNames = new string[] 
                { 
                    "idadmission", 
                    "status", 
                    "id", 
                    "service", 
                    "plan", 
                    "value", 
                    "company", 
                    "eps",                     
                    "adding", 
                    "surplus", 
                    "costcenter", 
                    "subcenter", 
                    "costname", 
                    "subcentername",
                    "patientname",
                    "patientdocument",
                    "patientsurname",
                    "user",
                    "authorization",
                    "date",
                    "documenttype",
                    "scharge",
                    "iline",
                    "iqty",                    
                    "ssource"
                };
                this.lCharges = oFacade.GetCharges(oCargo);
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

        private void SaveSupport(int status)
        {
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
                this.idcgharge = oFacade.CreateCharge(this.oEntity);
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

        protected void gvCargos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Entity.Cargo oCargo = (Entity.Cargo)e.Row.DataItem;                
                e.Row.BackColor = (oCargo.status < (int)ChargeStatus.dispatched) ? System.Drawing.Color.White: System.Drawing.Color.LightSeaGreen;
            }
        }
    }
}