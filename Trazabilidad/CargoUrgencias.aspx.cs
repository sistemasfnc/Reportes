using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Text;
using FNCDAC;
using Config;
using OfficeOpenXml;
using System.Data;
using Entity;
using System.Web.Services.Description;
using FNCUtils;
using FNCFacade;
using Facade;
using FNCEntity;
using EventLog;
using Utils;
using System.IO;

namespace Trazabilidad
{
    public partial class CargoUrgencias : System.Web.UI.Page
    {
        /// <summary>
        /// Nombre del archivo seleccionado para cargar
        /// </summary>
        private string FileName
        {
            get { return ViewState["FileName"].ToString(); }
            set { ViewState["FileName"] = value; }
        }

        /// <summary>
        /// Lista de pacientes cargados
        /// </summary>
        private List<ServintePatient> lPatient
        {
            get { return (ViewState["lPatient"] != null) ? (List<ServintePatient>)ViewState["lPatient"] : new List<ServintePatient>(); }
            set { ViewState["lPatient"] = value; }
        }

        /// <summary>
        /// Usuario logueado
        /// </summary>
        private Entity.User oUser
        {
            get { return Session["oUser"] as Entity.User; }
        }


        /// <summary>
        /// Lista de ciudades
        /// </summary>
        private List<Entity.Generic> lCity
        {
            get { return (ViewState["lCity"] != null) ? (List<Entity.Generic>)ViewState["lCity"] : new List<Entity.Generic>(); }
            set { ViewState["lCity"] = value; }
        }

        /// <summary>
        /// Lista de países
        /// </summary>
        private List<Entity.Generic> lCountry
        {
            get { return (ViewState["lCountry"] != null) ? (List<Entity.Generic>)ViewState["lCountry"] : new List<Entity.Generic>(); }
            set { ViewState["lCountry"] = value; }
        }

        /// <summary>
        /// Lista de centros de costo
        /// </summary>
        private List<Entity.Generic> lCostCenter
        {
            get { return (ViewState["lCostCenter"] != null) ? (List<Entity.Generic>)ViewState["lCostCenter"] : new List<Entity.Generic>(); }
            set { ViewState["lCostCenter"] = value; }
        }

        /// <summary>
        /// Lista de conceptos
        /// </summary>
        private List<Entity.Generic> lConcept
        {
            get { return (ViewState["lConcept"] != null) ? (List<Entity.Generic>)ViewState["lConcept"] : new List<Entity.Generic>(); }
            set { ViewState["lConcept"] = value; }
        }

        /// <summary>
        /// Lista de Barrios
        /// </summary>
        private List<Entity.Generic> lNeightboor
        {
            get { return (ViewState["lNeightboor"] != null) ? (List<Entity.Generic>)ViewState["lNeightboor"] : new List<Entity.Generic>(); }
            set { ViewState["lNeightboor"] = value; }
        }

        /// <summary>
        /// Lista de Convenios
        /// </summary>
        private List<Entity.Generic> lAgreement
        {
            get { return (ViewState["lAgreement"] != null) ? (List<Entity.Generic>)ViewState["lAgreement"] : new List<Entity.Generic>(); }
            set { ViewState["lAgreement"] = value; }
        }

        /// <summary>
        /// Lista de Convenios
        /// </summary>
        private List<Entity.Generic> lRate
        {
            get { return (ViewState["lRate"] != null) ? (List<Entity.Generic>)ViewState["lRate"] : new List<Entity.Generic>(); }
            set { ViewState["lRate"] = value; }
        }

        /// <summary>
        /// Lista de Planes
        /// </summary>
        private List<Entity.Generic> lPlan
        {
            get { return (ViewState["lPlan"] != null) ? (List<Entity.Generic>)ViewState["lPlan"] : new List<Entity.Generic>(); }
            set { ViewState["lPlan"] = value; }
        }

        /// <summary>
        /// Lista de Ocupaciones
        /// </summary>
        private List<Entity.Generic> lJob
        {
            get { return (ViewState["lJob"] != null) ? (List<Entity.Generic>)ViewState["lJob"] : new List<Entity.Generic>(); }
            set { ViewState["lJob"] = value; }
        }

        /// <summary>
        /// Lista de tipos de afiliación
        /// </summary>
        private List<Entity.Generic> lMembership
        {
            get { return (ViewState["lMembership"] != null) ? (List<Entity.Generic>)ViewState["lMembership"] : new List<Entity.Generic>(); }
            set { ViewState["lMembership"] = value; }
        }

        /// <summary>
        /// Lista de tipos de nivel de afiliación
        /// </summary>
        private List<Entity.Generic> lLevel
        {
            get { return (ViewState["lLevel"] != null) ? (List<Entity.Generic>)ViewState["lLevel"] : new List<Entity.Generic>(); }
            set { ViewState["lLevel"] = value; }
        }

        /// <summary>
        /// Lista de tipos atención
        /// </summary>
        private List<Entity.Generic> lAttentionType
        {
            get { return (ViewState["lAttentionType"] != null) ? (List<Entity.Generic>)ViewState["lAttentionType"] : new List<Entity.Generic>(); }
            set { ViewState["lAttentionType"] = value; }
        }

        /// <summary>
        /// Lista de tipos de servicios
        /// </summary>
        private List<Entity.Generic> lServiceType
        {
            get { return (ViewState["lServiceType"] != null) ? (List<Entity.Generic>)ViewState["lServiceType"] : new List<Entity.Generic>(); }
            set { ViewState["lServiceType"] = value; }
        }

        private List<EntryExtended> lEntryResponse
        {
            get { return (ViewState["lEntryResponse"] != null) ? (List<EntryExtended>)ViewState["lEntryResponse"] : new List<EntryExtended>(); }
            set { ViewState["lEntryResponse"] = value; }
        }

        private List<Paquete> lPackages
        {
            get { return (ViewState["lPackages"] != null) ? (List<Paquete>)ViewState["lPackages"] : new List<Paquete>(); }
            set { ViewState["lPackages"] = value; }
        }

        /// <summary>
        /// Evento cargar de la página
        /// </summary>
        /// <param name="sender">Objeto página</param>
        /// <param name="e">Argumentos</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Utils.Tools.HaveAccess(this.oUser.lSecurity, (int)Entity.Permissions.createhospadmission))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.LoadProductRates();
            }
        }

        /// <summary>
        /// Evento botón continuar del Wizard
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void Wizard1_NextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            //Primer paso del Wizard, carga las tarifas lee el archivo y carga los pacientes
            if (e.CurrentStepIndex == 0)
            {
                this.LoadLists();
                if (this.ValidateFile())
                {
                    this.lPatient = this.GetPatients();
                    this.BindGrid();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            //Segundo paso del wizard, inserta la información en la bd y muestra resultado de la operación
            else if (e.CurrentStepIndex == 1)
            {
                FacadeInspiraServinte oServinte = null;
                InspiraRequest inspiraRequest = null;
                InspiraServinteResponse inspiraServinteResponse = null;
                try
                {
                    inspiraRequest = new InspiraRequest()
                    {
                        lpatients = this.lPatient,
                        stype = "Urgencias",
                    };
                    oServinte = new FacadeInspiraServinte(Configuration.GetStringValue("ServinteOracle"));
                    oServinte.sConnection2 = Configuration.GetStringValue("OracleIntegra");
                    inspiraServinteResponse = oServinte.GenerateEntry(inspiraRequest);
                    if (inspiraServinteResponse.error == null)
                    {
                        this.lEntryResponse = FNCUtils.Tools.ResponseToChild(inspiraServinteResponse.lentry);
                        this.gvResultado.DataSource = this.lEntryResponse;
                        this.gvResultado.DataBind();
                    }
                    else
                    {
                        throw new ApplicationException(inspiraServinteResponse.error.smessage);
                    }
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", ex);
                    throw;
                }
                finally
                {
                    oServinte.Dispose();
                    oServinte = null;
                    inspiraServinteResponse = null;
                    inspiraRequest = null;
                }
            }
        }

        /// <summary>
        /// Evento botón finalizar del Wizard
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void FinishButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/CargoUrgencias.aspx");
        }

        /// <summary>
        /// Evento paginación de la grilla de resultados
        /// </summary>
        /// <param name="sender">Objeto grilla</param>
        /// <param name="e">Argumentos</param>
        protected void gvResultado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvResultado.PageIndex = e.NewPageIndex;
            this.gvResultado.DataSource = this.lEntryResponse;
            this.gvResultado.DataBind();
        }

        /// <summary>
        /// Evento paginador grilla
        /// </summary>
        /// <param name="sender">Objeto grilla</param>
        /// <param name="e">Argumentos</param>
        protected void gvIngresos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvIngresos.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón exportar información de la grilla a Excel
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos</param>
        protected void imbExportar_Click(object sender, ImageClickEventArgs e)
        {
            ExcelPackage oExcel = new ExcelPackage();
            ExcelWorksheet ws = oExcel.Workbook.Worksheets.Add("Ingresos");
            //ws.Cells["A1"].LoadFromDataTable(Utils.Tools.ToDataTable(this.lPatient) , true);
            ws.Cells["A1"].LoadFromDataTable(this.CreateDataTable(), true);
            Response.AddHeader("content-disposition", "attachment; filename=ingresoterapias.xlsx");
            Response.Charset = string.Empty;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Clear();
            Response.Buffer = true;
            Response.ContentEncoding = System.Text.Encoding.Default;
            Response.BinaryWrite(oExcel.GetAsByteArray());
            Response.End();
            oExcel.Dispose();
            oExcel = null;
        }

        /// <summary>
        /// Retorna un texto con el estado del ingreso
        /// </summary>
        /// <param name="sError">Mensaje de error</param>
        /// <returns>Mensaje resultante</returns>
        protected string GetStatus(object sError)
        {
            return (sError != null) ? "Error: " + sError.ToString() : "Cargado OK";
        }

        /// <summary>
        /// Método que valida la información del archivo cargado
        /// </summary>
        /// <returns>Verdadero si el archivo es correcto, falso en caso contrario</returns>
        private bool ValidateFile()
        {
            if (!this.fuArchivo.HasFile)
            {
                this.lblError.Text = "El archivo no puede ser vacío";
                return false;
            }
            else
            {
                MemoryStream ms = new MemoryStream(this.fuArchivo.FileBytes);
                string[] lines = Encoding.UTF8.GetString(ms.ToArray()).Trim(new char[] { '"' }).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if (lines.Length == 0)
                {
                    this.lblError.Text = "El archivo cargado no contiene registros";
                    return false;
                }
                else
                {
                    string svalidate = string.Empty;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        svalidate = this.ValidateColumns(lines[i].Replace(',', ';').Split(';'));
                        if (!string.IsNullOrEmpty(svalidate))
                        {
                            this.lblError.Text = "Error en la fila " + (i + 1).ToString() + ": " + svalidate;
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Método para verificar un campo en una lista genérica
        /// </summary>
        /// <param name="slist">String id de la lista genérica</param>
        /// <param name="sfield">String nombre del campo</param>
        /// <param name="svalue">String valor del campo</param>
        /// <returns>Boolean verdader si el valor se encuentra en la lista o falso en caso contrario</returns>
        private bool ExistsInList(string slist, string sfield, string svalue)
        {
            List<Entity.Generic> lgeneric = ViewState[slist] as List<Entity.Generic>;
            Entity.Generic generic = null;
            generic = (sfield == "code") ? lgeneric.FirstOrDefault(x => x.code.Trim() == svalue.Trim()) : lgeneric.FirstOrDefault(x => x.name.Trim() == svalue.Trim());
            return (generic != null);
        }

        /// <summary>
        /// Método para obtener el valor del servicio
        /// </summary>
        /// <param name="scostcenter"></param>
        /// <param name="sconcept"></param>
        /// <param name="sproduct"></param>
        /// <param name="srate"></param>
        /// <returns></returns>
        private int LoadProductValue(string scostcenter, string sconcept, string sproduct, string srate)
        {
            Paquete paquete = this.lPackages.FirstOrDefault(x => x.sconcepto == sconcept && x.starifa == srate && x.sservicio == sproduct);
            return (paquete != null) ? paquete.ivalor : 0;
        }

        /// <summary>
        /// Valida las columnas de una línea del archivo cargado
        /// </summary>
        /// <param name="values">Arreglo con columnas de la línea</param>
        /// <returns>Mensaje de error</returns>
        private string ValidateColumns(string[] values)
        {
            if (string.IsNullOrEmpty(values[0]))
            {
                return "El tipo de documento no puede ser vacío";
            }
            else if (!Utils.Tools.IsDocumentType(values[0].Trim().ToUpper()))
            {
                return "EL tipo de documento es incorrecto";
            }
            else if (string.IsNullOrEmpty(values[1]))
            {
                return "El documento no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(values[2]))
            {
                return "El primer nombre no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(values[4]))
            {
                return "El primer apellido no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(values[6]))
            {
                return "El género no puede ser vacío";
            }
            else if (!Utils.Tools.IsDate(values[7]))
            {
                return "La fecha de nacimiento es incorrecta";
            }
            else if (!this.ExistsInList("lCity", "code", values[8]))
            {
                return "Lugar de nacimiento incorrecto";
            }
            else if (!values[9].EqualsAnyOf("C", "S"))
            {
                return "Estado civil incorrecto";
            }
            else if (!this.ExistsInList("lNeightboor", "code", values[11]))
            {
                return "Barrio incorrecto";
            }
            else if (!values[12].EqualsAnyOf("U", "R"))
            {
                return "Zona incorrecta";
            }
            else if (!this.ExistsInList("lJob", "code", values[13]))
            {
                return "Profesión incorrecta";
            }
            else if (!this.ExistsInList("lMembership", "code", values[17]))
            {
                return "Tipo de afiliación incorrecto";
            }
            else if (!this.ExistsInList("lLevel", "code", values[18]))
            {
                return "Nivel incorrecto";
            }
            else if (!this.ExistsInList("lCountry", "code", values[20]))
            {
                return "País incorrecto";
            }
            else if (!this.ExistsInList("lCity", "code", values[21]) || !this.ExistsInList("lCity", "name", values[22]))
            {
                return "Ciudad de residencia incorrecta";
            }
            else if (!values[23].EqualsAnyOf("S", "N"))
            {
                return "Indicador de covid incorrecto";
            }
            else if (!values[24].EqualsAnyOf("S", "N"))
            {
                return "Indicador de covid incorrecto";
            }
            else if (!this.ExistsInList("lAttentionType", "code", values[25]))
            {
                return "Tipo de atención incorrecto";
            }
            else if (!this.ExistsInList("lServiceType", "code", values[26]))
            {
                return "Tipo de servicio";
            }
            else if (!values[27].EqualsAnyOf("E", "P"))
            {
                return "Empresa o particular incorrecto";
            }
            else if (!this.ExistsInList("lAgreement", "code", values[28]) || !this.ExistsInList("lAgreement", "name", values[29]))
            {
                return "Convenio incorrecto";
            }
            else if (!this.ExistsInList("lRate", "code", values[30]) || !this.ExistsInList("lRate", "name", values[31]))
            {
                return "Tarifa incorrecta";
            }
            else if (!values[32].EqualsAnyOf("1100", "1200"))
            {
                return "Unidad funcional incorrecta";
            }
            /*else if (string.IsNullOrEmpty(values[33]))
            {
                return "El documento del tercero no puede ser vacío";
            }*/
            else if (!Utils.Tools.IsDate(values[34]))
            {
                return "La fecha de atención es incorrecta";
            }
            /*else if (Convert.ToDateTime(values[34]).Month != DateTime.Now.Month)
            {
                return "El mes de atención no puede ser diferente al mes actual";
            }*/
            else if (!this.ExistsInList("lPlan", "code", values[36]))
            {
                return "Plan incorrecto";
            }
            else if (!this.ExistsInList("lConcept", "code", values[37]))
            {
                return "Concepto incorrecto";
            }
            else if (!this.ExistsInList("lCostCenter", "code", values[38]))
            {
                return "Centro de costos incorrecto";
            }
            else if (string.IsNullOrEmpty(values[39]))
            {
                return "El código del servicio no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(values[40]))
            {
                return "El nombre del servicio no puede ser vacío";
            }
            else if (!Utils.Tools.IsNumeric(values[41]))
            {
                return "La cantidad debe ser un número";
            }
            else if (!values[42].EqualsAnyOf("U", "H"))
            {
                return "Tipo de usuario es incorrecto";
            }
            return string.Empty;
        }

        /// <summary>
        /// Método que genera el listado de pacientes a cargar por medio del archivo plano
        /// </summary>
        /// <returns>Lista genérica objeto Paciente</returns>
        private List<ServintePatient> GetPatients()
        {
            List<ServintePatient> lPatient = new List<ServintePatient>();
            MemoryStream ms = null;
            string[] lines = null;
            ServintePatient oPatient = null;
            InspiraCita inspiraCita = null;
            ServiceRequest serviceRequest = null;
            string[] values = null;
            try
            {
                ms = new MemoryStream(this.fuArchivo.FileBytes);
                lines = Encoding.UTF8.GetString(ms.ToArray()).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string item in lines)
                {
                    values = item.Replace(',', ';').Split(';');
                    inspiraCita = new InspiraCita()
                    {
                        sattentiontype = values[25],
                        sservicetype = values[26],
                        sagreementtype = values[27],
                        sagreement = values[28],
                        sagreementname = values[29],
                        srate = values[30],
                        sratename = values[31],
                        sunit = values[32],
                        sthird = values[33],
                        ddate = Convert.ToDateTime(values[34]),
                        sauthorization = values[35],
                        splan = values[36],
                        scostcenter = values[38],
                        lservices = new List<ServiceRequest>(),
                        scie10 = "Z000",
                        sservicegroup = values[42],
                    };
                    serviceRequest = new ServiceRequest()
                    {
                        sauthorization = values[35],
                        sconcept = values[37],
                        scostcenter = values[38],
                        srate = values[30],
                        sservice = values[39],
                        sservicename = values[40],
                        iqty = Convert.ToInt32(values[41]),
                        bbilleable = true,
                        //ivalue = Convert.ToInt32(values[42]),
                        ivalue = this.LoadProductValue(values[38], values[37], values[39], values[30]),
                        bisprocedure = true,
                    };
                    inspiraCita.lservices.Add(serviceRequest);
                    oPatient = new ServintePatient()
                    {
                        sdocumenttype = FNCUtils.Tools.GetDocumentType(values[0].ToUpper(), false),
                        sdocument = values[1],
                        sfirstname = values[2].ToUpper(),
                        ssecondname = values[3].ToUpper(),
                        ssurname = values[4].ToUpper(),
                        ssecondsurname = values[5].ToUpper(),
                        sgender = values[6].ToUpper().Trim(),
                        dbirthdate = Convert.ToDateTime(values[7]),
                        sbornplace = values[8],
                        smaritalstatus = values[9].ToUpper(),
                        scellphone = values[10],
                        sneighborhood = values[11],
                        surbanzone = values[12].ToUpper(),
                        sjob = values[13].ToUpper(),
                        saddress = values[14].ToUpper(),
                        smail = values[16],
                        safiliation = values[17],
                        slevel = values[18],
                        sphone = values[19],
                        snation = values[20],
                        scity = values[21],
                        scityname = values[22],
                        scovid1 = values[23].ToUpper(),
                        scovid2 = values[24].ToUpper(),
                        sagreementcode = values[28],
                        lappointments = new List<InspiraCita>(),
                    };
                    oPatient.lappointments.Add(inspiraCita);
                    lPatient.Add(oPatient);
                }
                return lPatient;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                ms.Dispose();
                ms = null;
                lines = null;
                values = null;
                oPatient = null;
            }
        }
       

        /// <summary>
        /// Crea datatable para el archivo de excel
        /// </summary>
        /// <returns></returns>
        private DataTable CreateDataTable()
        {
            DataTable dt = new DataTable();
            object[] values = new object[12] { "Tipo de documento", "Documento", "Plan", "Tarifa", "Centro de Costos", "Concepto", "Autorización", "Servicio", "Cantidad", "Valor", "Ingreso", "Cargo" };
            for (int i = 0; i < values.Length; i++)
            {
                dt.Columns.Add(values[i].ToString());
            }
            foreach (EntryExtended item in this.lEntryResponse)
            {
                values = new object[12]
                {
                    item.sdocumenttype,
                    item.sdocument,
                    item.splan,
                    item.srate,
                    item.scostcenter,
                    item.sconcept,
                    item.sauthorization,
                    item.sservice,
                    item.iqty,
                    item.dvalue,
                    item.ientry,
                    item.icharge,
                };
                dt.Rows.Add(values);
            }
            return dt;
        }

        /// <summary>
        /// Método que llena la grilla de pacientes a cargar
        /// </summary>
        private void BindGrid()
        {
            this.gvIngresos.DataSource = this.lPatient;
            this.gvIngresos.DataBind();
        }


        /// <summary>
        /// Método para cargar las listas de campos para validar
        /// </summary>
        private void LoadLists()
        {
            FacadeCargo facadeCargo = null;
            List<Entity.Generic> lgeneric = new List<Entity.Generic>();
            try
            {
                facadeCargo = new FacadeCargo(Configuration.GetStringValue("ServinteIntegra"));
                lgeneric = facadeCargo.GetGenericTable();
                this.lCity = lgeneric.FindAll(x => x.table == "CIUDADES");
                this.lAgreement = lgeneric.FindAll(x => x.table == "EMPRESAS");
                this.lCountry = lgeneric.FindAll(x => x.table == "PAISES");
                this.lCostCenter = lgeneric.FindAll(x => x.table == "CENTROS");
                this.lConcept = lgeneric.FindAll(x => x.table == "CONCEPTOS");
                this.lNeightboor = lgeneric.FindAll(x => x.table == "BARRIOS");
                this.lRate = lgeneric.FindAll(x => x.table == "TARIFAS");
                this.lPlan = lgeneric.FindAll(x => x.table == "PLANES");
                this.lJob = lgeneric.FindAll(x => x.table == "OCUPACIONES");
                this.lMembership = lgeneric.FindAll(x => x.table == "AFILIACIONES");
                this.lLevel = lgeneric.FindAll(x => x.table == "NIVELES");
                this.lAttentionType = lgeneric.FindAll(x => x.table == "ATENCIONES");
                this.lServiceType = lgeneric.FindAll(x => x.table == "SERVICIOS");
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                throw;
            }
            finally
            {
                facadeCargo.Dispose();
                facadeCargo = null;
            }
        }

        private void LoadProductRates()
        {
            using (FacadeCargo facadeCargo = new FacadeCargo(Configuration.GetStringValue("ServinteIntegra")))
            {
                this.lPackages = facadeCargo.GetProductRates();
            }
        }
    }
}