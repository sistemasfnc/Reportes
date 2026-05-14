using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Trazabilidad
{
    public partial class ExportarLogCargo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["lReturn"] != null)
            {
                Response.Charset = string.Empty;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.ContentType = "application/vnd.ms-excel";
                System.IO.StringWriter tw = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(tw);
                System.Web.UI.HtmlControls.HtmlForm form = new System.Web.UI.HtmlControls.HtmlForm();
                Response.AddHeader("content-disposition", "attachment; filename=logcargo.xls");
                this.gvCargos.DataSource = Session["lCargo"];
                this.gvCargos.DataBind();
                form.Controls.Add(this.gvCargos);
                this.Controls.Add(form);
                form.RenderControl(hw);
                Response.Clear();
                Response.Buffer = true;
                Response.ContentEncoding = System.Text.Encoding.Default;                
                Response.Write(tw.ToString());
                HttpContext.Current.Response.End();
            }
        }
    }
}