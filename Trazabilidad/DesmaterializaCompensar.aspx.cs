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
using Config;
using System.IO;
using System.Text;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Microsoft.Ajax.Utilities;
using System.Diagnostics;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Trazabilidad.clases;
using FluentFTP;
using AjaxControlToolkit.HTMLEditor.ToolbarButton;
using FluentFTP.Helpers;
using System.Net;
using System.Security.Authentication;
using FluentFTP.Exceptions;
using System.Net.Sockets;
using static System.Net.WebRequestMethods;
using Renci.SshNet;
using System.Web.Services.Description;

namespace Trazabilidad
{
    public partial class DesmaterializaCompensar : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        private string sRelation
        {
            get { return ViewState["sRelation"].ToString(); }
            set { ViewState["sRelation"] = value; }
        }

        private bool bSupports
        {
            get { return Convert.ToBoolean(ViewState["bSupports"]); }
            set { ViewState["bSupports"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.compensardematerialize))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.BindGrid();
            }
        }

        /// <summary>
        /// Eventos al hacer clic en un botón de la grilla
        /// </summary>
        /// <param name="sender">Objeto grid view</param>
        /// <param name="e">Evento del grid</param>
        protected void gvRelaciones_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
            if (gr != null)
            {
                string scompany = this.gvRelaciones.DataKeys[gr.RowIndex]["sempresa"].ToString();
                this.sRelation = this.gvRelaciones.DataKeys[gr.RowIndex]["snumero"].ToString();
                if (e.CommandName == "Generar")
                {
                    this.ShowModal(scompany);
                    //this.mpeSoportes.Show();
                    /*this.GenerateFiles();
                    this.BindGrid();*/
                }
                else if (e.CommandName == "GenerarSura")
                {
                    //this.ShowModal(scompany);
                    this.GenerateSura();
                }
                else if (e.CommandName == "GenerarSanitas")
                {
                    this.GenerateSanitas();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarSuramericana")
                {
                    this.GenerateSuramericana();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarBolivar")
                {
                    this.Generate2885();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarAxaColpatria")
                {
                    this.GenerateAxaColpatria();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarCoomevaPrepagada")
                {
                    this.GenerateSanitas();
                    //this.GenerateCoomevaPrepagada();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarPositiva")
                {
                    this.GeneratePositiva();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarFamisanar")
                {
                    this.GenerateFamisanar();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarSaludTotal")
                {
                    this.GenerarSaludTotal();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarEcopetrol")
                {
                    this.GenerarSaludTotal();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarColmenaARL")
                {
                    this.GenerateColmenaARL();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarColsanitas")
                {
                    this.GenerateColsanitas();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarMedplus")
                {
                    this.GenerateMedPlus();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarNuevaEPS")
                {
                    this.GenerateNuevaEPS();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarAliansalud")
                {
                    this.GenerateColmedica();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarColmedica")
                {
                    this.GenerateColmedica();
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarArmada")
                {
                    this.Generate2885(scompany);
                    this.BindGrid();
                }
                else if (e.CommandName == "GenerarPolicia")
                {
                    this.GeneratePolicia();
                    this.BindGrid();
                }
                else if (e.CommandName == "VerArchivos")
                {
                    this.BindFilesGrid();
                    this.mpeValidar.Show();
                }
                else if (e.CommandName == "Generar2885")
                {
                    this.Generate2885(scompany);
                    this.BindGrid();
                }
                else if (e.CommandName == "Cargar")
                {
                    try
                    {
                        string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
                        if (Directory.Exists(directory))
                        {
                            string file = Path.Combine(directory, "SOPORTES" + this.sRelation + ".zip");
                            if (System.IO.File.Exists(file))
                            {
                                this.UploadCompensarFile(file, "SOPORTES" + this.sRelation + ".zip");
                                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivo de soportes cargado correctamente');", true);
                            }
                            else
                            {
                                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivo de soportes no encontrado');", true);
                            }
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivo de soportes no encontrado');", true);
                        }
                        this.BindGrid();
                    }
                    catch (Exception ex)
                    {
                        LogError.WriteError("Facturacion", "Aplicacion", ex);
                        throw;
                    }
                }
                else if (e.CommandName == "CargarSaludTotal")
                {
                    try
                    {
                        using (FacturacionSaludTotal facturacionSaludTotal = new FacturacionSaludTotal())
                        {
                            facturacionSaludTotal.sRelation = this.sRelation;
                            facturacionSaludTotal.UploadFiles();
                            ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivo de soportes cargados correctamente');", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError.WriteError("Facturacion", "Aplicacion", ex);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al cargar los archivos');", true);
                    }
                }
                else if (e.CommandName == "CargarFamisanar")
                {
                    try
                    {
                        string spath = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
                        foreach (string sfile in Directory.GetFiles(spath, "*.PDF"))
                        {
                            this.UploadFile(sfile);
                        }
                        //ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivo de soportes cargados correctamente');", true);
                        ClientScript.RegisterStartupScript(this.GetType(), string.Empty, "alert('Archivo de soportes cargados correctamente');", true);
                    }
                    catch (Exception ex)
                    {
                        LogError.WriteError("Facturacion", "Aplicacion", ex);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al cargar los archivos');", true);
                    }
                }
                else if (e.CommandName == "CargarNuevaEPS")
                {
                    try
                    {
                        this.UpoladNuevaEPSFiles();
                    }
                    catch (Exception ex)
                    {
                        LogError.WriteError("Facturacion", "Aplicacion", ex);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al cargar los archivos');", true);
                    }
                }
                else if (e.CommandName == "CargarMedplus")
                {
                    try
                    {
                        this.UploadMedPlusFiles();
                    }
                    catch (Exception ex)
                    {
                        LogError.WriteError("Facturacion", "Aplicacion", ex);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al cargar los archivos');", true);
                    }
                }

            }           
            
        }             

        protected bool EnableView(object srelation)
        {
            string spath = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), srelation.ToString());
            return Directory.Exists(spath);
        }
        
        protected void gvRelaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvRelaciones.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void btnSi_Click(object sender, EventArgs e)
        {
            this.mpeSoportes.Hide();
            this.bSupports = true;
            this.GenerateCompensar();
            this.BindGrid();
        }

        protected void btnNo_Click(object sender, EventArgs e)
        {
            this.mpeSoportes.Hide();
            this.bSupports = false;
            this.GenerateCompensar();
            this.BindGrid();
        }

        protected bool ViewCompany(object scompany, object scode)
        {
            return (scode.ToString() == scompany.ToString());
        }

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            this.mpeSoportes.Hide();
            this.GenerateSura();
        }

        private void BindFilesGrid()
        {
            string spath = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            List<string> lfiles = new List<string>();
            if (Directory.Exists(spath))
            {
                foreach (string item in Directory.GetDirectories(spath))
                {
                    lfiles.Add(item);
                }
                foreach (string f in Directory.GetFiles(spath))
                {
                    lfiles.Add(f);
                }
            }
            this.gvArchivos.DataSource = lfiles;
            this.gvArchivos.DataBind();
        }

        /// <summary>
        /// Método para llenar la grilla de relaciones de envío
        /// </summary>
        private void BindGrid()
        {
            RelacionEnvio relacionEnvio = new RelacionEnvio()
            {
                sempresa = Configuration.GetStringValue("EmpresasDesmaterializacion"),
            };
            using (FacadeDesmaterializacion facade = new FacadeDesmaterializacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                this.gvRelaciones.DataKeyNames = new string[] { "snumero", "sempresa" };
                this.gvRelaciones.DataSource = facade.GetRelationships(relacionEnvio);
                this.gvRelaciones.DataBind();
            }
        }

        /// <summary>
        /// Método que genera los soportes para el convenio Compensar
        /// </summary>
        private void GenerateCompensar()
        {
            FacturacionCompensar facturacionCompensar = new FacturacionCompensar();
            try
            {
                facturacionCompensar.lFiles = new List<string>();
                facturacionCompensar.bSupports = this.bSupports;
                facturacionCompensar.sRelation = this.sRelation;
                var ldesmaterializacion = this.GetInvoices();
                this.DeleteDirectory();
                facturacionCompensar.GenerateHeader(ldesmaterializacion);
                facturacionCompensar.GenerateDetail(ldesmaterializacion);
                facturacionCompensar.GenerateNotes(ldesmaterializacion);
                facturacionCompensar.GenerateFileIndex(ldesmaterializacion);
                facturacionCompensar.CompressFiles();
                facturacionCompensar.ZipHeader();
                this.DeleteDirectory();
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
                this.BindGrid();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);                
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);                
            }
            finally
            {
                facturacionCompensar.Dispose();
                facturacionCompensar = null;
            }
        }

        /// <summary>
        /// Método que obtiene el detalle de la relación de envío seleccionada
        /// </summary>
        /// <returns>Lista genérica con la relación de envío</returns>
        private List<Desmaterializacion> GetInvoices()
        {
            using (FacadeDesmaterializacion facade = new FacadeDesmaterializacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                return facade.GetInvoicesDetail(this.sRelation, "EV");
            }
        }

        /// <summary>
        /// Método para eliminar el directorio de los soportes generados
        /// </summary>
        private void DeleteDirectory()
        {
            string spath = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SOPORTES" + this.sRelation);
            if (Directory.Exists(spath))
            {
                try
                {
                    Directory.Delete(spath, true);
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", ex);
                }

            }
        }

        /// <summary>
        /// Método que genera los soportes para el convenio Sura
        /// </summary>
        private void GenerateSura()
        {
            FacturacionSura facturacionSura = new FacturacionSura() { sRelation = this.sRelation, stype = this.ddlPBS.SelectedValue };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSura.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
                this.BindGrid();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSura.Dispose();
                facturacionSura = null;
            }
        }

        /// <summary>
        /// Método para marcar el archivod de compensar como listo para carga en el FTP
        /// </summary>
        private void InsertFileData()
        {
            string spath = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            string sfile = string.Empty;
            if (Directory.Exists(spath))
            {
                string[] files = Directory.GetFiles(spath, "SOPORTES" + this.sRelation + ".zip");
                if (files.Length > 0)
                {
                    sfile = files[0];
                    using (FacadeDesmaterializacion facade = new FacadeDesmaterializacion(Configuration.GetStringValue("FNCFacturacion")))
                    {
                        facade.PutUploadFile(this.sRelation, sfile);
                    }
                }
            }
        }

        /// <summary>
        /// Método que muestra el modal de opciones para la generación de los soportes
        /// </summary>
        /// <param name="scompany">String código del convenio</param>
        private void ShowModal(string scompany)
        {
            this.pnlSoporte.Visible = (scompany == "21");
            this.pnlTipo.Visible = !this.pnlSoporte.Visible;
            this.mpeSoportes.Show();
        }

        /// <summary>
        /// Método que genera los soportes para el convenio Sanitas
        /// </summary>
        private void GenerateSanitas()
        {
            FacturacionSanitas facturacionSanitas = new FacturacionSanitas() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSanitas.Dispose();
                facturacionSanitas = null;
            }
        }

        private void GenerateNuevaEPS()
        {
            FacturacionNuevaEPS facturacionSanitas = new FacturacionNuevaEPS() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSanitas.Dispose();
                facturacionSanitas = null;
            }
        }

        private void GenerateSuramericana()
        {
            FacturacionSuramericana facturacionSanitas = new FacturacionSuramericana() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSanitas.Dispose();
                facturacionSanitas = null;
            }
        }

        private void GenerateAxaColpatria()
        {
            FacturacionAxaColpatria facturacionSanitas = new FacturacionAxaColpatria() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSanitas.Dispose();
                facturacionSanitas = null;
            }
        }

        private void GenerateCoomevaPrepagada()
        {
            FacturacionCoomevaPrep facturacionSanitas = new FacturacionCoomevaPrep() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSanitas.Dispose();
                facturacionSanitas = null;
            }
        }

        private void GeneratePositiva()
        {
            FacturacionPositiva facturacionSanitas = new FacturacionPositiva() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSanitas.Dispose();
                facturacionSanitas = null;
            }
        }

        private void GenerateBolivar()
        {
            FacturacionBolivar facturacionSanitas = new FacturacionBolivar() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSanitas.Dispose();
                facturacionSanitas = null;
            }
        }

        private void GenerarSaludTotal()
        {
            FacturacionSaludTotal facturacionSanitas = new FacturacionSaludTotal() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionSanitas.GenerateFiles(ldesmaterializacion);               
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionSanitas.Dispose();
                facturacionSanitas = null;
            }
        }
        

        private void GenerateFamisanar()
        {
            FacturacionFamisanar facturacionFamisanar = new FacturacionFamisanar() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionFamisanar.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);                
            }
            finally
            {
                facturacionFamisanar.Dispose();
                facturacionFamisanar = null;
            }
        }

        private void GenerateColmenaARL()
        {
            FacturacionColmenaARL facturacionColmenaARL = new FacturacionColmenaARL() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionColmenaARL.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionColmenaARL.Dispose();
                facturacionColmenaARL = null;
            }
        }

        private void GenerateColsanitas()
        {
            FacturacionColsanitas facturacionColsanitas = new FacturacionColsanitas() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionColsanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionColsanitas.Dispose();
                facturacionColsanitas = null;
            }
        }

        private void GenerateMedPlus()
        {
            FacturacionMedplus facturacionColsanitas = new FacturacionMedplus() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionColsanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionColsanitas.Dispose();
                facturacionColsanitas = null;
            }
        }

        private void GenerateColmedica()
        {
            FacturacionColmedica facturacionColsanitas = new FacturacionColmedica() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionColsanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionColsanitas.Dispose();
                facturacionColsanitas = null;
            }
        }

        private void GeneratePolicia()
        {
            FacturacionPolicia facturacionColsanitas = new FacturacionPolicia() { sRelation = this.sRelation };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionColsanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionColsanitas.Dispose();
                facturacionColsanitas = null;
            }
        }

        private void Generate2885(string scompany = "")
        {
            Facturacion2885 facturacionColsanitas = new Facturacion2885() { sRelation = this.sRelation, scompany = scompany };
            try
            {
                var ldesmaterializacion = this.GetInvoices();
                facturacionColsanitas.GenerateFiles(ldesmaterializacion);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Archivos generados correctamente');", true);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error al generar los archivos');", true);
            }
            finally
            {
                facturacionColsanitas.Dispose();
                facturacionColsanitas = null;
            }

        }

        protected void UpoladNuevaEPSFiles()
        {
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory);
                foreach (string file in files) 
                {
                    this.UploadNuevaEPS(file);
                }
            }
        }

        protected void UploadMedPlusFiles()
        {
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            string dirname = string.Empty;
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetDirectories(directory);
                foreach (string file in files)
                {
                    dirname = Path.GetFileName(file.TrimEnd('\\', '/'));
                    this.UploadMedPlus(file, dirname);
                }
            }
        }

        private void UploadDirectory(SftpClient sftp, string localPath, string remotePath)
        {
            // Normalizar la ruta remota
            if (!remotePath.EndsWith("/"))
            {
                remotePath += "/";
            }

            // Crear el directorio remoto si no existe
            if (!sftp.Exists(remotePath))
            {
                sftp.CreateDirectory(remotePath);
            }

            // Subir todos los archivos en el directorio actual
            foreach (string filePath in Directory.GetFiles(localPath))
            {
                string fileName = Path.GetFileName(filePath);
                string remoteFilePath = remotePath + fileName;

                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    sftp.UploadFile(fileStream, remoteFilePath);
                }
            }

            // Subir los subdirectorios recursivamente
            foreach (string directoryPath in Directory.GetDirectories(localPath))
            {
                string directoryName = Path.GetFileName(directoryPath);
                string remoteSubDirectory = remotePath + directoryName + "/";

                // Llamada recursiva para subir subdirectorios
                UploadDirectory(sftp, directoryPath, remoteSubDirectory);
            }
        }

        private void UploadMedPlus(string sdirectory, string remoteDirectory)
        {
            using (var sftp = new SftpClient(Configuration.GetStringValue("MedPlusHost"), Configuration.GetIntegerValue("MedPlusPort"), Configuration.GetStringValue("MedPlusUser"), Configuration.GetStringValue("MedPlusPassword")))
            {
                try
                {
                    // Conexión al servidor SFTP
                    sftp.Connect();
                    this.UploadDirectory(sftp, sdirectory, remoteDirectory);
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", ex);
                }
                finally
                {
                    sftp.Disconnect();
                }
            }
        }        

        private void UploadNuevaEPS(string sfile)
        {
            string logname = Configuration.GetStringValue("WinSCPLog");
            StringBuilder strCommand = new StringBuilder("open sftp://");
            strCommand.Append(Configuration.GetStringValue("NuevaEPSUser"));
            strCommand.Append(":");
            strCommand.Append(Configuration.GetStringValue("NuevaEPSPassword"));
            strCommand.Append("@");
            strCommand.Append(Configuration.GetStringValue("NuevaEPSSFTP"));
            strCommand.Append(":");
            strCommand.Append(Configuration.GetStringValue("NuevaEPSPort"));
            Process winscp = new Process();
            try
            {
                winscp.StartInfo.FileName = Configuration.GetStringValue("WinSCP");
                winscp.StartInfo.Arguments = "/xmllog=\"" + logname + "\"";
                winscp.StartInfo.UseShellExecute = false;
                winscp.StartInfo.RedirectStandardInput = true;
                winscp.StartInfo.RedirectStandardOutput = true;
                winscp.StartInfo.CreateNoWindow = true;
                winscp.Start();
                //winscp.StandardInput.WriteLine("open ftps://800180553:1YtRat3MZ0ty@ftp.elyon.com.co");
                winscp.StandardInput.WriteLine(strCommand.ToString());
                winscp.StandardInput.WriteLine("cd " + Configuration.GetStringValue("NuevaEPSFolder"));
                winscp.StandardInput.WriteLine("put " + sfile);
                winscp.StandardInput.Close();
                winscp.WaitForExit();
                if (winscp.ExitCode != 0)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", new ApplicationException("Error de conexión al FTP"));
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                winscp.Dispose();
                winscp = null;
            }
        }

        private void UploadFile(string sfile)
        {
            string logname = Configuration.GetStringValue("WinSCPLog");
            StringBuilder strCommand = new StringBuilder("open ftp://");
            strCommand.Append(Configuration.GetStringValue("FamisanarUser"));
            strCommand.Append(":");
            strCommand.Append(Configuration.GetStringValue("FamisanarPassword"));
            strCommand.Append("@");
            strCommand.Append(Configuration.GetStringValue("FamisanarFTP"));
            strCommand.Append(" -explicit -certificate=*");
            Process winscp = new Process();
            try
            {
                winscp.StartInfo.FileName = Configuration.GetStringValue("WinSCP");
                winscp.StartInfo.Arguments = "/xmllog=\"" + logname + "\"";
                winscp.StartInfo.UseShellExecute = false;
                winscp.StartInfo.RedirectStandardInput = true;
                winscp.StartInfo.RedirectStandardOutput = true;
                winscp.StartInfo.CreateNoWindow = true;
                winscp.Start();
                //winscp.StandardInput.WriteLine("open ftps://800180553:1YtRat3MZ0ty@ftp.elyon.com.co");
                winscp.StandardInput.WriteLine(strCommand.ToString());
                //winscp.StandardInput.WriteLine("cd /neumologica");
                winscp.StandardInput.WriteLine("put " + sfile);
                winscp.StandardInput.Close();
                winscp.WaitForExit();
                if (winscp.ExitCode != 0)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", new ApplicationException("Error de conexión al FTP"));
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);               
            }
            finally
            {
                winscp.Dispose();
                winscp = null;
            }
        }

        private static void Client_ValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            // Solo acepta el certificado si no hay errores de política de SSL
            if (e.PolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                e.Accept = true;
            }
            else
            {
                Console.WriteLine($"Error de validación del certificado: {e.PolicyErrors}");
                e.Accept = false;
            }
        }

        private void UploadCompensarFile(string sfile, string sremotefile)
        {
            /*try
            {
                using (FtpClient client = new FtpClient(Configuration.GetStringValue("ElyonFtp"), new NetworkCredential(Configuration.GetStringValue("ElyonUser"), Configuration.GetStringValue("ElyonPassword"))))
                {
                    client.Config.EncryptionMode = FtpEncryptionMode.Implicit;
                    client.Config.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                    client.Config.ValidateAnyCertificate = true;
                    client.Config.DataConnectionType = FtpDataConnectionType.PASV;
                    client.Config.SocketKeepAlive = true;
                    client.Config.ConnectTimeout = 60000; // 60 segundos
                    client.Config.DataConnectionConnectTimeout = 60000;
                    client.Config.ReadTimeout = 60000;
                    client.ValidateCertificate += (control, e) =>
                    {                        
                        e.Accept = true;
                    };
                    client.Connect();
                    client.UploadFile(sfile, "/" + sremotefile, FtpRemoteExists.Overwrite, true, FtpVerify.Retry);
                }
            }
            catch (SocketException sockEx)
            {
                LogError.WriteError("Facturacion", "Aplicacion", sockEx);
                throw;
            }
            catch (TimeoutException timeEx)
            {
                LogError.WriteError("Facturacion", "Aplicacion", timeEx);
                throw;
            }
            catch (FtpCommandException ftpEx)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ftpEx);
                throw;
            }
            catch (FtpException ftpEx)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ftpEx);
                throw;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;  
            }
            /*FTPManage fTPManage = null;
            try
            {
                fTPManage = new FTPManage(Configuration.GetStringValue("ElyonFtp"), Configuration.GetStringValue("ElyonUser"), Configuration.GetStringValue("ElyonPassword"), "/", 990);
                fTPManage.UploadFile(sfile);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                fTPManage.Dispose();
                fTPManage = null;
            }*/
            string logname = Configuration.GetStringValue("WinSCPLog");
            StringBuilder strCommand = new StringBuilder("open ftps://");
            strCommand.Append(Configuration.GetStringValue("ElyonUser"));
            strCommand.Append(":");
            strCommand.Append(Configuration.GetStringValue("ElyonPassword"));
            strCommand.Append("@");
            strCommand.Append(Configuration.GetStringValue("ElyonFtp"));
            Process winscp = new Process();
            try
            {
                winscp.StartInfo.FileName = Configuration.GetStringValue("WinSCP");
                winscp.StartInfo.Arguments = "/xmllog=\"" + logname + "\" -certificate=45:48:4d:b9:72:1d:4b:58:15:a3:d3:e9:27:ec:7f:d0:1d:6f:b9:ac:b1:f4:18:3a:74:33:86:69:5f:a9:62:2b";
                winscp.StartInfo.UseShellExecute = false;
                winscp.StartInfo.RedirectStandardInput = true;
                winscp.StartInfo.RedirectStandardOutput = true;
                winscp.StartInfo.CreateNoWindow = true;
                winscp.Start();
                //winscp.StandardInput.WriteLine("open ftps://800180553:1YtRat3MZ0ty@ftp.elyon.com.co");
                winscp.StandardInput.WriteLine(strCommand.ToString());
                //winscp.StandardInput.WriteLine("cd /neumologica");
                winscp.StandardInput.WriteLine("put " + sfile);
                winscp.StandardInput.Close();
                winscp.WaitForExit();
                if (winscp.ExitCode != 0)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", new ApplicationException("Error de conexión al FTP"));
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                winscp.Dispose();
                winscp = null;
            }
        }

        private void Client_ValidateCertificate(FluentFTP.Client.BaseClient.BaseFtpClient control, FtpSslValidationEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}