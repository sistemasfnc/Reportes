using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Entity;

namespace Trazabilidad.controls
{
    public partial class menu : System.Web.UI.UserControl
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.lblUsuario.Text = oUser.username;
        }
    }
}