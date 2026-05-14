using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Entity;
using Config;
using EventLog;
using Facade;
using Utils;
using System.Collections;
using System.Data;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Trazabilidad
{
    public partial class TramitarRelacion : System.Web.UI.Page
    {
        /// <summary>
        /// Usuario logueado en el sistema
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Id de la relación seleccionada
        /// </summary>
        private long iRelationship
        {
            get { return (ViewState["iRelationship"] != null) ? Convert.ToInt64(ViewState["iRelationship"]) : 0; }
            set { ViewState["iRelationship"] = value; }
        }

        /// <summary>
        /// Lista que contiene las relaciones de envío
        /// </summary>
        private List<RelacionEnvio> lRelacion
        {
            get { return (ViewState["lRelacion"] != null) ? ViewState["lRelacion"] as List<RelacionEnvio> : null; }
            set { ViewState["lRelacion"] = value; }
        }

        /// <summary>
        /// Arreglo que contiene los checkbox seleccionados
        /// </summary>
        private ArrayList CheckedItems
        {
            get { return (ViewState["CheckedItems"] == null) ? null : (ArrayList)ViewState["CheckedItems"]; }
            set { ViewState["CheckedItems"] = value; }
        }

        /// <summary>
        /// Estado del check principal
        /// </summary>
        private bool bMainCheckStatus
        {
            get { return (ViewState["bMainCheckStatus"] != null) ? Convert.ToBoolean(ViewState["bMainCheckStatus"]) : false; }
            set { ViewState["bMainCheckStatus"] = value; }
        }

        /// <summary>
        /// Método que pobla la grilla con las relaciones de envío
        /// </summary>
        private void BindGrid()
        {
            FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion"));
            RelacionEnvio oEntity = null;
            try
            {
                oEntity = new RelacionEnvio()
                {
                    snumero = this.txtRelacion.Text,
                    dtfechainicial = (!string.IsNullOrEmpty(this.txtFechaInicio.Text)) ? Convert.ToDateTime(this.txtFechaInicio.Text) : new DateTime(),
                    dtfechafinal = (!string.IsNullOrEmpty(this.txtFechaFin.Text)) ? Convert.ToDateTime(this.txtFechaFin.Text) : new DateTime(),                    
                };
                if (!string.IsNullOrEmpty(this.ddlEstado.SelectedValue)) oEntity.cestado = Convert.ToChar(this.ddlEstado.SelectedValue);
                this.lRelacion = oFacade.GetRelationships(oEntity);
                this.gvRelaciones.DataKeyNames = new string[] { "iid" };
                this.gvRelaciones.DataSource = this.lRelacion;
                this.gvRelaciones.DataBind();
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
                oEntity = null;
            }
        }

        /// <summary>
        /// Método para llegar la grilla de facturas
        /// </summary>
        private void BindInvoicesGrid(bool View = true)
        {            
            RelacionEnvio oRelacion = null;
            try
            {
                oRelacion = this.GetRelationship();
                if (oRelacion != null)
                {
                    if (View)
                    {
                        this.gvFacturas.DataSource = oRelacion.lDetalle;
                        this.gvFacturas.DataKeyNames = new string[] { "ifactura" };
                        this.gvFacturas.DataBind();
                    }
                    else
                    {
                        this.gvVisorFacturas.DataSource = oRelacion.lDetalle;
                        this.gvVisorFacturas.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oRelacion = null;
            }
        }

        /// <summary>
        /// Método que busca la relación de envío correspondiente al id seleccionado en la grilla de relaciones
        /// </summary>
        /// <returns>Objeto relación de envío encontrada</returns>
        private RelacionEnvio GetRelationship()
        {
            return this.lRelacion.FirstOrDefault(x => x.iid == this.iRelationship);
        }

        /// <summary>
        /// Método que almacena el nuevo estado de la factura de la relación de envío
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        private void SaveValues(RelacionEnvio oRelacion)
        {
            using (FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                oFacade.SaveInvoices(oRelacion, 1);                
            }
        }

        /// <summary>
        /// Método para recordar los checkbox seleccionados para la paginación
        /// </summary>
        private void RememberOldValues()
        {
            ArrayList categoryIDList = new ArrayList();
            int index = -1;
            foreach (GridViewRow row in this.gvFacturas.Rows)
            {
                index = Convert.ToInt32(this.gvFacturas.DataKeys[row.RowIndex].Value);
                bool result = ((CheckBox)row.FindControl("chkSeleccionar")).Checked;
                if (this.CheckedItems != null)
                    categoryIDList = this.CheckedItems;
                if (result)
                {
                    if (!categoryIDList.Contains(index))
                        categoryIDList.Add(index);
                }
                else
                    categoryIDList.Remove(index);
            }
            if (categoryIDList != null && categoryIDList.Count > 0)
                this.CheckedItems = categoryIDList;
        }

        /// <summary>
        /// Método para repoblar los checkbox seleccionados en la paginación
        /// </summary>
        private void RePopulateValues()
        {
            ArrayList categoryIDList = this.CheckedItems;
            if (categoryIDList != null && categoryIDList.Count > 0)
            {
                foreach (GridViewRow row in this.gvFacturas.Rows)
                {
                    int index = Convert.ToInt32(this.gvFacturas.DataKeys[row.RowIndex].Value);
                    if (categoryIDList.Contains(index))
                    {
                        CheckBox myCheckBox = (CheckBox)row.FindControl("chkSeleccionar");
                        myCheckBox.Checked = true;
                    }
                }
            }
        }

        /// <summary>
        /// Método que deshabilita la paginación para poder tener todos los valores disponibles en el guardado
        /// </summary>
        private void DisablePaging(bool bEnable)
        {
            this.gvFacturas.AllowPaging = bEnable;
            this.RememberOldValues();
            this.BindInvoicesGrid();
            this.RePopulateValues();
        }

        /// <summary>
        /// Método que llena la grilla de observaciones para la relación de envío
        /// </summary>
        private void BindObservationsGrid()
        {
            FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.gvObservaciones.DataSource = oFacade.GetLog(this.iRelationship);
                this.gvObservaciones.DataBind();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
        }

        /// <summary>
        /// Método para recibir una relación de envío enviada desde facturación
        /// </summary>
        private void RecieveRelationship()
        {
            FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion"));
            RelacionEnvio oRelacion = null;
            try
            {
                oRelacion = new RelacionEnvio()
                {
                    iid = this.iRelationship,
                    cestado = 'R',
                    dtfecharecepcion = DateTime.Now,
                };
                oFacade.UpdateRelationship(oRelacion);
                this.BindGrid();
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
                oRelacion = null;
            }
        }

        /// <summary>
        /// Método para generar el archivo en PDF para mensajería
        /// </summary>
        /// <param name="lRelationships">Lista genérica con los números de relaciones de envío a generar</param>
        private void GenerateFile(List<string> lRelationships)
        {
            if (lRelationships.Count > 0)
            {
                string[] asRelationships = lRelationships.ToArray();
                string sResult = String.Join(", ", asRelationships);
                DataTable dt = new DataTable();
                MemoryStream outputStream = null;
                using (FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion")))
                {
                    dt = oFacade.GenerateTemplate(sResult);
                    if (dt.Rows.Count > 0)
                    {
                        outputStream = this.GetPDFDocument(dt);
                    }
                    dt.Dispose();
                    dt = null;
                }
                if (outputStream != null)
                {
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Expires", "0");
                    Response.AddHeader("Cache-Control", "");
                    Response.AddHeader("Content-Disposition", "attachment; filename=Planilla.pdf");
                    Response.AddHeader("Content-length", outputStream.GetBuffer().Length.ToString());
                    Response.OutputStream.Write(outputStream.GetBuffer(), 0, outputStream.GetBuffer().Length);
                    Response.End();
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Debe seleccionar por lo menos una relación para generar los archivos');", true);
            }
        }

        /// <summary>
        /// Método que obtiene el archivo en PDF para envío a mensajería
        /// </summary>
        /// <param name="dt">Data Table con la información de las relaciones de envío seleccionadas</param>
        /// <returns>MemoryStream que contiene el archivo generado</returns>
        private MemoryStream GetPDFDocument(DataTable dt)
        {
            MemoryStream outputStream = new MemoryStream();
            Document oDocument = new Document();
            Font fntNormal = FontFactory.GetFont(FontFactory.COURIER, 8);
            Font fntBold = FontFactory.GetFont(FontFactory.COURIER_BOLD, 8);
            try
            {
                oDocument.SetPageSize(iTextSharp.text.PageSize.LETTER.Rotate());
                PdfWriter.GetInstance(oDocument, outputStream);
                oDocument.Open();
                oDocument.Add(new Paragraph("Orden de mensajería para el día " + DateTime.Now.ToString("dd/MM/yyyy"), fntBold));
                oDocument.Add(Chunk.NEWLINE);
                string[] asHeader = new string[] { "FECHA", "EMPRESA", "DIRECCION", "# RELACION", "CANT. FACTURAS", "USUARIO" };
                PdfPTable pdfTable = new PdfPTable(asHeader.Length);
                pdfTable.WidthPercentage = 100;
                pdfTable.SetWidths(new float[] { 10f, 35f, 20f, 10f, 15f, 10f });
                PdfPCell pdfCell = null;
                for (int i = 0; i < asHeader.Length; i++)
                {                    
                    pdfCell = new PdfPCell(new Paragraph(asHeader[i], fntBold));
                    pdfCell.HorizontalAlignment = 1;
                    pdfTable.AddCell(pdfCell);
                }
                foreach (DataRow dr in dt.Rows)
                {
                    pdfCell = new PdfPCell(new Paragraph(Convert.ToDateTime(dr["Fecha"]).ToString("dd/MM/yyyy"), fntNormal));
                    pdfCell.HorizontalAlignment = 1;                    
                    pdfTable.AddCell(pdfCell);
                    pdfCell = new PdfPCell(new Paragraph(dr["Empresa"].ToString(), fntNormal));
                    pdfCell.HorizontalAlignment = 0;                    
                    pdfTable.AddCell(pdfCell);
                    pdfTable.AddCell(new PdfPCell(new Paragraph(dr["Direccion"].ToString(), fntNormal)));
                    pdfCell = new PdfPCell(new Paragraph(dr["Numero"].ToString(), fntNormal));
                    pdfCell.HorizontalAlignment = 1;
                    pdfTable.AddCell(pdfCell);                    
                    pdfCell = new PdfPCell(new Paragraph(dr["Cantidad"].ToString(), fntNormal));
                    pdfCell.HorizontalAlignment = 2;                    
                    pdfTable.AddCell(pdfCell);
                    pdfTable.AddCell(new PdfPCell(new Paragraph(dr["Usuario"].ToString(), fntNormal)));
                }
                oDocument.Add(pdfTable);
                oDocument.Close();
                return outputStream;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oDocument = null;
                fntBold = null;
                fntNormal = null;
            }
        }

        /// <summary>
        /// Evento cargar página
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.relationshiplist))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }  
                if (!this.IsPostBack)
                {
                    this.BindGrid();
                }
            }
        }

        /// <summary>
        /// Método que retorna la imagen del semáforo dependiendo del estado 
        /// </summary>
        /// <param name="oStatus"></param>
        /// <returns></returns>
        protected string GetImage(object oStatus)
        {
            if (oStatus.ToString() == "E") return "~/images/redlight.png";
            else if (oStatus.ToString() == "P") return "~/images/yellowlight.png";
            else if (oStatus.ToString() == "R") return "~/images/bluelight.png";
            else return "~/images/greenlight.png";
        }

        /// <summary>
        /// Evento comando de la grilla se dispara cuando se hace clic en el botón revisar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvRelaciones_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;            
            if (e.CommandName == "Tramitar")
            {
                this.iRelationship = Convert.ToInt32(this.gvRelaciones.DataKeys[gr.RowIndex]["iid"]);
                this.mpeValidar.Show();
                this.BindInvoicesGrid();
                this.BindObservationsGrid();
            }
            else if (e.CommandName == "Recibir")
            {
                this.iRelationship = Convert.ToInt32(this.gvRelaciones.DataKeys[gr.RowIndex]["iid"]);
                this.mpeFacturas.Show();
                this.BindInvoicesGrid(false);
                //this.RecieveRelationship();
                //this.BindGrid();
            }
        }

        /// <summary>
        /// Evento botón buscar para filtrar la grilla de relaciones de envío
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón cancelar para limpiar los filtros de búsqueda para la grilla de relaciones de envio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtFechaFin.Text = this.txtFechaInicio.Text = string.Empty;
            this.ddlEstado.SelectedValue = string.Empty;
            this.BindGrid();
        }

        /// <summary>
        /// Evento checkbox seleccionar todos, asigna el valor checked a los checkbox de la grilla de facturas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void checkAll_CheckedChanged(object sender, EventArgs e)
        {
            this.bMainCheckStatus = ((CheckBox)sender).Checked;
            this.gvFacturas.AllowPaging = false;
            this.BindInvoicesGrid();
            foreach (GridViewRow gr in this.gvFacturas.Rows)
            {
                CheckBox chk = gr.FindControl("chkSeleccionar") as CheckBox;
                if (chk.Visible) chk.Checked = ((CheckBox)sender).Checked;
            }
            this.RememberOldValues();
            this.gvFacturas.AllowPaging = true;
            this.BindInvoicesGrid();
            this.RePopulateValues();
            this.mpeValidar.Show();
        }

        /// <summary>
        /// Método que obtiene el texto del estado de columna enviada, tramitada o asignada en la factura
        /// </summary>
        /// <param name="oItem">Columna de estado</param>
        /// <returns>Cadena de caracteres Si o No</returns>
        protected string GetText(object oItem)
        {
            return (Convert.ToInt32(oItem) == 0) ? "No" : "Si";
        }

        /// <summary>
        /// Método que obtiene la fecha en texto para las columnas fecha de envío, fecha de recepción y fecha de asignación de la grilla de relaciones de envío
        /// </summary>
        /// <param name="oDate">Objeto con la fecha</param>
        /// <returns>Fecha en texto formato dd/MM/yyyy</returns>
        protected string GetTextDate(object oDate)
        {
            return (Convert.ToDateTime(oDate).Year != 1900) ? Convert.ToDateTime(oDate).ToString("dd/MM/yyyy") : string.Empty;
        }

        /// <summary>
        /// Método que verifica si una factura fue enviada e inhabilita el checkbox
        /// </summary>
        /// <param name="oEnviado">Estado del envío</param>
        /// <returns>Boolean para activar o inactivar el checkbox</returns>
        protected bool EnableCheck(object oEnviado)
        {
            return (Convert.ToInt32(oEnviado) == 0);
        }

        /// <summary>
        /// Evento botón enviar facturas para su recepción en facturación
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbEnviar_Click(object sender, ImageClickEventArgs e)
        {
            RelacionEnvio oRelacion = new RelacionEnvio() { iid = this.iRelationship };
            RelacionEnvio oTmp = this.GetRelationship();
            List<DetalleRelacion> lDetalle = new List<DetalleRelacion>();
            DetalleRelacion oDetalle = null;
            this.DisablePaging(false);
            int iTotal = 0;
            foreach (GridViewRow gr in this.gvFacturas.Rows)
            {
                CheckBox chk = gr.FindControl("chkSeleccionar") as CheckBox;
                if (chk.Checked && chk.Visible)
                {
                    oDetalle = new DetalleRelacion()
                    {
                        dtfechaenviado = DateTime.Now,
                        ienviado = 1,
                        ifactura = Convert.ToInt32(this.gvFacturas.DataKeys[gr.RowIndex]["ifactura"]),                
                        iusuariologistica = this.oUser.id,
                    };
                    iTotal++;
                    lDetalle.Add(oDetalle);
                }
                else if (!chk.Visible)
                {
                    iTotal++;
                }
            }
            if (lDetalle.Count == 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Debe seleccionar por lo menos una factura');", true);
                this.DisablePaging(true);
                this.mpeValidar.Show(); 
            }
            else
            {                
                oRelacion.lDetalle = lDetalle;
                oRelacion.cestado = (iTotal == oTmp.lDetalle.Count) ? 'F' : 'P';
                //oRelacion.cestado = 'P';
                if (!string.IsNullOrEmpty(this.txtObservacion.Text))
                {
                    RelacionLog oLog = new RelacionLog() { iid = this.iRelationship, iuser = this.oUser.id, sobservacion = this.txtObservacion.Text };
                    oRelacion.oLog = oLog;
                }                    
                try
                {
                    this.SaveValues(oRelacion);
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Información almacenada correctamente');", true);
                    this.gvFacturas.AllowPaging = true;
                    this.mpeValidar.Hide();
                    this.BindGrid();
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", ex);
                    throw;
                }
            }            
        }

        /// <summary>
        /// Evento paginación de la grilla de relaciones de envío
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvRelaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvRelaciones.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        /// <summary>
        /// Evento paginación de la grilla de facturas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvFacturas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.RememberOldValues();
            this.gvFacturas.PageIndex = e.NewPageIndex;
            this.BindInvoicesGrid();
            this.RePopulateValues();
            this.mpeValidar.Show();
            this.BindObservationsGrid();
        }

        /// <summary>
        /// Evento generación de fila grilla de facturas para pintar las filas del color dependiendo de los estados
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvFacturas_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DetalleRelacion oDetalle = (DetalleRelacion)e.Row.DataItem;
                if (oDetalle.ienviado == 0) e.Row.BackColor = System.Drawing.Color.LightPink;
                else if (oDetalle.ienviado == 1 && oDetalle.irecibido == 0) e.Row.BackColor = System.Drawing.Color.LightSkyBlue;
                else if (oDetalle.irecibido == 1) e.Row.BackColor = System.Drawing.Color.LightSeaGreen;                
            }
            else if (e.Row.RowType == DataControlRowType.Header)
            {
                CheckBox check = (e.Row.FindControl("checkAll") as CheckBox);
                if (check != null)
                {
                    check.Checked = this.bMainCheckStatus;
                }
            }
        }

        /// <summary>
        /// Método para habilitar el botón de recibir relación de envío
        /// </summary>
        /// <param name="oRecieved">Objeto estado de la relación de envío</param>
        /// <returns>Booleano para activar o inactivar el botón</returns>
        protected bool EnableButton(object oRecieved)
        {
            return (oRecieved.ToString() == "E");
        }

        /// <summary>
        /// /Evento paginador grilla visor de facturas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvVisorFacturas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvVisorFacturas.PageIndex = e.NewPageIndex;
            this.mpeFacturas.Show();
            this.BindInvoicesGrid(false);
        }

        /// <summary>
        /// Evento del botón recibir relación de envío en la grilla de facturas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbRecibir_Click(object sender, ImageClickEventArgs e)
        {
            this.RecieveRelationship();
            this.BindGrid();
        }

        /// <summary>
        /// Evento para generar documento físico para las relaciones de envío
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbGenerarRelacion_Click(object sender, ImageClickEventArgs e)
        {
            List<string> lRelacion = new List<string>();
            CheckBox check = null;
            string sid = string.Empty;
            foreach (GridViewRow gr in this.gvRelaciones.Rows)
            {
                check = gr.FindControl("chkRelacion") as CheckBox;
                if (check.Checked)
                {
                    sid = this.gvRelaciones.DataKeys[gr.RowIndex]["iid"].ToString();
                    lRelacion.Add(sid);
                }
            }
            this.GenerateFile(lRelacion);
        }
    }
}