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
    public partial class CargosFCI : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.chargesfci))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
                this.BindGrid();
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
                    plan = this.ddlPlan.SelectedValue,
                    eps = this.txtEPS.Text,
                    service = this.ddlServicio.SelectedValue,
                    status = (int)ChargeStatus.dispatched,
                    notuser = string.Join(",", this.oUser.otheruser.ToArray()),
                    patientdocument = this.txtDocumento.Text,
                    authorization = this.txtAutorizacion.Text,
                    costcenter = " AND ca_codigocentro IN ('1100', '3500')",
                    canceled = 0,
                    invoiced = "N",
                };
                this.gvCargos.DataKeyNames = new string[] { "id", "idadmission" };
                this.lCharges = oFacade.GetCharges(oCargo, true);
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
            Response.Redirect("~/CargosFCI.aspx");
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
            this.ddlEmpresa.SelectedValue = this.ddlPlan.SelectedValue = this.ddlServicio.SelectedValue = string.Empty;            
            this.BindGrid();
        }

        protected void gvCargos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvCargos.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void imbExportar_Click(object sender, ImageClickEventArgs e)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"window.open('Exportar.aspx');", true);
        }

        protected void checkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (GridViewRow gr in this.gvCargos.Rows)
            {
                (gr.FindControl("chkEnviar") as CheckBox).Checked = (sender as CheckBox).Checked;
            }
        }

        protected void imbEnviar_Click(object sender, ImageClickEventArgs e)
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
                    };
                    lAdmission.Add(oEntity);
                }
            }
            if (flag)
            {
                this.SendAdmissions(lAdmission);
                Response.Redirect("~/CargosFCI.aspx");
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Debe seleccionar por lo menos un cargo');", true);
            }
        }
    }
}