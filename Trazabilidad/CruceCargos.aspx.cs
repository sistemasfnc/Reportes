using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utils;
using EventLog;
using Config;
using System.IO;
using System.Text;
using System.Diagnostics;
using FNCEntity;
using FNCFacade;
using Entity;

namespace Trazabilidad
{
    public partial class CruceCargos : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        private List<EntryResponse> lIntegration
        {
            get { return ViewState["lIntegration"] as List<EntryResponse>; }
            set { ViewState["lIntegration"] = value; }
        }

        private List<ServintePatient> lPatient
        {
            get { return ViewState["lPatient"] as List<ServintePatient>; }
            set { ViewState["lPatient"] = value; }
        }

        private List<ServintePatient> lInsert
        {
            get { return ViewState["lInsert"] as List<ServintePatient>; }
            set { ViewState["lInsert"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.compensardematerialize))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }                
            }
        }

        protected void imbProcesar_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                this.GetDataFromStatistics();
                this.GetDataFromInspira();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                throw;
            }
        }

        private void GetDataFromStatistics()
        {
            int iyear = Convert.ToInt32(this.ddlYear.SelectedValue);
            int imonth = Convert.ToInt32(this.ddlMonth.SelectedValue);
            using (FacadeStatistics facadeStatistics = new FacadeStatistics())
            {
                facadeStatistics.sConnection = Configuration.GetStringValue("OracleIntegra");
                this.lIntegration = facadeStatistics.GetDataForDate(iyear, imonth);
            }            
        }

        private void GetDataFromInspira()
        {
            int iyear = Convert.ToInt32(this.ddlYear.SelectedValue);
            int imonth = Convert.ToInt32(this.ddlMonth.SelectedValue);
            using (FacadeInspiraServinte facadeInspiraServinte = new FacadeInspiraServinte())
            {
                this.lPatient = facadeInspiraServinte.GetPatientsForPrograms(iyear, imonth);
            }
        }

        private void ProccessInformation()
        {
            ServintePatient servintePatient = null;
            foreach (var item in this.lIntegration)
            {
                servintePatient = this.lPatient.FirstOrDefault(x => x.lappointments[0].sappointment == item.sappointment);
                if (servintePatient == null)
                {
                    
                }
            }
        }
    }
}