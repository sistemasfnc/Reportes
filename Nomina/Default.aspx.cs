using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using Entity;
using Utils;
using EventLog;
using System.Configuration;
using System.Xml.Linq;

namespace Nomina
{
    public partial class _Default : Page
    {
        /// <summary>
        /// 
        /// </summary>
        private List<Entity.Nomina> lNomina 
        {
            get 
            {
                return (ViewState["lNomina"] != null) ? ViewState["lNomina"] as List<Entity.Nomina> : new List<Entity.Nomina>();
            }
            set
            {
                ViewState["lNomina"] = value;
            } 
        }

        private List<NominaStatus> lNominaStatus
        {
            get
            {
                return (ViewState["lNominaStatus"] != null) ? ViewState["lNominaStatus"] as List<NominaStatus> : new List<NominaStatus>();
            }
            set
            {
                ViewState["lNominaStatus"] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private NominaStatus oNomina
        {
            get
            {
                return (ViewState["oNomina"] != null) ? ViewState["oNomina"] as NominaStatus : null;
            }
            set
            {
                ViewState["oNomina"] = value;
            }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (!this.IsPostBack && Session["User"] != null)
            if (!this.IsPostBack)
            {
                //this.txtInitialDate.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
                //this.txtFinalDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
                this.LoadControls();
                this.FillStatus();
                this.BindGrid();
            }
        }

        protected void gvDatos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if(e.CommandName == "Editar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                Entity.Nomina oEntity = new Entity.Nomina()
                {
                    document = this.gvDatos.DataKeys[gr.RowIndex]["document"].ToString(),
                    incdays = Convert.ToInt32(this.gvDatos.DataKeys[gr.RowIndex]["incdays"]),
                    incdate = Convert.ToDateTime(this.gvDatos.DataKeys[gr.RowIndex]["incdate"]),
                    inccode = this.gvDatos.DataKeys[gr.RowIndex]["inccode"].ToString(),
                    incnum = this.gvDatos.DataKeys[gr.RowIndex]["incnum"].ToString(),
                };
                this.oNomina = oEntity;
                this.txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
                this.BindStatusGrid(oEntity);                
                this.mpEstados.Show();                
            }
        }        

        protected void btnEstado_Click(object sender, EventArgs e)
        {
            FacadeNomina oFacade = new FacadeNomina(ConfigurationManager.ConnectionStrings["DBNovasoft"].ConnectionString);
            try
            {
                this.oNomina.status = Convert.ToInt32(this.ddlEstado.SelectedValue);
                this.oNomina.statusdate = Convert.ToDateTime(this.txtFecha.Text);
                this.oNomina.observations = this.txtObservacion.Text;
                this.oNomina.value = (!string.IsNullOrEmpty(this.txtValor.Text)) ? Convert.ToInt32(this.txtValor.Text) : 0;
                this.oNomina.diagnosis = this.txtDiagnostico.Text;
                oFacade.InsertStatus(this.oNomina);                
                this.mpEstados.Hide();
                ClientScript.RegisterStartupScript(this.GetType(), "", "<script>alert('El registro ha sido insertado correctamente');</script>");
                this.FillStatus();
                this.BindGrid();                
            }
            catch (Exception ex)
            {
                LogError.WriteError("Nomina", "WEB", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }
        
        private void LoadControls()
        {
            FacadeNomina oFacade = new FacadeNomina(ConfigurationManager.ConnectionStrings["DBNovasoft"].ConnectionString);
            try
            {
                this.ddlEps.DataSource = oFacade.GetEnsurance();
                this.ddlEps.DataValueField = "code";
                this.ddlEps.DataTextField = "name";
                this.ddlEps.DataBind();
                this.ddlEps.Items.Add(new ListItem(string.Empty, string.Empty));
                this.ddlEps.SelectedValue = string.Empty;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Nomina", "WEB", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void BindGrid()
        {
            string[] keys = new string[] { "document", "incdate", "incnum", "inccode", "incdays" };
            this.gvDatos.DataKeyNames = keys;
            FacadeNomina oFacade = new FacadeNomina(ConfigurationManager.ConnectionStrings["DBNovasoft"].ConnectionString);
            Entity.Nomina oNomina = new Entity.Nomina() { document = this.txtDocument.Text, inccode = this.ddlCode.SelectedValue};
            if (!string.IsNullOrEmpty(this.txtInitialDate.Text)) oNomina.initialdate = Convert.ToDateTime(this.txtInitialDate.Text);
            if (!string.IsNullOrEmpty(this.txtFinalDate.Text)) oNomina.finaldate = Convert.ToDateTime(this.txtFinalDate.Text);
            try
            {
                this.lNomina = oFacade.GetList(oNomina);
                this.FillListStatus();
                if (!string.IsNullOrEmpty(this.ddlStatus.SelectedValue))
                {
                    this.lNomina = this.lNomina.FindAll(x => x.status == Convert.ToInt32(this.ddlStatus.SelectedValue));
                }
                this.gvDatos.DataSource = this.lNomina;
                this.gvDatos.DataBind();
            }
            catch (ApplicationException ex)
            {
                LogError.WriteError("Nomina", "WEB", ex);  
                throw;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Nomina", "WEB", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        private void BindStatusGrid(NominaStatus oEntity)
        {
            FacadeNomina oFacade = new FacadeNomina(ConfigurationManager.ConnectionStrings["DBNovasoft"].ConnectionString);
            try
            {
                this.gvEstados.DataSource = oFacade.GetStatusList(oEntity);
                this.gvEstados.DataBind();
                this.txtObservacion.Text = string.Empty;
                this.ddlEstado.ClearSelection();
                this.txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Nomina", "WEB", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        private void FillStatus()
        {
            FacadeNomina oFacade = new FacadeNomina(ConfigurationManager.ConnectionStrings["DBNovasoft"].ConnectionString);
            try
            {
                this.lNominaStatus = oFacade.GetStatusList(new NominaStatus());                
            }
            catch (Exception ex)
            {
                LogError.WriteError("Nomina", "WEB", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        private void FillListStatus()
        {
            NominaStatus oEntity = null;
            for (int i = 0; i < this.lNomina.Count; i++)
            {
                oEntity = this.lNominaStatus.FirstOrDefault(x => x.inccode == this.lNomina[i].inccode && x.incdate == this.lNomina[i].incdate && x.incdays == this.lNomina[i].incdays && x.document == this.lNomina[i].document);
                if (oEntity != null)
                {
                    this.lNomina[i].status = oEntity.status;
                    this.lNomina[i].statusdate = oEntity.statusdate;
                    this.lNomina[i].observations = oEntity.observations;
                    this.lNomina[i].diagnosis = oEntity.diagnosis;
                    this.lNomina[i].value = oEntity.value;
                }
                
            }
        }

        protected string GetStatus(object sDocument, object sCode, object dDate, object iDays)
        {
            NominaStatus oEntity = this.lNominaStatus.FirstOrDefault(x => x.inccode == sCode.ToString() && x.incdate == Convert.ToDateTime(dDate) && x.incdays == Convert.ToInt32(iDays) && x.document == sDocument.ToString());
            if (oEntity != null)
            {                
                return this.GetStatus(oEntity.status);
            }
            return string.Empty;
        }

        protected string GetValue(object sDocument, object sCode, object dDate, object iDays)
        {
            List<NominaStatus> lTmp = this.lNominaStatus.FindAll(x => x.inccode == sCode.ToString() && x.incdate == Convert.ToDateTime(dDate) && x.incdays == Convert.ToInt32(iDays) && x.document == sDocument.ToString());
            for (int i = 0; i < lTmp.Count; i++)
            {
                if (lTmp[i].value != 0)
                {
                    this.lNomina.Find(x => x.inccode == sCode.ToString() && x.incdate == Convert.ToDateTime(dDate) && x.incdays == Convert.ToInt32(iDays) && x.document == sDocument.ToString()).value = lTmp[i].value;
                    return lTmp[i].value.ToString("C");
                }
            }
            return string.Empty;
        }

        protected string GetDiagnosis(object sDocument, object sCode, object dDate, object iDays)
        {
            List<NominaStatus> lTmp = this.lNominaStatus.FindAll(x => x.inccode == sCode.ToString() && x.incdate == Convert.ToDateTime(dDate) && x.incdays == Convert.ToInt32(iDays) && x.document == sDocument.ToString());
            for (int i = 0; i < lTmp.Count; i++)
            {
                if (!string.IsNullOrEmpty(lTmp[i].diagnosis.Trim()))
                {
                    this.lNomina.Find(x => x.inccode == sCode.ToString() && x.incdate == Convert.ToDateTime(dDate) && x.incdays == Convert.ToInt32(iDays) && x.document == sDocument.ToString()).diagnosis = lTmp[i].diagnosis.Trim();
                    return lTmp[i].diagnosis;
                }
            }
            return string.Empty;            
        }

        protected string GetStatus(int iStatus)
        {
            if (iStatus != 0)
            {
                string sFile = Server.MapPath("~") + "Estados.xml";
                XElement oXML = XElement.Load(sFile);
                var x = from a in oXML.Descendants("estado") where a.Attribute("code").Value == iStatus.ToString() select a.Value;
                return x.First().ToString();                        
            }
            return string.Empty;
        }


        private string GetStatusTe(object iStatus)
        {
            string sFile = Server.MapPath("~") + "Estados.xml";
            XElement oXML = XElement.Load(sFile);
            var x = from a in oXML.Descendants("estado") where a.Attribute("code").Value == iStatus.ToString() select a.Value;
            return x.First().ToString();
        }

        protected void btnCerrar_Click(object sender, EventArgs e)
        {
            this.mpEstados.Hide();
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            this.BindGrid();
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            this.ddlEstado.ClearSelection();
            this.ddlStatus.ClearSelection();
            this.ddlCode.ClearSelection();
            this.txtDocument.Text = string.Empty;
            //this.txtInitialDate.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy");
            //this.txtFinalDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.BindGrid();
            this.ddlEps.SelectedValue = string.Empty;
        }

        protected void btnExportar_Click(object sender, EventArgs e)
        {
            Session["lNomina"] = this.lNomina;
            ClientScript.RegisterStartupScript(this.GetType(), "", "<script>window.open('exportnomina.aspx');</script>");
        }

        protected void gvDatos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvDatos.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }
    }
}