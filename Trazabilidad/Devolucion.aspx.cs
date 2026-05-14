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
    public partial class Devolucion : System.Web.UI.Page
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
       
        protected void gvCargos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Ver")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.idcgharge = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]);
                this.BindReasonGrid();
                this.imbGuardar.Visible = (this.gvSoportes.Rows.Count > 0);
                this.mpeValidar.Show();                
            }
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
            this.ddlPlan.SelectedValue = this.ddlServicio.SelectedValue = this.ddlEmpresa.SelectedValue = string.Empty;            
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
                this.ddlEmpresa.DataSource = oFacade.GetList("company");
                this.ddlEmpresa.DataTextField = "name";
                this.ddlEmpresa.DataValueField = "name";
                this.ddlEmpresa.DataBind();
                this.ddlPlan.SelectedValue = this.ddlServicio.SelectedValue = this.ddlEmpresa.SelectedValue = string.Empty;
                this.imbGuardar.Visible = Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.returnresponse);
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
                    status = (int)ChargeStatus.recievedreturned,
                    company = this.ddlEmpresa.SelectedValue,
                    service = this.ddlServicio.SelectedValue,
                    plan = this.ddlPlan.SelectedValue,
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

        private void Save(int iStatus)
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            Cargo oEntity = null;
            List<Support> lSupport = new List<Support>();
            Support oSupport = null;
            foreach (GridViewRow gr in this.gvSoportes.Rows)
            {
                if (!string.IsNullOrEmpty((gr.FindControl("txtRespuesta") as TextBox).Text))
                {
                    oSupport = new Support()
                    {
                        id = Convert.ToInt32(this.gvSoportes.DataKeys[gr.RowIndex]["id"]),
                        idcharge = this.idcgharge,
                        response = (gr.FindControl("txtRespuesta") as TextBox).Text,
                    };
                    lSupport.Add(oSupport);
                }
            }
            try
            {
                oFacade.UpdateReasonsResponse(lSupport);
                if (iStatus == (int)ChargeStatus.recieved)
                {
                    oEntity = new Cargo()
                    {
                        id = this.idcgharge,
                        status = iStatus,
                        iduser = this.oUser.id,
                        lastuser = this.oUser.username
                    };
                    oFacade.CreateCharge(oEntity);                
                }                
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
                oEntity = null;
            }            
        }

        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {            
            this.Save((int)ChargeStatus.recieved);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "", @"alert('Cargo enviado correctamente');", true);
            Response.Redirect("~/Devolucion.aspx");
        }

        protected void imbSalvar_Click(object sender, ImageClickEventArgs e)
        {
            this.Save((int)ChargeStatus.recievedreturned);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "", @"alert('Cargo almacenado correctamente');", true);
            Response.Redirect("~/Devolucion.aspx");
        }
    }
}