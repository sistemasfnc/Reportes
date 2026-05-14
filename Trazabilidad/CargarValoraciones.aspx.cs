using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using FNCEntity;
using EventLog;
using Utils;
using System.IO;
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

namespace Trazabilidad
{
    public partial class CargarValoraciones : System.Web.UI.Page
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
                        stype = "Valoraciones",
                    };
                    oServinte = new FacadeInspiraServinte(Configuration.GetStringValue("ServinteOracle"));
                    oServinte.sConnection2 = Configuration.GetStringValue("OracleIntegra");
                    inspiraServinteResponse = oServinte.GenerateEntry(inspiraRequest, true);
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

        protected void FinishButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/CargarValoraciones.aspx");
        }

        protected void gvIngresos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvIngresos.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void gvResultado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvResultado.PageIndex = e.NewPageIndex;
            this.gvResultado.DataSource = this.lEntryResponse;
            this.gvResultado.DataBind();
        }

        protected void imbExportar_Click(object sender, ImageClickEventArgs e)
        {
            ExcelPackage oExcel = new ExcelPackage();
            ExcelWorksheet ws = oExcel.Workbook.Worksheets.Add("Ingresos");
            //ws.Cells["A1"].LoadFromDataTable(Utils.Tools.ToDataTable(this.lPatient) , true);
            ws.Cells["A1"].LoadFromDataTable(this.CreateDataTable(), true);
            Response.AddHeader("content-disposition", "attachment; filename=ingresosvarios.xlsx");
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
            Paquete paquete = this.lPackages.FirstOrDefault(x => x.scentro == scostcenter && x.sconcepto == sconcept && x.starifa == srate && x.sservicio == sproduct);
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
            else if ((!Utils.Tools.ValidateDates(Convert.ToDateTime(values[7]), DateTime.Now)))
            {
                return "La fecha de nacimiento debe ser menor igual al día de hoy";
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
            else if (!values[42].EqualsAnyOf("C"))
            {
                return "Tipo de usuario es incorrecto";
            }
            else if (!this.ExistsInList("lCountry", "code", values[43]))
            {
                return "País de expedición del documento incorrecto";
            }
            else if (this.ddlTipo.SelectedValue == "Si" && !string.IsNullOrEmpty(values[44]))
            {
                if (!Utils.Tools.IsNumeric(values[44]))
                    return "El ingreso origen debe ser un número";
            }
            return string.Empty;
        }

        private void LoadProductRates()
        {
            using (FacadeCargo facadeCargo = new FacadeCargo(Configuration.GetStringValue("ServinteIntegra")))
            {
                this.lPackages = facadeCargo.GetProductRates();
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
            List<string[]> lstrings = new List<string[]>();
            try
            {
                ms = new MemoryStream(this.fuArchivo.FileBytes);
                lines = Encoding.UTF8.GetString(ms.ToArray()).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string item in lines)
                {
                    values = item.Replace(',', ';').Split(';');
                    lstrings.Add(values);
                }
                (from string[] tmp in lstrings
                 group tmp by tmp[0] + tmp[1] into f
                 select new
                 {
                     key = f.Key,
                     Elements = f,
                 }).ToList().ForEach(f =>
                 {
                     oPatient = new ServintePatient()
                     {
                         sdocumenttype = FNCUtils.Tools.GetDocumentType(f.Elements.First()[0].ToUpper(), false),
                         sdocument = f.Elements.First()[1],
                         sfirstname = f.Elements.First()[2].ToUpper(),
                         ssecondname = f.Elements.First()[3].ToUpper(),
                         ssurname = f.Elements.First()[4].ToUpper(),
                         ssecondsurname = f.Elements.First()[5].ToUpper(),
                         sgender = f.Elements.First()[6].ToUpper(),
                         dbirthdate = Convert.ToDateTime(f.Elements.First()[7]),
                         sbornplace = f.Elements.First()[8],
                         smaritalstatus = f.Elements.First()[9].ToUpper(),
                         scellphone = f.Elements.First()[10],
                         sneighborhood = f.Elements.First()[11],
                         surbanzone = f.Elements.First()[12].ToUpper(),
                         sjob = f.Elements.First()[13].ToUpper(),
                         saddress = f.Elements.First()[14].ToUpper(),
                         smail = f.Elements.First()[16],
                         safiliation = f.Elements.First()[17],
                         slevel = f.Elements.First()[18],
                         sphone = f.Elements.First()[19],
                         snation = f.Elements.First()[20],
                         scity = f.Elements.First()[21],
                         scityname = f.Elements.First()[22],
                         scovid1 = f.Elements.First()[23].ToUpper(),
                         scovid2 = f.Elements.First()[24].ToUpper(),
                         sagreementcode = f.Elements.First()[28],
                         ssourcecountry = f.Elements.First()[43],
                         sissuingentity = (f.Elements.First()[43] == "169") ? "REGISTRADURÍA NACIONAL DEL ESTADO CIVIL" : string.Empty,
                         spolicy = f.Elements.First()[1],
                         lappointments = new List<InspiraCita>(),
                     };
                     (from string[] tmp in f.Elements
                      where tmp[0] == f.Elements.First()[0] && tmp[1] == f.Elements.First()[1]
                      group tmp by Convert.ToDateTime(tmp[34]) into a
                      select new
                      {
                          Key = a.Key,
                          Elements = a,
                      }).ToList().ForEach(a =>
                      {
                          inspiraCita = new InspiraCita()
                          {
                              sattentiontype = a.Elements.First()[25],
                              sservicetype = a.Elements.First()[26],
                              sagreementtype = a.Elements.First()[27],
                              sagreement = a.Elements.First()[28],
                              sagreementname = a.Elements.First()[29],
                              srate = a.Elements.First()[30],
                              sratename = a.Elements.First()[31],
                              sunit = a.Elements.First()[32],
                              sthird = a.Elements.First()[33],
                              ddate = Convert.ToDateTime(a.Elements.First()[34]),
                              sauthorization = a.Elements.First()[35],
                              splan = a.Elements.First()[36],
                              lservices = new List<ServiceRequest>(),
                              scie10 = "Z000",
                              scontract = string.Empty,
                              sattendingtype = a.Elements.First()[42],
                              sname = !string.IsNullOrEmpty(a.Elements.First()[44]) ? a.Elements.First()[44] : string.Empty,
                          };
                          if (this.ddlTipo.SelectedValue == "Si" && !string.IsNullOrEmpty(a.Elements.First()[44]))
                          {
                              inspiraCita.ientrysource = Convert.ToInt32(a.Elements.First()[44]);
                          }
                          (from string[] tmp in a.Elements
                           where tmp[0] == f.Elements.First()[0] &&
                                 tmp[1] == f.Elements.First()[1] &&
                                 DateTime.Equals(Convert.ToDateTime(tmp[34]), Convert.ToDateTime(a.Elements.First()[34]))
                           select tmp).ToList().ForEach(tmp =>
                           {
                               serviceRequest = new ServiceRequest()
                               {
                                   sauthorization = tmp[35],
                                   sconcept = tmp[37],
                                   scostcenter = tmp[38],
                                   srate = tmp[30],
                                   sservice = tmp[39],
                                   sservicename = tmp[40],
                                   iqty = Convert.ToInt32(tmp[41]),
                                   bbilleable = true,
                                   //ivalue = Convert.ToInt32(values[42]),
                                   ivalue = this.LoadProductValue(tmp[38], tmp[37], tmp[39], tmp[30]),
                                   bisprocedure = !tmp[40].Contains("CONSULTA"),
                               };
                               if (serviceRequest.ivalue == 0)
                               {
                                   throw new ApplicationException("El valor del servicio para el paciente " + oPatient.sdocument + " en la fecha " + inspiraCita.ddate.ToString("yyyy-MM-dd") + " es 0, por favor revisar el maestro de paquetes.");
                               }
                               inspiraCita.lservices.Add(serviceRequest);
                           });
                          oPatient.lappointments.Add(inspiraCita);
                      });
                     lPatient.Add(oPatient);
                 });
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
    }
}