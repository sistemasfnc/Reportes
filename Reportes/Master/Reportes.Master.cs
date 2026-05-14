using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Reportes
{
    public partial class Reportes : System.Web.UI.MasterPage
    {
        private int userid
        {
            get { return (Session["userid"] != null) ? Convert.ToInt32(Session["userid"]) : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.userid == 0)
            {
                Response.Redirect("~/Default.aspx");
            }
        }
    }
}