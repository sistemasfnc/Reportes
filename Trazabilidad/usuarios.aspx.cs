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

namespace Trazabilidad
{
    public partial class usuarios : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.listuser))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();
                this.BindGrid();
            }
        }

        protected void imbAgregar_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/editarusuario.aspx");
        }
        
        protected void gvUsuarios_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvUsuarios.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            this.BindGrid();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtApellido.Text = this.txtUsuario.Text = this.txtNombre.Text = string.Empty;
            this.ddlPerfil.SelectedValue = string.Empty;
            this.BindGrid();
        }

        protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                Session["iduser"] = Convert.ToInt32(this.gvUsuarios.DataKeys[gr.RowIndex]["id"]);
                Response.Redirect("~/editarusuario.aspx");
            }
        }

        private void BindGrid()
        {
            FacadeUser oFacade = new FacadeUser(Configuration.GetStringValue("FNCFacturacion"));
            string[] datakeys = new string[] { "id" };
            User oEntity = null;
            try
            {
                oEntity = new User()
                {
                    username = this.txtUsuario.Text,
                    firstname = this.txtNombre.Text,
                    lastname = this.txtApellido.Text,
                };
                if (!string.IsNullOrEmpty(this.ddlPerfil.SelectedValue)) oEntity.idprofile = Convert.ToInt32(this.ddlPerfil.SelectedValue);
                this.gvUsuarios.DataKeyNames = datakeys;
                this.gvUsuarios.DataSource = oFacade.GetAll(oEntity);
                this.gvUsuarios.DataBind();
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

        private void LoadControls()
        {
            FacadeSecurity oFacade = new FacadeSecurity(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlPerfil.DataSource = oFacade.GetProfiles();
                this.ddlPerfil.DataValueField = "idprofile";
                this.ddlPerfil.DataTextField = "name";
                this.ddlPerfil.DataBind();
                this.ddlPerfil.SelectedValue = "0";                
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