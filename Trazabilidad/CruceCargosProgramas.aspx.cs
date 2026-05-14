using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FNCFacade;
using FNCEntity;
using EventLog;
using Entity;
using Utils;
using Config;

namespace Trazabilidad
{
    public partial class CruceCargosProgramas : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        private List<EntryExtended> lentryExtendeds
        {
            get { return (ViewState["lentryExtendeds"] != null) ? ViewState["lentryExtendeds"] as List<EntryExtended> : new List<EntryExtended>(); }
            set { ViewState["lentryExtendeds"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.programsinvoices))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.imbGuardar.Visible = (this.gvIngresos.Rows.Count > 0);
            }
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        private void BindGrid()
        {
            using (FacadeStatistics facadeStatistics = new FacadeStatistics())
            {
                facadeStatistics.sConnection = Configuration.GetStringValue("ServinteIntegra");
                EntryExtended entryExtended = new EntryExtended();
                if (!string.IsNullOrEmpty(this.txtFechaInicio.Text))
                {
                    entryExtended.dinitial = Convert.ToDateTime(this.txtFechaInicio.Text);
                }
                if (!string.IsNullOrEmpty(this.txtFechaFin.Text))
                {
                    entryExtended.dfinal = Convert.ToDateTime(this.txtFechaFin.Text);
                }
                this.lentryExtendeds = facadeStatistics.GetEntriesForPrograms(entryExtended);
                var result = this.lentryExtendeds.GroupBy(x => x.ientry).Select(g => new
                {
                    dvalue = g.Sum(s => s.dvalue),
                    ientry = g.Key,
                    ddate = Convert.ToDateTime(g.FirstOrDefault().ddate),
                    sagreement = g.FirstOrDefault().sagreement,
                    splan = g.FirstOrDefault().splan,
                    srate = g.FirstOrDefault().srate,
                    iinvoice = g.FirstOrDefault().iinvoice,
                    spatient = g.FirstOrDefault().spatient,
                    sdocument = g.FirstOrDefault().sdocument,
                    sdocumenttype = g.FirstOrDefault().sdocumenttype,
                });
                this.gvIngresos.DataSource = result;
                this.gvIngresos.DataKeyNames = new string[] { "ientry" };
                this.gvIngresos.DataBind();
            }
        }

        protected void gvIngresos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int ientry = 0;
            if (e.CommandName == "Detail")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                GridView gvDetail = gr.FindControl("gvDetalle") as GridView;
                if (gvDetail.Visible)
                {
                    gvDetail.Visible = false;
                    gvDetail.DataSource = null;
                    gvDetail.DataBind();
                }
                else
                {
                    gvDetail.Visible = true;
                    ientry = Convert.ToInt32(this.gvIngresos.DataKeys[gr.RowIndex]["ientry"]);
                    gvDetail.DataSource = this.lentryExtendeds.FindAll(x => x.ientry == ientry);
                    gvDetail.DataBind();
                }
            }
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtFechaInicio.Text = this.txtFechaFin.Text = string.Empty;
            this.gvIngresos.DataSource = null;
            this.gvIngresos.DataBind();
        }

        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            List<EntryExtended> lResult = new List<EntryExtended>();
            EntryExtended entryExtended = null;
            TextBox textBox = null;
            foreach (GridViewRow gr in this.gvIngresos.Rows)
            {
                textBox = gr.FindControl("txtInvoce") as TextBox;
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    entryExtended = new EntryExtended()
                    {
                        ientry = Convert.ToInt32(this.gvIngresos.DataKeys[gr.RowIndex]["ientry"]),
                        iinvoice = Convert.ToInt32(textBox.Text),
                    };
                    lResult.Add(entryExtended);
                }
            }
            try
            {
                this.UpdateEntries(lResult);
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Los ingresos han sido actualizados correctamente');", true);
                this.BindGrid();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                throw;
            }            
            lResult = null;
            entryExtended = null;
            textBox = null;
        }

        private void UpdateEntries(List<EntryExtended> lResult)
        {
            using (FacadeStatistics facadeStatistics = new FacadeStatistics())
            {
                facadeStatistics.sConnection = Config.Configuration.GetStringValue("OracleIntegra");
                facadeStatistics.UpdateEntriesInvoice(lResult);
            }
        }
    }
}