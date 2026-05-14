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

namespace Nomina.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterHyperLink.NavigateUrl = "Register";            

            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                RegisterHyperLink.NavigateUrl += "?ReturnUrl=" + returnUrl;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string adPath = "LDAP://FNC";
            LdapAuthentication adAuth = new LdapAuthentication(adPath);
            try
            {
                if (true == adAuth.IsAuthenticated("FNC", this.pnlLogin.UserName, this.pnlLogin.Password))
                {
                    string groups = adAuth.GetGroups();
                    //Create the ticket, and add the groups.
                    //bool isCookiePersistent = this. RememberMe.Checked;
                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1,
                              this.pnlLogin.UserName, DateTime.Now, DateTime.Now.AddMinutes(60), true, groups);
                    //Encrypt the ticket.
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    //Create a cookie, and then add the encrypted ticket to the cookie as data.
                    HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    /*if (true == isCookiePersistent)
                        authCookie.Expires = authTicket.Expiration;*/
                    //Add the cookie to the outgoing cookies collection.
                    Response.Cookies.Add(authCookie);
                    //You can redirect now.
                    Response.Redirect(FormsAuthentication.GetRedirectUrl(this.pnlLogin.UserName, false));
                    //Response.Redirect("~/Default.aspx");
                }
                else
                {
                    this.pnlLogin.FailureText = "Usuario o contrase&ntilde;a incorrectos.";
                }
            }
            catch (Exception ex)
            {
                EventLog.LogError.WriteError("Nomina", "Aplicacion", ex);
                this.pnlLogin.FailureText = "Error de inicio de sesion.";
            }
            
            //User oEntity = new User() { username = this.pnlLogin.UserName, password = Tools.SHA256Crypt(this.pnlLogin.Password) };
            //FacadeUser oFacade = new FacadeUser();
            //try
            //{
            //    oEntity = oFacade.Get(oEntity);
            //    if (oEntity.id == 0)
            //    {
            //        this.pnlLogin.FailureText = "Nombre de usuario o contrase&ntilde;a incorrectos";
            //    }
            //    else
            //    {
            //        oFacade.UpdateLastLogin(oEntity.id, DateTime.Now);
            //        Session["User"] = oEntity;                    
            //        Response.Redirect("~/Default.aspx");
            //    }
            //}
            //catch (ApplicationException ex)
            //{
            //    throw;
            //}
            //finally
            //{
            //    oFacade.Dispose();
            //    oFacade = null;
            //    oEntity = null;
            //}
        }
    }
}