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
    public partial class Facturas : System.Web.UI.Page
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

        private string invoice
        {
            get { return (ViewState["invoice"] != null) ? ViewState["invoice"].ToString() : string.Empty; }
            set { ViewState["invoice"] = value; }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.invoicelist))
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
                this.ddlUsuario.DataSource = oFacade.GetList("user");
                this.ddlUsuario.DataTextField = "code";
                this.ddlUsuario.DataValueField = "code";
                this.ddlUsuario.DataBind();                
                this.ddlUsuario.SelectedValue = string.Empty;                
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
            FacadeFactura oFacade = new FacadeFactura(Configuration.GetStringValue("FNCFacturacion"));
            Invoice oInvoice = null;
            try
            {
                oInvoice = new Invoice()
                {
                    eps = this.txtEps.Text,
                    status = this.ddlEstado.SelectedValue,
                    invoice = this.txtFactura.Text,
                    initialdate = (!string.IsNullOrEmpty(this.txtFechaInicio.Text)) ? Convert.ToDateTime(this.txtFechaInicio.Text) : new DateTime(),
                    finaldate = (!string.IsNullOrEmpty(this.txtFechaFin.Text)) ? Convert.ToDateTime(this.txtFechaFin.Text) : new DateTime(),
                };
                Session["lFacturas"] = oFacade.GetList(oInvoice);
                this.gvFacturas.DataKeyNames = new string[] { "invoice" };
                this.gvFacturas.DataSource = Session["lFacturas"];
                this.gvFacturas.DataBind();
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
                oInvoice = null;
            }

        }

        private void BindPendingGrid()
        {
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvPendientes.DataSource = oFacade.GetList("pending");
                this.gvPendientes.DataKeyNames = new string[] { "id" };
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

        protected bool ViewField(object status)
        {
            return (status.ToString() == "RD");
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

        protected void gvPendientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Pendiente")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.invoice = this.gvFacturas.DataKeys[gr.RowIndex]["invoice"].ToString();
                using (FacadeFactura oFacade = new FacadeFactura(Configuration.GetStringValue("FNCFacturacion")))
                {
                    this.lSupport = oFacade.GetPendings(this.invoice);
                }
                this.BindPendingGrid();
                this.mpePendiente.Show();
            }
        }

        protected void imbSave_Click(object sender, ImageClickEventArgs e)
        {
            List<Support> lSupport = new List<Support>();
            Support oEntity = null;
            foreach (GridViewRow gr in this.gvPendientes.Rows)
            {
                if ((gr.FindControl("chkSoporte") as CheckBox).Checked)
                {
                    oEntity = new Support()
                    {
                        code = this.invoice,
                        observation = (gr.FindControl("txtObservacion") as TextBox).Text,
                        id = Convert.ToInt32(this.gvPendientes.DataKeys[gr.RowIndex]["id"]),
                    };
                    lSupport.Add(oEntity);
                }
            }
            using (FacadeFactura oFacade = new FacadeFactura(Configuration.GetStringValue("FNCFacturacion")))
            {
                oFacade.InsertPending(lSupport);
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", "alert('Pendientes almacenados correctamente');", true);
                this.mpePendiente.Hide();
            }
            
        }

        protected void imbCancel_Click(object sender, ImageClickEventArgs e)
        {
            this.mpePendiente.Hide();
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        protected void chkSoporte_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            GridViewRow gr = chk.NamingContainer as GridViewRow;
            int id = Convert.ToInt32(this.gvPendientes.DataKeys[gr.RowIndex]["id"]);
            (gr.FindControl("txtObservacion") as TextBox).Visible = ((id == 7) && chk.Checked);
            this.mpePendiente.Show();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.ddlEstado.SelectedValue = this.ddlUsuario.SelectedValue = string.Empty;
            this.txtFactura.Text = this.txtEps.Text = string.Empty;
            this.txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.BindGrid();
        }

        protected void gvFacturas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvFacturas.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void imbExportar_Click(object sender, ImageClickEventArgs e)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"window.open('ExportaFactura.aspx');", true);
        }
    }
}