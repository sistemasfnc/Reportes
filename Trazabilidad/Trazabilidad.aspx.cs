using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Entity;
using Facade;
using Config;
using Utils;
using EventLog;

namespace Trazabilidad
{
    public partial class Trazabilidad : System.Web.UI.Page
    {        
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        private List<Cargo> lCharges
        {
            get { return (ViewState["lCharges"] != null) ? ViewState["lCharges"] as List<Cargo> : new List<Cargo>(); }
            set { ViewState["lCharges"] = value; }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.entryreport))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
            }
        }

        protected void imbBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindListView();
        }

        protected void lvIngresos_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            (this.lvIngresos.FindControl("DataPager1") as DataPager).SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            this.BindListView();
        }

        private void LoadControls()
        {
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlEmpresa.DataSource = oFacade.GetList("company");
                this.ddlEmpresa.DataTextField = "name";
                this.ddlEmpresa.DataValueField = "name";
                this.ddlEmpresa.DataBind();
                this.ddlEmpresa.SelectedValue = string.Empty;
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

        private void BindListView()
        {
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.lCharges = oFacade.GetChargesLog(this.txtIngreso.Text, "01001");
                this.lvIngresos.DataKeyNames = new string[] { "id" };
                this.lvIngresos.DataSource = this.lCharges;
                this.lvIngresos.DataBind();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
        }

        protected string GetStatusDate(object id, object status)
        {
            if (id == null) return string.Empty;
            Cargo oCargo = lCharges.Find(x => x.id == Convert.ToInt32(id));
            RegistroCargo oRegistro = null;
            if ((int)status == (int)ChargeStatus.recieved) oRegistro = oCargo.lLog.Find(x => x.idstatus >= 2 && x.idstatus <= 4);
            else if ((int)status == (int)ChargeStatus.incomplete) oRegistro = oCargo.lLog.Find(x => x.idstatus <= (int)status);
            else oRegistro = oCargo.lLog.Find(x => x.idstatus == (int)status);
            return (oRegistro != null) ? oRegistro.date.ToString("dd/MM/yyyy") : string.Empty;
        }

        protected void imbSoportes_Click(object sender, ImageClickEventArgs e)
        {
            this.mpeValidar.Show();
            this.gvSoportes.Visible = true;
            this.gvMotivos.Visible = false;
            int idcharge = Convert.ToInt32(((ImageButton)sender).CommandArgument);
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvSoportes.DataSource = oFacade.GetSupports(idcharge);
                this.gvSoportes.DataBind();
                this.lblError.Visible = (this.gvSoportes.Rows.Count == 0);
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

        protected void imbMotivos_Click(object sender, ImageClickEventArgs e)
        {
            this.mpeValidar.Show();
            this.gvSoportes.Visible = false;
            this.gvMotivos.Visible = true;
            int idcharge = Convert.ToInt32(((ImageButton)sender).CommandArgument);
            FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvMotivos.DataSource = oFacade.GetReasons(idcharge, false);
                this.gvMotivos.DataBind();
                this.lblError.Visible = (this.gvMotivos.Rows.Count == 0);
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