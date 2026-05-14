using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utils;
using Entity;

namespace Trazabilidad
{
    public partial class ExportarCargos : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["lCharges"] != null)
            {
                Response.Charset = string.Empty;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.ContentType = "application/vnd.ms-excel";
                System.IO.StringWriter tw = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(tw);
                System.Web.UI.HtmlControls.HtmlForm form = new System.Web.UI.HtmlControls.HtmlForm();
                Response.AddHeader("content-disposition", "attachment; filename=cargos.xls");
                this.gvCargos.DataSource = Session["lCharges"];
                this.gvCargos.DataBind();
                form.Controls.Add(this.gvCargos);
                this.Controls.Add(form);
                form.RenderControl(hw);
                Response.Clear();
                Response.Buffer = true;
                Response.ContentEncoding = System.Text.Encoding.Default;
                string style = @"<style> .text { mso-number-format:\@; } </style> ";
                Response.Write(style);
                Response.Write(tw.ToString());
                HttpContext.Current.Response.End();
            }
        }

        protected string ShowStatus(int iStatus)
        {
            if (this.oUser.idprofile == (int)ProfileEnum.cashier) return Tools.GetStatus(iStatus);
            else if (this.oUser.idprofile == (int)ProfileEnum.invoicingaux) return Tools.GetStatus(iStatus, true);
            else return Tools.GetStatus(iStatus, false);
        }
    }
}