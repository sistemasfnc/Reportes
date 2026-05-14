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

namespace Trazabilidad
{
    public partial class RecibirRelacion : System.Web.UI.Page
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
                    cestado = 'F',
                };
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
        /// Método que almacena el nuevo estado de la factura de la relación de envío
        /// </summary>
        /// <param name="oRelacion">Objeto relación de envío</param>
        private void SaveValues(RelacionEnvio oRelacion)
        {
            using (FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                oFacade.SaveInvoices(oRelacion, 2);
            }
        }

        /// <summary>
        /// Método para llegar la grilla de facturas
        /// </summary>
        private void BindInvoicesGrid()
        {
            RelacionEnvio oRelacion = null;
            try
            {
                oRelacion = this.GetRelationship();
                if (oRelacion != null)
                {

                    this.gvFacturas.DataSource = oRelacion.lDetalle;
                    this.gvFacturas.DataKeyNames = new string[] { "ifactura", "irecibido" };
                    this.gvFacturas.DataBind();
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
        /// Evento cargar página
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.relationshipvalidation))
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
            this.BindGrid();
        }

        /// <summary>
        /// Método que retorna la imagen del semáforo dependiendo del estado 
        /// </summary>
        /// <param name="oStatus">Objeto con el estado de la relación de envío</param>
        /// <returns>Cadena de caracteres de la ruta con la imagen del semáforo</returns>
        protected string GetImage(object oStatus)
        {
            if (oStatus.ToString() == "E") return "~/images/redlight.png";
            if (oStatus.ToString() == "R" || oStatus.ToString() == "P") return "~/images/bluelight.png";
            else if (oStatus.ToString() == "F") return "~/images/yellowlight.png";
            else return "~/images/greenlight.png";
        }

        /// <summary>
        /// Método para mostrar el número de id de la relación de envío en caso de que ya se encuentre tramitada completa
        /// </summary>
        /// <param name="oStatus">Objeto con el estado de la relación de envío</param>
        /// <param name="oId">String Id de la relación de envío</param>
        /// <returns>Texto con el número de la relación de envío</returns>
        protected string ShowText(object oStatus, object oId)
        {
            return (oStatus.ToString() == "T") ? oId.ToString() : string.Empty;
        }


        /// <summary>
        /// Evento comando de la grilla se dispara cuando se hace clic en el botón revisar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvRelaciones_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Tramitar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.iRelationship = Convert.ToInt32(this.gvRelaciones.DataKeys[gr.RowIndex]["iid"]);
                this.mpeValidar.Show();
                this.BindInvoicesGrid();
                this.BindObservationsGrid();
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
        /// <param name="oRecibido">Estado de recepción</param>
        /// <returns>Boolean para activar o inactivar el checkbox</returns>
        protected bool EnableCheck(object oEnviado, object oRecibido)
        {
            return (Convert.ToInt32(oEnviado) == 1 && Convert.ToInt32(oRecibido) == 0);
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
            int iRecibido = 0;
            int iTotal = 0;
            foreach (GridViewRow gr in this.gvFacturas.Rows)
            {
                iRecibido = 0;
                CheckBox chk = gr.FindControl("chkSeleccionar") as CheckBox;
                if (chk.Checked && chk.Visible)
                {
                    oDetalle = new DetalleRelacion()
                    {
                        dtrecibido = DateTime.Now,
                        irecibido = 1,
                        ifactura = Convert.ToInt32(this.gvFacturas.DataKeys[gr.RowIndex]["ifactura"]),
                        isuariofacturacion = this.oUser.id,
                    };
                    lDetalle.Add(oDetalle);
                    iRecibido = 1;
                }
                else if (!chk.Visible)
                {
                    iRecibido = Convert.ToInt32(this.gvFacturas.DataKeys[gr.RowIndex]["irecibido"]);
                }
                if (iRecibido == 1)
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
                oRelacion.iusuario = this.oUser.id;
                oRelacion.lDetalle = lDetalle;
                oRelacion.cestado = (oTmp.lDetalle.Count == iTotal) ? 'T' : 'F';
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
                if (oDetalle.ienviado == 1 && oDetalle.irecibido == 0) e.Row.BackColor = System.Drawing.Color.LightPink;
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

    }
}