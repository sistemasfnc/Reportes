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

namespace Trazabilidad
{
    public partial class CrearRelacion : System.Web.UI.Page
    {

        /// <summary>
        /// Usuario logueado en el sistema
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Lista que contiene las facturas por número de relación de envío
        /// </summary>
        private List<Invoice> lInvoice
        {
            get { return (ViewState["lInvoice"] != null) ? ViewState["lInvoice"] as List<Invoice> : null; }
            set { ViewState["lInvoice"] = value; }
        }

        /// <summary>
        /// Método que busca las facturas correspondientes a la relación de envío buscada
        /// </summary>
        private void BindGrid()
        {
            Invoice oInvoice = null;
            FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion")); 
            try
            {
                oInvoice = new Invoice()
                {
                    observations = this.txtRadicado.Text
                };
                this.lInvoice = oFacade.GetInvoicesByRelation(oInvoice);                
                this.gvFacturas.DataSource = this.lInvoice;
                this.gvFacturas.DataBind();
                this.imbEnviar.Visible = this.pnlTitulo.Visible = (this.lInvoice.Count > 0);
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
                oInvoice = null;
            }
        }

        /// <summary>
        /// Método que obtiene el detalle de un número de radicación
        /// </summary>
        /// <returns>Lista Genérica con los detalles de la radicaicón</returns>
        private List<DetalleRelacion> GetDetail()
        {
            List<DetalleRelacion> lDetail = new List<DetalleRelacion>();
            DetalleRelacion oDetail = null;
            this.lInvoice.ForEach(a =>
            {
                oDetail = new DetalleRelacion()
                {
                    dtfechaasignado = DateTime.Now,
                    ifactura = Convert.ToInt32(a.invoice),
                    sfuente = a.source,
                    iasignado = 1,                    
                };
                lDetail.Add(oDetail);
            });
            return lDetail;
        }

        /// <summary>
        /// Método que valida si una relación ya tiene registro en la base de datos
        /// </summary>
        /// <returns></returns>
        private bool IsInBD()
        {
            FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion"));
            RelacionEnvio oRelacion = new RelacionEnvio() { snumero = this.txtRadicado.Text };
            try
            {
                List<RelacionEnvio> lRelacion = oFacade.GetRelationships(oRelacion);
                return (lRelacion.Count() == 0);                
            }
            catch (Exception)
            {
                return false;                
            }
            finally
            {
                oFacade.Dispose();
                oRelacion = null;
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
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.relationshipgeneration))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                //Inicia valores en controles
                
            }
        }

        /// <summary>
        /// Evento buscar facturas por relación de envío
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            if (this.IsInBD())
            {
                this.gvFacturas.Visible = this.imbEnviar.Visible = true;
                this.lInvoice = new List<Invoice>();
                this.BindGrid();
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('El número de radicado ingresado ya se encuentra con trámites');", true);
            }
        }

        /// <summary>
        /// Evento botón enviar relación a logística
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbEnviar_Click(object sender, ImageClickEventArgs e)
        {
            RelacionEnvio oRelacion = null;
            FacadeRelacion oFacade = new FacadeRelacion(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                oRelacion = new RelacionEnvio()
                {
                    snumero = this.txtRadicado.Text,
                    dtfecha = DateTime.Now,
                    cestado = 'E',
                    lDetalle = this.GetDetail(),
                    iusuario = oUser.id,
                };
                oFacade.CreateRelation(oRelacion);
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Relación creada correctamente');", true);                
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw ex;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
                oRelacion = null;
            }
            Response.Redirect("~/CrearRelacion.aspx");
        }
    }
}