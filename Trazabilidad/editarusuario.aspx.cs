using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Entity;
using Facade;
using EventLog;
using Config;
using Utils;

namespace Trazabilidad
{
    public partial class editarusuario : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        private int iduser
        {
            get { return (Session["iduser"] != null) ? Convert.ToInt32(Session["iduser"]) : 0; }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.createuser) && this.iduser == 0)
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                else if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.edituser) && this.iduser != 0)
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();                
            }
        }

        private void LoadControls()
        {

            FacadeSecurity oFacade = new FacadeSecurity(Configuration.GetStringValue("FNCFacturacion"));
            FacadeCosto oFacadeNova = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlPerfil.DataSource = oFacade.GetProfiles();
                this.ddlPerfil.DataValueField = "idprofile";
                this.ddlPerfil.DataTextField = "name";
                this.ddlPerfil.DataBind();
                this.ddlUsuario.DataSource = oFacade.GetGestorUser();
                this.ddlUsuario.DataTextField = "code";
                this.ddlUsuario.DataValueField = "code";
                this.ddlUsuario.DataBind();
                this.ddlUsuario.SelectedValue = string.Empty;
                this.ddlCentro.DataSource = oFacadeNova.GetCosts();
                this.ddlCentro.DataTextField = "name";
                this.ddlCentro.DataValueField = "code";                                
                this.ddlCentro.DataBind();
                this.ddlCentro.SelectedValue = string.Empty;
                if (this.iduser != 0)
                {
                    using (FacadeUser oFacadeUser = new FacadeUser(Configuration.GetStringValue("FNCFacturacion")))
                    {
                        User oUser = oFacadeUser.Get(new User() { id = this.iduser });
                        this.txtUsuario.Text = oUser.username;
                        this.txtNombre.Text = oUser.firstname;
                        this.txtApellido.Text = oUser.lastname;
                        this.ddlPerfil.SelectedValue = oUser.idprofile.ToString();
                        for (int i = 0; i < this.ddlUsuario.Items.Count; i++)
                        {
                            this.ddlUsuario.Items[i].Selected = (oUser.otheruser.Contains(this.ddlUsuario.Items[i].Value));
                        }
                        for (int i = 0; i < this.ddlCentro.Items.Count; i++)
                        {
                            this.ddlCentro.Items[i].Selected = oUser.lCost.Exists(x => x.scode == this.ddlCentro.Items[i].Value);
                        }
                        this.txtEmail.Text = oUser.email;
                        this.btnBuscar.Visible = false;
                    }                    
                }
                else
                {
                    this.txtUsuario.Enabled = this.txtNombre.Enabled = this.txtApellido.Enabled = false;
                    this.BindGrid();
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
                oFacadeNova.Dispose();
                oFacadeNova = null;
            }
        }

        private void BindGrid()
        {
            LdapAuthentication oLdap = new LdapAuthentication("LDAP://FNC");
            List<User> lUser = new List<User>();
            try
            {                
                lUser.AddRange(oLdap.GetUsers("Facturacion"));
                lUser.AddRange(oLdap.GetUsers("Personal Administrativo"));                
                lUser.AddRange(oLdap.GetUsers("Coordinadores Administrativos"));
                lUser.AddRange(oLdap.GetUsers("Coordinadores Administracion"));
                lUser.AddRange(oLdap.GetUsers("Auxiliar Administrativo"));
                lUser.AddRange(oLdap.GetUsers("Jefes Area"));
                lUser.AddRange(oLdap.GetUsers("Quimico Farmaceutico"));
                lUser.AddRange(oLdap.GetUsers("Jefes y Coordinadores de Area"));
                lUser.AddRange(oLdap.GetUsers("Asis_Farm_E"));
                //this.gvUsuarios.DataSource = lUser.Select(x => x.username).Distinct().ToList();
                this.gvUsuarios.DataSource = lUser;
                this.gvUsuarios.DataKeyNames = new string[] { "username", "firstname", "lastname", "email" };
                this.gvUsuarios.DataBind();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oLdap = null;
            }
        }

        protected void imbAceptar_Click(object sender, ImageClickEventArgs e)
        {
            FacadeSecurity oFacade = new FacadeSecurity(Configuration.GetStringValue("FNCFacturacion"));
            User oUser = null;
            try
            {
                oUser = new User()
                {
                    username = this.txtUsuario.Text,
                    firstname = this.txtNombre.Text,
                    lastname = this.txtApellido.Text,
                    email = this.txtEmail.Text,
                    idprofile = Convert.ToInt32(this.ddlPerfil.SelectedValue),
                    otheruser = this.GetUsers(),
                    id = this.iduser,
                    //costcenter = this.ddlCentro.SelectedValue,
                    lCost = this.GetCosts(),
                };
                oFacade.CreateUser(oUser);
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
            if (this.iduser == 0)
                ClientScript.RegisterStartupScript(this.GetType(), "", @"<script>alert('Usuario creado correctamente');</script>");
            else
                ClientScript.RegisterStartupScript(this.GetType(), "", @"<script>alert('Usuario editado correctamente');</script>");
            Response.Redirect("~/usuarios.aspx");
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.mpeUsuarios.Show();
        }

        protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Seleccionar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.txtUsuario.Text = this.gvUsuarios.DataKeys[gr.RowIndex]["username"].ToString();
                this.txtNombre.Text = this.gvUsuarios.DataKeys[gr.RowIndex]["firstname"].ToString();
                this.txtApellido.Text = this.gvUsuarios.DataKeys[gr.RowIndex]["lastname"].ToString();
                this.txtEmail.Text = this.gvUsuarios.DataKeys[gr.RowIndex]["email"].ToString();
            }
            this.mpeUsuarios.Hide();
        }

        protected void gvUsuarios_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvUsuarios.PageIndex = e.NewPageIndex;
            this.BindGrid();
            this.mpeUsuarios.Show();
        }

        protected void imbCancelar_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/usuarios.aspx");
        }

        private List<string> GetUsers()
        {
            List<string> lUser = new List<string>();
            foreach (ListItem item in this.ddlUsuario.Items)
            {
                if (item.Selected) lUser.Add(item.Value);
            }
            return lUser;
        }

        private List<CostUser> GetCosts()
        {
            List<CostUser> lCost = new List<CostUser>();
            foreach (ListItem item in this.ddlCentro.Items)
            {
                if (item.Selected) lCost.Add(new CostUser() { scode = item.Value });
            }
            return lCost;
        }
    }
}