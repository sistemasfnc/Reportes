using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using Entity;
using Utils;
using System.Web.Security;
using Config;

namespace Trazabilidad
{
    public partial class Login : System.Web.UI.Page
    {                
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        private string SetUser(User user)
        {
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, this.txtUsuario.Text, DateTime.Now, DateTime.Now.AddMinutes(60), true, string.Empty);
            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(authCookie);
            return user.spagina;
        }
        protected void btnIngreso_Click(object sender, EventArgs e)
        {
            string adPath = "LDAP://FNC";            
            LdapAuthentication adAuth = new LdapAuthentication(adPath);
            string sUrl = string.Empty;
            try
            {
                //if (true)
                if (adAuth.IsAuthenticated("FNC", this.txtUsuario.Text, this.txtPassword.Text))
                {
                    User oUser = this.GetUser();
                    if (oUser.id == 0)
                    {
                        this.lblError.Text = "El usuario no existe en el sistema, favor comun&iacute;quese con el administrador";                        
                    }
                    else
                    {
                        Session["oUser"] = oUser;
                        if (oUser.idprofile == (int)ProfileEnum.cashier || oUser.idprofile == (int)ProfileEnum.rhbcashier)
                        {
                            this.mpeValidar.Show();
                        }                        
                        else
                        {
                            sUrl = this.SetUser(oUser);
                        }                        
                    }                    
                }
                else
                {
                    this.lblError.Text = "Usuario o contrase&ntilde;a incorrectos.";
                }
            }
            catch (Exception ex)
            {
                EventLog.LogError.WriteError("Facturacion", "Aplicacion", ex);
                this.lblError.Text = "Error de inicio de sesi&oacute;n.";
            }
            if (!string.IsNullOrEmpty(sUrl)) Response.Redirect(sUrl);
        }

        private string GetRedirectURL(int profileid)
        {
            if (profileid == (int)ProfileEnum.cashier)
            {
                return "~/Listado.aspx";
            }
            else if (profileid == (int)ProfileEnum.invoicingaux)
            {
                return "~/Central.aspx";
            }
            else if (profileid == (int)ProfileEnum.director)
            {
                return "~/Trazabilidad.aspx";
            }
            else if (profileid == (int)ProfileEnum.administrator)
            {
                return "~/usuarios.aspx";
            }
            else if (profileid >= (int)ProfileEnum.healthcarecoordinator && profileid <= (int)ProfileEnum.investigationcoordiator)
            {
                return "~/DistribuirCostos.aspx";
            }
            return "~/Listado.aspx";
        }

        private User GetUser()
        {
            FacadeUser oFacade = new FacadeUser(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                return oFacade.Get(new User() { username = this.txtUsuario.Text });
            }
            catch (Exception ex)
            {
                EventLog.LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            (Session["oUser"] as User).idprofile = Convert.ToInt32(this.ddlPerfil.SelectedValue);
            using (FacadeUser oFacade = new FacadeUser(Configuration.GetStringValue("FNCFacturacion")))
            {
                (Session["oUser"] as User).lSecurity = oFacade.GetPermissionsByProfile((Session["oUser"] as User).idprofile);
            }            
            string sUrl = this.SetUser((Session["oUser"] as User));
            Response.Redirect(sUrl);            
        }
    }
}