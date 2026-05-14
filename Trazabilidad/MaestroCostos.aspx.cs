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
using EventLog;

namespace Trazabilidad
{
    public partial class MaestroCostos : System.Web.UI.Page
    {
        /// <summary>
        /// Propiedad de la clase objeto que almacena el usuario en sesión
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Id del centro de costo seleccionado
        /// </summary>
        private int iCost
        {
            get { return (ViewState["iCost"] != null) ? Convert.ToInt32(ViewState["iCost"]) : 0; }
            set { ViewState["iCost"] = value; }
        }

        /// <summary>
        /// Evento cargar página
        /// </summary>
        /// <param name="sender">Objeto página</param>
        /// <param name="e">Argumentos evento</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.listcostmaster))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }                
                this.BindGrid();
            }
        }        

        /// <summary>
        /// Evento paginación de la grilla
        /// </summary>
        /// <param name="sender">Objeto grilla</param>
        /// <param name="e">Argumentos del evento</param>
        protected void gvCostos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvCostos.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón buscar
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos evento</param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón cancelar filtros de búsqueda
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos evento</param>
        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtCentro.Text = this.txtNombre.Text = string.Empty;
            this.ddlTipo.ClearSelection();
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón guardar datos del centro de costo del modal
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos del evento</param>
        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            this.Save();
            this.mpeValidar.Hide();
            this.BindGrid();
        }

        /// <summary>
        /// Evento botón cancelar creación / edición centro de costos
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos evento</param>
        protected void imbCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.mpeValidar.Hide();
            this.txtNombre1.Text = this.txtCentro.Text = string.Empty;
            this.ddlTipo1.ClearSelection();
        }

        /// <summary>
        /// Evento botón agregar para mostrar modal de creación de centro de costos
        /// </summary>
        /// <param name="sender">Objeto botón</param>
        /// <param name="e">Argumentos evento</param>
        protected void imbAgregar_Click(object sender, ImageClickEventArgs e)
        {
            this.mpeValidar.Show();
        }
        
        /// <summary>
        /// Evento Row Command de la grilla
        /// </summary>
        /// <param name="sender">Objeto Grilla</param>
        /// <param name="e">Argumentos del evento</param>
        protected void gvCostos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.iCost = Convert.ToInt32(this.gvCostos.DataKeys[gr.RowIndex]["id"]);
                this.txtCodigo.Text = this.gvCostos.DataKeys[gr.RowIndex]["scode"].ToString();
                this.txtNombre1.Text = this.gvCostos.DataKeys[gr.RowIndex]["sname"].ToString();
                this.ddlTipo1.SelectedValue = this.gvCostos.DataKeys[gr.RowIndex]["ctype"].ToString();
                this.mpeValidar.Show();
            }
            else if (e.CommandName == "Eliminar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.iCost = Convert.ToInt32(this.gvCostos.DataKeys[gr.RowIndex]["id"]);
                this.Delete();
                this.mpeValidar.Hide();
                this.BindGrid();
            }
        }

        /// <summary>
        /// Método que carga la grilla de costos
        /// </summary>
        private void BindGrid()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            Cost oCost = null;
            try
            {
                oCost = new Cost()
                {
                    sname = this.txtNombre.Text,
                    scode = this.txtCentro.Text,                            
                };
                if (!string.IsNullOrEmpty(this.ddlTipo.SelectedValue)) oCost.ctype = Convert.ToChar(this.ddlTipo.SelectedValue);
                this.gvCostos.DataKeyNames = new string[] { "id", "scode", "sname", "ctype" };
                this.gvCostos.DataSource = oFacade.GetCosts(oCost);
                this.gvCostos.DataBind();
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
                oCost = null;
            }
        }
        
        /// <summary>
        /// Método para almacenar la información (Creación / Edición) del centro de costos
        /// </summary>
        private void Save()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                Cost oCost = new Cost()
                {
                    id = this.iCost,
                    sname = this.txtNombre1.Text,
                    scode = this.txtCodigo.Text,
                    ctype = Convert.ToChar(this.ddlTipo1.SelectedValue),
                };
                if (oCost.id == 0)
                {
                    oFacade.Insert(oCost);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "Insert", "alert('El centro de costo ha sido creado correctamente');", true);
                }
                else
                {
                    oFacade.Edit(oCost);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "Edit", "alert('El centro de costo ha sido editado correctamente');", true);
                }
                oCost = null;
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
        /// Método para inactivar un centro de costos
        /// </summary>
        private void Delete()
        {
            FacadeCosto oFacade = new FacadeCosto(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                oFacade.Delete(this.iCost);                
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