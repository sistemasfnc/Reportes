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
    public partial class ReporteRHB : System.Web.UI.Page
    {
        #region Propiedades privadas

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

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                /*if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.entryreception))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }*/
                this.LoadControls();
                this.BindGrid();
            }
        }

        /// <summary>
        /// Método para cargar los controles 
        /// </summary>
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

        /// <summary>
        /// Método
        /// </summary>
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
                    //eps = this.txtEPS.Text,
                    service = this.ddlServicio.SelectedValue,
                    status = (int)ChargeStatus.dispatched,                    
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
    }
}