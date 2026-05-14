using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Entity;
using Facade;
using Utils;
using Config;
using OfficeOpenXml;
using System.IO;
using System.Text;
using EventLog;

namespace Trazabilidad
{
    public partial class GenerarPlano : System.Web.UI.Page
    {
        /// <summary>
        /// Propiedad de la clase objeto que almacena el usuario en sesión
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Evento de la página Cargar
        /// </summary>
        /// <param name="sender">Objeto página</param>
        /// <param name="e">Argumentos</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.generatecost))
                {                    
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();           
            }
        }

        /// <summary>
        /// Evento botón buscar para generar plano
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            CostReport oReport = null;
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            List<CostReport> lReport = new List<CostReport>();
            try
            {
                oReport = new CostReport()
                {
                    imonth = Convert.ToInt32(this.ddlMes.SelectedValue),
                    iyear = Convert.ToInt32(this.ddlAno.SelectedValue),
                };
                lReport = oFacade.GetCostReport(oReport);                
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oReport = null;
                oFacade.Dispose();
                oFacade = null;
            }
            this.GenerateFile(lReport);
        }

        /// <summary>
        /// Método que genera al vuelo el archivo txt
        /// </summary>
        /// <param name="lReport">Lista genérica con el resultado de la consulta</param>
        private void GenerateFile(List<CostReport> lReport)
        {
            Response.Clear();
            Response.AddHeader("content-disposition", "attachment; filename=plano.txt");
            Response.ContentType = "application/text";
            using (StreamWriter writer = new StreamWriter(Response.OutputStream))
            {
                foreach (CostReport oReport in lReport)
                {
                    StringBuilder sText = new StringBuilder(oReport.sdocument.Trim());
                    sText.Append(",");
                    sText.Append(oReport.scode.Trim());
                    sText.Append(",");
                    sText.Append(oReport.dvalue.ToString("F2").Replace(",", "."));
                    sText.Append(",01");
                    writer.WriteLine(sText.ToString());
                }
            }
            Response.End();
        }

        /// <summary>
        /// Método que asigna los valores iniciales a los controles
        /// </summary>
        private void LoadControls()
        {
            this.ddlAno.DataSource = Tools.GetYears();
            this.ddlAno.DataValueField = "Key";
            this.ddlAno.DataTextField = "Value";
            this.ddlAno.DataBind();
            this.ddlMes.DataSource = Tools.GetMonths();
            this.ddlMes.DataValueField = "Key";
            this.ddlMes.DataTextField = "Value";
            this.ddlMes.DataBind();
        }
    }
}