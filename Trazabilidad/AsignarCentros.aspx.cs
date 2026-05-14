using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using Entity;
using EventLog;
using Config;
using Utils;

namespace Trazabilidad
{
    public partial class AsignarCentros : System.Web.UI.Page
    {
        #region Propiedades de la clase

        /// <summary>
        /// Objeto que contiene la sesión del usuario logueado
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Documento del empleado pasado por URL
        /// </summary>
        private string sDocument
        {
            get { return (Request.QueryString["document"] != null) ? Request.QueryString["document"] : string.Empty; }
        }

        /// <summary>
        /// Objeto que contiene la información del empleado seleccionado
        /// </summary>
        private Employee oEmployee
        {
            get { return (ViewState["oEmployee"]) != null ? (Employee)ViewState["oEmployee"] : null; }
            set { ViewState["oEmployee"] = value; }
        }

        /// <summary>
        /// Objeto que contiene la información del proceso de costos generado
        /// </summary>
        private CostProcess oProcess
        {
            get { return (ViewState["oProcess"]) != null ? (CostProcess)ViewState["oProcess"] : null; }
            set { ViewState["oProcess"] = value; }
        }

        /// <summary>
        /// Variable que almacena el porcentaje de investigación
        /// </summary>
        private decimal dInve
        {
            get { return (ViewState["dInve"]) != null ? (decimal)ViewState["dInve"] : 0; }
            set { ViewState["dInve"] = value; }
        }

        /// <summary>
        /// Variable que almacena el porcentaje de investigación
        /// </summary>
        private decimal dEdu
        {
            get { return (ViewState["dEdu"]) != null ? (decimal)ViewState["dEdu"] : 0; }
            set { ViewState["dEdu"] = value; }
        }

        /// <summary>
        /// Variable que almacena el valor de investigación
        /// </summary>
        private decimal dInveCoord
        {
            get { return (ViewState["dInveCoord"]) != null ? (decimal)ViewState["dInveCoord"] : 0; }
            set { ViewState["dInveCoord"] = value; }
        }

        /// <summary>
        /// Variable que almacena el valor de investigación
        /// </summary>
        private decimal dEduCoord
        {
            get { return (ViewState["dEduCoord"]) != null ? (decimal)ViewState["dEduCoord"] : 0; }
            set { ViewState["dEduCoord"] = value; }
        }

        #endregion

        #region Eventos controles

        /// <summary>
        /// Evento cargar página
        /// </summary>
        /// <param name="sender">Objeto página</param>
        /// <param name="e">Argumentos</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.specialcostassign))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                else if (string.IsNullOrEmpty(sDocument))
                {
                    this.pnlDatos.Visible = false;
                }
                else
                {
                    this.pnlDatos.Visible = true;
                    this.LoadEmployee();
                    this.lblNombre.Text =  Tools.ProperCase(this.oEmployee.sname + " " + this.oEmployee.slastname);
                    this.LoadControls();
                }
            }
        }

        /// <summary>
        /// Evento del botón Agregar centro de costo adicional exclusivo para coordinador de nómina, recalcula los porcentajes ingresados
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos evento</param>
        protected void imbAdicional_Click(object sender, ImageClickEventArgs e)
        {
            this.oProcess.lCost = this.GetCostList();
            if (this.oProcess.lCost.Count > 0 && this.rbtPorcentaje.Checked)
            {
                for (int i = 0; i < this.oProcess.lCost.Count; i++)
                {
                    this.oProcess.lCost[i].dvalue -= this.oProcess.lCost[i].dvalue * (Convert.ToDecimal(this.txtValorAdicional.Text) / 100);
                }                
                FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
                try
                {
                    this.oProcess.lCost.Add(new Cost() { dvalue = Convert.ToDecimal(this.txtValorAdicional.Text), scode = this.ddlCostoAdicional.Text, iuser = this.oUser.id, id = this.oProcess.lCost.Count + 1 });
                    this.oProcess.itype = (this.rbtPorcentaje.Checked) ? 1 : 2;
                    this.oProcess.sdocument = this.sDocument;
                    this.oProcess.imonth = Convert.ToInt32(this.ddlMes.SelectedValue);
                    this.oProcess.iyear = Convert.ToInt32(this.ddlAno.SelectedValue);
                    this.oProcess.iuser = this.oUser.id;                    
                    this.oProcess.dtotal = (this.rbtHoras.Checked) ? (this.oProcess.lCost.Sum(x => x.dvalue) / this.oEmployee.ihours) * 100 : this.oProcess.lCost.Sum(x => x.dvalue);
                    this.oProcess.ihours = this.oEmployee.ihours;
                    if (this.oProcess.iid == 0)
                    {
                        this.oProcess.iid = oFacade.Save(this.oProcess, this.oEmployee);
                    }
                    else
                    {
                        oFacade.Save(this.oProcess);
                    }
                    ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "alert('La informacion ha sido almacenada correctamente');", true);
                    this.BindGrid();
                    this.SetTotal();
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", ex);
                    throw;
                }
                finally
                {
                    oFacade.Dispose();
                    oFacade = null;
                }
            }
            else
            {
                ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "alert('No existen centros de costo o la liquidacion no esta distribuida por porcentajes');", true);
            }
        }

        /// <summary>
        /// Evento del botón Buscar costos del período asociado al empleado, en caso de encontrar costos muestra modal de carga de los mismos
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos Evento</param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));            
            try
            {                
                this.oProcess = new CostProcess()
                {
                    sdocument = this.sDocument,
                    imonth = Convert.ToInt32(this.ddlMes.SelectedValue),
                    iyear = Convert.ToInt32(this.ddlAno.SelectedValue),
                    iuser = this.oUser.id,
                    lCost = new List<Cost>(),
                    bisrooter = (this.oUser.idprofile == (int)ProfileEnum.rostercoordinator || this.oUser.idprofile == (int)ProfileEnum.administrator),
                };
                this.oProcess = oFacade.GetProcess(oProcess);
                this.oProcess.lCost = this.FilterCostByType();                
                this.pnlProceso.Visible = true;
                if ((this.dInve == 0 && this.oUser.idprofile == (int)ProfileEnum.investigationcoordiator) || (this.dEdu == 0 && this.oUser.idprofile == (int)ProfileEnum.educationcoordinator))
                {
                    ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "alert('Aún no existe liquidación para los parámetros ingresados');", true);
                    this.pnlProceso.Visible = false;
                    return;
                }                
                else if (this.oProcess.lCost.Count == 0 && this.oEmployee.lCost.Count > 0)
                {
                    this.mpeValidar.Show();
                }
                else if (this.oProcess.lCost.Count > 0)
                {                    
                    this.BindGrid();
                    this.SetTotal();                    
                }
                this.EnableControls();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }        

        /// <summary>
        /// Evento del botón del modal que lo cierra sin cargar costos previos
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void btnNo_Click(object sender, EventArgs e)
        {
            this.mpeValidar.Hide();
        }

        /// <summary>
        /// Evento del botón del Modal que permite cargar la liquidación previa de costos guardada en novasoft
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void btnSi_Click(object sender, EventArgs e)
        {
            this.rbtHoras.Checked = false;
            this.rbtPorcentaje.Checked = true;
            if (this.oUser.idprofile != (int)ProfileEnum.rostercoordinator && this.oUser.idprofile != (int)ProfileEnum.administrator)
            {
                this.oEmployee.lCost = this.FilterCost();
            }                        
            this.oProcess.lCost = this.oEmployee.lCost;
            this.pnlAdicional.Visible = (Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.addspecialcost) && this.oProcess.lCost.Count > 0);
            this.BindGrid();
            this.SetTotal();
            this.EnableControls();
        }
                           
        /// <summary>
        /// Evento de la grilla de costos que se dispara al hacer clic en el botón eliminar costo
        /// </summary>
        /// <param name="sender">Objeto fila de la grilla</param>
        /// <param name="e">Argumentos</param>
        protected void gvCostos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
            if (e.CommandName == "DeleteItem")
            {
                long iItem = Convert.ToInt64(this.gvCostos.DataKeys[gr.RowIndex]["id"]);
                this.RemoveItem(iItem);
                this.BindGrid();
            }
        }

        /// <summary>
        /// Evento del botón agregar nuevo costo al listado de costos actual
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void btnAgregar_Click(object sender, ImageClickEventArgs e)
        {
            long index = 1;
            Cost oCost = null;            
            TextBox txt = null;
            decimal dValue = 0;
            List<Cost> lCost = new List<Cost>();
            foreach (GridViewRow gr in this.gvCostos.Rows)
            {
                txt = (gr.FindControl("txtValor") as TextBox);
                dValue = (!string.IsNullOrEmpty(txt.Text)) ? Convert.ToDecimal(txt.Text) : 0;
                oCost = new Cost()
                {
                    scode = this.gvCostos.DataKeys[gr.RowIndex]["scode"].ToString(),
                    dvalue = dValue,
                    id = index,
                    iuser = Convert.ToInt32(this.gvCostos.DataKeys[gr.RowIndex]["iuser"]),
                    dtotal = dValue,
                };
                lCost.Add(oCost);
                index++;
            }
            index++;
            oCost = new Cost()
            {
                dvalue = Convert.ToDecimal(this.txtValor1.Text),
                scode = this.ddlCentro.SelectedValue,
                iuser = this.oUser.id,
                id = index,
            };
            lCost.Add(oCost);
            this.oProcess.lCost = lCost;
            this.BindGrid();                                               
        }
        
        /// <summary>
        /// Evento del botón guardar liquidación, se valida que los costos asignados no sean mayores a 100% o a las horas disponibles
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void btnGuardar_Click(object sender, ImageClickEventArgs e)
        {
            if (this.ValidateTotal())
            {
                this.oProcess.iclosed = 0;
                this.Save();
                this.SetTotal();
                ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "alert('La información ha sido almacenada correctamente');", true);
            }
            else
            {
                string sMessage = string.Empty;
                if (this.rbtHoras.Checked)
                {
                    sMessage = "La informacion agregada supera el máximo de horas asignadas";
                }
                else
                {
                    sMessage = "La informacion agregada supera el máximo del porcentaje permitido";
                }
                this.SetTotal();
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "ShowMessage('" + sMessage + "');", true);
            }
        }

        /// <summary>
        /// Evento del botón cerrar liquidación
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void btnCerrar_Click(object sender, ImageClickEventArgs e)
        {
            if (this.ValidateTotal())
            {
                this.oProcess.iclosed = 1;
                this.Save();
                ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "alert('El periodo ha sido cerrado correctamente');", true);
                this.EnableControls();
            }
            else
            {
                string sMessage = string.Empty;
                if (this.rbtHoras.Checked)
                {
                    sMessage = "La informacion agregada supera el máximo de horas asignadas: " + this.oEmployee.ihours.ToString();
                }
                else
                {
                    sMessage = "La informacion agregada supera el máximo de porcentaje permito: 100%";
                }
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "ShowMessage('" + sMessage + "');", true);
            }            
        }        
        

        /// <summary>
        /// Evento que reasigna los valores de los costos dependiendo del tipo de liquidación, horas o porcentaje
        /// </summary>
        /// <param name="sender">Objeto radiobutton</param>
        /// <param name="e">Argunemtos</param>
        protected void rbtPorcentaje_CheckedChanged(object sender, EventArgs e)
        {
            string sValue = string.Empty;
            TextBox txt = null;
            decimal dvalue = 0;
            foreach (GridViewRow gr in this.gvCostos.Rows)
            {
                txt = (gr.FindControl("txtValor") as TextBox);
                sValue = (!string.IsNullOrEmpty(txt.Text)) ? txt.Text : "0";
                if (this.rbtHoras.Checked)
                {
                    dvalue = this.oEmployee.ihours * (Convert.ToDecimal(sValue) / 100);
                    //txt.Text = Math.Floor(dvalue).ToString();
                }
                else
                {
                    dvalue = (Convert.ToDecimal(sValue) / this.oEmployee.ihours) * 100;                    
                }
                txt.Text = this.FormatValue(dvalue);
            }
        }

        #endregion

        #region Métodos desarrollados
        /// <summary>
        /// Método que formatea el valor de un dato con formato entero 2 decimales
        /// </summary>
        /// <param name="oValue">Valor 1</param>
        /// <param name="oTotal">Valor 2</param>
        /// <returns>Valor con formato</returns>
        protected string FormatValue(object oValue, object oTotal)
        {
            return (this.oUser.idprofile != (int)ProfileEnum.rostercoordinator && this.oUser.idprofile != (int)ProfileEnum.administrator) ? String.Format("{0:F2}", oValue) : String.Format("{0:F2}", oTotal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oValue"></param>        
        /// <returns>Valor con formato</returns>
        protected string FormatValue(object oValue)
        {
            return String.Format("{0:F2}", oValue);
        }

        /// <summary>
        /// Método que habilita el botón de eliminar el costo asociado
        /// </summary>
        /// <param name="iuser">Id del usuario</param>
        /// <returns>Verdadero si el usuario logueado es el mismo que ingresó el costo, falso en caso contrario</returns>
        protected bool EnableDeleteButton(object iuser, object scode)
        {
            if (scode.ToString() == "INVE" || scode.ToString() == "EDDO") return false;
            else return (Convert.ToInt32(iuser) == this.oUser.id);
        }

        /// <summary>
        /// Método que obtiene el listado de costos asignados
        /// </summary>
        /// <returns>Lista génerica con los costos asignados</returns>
        private List<Cost> GetCostList()
        {
            long index = 1;
            Cost oCost = null;
            TextBox txt = null;
            decimal dValue = 0;
            List<Cost> lCost = new List<Cost>();
            foreach (GridViewRow gr in this.gvCostos.Rows)
            {
                txt = (gr.FindControl("txtValor") as TextBox);
                dValue = (!string.IsNullOrEmpty(txt.Text)) ? Convert.ToDecimal(txt.Text) : 0;
                oCost = new Cost()
                {
                    scode = this.gvCostos.DataKeys[gr.RowIndex]["scode"].ToString(),
                    dvalue = dValue,
                    id = index,
                    dtotal = (this.rbtHoras.Checked) ? (dValue / this.oEmployee.ihours) * 100 : dValue,
                    iuser = Convert.ToInt32(this.gvCostos.DataKeys[gr.RowIndex]["iuser"]),
                };
                lCost.Add(oCost);
                index++;
            }
            return lCost;
        }

        /// <summary>
        /// Método que salva los costos asignados
        /// </summary>
        private void Save()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.oProcess.itype = (this.rbtPorcentaje.Checked) ? 1 : 2;
                this.oProcess.sdocument = this.sDocument;
                this.oProcess.imonth = Convert.ToInt32(this.ddlMes.SelectedValue);
                this.oProcess.iyear = Convert.ToInt32(this.ddlAno.SelectedValue);
                this.oProcess.iuser = this.oUser.id;
                this.oProcess.lCost = this.GetCostList();
                this.oProcess.dtotal = (this.rbtHoras.Checked) ? (this.oProcess.lCost.Sum(x => x.dvalue) / this.oEmployee.ihours) * 100 : this.oProcess.lCost.Sum(x => x.dvalue);
                this.oProcess.ihours = this.oEmployee.ihours;
                if (this.oProcess.iid == 0)
                {
                    this.oProcess.iid = oFacade.Save(this.oProcess, this.oEmployee);
                }
                else
                {
                    oFacade.Save(this.oProcess);
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        /// <summary>
        /// Método que elimina un item del listado de costos
        /// </summary>
        /// <param name="iItem">Id del item</param>
        private void RemoveItem(long iItem)
        {
            int iIndex = this.oProcess.lCost.FindIndex(x => x.id == iItem);
            this.oProcess.lCost.RemoveAt(iIndex);
        }

        /// <summary>
        /// Método que valida que el total de costos asignados no supere el 100% o el total de horas disponibles
        /// </summary>
        /// <returns>Verdadero en caso de que los costos asignados no superen el valor, falso en caso contrario</returns>
        private bool ValidateTotal()
        {
            decimal dTotal = 0;
            List<Cost> lCost = this.GetCostList();
            dTotal = lCost.Sum(x => x.dtotal);
            if (Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.viewspecialcost) || this.oUser.idprofile == (int)ProfileEnum.rostercoordinator)
            {
                return (dTotal <= 100);
            }
            else
            {
                dTotal = lCost.Sum(x => x.dvalue);
                if (this.oUser.idprofile == (int)ProfileEnum.investigationcoordiator) return dTotal <= dInveCoord;
                else if (this.oUser.idprofile == (int)ProfileEnum.educationcoordinator) return dTotal <= dEduCoord;
                /*if (dInve > 0) return dTotal <= dInve;
                if (dEdu > 0) return dTotal <= dEdu;*/
                return true;
            }            
        }

        /// <summary>
        /// Método que habilita los controles de la página
        /// </summary>
        private void EnableControls()
        {
            this.gvCostos.Enabled = (this.oProcess.iclosed == 0);
            if (this.oUser.idprofile == (int)ProfileEnum.investigationcoordiator)
            {
                this.pnlInvestiga.Visible = true;
                this.lblInvestiga.Text = this.dInveCoord.ToString();
            }
            else if (this.oUser.idprofile == (int)ProfileEnum.educationcoordinator)
            {
                this.pnlEducacion.Visible = true;
                this.lblEduca.Text = this.dEduCoord.ToString();
                
            }            
            this.rbtHoras.Checked = (this.oProcess.lCost.Count > 0) ? (this.oProcess.lCost[0].istatus == 2) : false;
            this.rbtPorcentaje.Checked = !this.rbtHoras.Checked;
            this.btnAgregar.Enabled = this.btnCerrar.Enabled = this.btnGuardar.Enabled = (this.oProcess.iclosed == 0);
            this.pnlFormulario.Enabled = (this.oProcess.iclosed == 0);
            this.rbtPorcentaje.Enabled = this.rbtHoras.Enabled = (this.oProcess.iclosed == 0);
            this.pnlAdicional.Visible = (Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.addspecialcost) && this.oProcess.lCost.Count > 0);            
        }

        /// <summary>
        /// Método para deshabilitar los controles
        /// </summary>
        private void DisableControls()
        {
            this.gvCostos.Enabled = false;
            this.rbtHoras.Checked = false;
            this.rbtPorcentaje.Checked = !this.rbtHoras.Checked;
            this.btnAgregar.Enabled = this.btnCerrar.Enabled = this.btnGuardar.Enabled = false;
            this.pnlFormulario.Enabled = false;            
            this.pnlAdicional.Visible = false;
        }
        /// <summary>
        /// Método que genera y presenta la grilla de costos
        /// </summary>
        private void BindGrid()
        {
            this.gvCostos.DataSource = this.oProcess.lCost;
            this.gvCostos.DataKeyNames = new string[] { "id", "scode", "iuser" };
            this.gvCostos.DataBind();
        }

        /// <summary>
        /// Método que asigna la etiqueta de porcentaje asignado
        /// </summary>
        private void SetTotal()
        {
            decimal dTotal = 0;
            if (dInve != 0 && !Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.viewspecialcost))
            {
                dTotal = dInve;
            }
            else if (dEdu != 0 && !Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.viewspecialcost))
            {
                dTotal = dEdu;
            }
            /*else if (this.rbtHoras.Checked)
            {
                dTotal = (this.oProcess.lCost.Sum(x => x.dtotal) / this.oEmployee.ihours) * 100;
            }*/
            else
            {
                dTotal = this.oProcess.lCost.Sum(x => x.dtotal);
            }
            this.lblAsignado.Text = this.FormatValue(dTotal) + "%";
        }       

        /// <summary>
        /// Método que busca la información del empleado en la base de datos y la carga en el objeto empleado
        /// </summary>
        private void LoadEmployee()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("DBNovasoft"));
            List<Employee> lEmployee = null;
            try
            {
                this.oEmployee = new Employee() { sdocument = this.sDocument };
                lEmployee = oFacade.GetEmployees(this.oEmployee);
                if (lEmployee.Count > 0)
                {
                    this.oEmployee = lEmployee[0];
                    this.oEmployee.lCostTotal = this.oEmployee.lCost;
                    this.lblHoras.Text = this.oEmployee.ihours.ToString();
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                lEmployee = null;
                oFacade = null;
            }
        }

        /// <summary>
        /// Método que carga los controles iniciales de la página
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
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlCentro.DataSource = this.ddlCostoAdicional.DataSource = oFacade.GetCosts();
                this.ddlCentro.DataTextField = this.ddlCostoAdicional.DataTextField = "code";
                this.ddlCentro.DataValueField = this.ddlCostoAdicional.DataValueField = "code";
                this.ddlCentro.DataBind();
                this.ddlCostoAdicional.DataBind();
                this.btnCerrar.Visible = Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.savecost);
                this.btnCerrarEspecial.Visible = (this.oUser.idprofile == (int)ProfileEnum.investigationcoordiator || this.oUser.idprofile == (int)ProfileEnum.educationcoordinator);
                this.dInve = 0;
                this.dEdu = 0;
                this.dInveCoord = 0;
                this.dEduCoord = 0;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        /// <summary>
        /// Método que filtra el listado de costos del empleado para que aparezcan solo los costos que maneja el usuario
        /// </summary>
        /// <returns>Lista genérica con los costros filtrados</returns>
        private List<Cost> FilterCost()
        {
            List<Cost> lResult = new List<Cost>();            
            foreach (CostUser oCost in this.oUser.lCost)
            {
                var oFind = this.oEmployee.lCost.FirstOrDefault(x => x.scode.Trim() == oCost.scode);
                if (oFind != null)
                    lResult.Add(oFind);
            }
            //Si el usuario tiene permiso de asignar porcentajes a investigación y a educación se le agregan estos centros de costo "especiales"
            if (Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.viewspecialcost))
            {
                lResult.Add(new Cost() { scode = "INVE", sname = "INVESTIGACION", dvalue = dInve, ctype = 'O', dtotal = dInve });
                lResult.Add(new Cost() { scode = "EDDO", sname = "EDUCACION", dvalue = dEdu, ctype = 'O', dtotal = dEdu });
            }                
            return lResult;
        }

        /// <summary>
        /// Método que filtra los centros de costo según el tipo de usuario
        /// </summary>
        /// <returns></returns>
        private List<Cost> FilterCostByType()
        {
            List<Cost> lResult = new List<Cost>();
            if (this.oUser.idprofile == (int)ProfileEnum.investigationcoordiator)
            {
                lResult = this.oProcess.lCost.FindAll(x => x.ctype == 'I');
                if (this.oProcess.lCost.FirstOrDefault(x => x.scode == "INVE") != null)
                {
                    dInve = this.oProcess.lCost.FirstOrDefault(x => x.scode == "INVE").dtotal;
                    dInveCoord = this.oProcess.lCost.FirstOrDefault(x => x.scode == "INVE").dvalue;
                }
            }
            else if (this.oUser.idprofile == (int)ProfileEnum.educationcoordinator)
            {
                lResult = this.oProcess.lCost.FindAll(x => x.ctype == 'E');
                if (this.oProcess.lCost.FirstOrDefault(x => x.scode == "EDDO") != null)
                {
                    dEdu = this.oProcess.lCost.FirstOrDefault(x => x.scode == "EDDO").dtotal;
                    dEduCoord = this.oProcess.lCost.FirstOrDefault(x => x.scode == "EDDO").dvalue;
                }
            }
            else if (this.oUser.idprofile == (int)ProfileEnum.healthcarecoordinator)
            {
                lResult = this.oProcess.lCost.FindAll(x => x.ctype == 'N' || x.ctype == 'O');
                if (this.oProcess.lCost.FirstOrDefault(x => x.ctype == 'I') != null)
                {
                    dInve = this.oProcess.lCost.FindAll(x => x.ctype == 'I').Sum(x => x.dtotal);
                }
                if (this.oProcess.lCost.FirstOrDefault(x => x.ctype == 'E') != null)
                {
                    dEdu = this.oProcess.lCost.FindAll(x => x.ctype == 'E').Sum(x => x.dtotal);
                }
            }
            else
            {
                lResult = this.oProcess.lCost.FindAll(x => x.ctype != 'O');
            }
            //Si el usuario tiene permiso de asignar porcentajes a investigación y a educación se le agregan estos centros de costo "especiales"
            if (Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.viewspecialcost))
            {
                lResult.Add(new Cost() { scode = "INVE", sname = "INVESTIGACION", dvalue = dInve, ctype = 'O', dtotal = dInve });
                lResult.Add(new Cost() { scode = "EDDO", sname = "EDUCACION", dvalue = dEdu, ctype = 'O', dtotal = dEdu });
            }
            return lResult;
        }

        #endregion

        /// <summary>
        /// Evento botón cerrar el proceso de liquidación para perfiles de investigación y educación
        /// </summary>
        /// <param name="sender">Botón cerrar especial</param>
        /// <param name="e">Argumentos evento</param>
        protected void btnCerrarEspecial_Click(object sender, ImageClickEventArgs e)
        {
            if (this.ValidateTotal())
            {
                this.CloseSpecial();
                this.DisableControls();
                ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "alert('El proceso ha sido cerrado correctamente');", true);
            }
            else
            {
                string sMessage = string.Empty;
                if (this.rbtHoras.Checked)
                {
                    sMessage = "La informacion agregada supera el máximo de horas asignadas";
                }
                else
                {
                    sMessage = "La informacion agregada supera el máximo del porcentaje permitido";
                }
                this.SetTotal();
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "ShowMessage('" + sMessage + "');", true);
            }
        }

        private void CloseSpecial()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            string sType = (this.oUser.idprofile == (int)ProfileEnum.investigationcoordiator) ? "INVE" : "EDDO";
            try
            {
                this.oProcess.itype = (this.rbtPorcentaje.Checked) ? 1 : 2;
                this.oProcess.sdocument = this.sDocument;
                this.oProcess.imonth = Convert.ToInt32(this.ddlMes.SelectedValue);
                this.oProcess.iyear = Convert.ToInt32(this.ddlAno.SelectedValue);
                this.oProcess.iuser = this.oUser.id;
                this.oProcess.lCost = this.GetCostList();
                this.oProcess.dtotal = (this.rbtHoras.Checked) ? (this.oProcess.lCost.Sum(x => x.dvalue) / this.oEmployee.ihours) * 100 : this.oProcess.lCost.Sum(x => x.dvalue);
                this.oProcess.ihours = this.oEmployee.ihours;
                oFacade.SaveSpecialCostList(this.oProcess, sType);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }
    }
}