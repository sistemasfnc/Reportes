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
using System.Text;
using EventLog;
using Trazabilidad.WSIntegrator;
using FNCEntity;

namespace Trazabilidad
{
    public partial class CostoXMedico : System.Web.UI.Page
    {
        /// <summary>
        /// Objeto que guarda al usuario logueado en el sistema
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Objeto que almacena el listado de profesionales
        /// </summary>
        private List<Employee> lProfessionals
        {
            get
            {
                return (ViewState["lProfessionals"] != null) ? ViewState["lProfessionals"] as List<Employee> : new List<Employee>();
            }
            set
            {
                ViewState["lProfessionals"] = value;
            }
        }

        
        /// <summary>
        /// Objeto lista genérica que almacena los centros de costo
        /// </summary>
        private List<Entity.Generic> lCostCenter
        {
            get
            {
                return (ViewState["lCostCenter"] != null) ? ViewState["lCostCenter"] as List<Entity.Generic> : new List<Entity.Generic>();
            }
            set
            {
                ViewState["lCostCenter"] = value;
            }
        }

        /// <summary>
        /// Objeto lista del proceso de liquidación de las horas
        /// </summary>
        private List<CostProcess> lProcess
        {
            get
            {
                return (ViewState["lProcess"] != null) ? ViewState["lProcess"] as List<CostProcess> : new List<CostProcess>();
            }
            set
            {
                ViewState["lProcess"] = value;
            }
        }

        /// <summary>
        /// Objeto lista de errores de liquidación
        /// </summary>
        private List<FNCEntity.Generic> lError
        {
            get
            {
                return (ViewState["lError"] != null) ? ViewState["lError"] as List<FNCEntity.Generic> : new List<FNCEntity.Generic>();
            }
            set
            {
                ViewState["lError"] = value;
            }
        }

        /// <summary>
        /// Evento cargar página
        /// </summary>
        /// <param name="sender">Objeto página</param>
        /// <param name="e">Argumentos</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this.lProcess = new List<CostProcess>();
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.assigncost))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadControls();                
            }
        }

        /// <summary>
        /// Método que carga los controles iniciales de la página
        /// </summary>
        private void LoadControls()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlCentro.DataSource = this.ddlCentroCosto.DataSource = oFacade.GetCosts();
                this.ddlCentro.DataTextField = "name";
                this.ddlCentro.DataValueField = "code";
                this.ddlCentro.DataBind();
                this.ddlCentro.Enabled = Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.filteremployee);
                this.ddlCentro.SelectedValue = string.Empty;
                this.ddlCentro.DataTextField = "name";
                this.ddlCentroCosto.DataValueField = "code";
                this.ddlCentroCosto.DataBind();                
                this.ddlCentroCosto.SelectedValue = string.Empty;
                this.ddlAno.DataSource = Tools.GetYears();
                this.ddlAno.DataValueField = "Key";
                this.ddlAno.DataTextField = "Value";
                this.ddlAno.DataBind();
                this.ddlMes.DataSource = Tools.GetMonths();
                this.ddlMes.DataValueField = "Key";
                this.ddlMes.DataTextField = "Value";
                this.ddlMes.DataBind();                
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
        /// Método botón buscar centros de costo por empleado
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos evento</param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            WSDigiturno wSDigiturno = new WSDigiturno();
            int iMonth = Convert.ToInt32(this.ddlMes.SelectedValue);
            int iYear = Convert.ToInt32(this.ddlAno.SelectedValue);
            try
            {
                this.lProcess = this.SearchProcess();
                this.lProfessionals = this.GetEmployees();
                this.lCostCenter = this.GetCosts();
                if (this.lProcess.Count == 0)
                {
                    WSIntegrator.Generic[] lGeneric = wSDigiturno.GetTimeDistribution(this.ddlCentro.SelectedValue, iMonth, iYear);
                    if (lGeneric.Length > 0)
                    {
                        this.pnlTabla.Visible = true;                        
                        this.lCostCenter = this.FilterCosts(lGeneric);
                        this.GenerateGrid(lGeneric);
                    }
                }
                else
                {
                    this.pnlTabla.Visible = true;
                    this.lCostCenter = this.FilterCosts();
                    this.RegenerateTable();
                }                
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                wSDigiturno.Dispose();
                wSDigiturno = null;
            }
        }

        /// <summary>
        /// Método para buscar si existe un proceso de liquidación de costo iniciado en el sistema
        /// </summary>
        /// <returns>Objeto lista genérica con el proceso de liquidación encontrado</returns>
        private List<CostProcess> SearchProcess()
        {            
            using (FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion")))
            {
                CostProcess costProcess = new CostProcess()
                {
                    imonth = Convert.ToInt32(this.ddlMes.SelectedValue),
                    iyear = Convert.ToInt32(this.ddlAno.SelectedValue),
                    scostcenter = this.ddlCentro.SelectedValue,
                };
                return oFacade.GetCostProcessList(costProcess);
            }
        }

        /// <summary>
        /// Método para generar la tabla de centros de costo por profesional
        /// </summary>
        /// <param name="lGeneric">Lista genérica con el resultado de los minutos en centros de costo por empleado</param>
        private void GenerateGrid(WSIntegrator.Generic[] lGeneric)
        {
            bool bFind = true;
            Table table = new Table();
            table.ID = "tblCentros";
            table.EnableViewState = true;
            var headercolor = System.Drawing.ColorTranslator.FromHtml("#006699");
            TableHeaderRow tableHeaderRow = new TableHeaderRow();
            TableHeaderRow tableHeaderRow1 = new TableHeaderRow();            
            TableRow tableRow = new TableRow();
            TextBox textBox = null;
            TableCell tableCell = new TableCell();
            HiddenField hiddenField = null;
            tableCell.Text = "CC / EMPLEADO";
            tableCell.BackColor = headercolor;
            tableCell.Font.Bold = true;
            tableCell.Font.Size = FontUnit.Small;
            tableCell.ForeColor = System.Drawing.Color.White;
            tableHeaderRow.Cells.Add(tableCell);
            tableCell = new TableCell();
            tableCell.Text = " ";
            tableCell.BackColor = headercolor;
            tableCell.Font.Bold = true;
            tableCell.Font.Size = FontUnit.Small;
            tableHeaderRow1.Cells.Add(tableCell);            
            for (int i = 0; i < this.lProfessionals.Count; i++)
            {
                tableCell = new TableCell();
                tableCell.Text = this.GetProfessionalName(this.lProfessionals[i].sname, this.lProfessionals[i].ssurname);
                tableCell.HorizontalAlign = HorizontalAlign.Center;
                tableCell.BackColor = headercolor;
                tableCell.Font.Bold = true;
                tableCell.Font.Size = FontUnit.Small;
                tableCell.ForeColor = System.Drawing.Color.White;
                tableHeaderRow.Cells.Add(tableCell);
                tableCell = new TableCell();
                tableCell.Text = this.lProfessionals[i].ihours.ToString();
                tableCell.HorizontalAlign = HorizontalAlign.Center;
                tableCell.BackColor = headercolor;
                tableCell.Font.Bold = true;
                tableCell.Font.Size = FontUnit.Small;
                tableCell.ForeColor = System.Drawing.Color.White;
                tableHeaderRow1.Cells.Add(tableCell);
            }
            table.Rows.Add(tableHeaderRow);
            table.Rows.Add(tableHeaderRow1);            
            for (int i = 0; i < this.lCostCenter.Count; i++)
            {
                if (!string.IsNullOrEmpty(this.lCostCenter[i].code))
                {                    
                    tableCell = new TableCell();
                    tableCell.Text = this.lCostCenter[i].code;
                    tableCell.BackColor = headercolor;
                    tableCell.Font.Bold = true;
                    tableCell.Font.Size = FontUnit.Small;
                    tableCell.ForeColor = System.Drawing.Color.White;
                    tableRow.Cells.Add(tableCell);
                    for (int j = 0; j < this.lProfessionals.Count; j++)
                    {
                        tableCell = new TableCell();
                        tableCell.HorizontalAlign = HorizontalAlign.Center;
                        if (bFind)
                        {
                            hiddenField = new HiddenField();
                            hiddenField.ID = "hdnValue_" + this.lProfessionals[j].sdocument;
                            hiddenField.Value = "0";
                            tableCell.Controls.Add(hiddenField);                            
                        }
                        textBox = new TextBox();
                        textBox.ID = "txtValue_" + this.lProfessionals[j].sdocument + "_" + this.lCostCenter[i].code;
                        textBox.Columns = 6;
                        textBox.Text = this.GetCostValue(lGeneric, this.lCostCenter[i].code, this.lProfessionals[j].sdocument);
                        textBox.EnableViewState = true;
                        tableCell.Controls.Add(textBox);
                        tableRow.Cells.Add(tableCell);
                    }
                    bFind = false;
                    table.Rows.Add(tableRow);
                    tableRow = new TableRow();
                }                
            }
            table.BorderStyle = BorderStyle.Solid;
            table.BorderWidth = 1;
            table.CellPadding = 3;
            table.BorderColor = System.Drawing.Color.Black;
            table.BackColor = System.Drawing.Color.White;
            this.plhTabla.Controls.Add(table);
        }

        /// <summary>
        /// Método que obtiene el valor del costo que viene desde Inspira
        /// </summary>
        /// <param name="lGeneric">Array con la información de los costos descargada de Inspira</param>
        /// <param name="sCost">String centro de costos</param>
        /// <param name="sDocument">String documento del empleado</param>
        /// <returns>String con el valor del centro de costo por empleado</returns>
        private string GetCostValue(WSIntegrator.Generic[] lGeneric, string sCost, string sDocument)
        {
            decimal dresult = 0;            
            WSIntegrator.Generic generic = lGeneric.FirstOrDefault(x => x.scode == sDocument && x.sfilter == sCost);
            return (generic != null) ? (generic.dextra2 / 600).ToString("F") : dresult.ToString("F");
        }

        /// <summary>
        /// Método para generar el nombre resumido del empleado
        /// </summary>
        /// <param name="sFirstName">String primer nombre</param>
        /// <param name="sLastName">String primer apellido</param>
        /// <returns>String con la primer letra del nombre y el apellido</returns>
        private string GetProfessionalName(string sFirstName, string sLastName)
        {
            StringBuilder sName = new StringBuilder(sFirstName.Substring(0, 1));
            sName.Append(sLastName);
            return sName.ToString();
        }
        /// <summary>
        /// Método para obtener el listado de empleados por centro de costo principal del coordinador
        /// </summary>
        /// <returns>Lista genérica de empleados</returns>
        private List<Employee> GetEmployees()
        {
            using (FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("DBNovasoft")))
            {
                return oFacade.GetEmployees(new Employee() { smaincostcenter = this.GetCostCenterList() });
            }
        }

        /// <summary>
        /// Método para obtener los centros de costo en lista genérica
        /// </summary>
        /// <returns>Lista genérica con los centros de costo</returns>
        private List<Entity.Generic> GetCosts()
        {
            using (FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion")))
            {
                return oFacade.GetCosts();
            }
        }

        /// <summary>
        /// Método para generar cadena de texto con los centros de costo asociados al usuario de sesión
        /// </summary>
        /// <returns>String con los centros de costo</returns>
        private string GetCostCenterList()
        {
            if (this.ddlCentro.Enabled)
            {
                return this.ddlCentro.SelectedValue;
            }
            string[] aText = new string[this.oUser.lCost.Count];
            for (int i = 0; i < aText.Length; i++)
            {
                aText[i] = this.oUser.lCost[i].scode;
            }
            return string.Join("','", aText);
        }

        /// <summary>
        /// Método para filtrar el listado de centros de costo por los que vienen de Inspira únicamente
        /// </summary>
        /// <param name="lGeneric">Objeto arreglo genérico con la información que viene de Inspira</param>
        /// <returns>Objeto lista genérica con los centros de costo filtrados</returns>
        private List<Entity.Generic> FilterCosts(WSIntegrator.Generic[] lGeneric)
        {
            List<Entity.Generic> lCost = new List<Entity.Generic>();
            foreach (Entity.Generic cost in this.lCostCenter)
            {
                WSIntegrator.Generic generic = lGeneric.FirstOrDefault(x => x.sfilter == cost.code);
                if (generic != null)
                {
                    lCost.Add(cost);
                }
            }
            lCost.Add(new Entity.Generic() { code = "INVE" });
            lCost.Add(new Entity.Generic() { code = "EDDO" });
            return lCost;
        }

        /// <summary>
        /// Método para filtrar el listado de centros de costo por los que tienen los usuarios únicamente
        /// </summary>
        /// <returns>Objeto lista genérica con los centros de costo filtrados</returns>
        private List<Entity.Generic> FilterCosts()
        {
            List<Entity.Generic> lCost = new List<Entity.Generic>();
            foreach (Cost cost in this.lProcess[0].lCost)
            {
                lCost.Add(new Entity.Generic() { code = cost.scode, name = cost.sname });
            }
            return lCost;
        }


        private Control FindChildControl(Control start, string id)
        {
            if (start != null)
            {
                Control foundControl;
                foundControl = start.FindControl(id);
                if (foundControl != null)
                {
                    return foundControl;
                }
                foreach (Control c in start.Controls)
                {
                    foundControl = FindChildControl(c, id);
                    if (foundControl != null)
                    {
                        return foundControl;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Evento del botón guardar
        /// </summary>
        /// <param name="sender">Botón Guardar</param>
        /// <param name="e">Argumentos evento</param>
        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            this.lError = new List<FNCEntity.Generic>();
            this.GetTableValues();
            this.ValidateTable();
            if (this.lError.Count > 0)
            {
                this.mpeValidar.Show();
                this.BindGrid();
                this.RegenerateTable();
            }
            else
            {                
                this.SaveValues();
                this.pnlTabla.Visible = false;
            }
        }

        /// <summary>
        /// Evento del botón agregar nuevo centro de costos al listado
        /// </summary>
        /// <param name="sender">Botón Agregar</param>
        /// <param name="e">Argumentos evento</param>
        protected void btnAgregar_Click(object sender, ImageClickEventArgs e)
        {            
            Entity.Generic generic = new Entity.Generic()
            {
                code = this.ddlCentroCosto.SelectedValue,
                name = this.ddlCentro.SelectedItem.Text,
            };
            this.GetTableValues();
            this.lCostCenter.Add(generic);
            this.RegenerateTable();
        }        

        /// <summary>
        /// Método para llenar la grilla de errores
        /// </summary>
        private void BindGrid()
        {
            this.gvResultado.DataSource = this.lError;
            this.gvResultado.DataBind();
        }

        /// <summary>
        /// Método para validar los valores ingresados en la tabla
        /// </summary>
        private void ValidateTable()
        {
            FNCEntity.Generic generic = null;
            foreach (CostProcess costProcess in this.lProcess)
            {
                if (costProcess.dtotal > costProcess.ihours)
                {
                    generic = new FNCEntity.Generic()
                    {
                        dextra2 = Convert.ToDouble(costProcess.dtotal),
                        sname = this.GetProfessionalName(costProcess.oEmployee.sname, costProcess.oEmployee.slastname),
                    };
                    lError.Add(generic);
                }
            }
        }

        /// <summary>
        /// Método para almacenar la información del proceso en la base de datos
        /// </summary>
        private void SaveValues()
        {
            using (FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion")))
            {
                this.GetTableValues();
                oFacade.SaveCostList(this.lProcess);
                string sMessage = "La información ha sido almacenada correctamente";                
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "ShowMessage('" + sMessage + "');", true);
                this.RegenerateTable();
            }            
        }

        /// <summary>
        /// Nétodo para obtener los valores de la tabla para poder reconstruirla
        /// </summary>
        private void GetTableValues()
        {
            this.lProcess = new List<CostProcess>();
            CostProcess costProcess = null;
            Cost cost = null;
            decimal did = 0, dValue = 0;
            for (int i = 0; i < this.lProfessionals.Count; i++)
            {
                costProcess = new CostProcess()
                {
                    sdocument = this.lProfessionals[i].sdocument,
                    iuser = this.oUser.id,
                    lCost = new List<Cost>(),
                    iclosed = 0,
                    imonth = Convert.ToInt32(this.ddlMes.SelectedValue),
                    iyear = Convert.ToInt32(this.ddlAno.SelectedValue),
                    itype = 2, //1 liquida porcentaje, 2 liquida horas
                    oEmployee = this.lProfessionals[i],
                    ihours = this.lProfessionals[i].ihours,    
                    scostcenter = this.ddlCentro.SelectedValue,                                        
                };
                did = this.GetTextValue(string.Empty, this.lProfessionals[i].sdocument, false);
                if (did != 0) costProcess.iid = Convert.ToInt32(did);
                for (int j = 0; j < this.lCostCenter.Count; j++)
                {
                    if (!string.IsNullOrEmpty(this.lCostCenter[j].code))
                    {
                        dValue = this.GetTextValue(this.lCostCenter[j].code, this.lProfessionals[i].sdocument);
                        cost = new Cost()
                        {
                            scode = this.lCostCenter[j].code,
                            dvalue = dValue,
                            ctype = (this.lCostCenter[j].code == "INVE" || this.lCostCenter[j].code == "EDDO") ? 'O' : 'N',
                            dtotal = (dValue / this.lProfessionals[i].ihours) * 100,                            
                        };
                        costProcess.lCost.Add(cost);
                        costProcess.dtotal += cost.dvalue;                        
                    }                    
                }
                this.lProcess.Add(costProcess);
            }
        }

        /// <summary>
        /// Método que obtiene el valor de un campo de texto dinámico de la tabla de costos
        /// </summary>
        /// <param name="sCost">String código del centro de costos</param>
        /// <param name="sDocument">String documento del empleado</param>
        /// <returns>Decimal valor ingresado en el campo</returns>
        private decimal GetTextValue(string sCost, string sDocument, bool bSearch = true)
        {
            string sValue = string.Empty;
            if (bSearch) sValue = Request.Form.AllKeys.Where(key => key.Contains("txtValue_" + sDocument + "_" + sCost)).FirstOrDefault();
            else sValue = Request.Form.AllKeys.Where(key => key.Contains("hdnValue_" + sDocument)).FirstOrDefault();
            if (!string.IsNullOrEmpty(sValue))
            {
                return (!string.IsNullOrEmpty(Request.Form[sValue])) ? Convert.ToDecimal(Request.Form[sValue]) : 0;
            }
            return 0;            
        }

        /// <summary>
        /// Método para regenerar la tabla con los centros de costo
        /// </summary>
        private void RegenerateTable()
        {
            bool bFind = true;
            Table table = new Table();
            table.ID = "tblCentros";
            table.EnableViewState = true;
            var headercolor = System.Drawing.ColorTranslator.FromHtml("#006699");
            TableHeaderRow tableHeaderRow = new TableHeaderRow();
            TableHeaderRow tableHeaderRow1 = new TableHeaderRow();
            TableRow tableRow = new TableRow();
            TextBox textBox = null;
            TableCell tableCell = new TableCell();
            HiddenField hiddenField = null;
            tableCell.Text = "CC / EMPLEADO";
            tableCell.BackColor = headercolor;
            tableCell.Font.Bold = true;
            tableCell.Font.Size = FontUnit.Small;
            tableCell.ForeColor = System.Drawing.Color.White;
            tableHeaderRow.Cells.Add(tableCell);
            tableCell = new TableCell();
            tableCell.Text = " ";
            tableCell.BackColor = headercolor;
            tableCell.Font.Bold = true;
            tableCell.Font.Size = FontUnit.Small;
            tableHeaderRow1.Cells.Add(tableCell);
            foreach (CostProcess costProcess in this.lProcess)
            {
                tableCell = new TableCell();
                tableCell.Text = this.GetProfessionalName(costProcess.oEmployee.sname, costProcess.oEmployee.ssurname);
                tableCell.HorizontalAlign = HorizontalAlign.Center;
                tableCell.BackColor = headercolor;
                tableCell.Font.Bold = true;
                tableCell.Font.Size = FontUnit.Small;
                tableCell.ForeColor = System.Drawing.Color.White;
                tableHeaderRow.Cells.Add(tableCell);
                tableCell = new TableCell();
                tableCell.Text = costProcess.ihours.ToString();
                tableCell.HorizontalAlign = HorizontalAlign.Center;
                tableCell.BackColor = headercolor;
                tableCell.Font.Bold = true;
                tableCell.Font.Size = FontUnit.Small;
                tableCell.ForeColor = System.Drawing.Color.White;
                tableHeaderRow1.Cells.Add(tableCell);
            }            
            table.Rows.Add(tableHeaderRow);
            table.Rows.Add(tableHeaderRow1);
            for (int i = 0; i < this.lCostCenter.Count ; i++)
            {
                if (!string.IsNullOrEmpty(this.lCostCenter[i].code))
                {
                    tableCell = new TableCell();
                    tableCell.Text = this.lCostCenter[i].code;
                    tableCell.BackColor = headercolor;
                    tableCell.Font.Bold = true;
                    tableCell.Font.Size = FontUnit.Small;
                    tableCell.ForeColor = System.Drawing.Color.White;
                    tableRow.Cells.Add(tableCell);
                    foreach (CostProcess costProcess in this.lProcess)
                    {
                        tableCell = new TableCell();
                        tableCell.HorizontalAlign = HorizontalAlign.Center;
                        if (bFind)
                        {
                            hiddenField = new HiddenField();
                            hiddenField.ID = "hdnValue_" + costProcess.sdocument;
                            hiddenField.Value = this.FindValueInList(this.lCostCenter[i].code, costProcess.sdocument, false);
                            tableCell.Controls.Add(hiddenField);
                            tableRow.Enabled = this.IsClosed(costProcess.oEmployee.sdocument);
                        }
                        textBox = new TextBox();
                        textBox.ID = "txtValue_" + costProcess.sdocument + "_" + this.lCostCenter[i].code;
                        textBox.Columns = 6;
                        textBox.Text = this.FindValueInList(this.lCostCenter[i].code, costProcess.sdocument);
                        textBox.EnableViewState = true;
                        tableCell.Controls.Add(textBox);
                        tableRow.Cells.Add(tableCell);
                    }
                    table.Rows.Add(tableRow);
                    tableRow = new TableRow();
                    bFind = false;
                }                
            }
            table.BorderStyle = BorderStyle.Solid;
            table.BorderWidth = 1;
            table.CellPadding = 3;
            table.BorderColor = System.Drawing.Color.Black;
            table.BackColor = System.Drawing.Color.White;
            this.plhTabla.Controls.Add(table);
        }

        /// <summary>
        /// Método que obtiene el valor del centro de costo por documento del empleado en el listado del proceso de liquidación
        /// </summary>
        /// <param name="sCost">String centro de costo</param>
        /// <param name="sDocument">String documento del empleado</param>
        /// <param name="bSearch">Boolean campo para indicar si se trae el valor del costo o el id del proceso</param>
        /// <returns>String valor encontrado</returns>
        private string FindValueInList(string sCost, string sDocument, bool bSearch = true)
        {
            decimal dresult = 0;
            CostProcess costProcess = this.lProcess.FirstOrDefault(x => x.sdocument == sDocument);
            if (costProcess == null)
            {
                return dresult.ToString("F");
            }
            else 
            {
                if (!bSearch)
                {
                    return costProcess.iid.ToString();
                }
                else
                {
                    Cost cost = costProcess.lCost.FirstOrDefault(x => x.scode == sCost);
                    return (cost != null) ? cost.dvalue.ToString("F") : dresult.ToString("F");
                }                                            
            }
        }

        /// <summary>
        /// Método que verifica si el proceso para um empleado ya se encuentra cerrado
        /// </summary>
        /// <param name="sDocument">String documento del empleado</param>
        /// <returns>Boolean que indica si el proceso se encuentra o no cerrado</returns>
        private bool IsClosed(string sDocument)
        {
            CostProcess costProcess = this.lProcess.FirstOrDefault(x => x.sdocument == sDocument);
            return (costProcess == null) ? false : (costProcess.iclosed == 0);            
        }

        /// <summary>
        /// Método para traer al empleado desde su documento de la lista de empleados
        /// </summary>
        /// <param name="sDocument">String número de documento</param>
        /// <returns>Objeto Empleado primer encontrado</returns>
        private Employee GetProfessional(string sDocument)
        {
            return this.lProfessionals.FirstOrDefault(x => x.sdocument == sDocument);
        }

        /// <summary>
        /// Evento botón continuar a pesar de los errores de validación
        /// </summary>
        /// <param name="sender">Botón aceptar</param>
        /// <param name="e">Argumentos evento</param>
        protected void imbAceptar_Click(object sender, ImageClickEventArgs e)
        {
            this.SaveValues();
            Response.Redirect("~/CostoXMedico.aspx");
        }
    }
}