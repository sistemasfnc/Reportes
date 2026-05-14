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
    public partial class RecepcionGlosa : System.Web.UI.Page
    {
        //Factura buscada para tramitar glosa
        private int iComment
        {
            get { return (ViewState["Comment"] != null) ? Convert.ToInt32(ViewState["Comment"]) : 0; }
            set { ViewState["Comment"] = value; }

        }

        /// <summary>
        /// Lista que contiene los conceptos por glosa almacenados
        /// </summary>
        private List<ConceptoGlosa> lConcepto
        {
            get { return (ViewState["lConcepto"] != null) ? ViewState["lConcepto"] as List<ConceptoGlosa> : null; }
            set { ViewState["lConcepto"] = value; }
        }

        /// <summary>
        /// Conceptos seleccionados
        /// </summary>
        private ArrayList CheckedItems
        {
            get { return (ViewState["CheckedItems"] == null) ? null : (ArrayList)ViewState["CheckedItems"]; }
            set { ViewState["CheckedItems"] = value; }
        }

        /// <summary>
        /// Usuario logueado en el sistema
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Método que se ejecuta al cargar la página
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.recievecomment))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                //Inicia valores en controles
                this.lConcepto = new List<ConceptoGlosa>();
                this.LoadControls();
            }
        }

        /// <summary>
        /// Evento del botón guardar concepto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            //Asignación de variables
            this.RememberConceptValues();
            CheckBox chk = null;
            List<ConceptoGlosa> lConcepto = new List<ConceptoGlosa>();
            Glosa oGlosa = new Glosa();
            ConceptoGlosa oConcepto = null;
            //Desactiva paginación
            this.DisablePaging();          
            //Recorre las filas de la grilla
            foreach (GridViewRow gr in this.gvConceptos.Rows)
            {
                //Asigna objeto Checkbox
                chk = gr.FindControl("chkConcepto") as CheckBox;
                //Valida si el checkbox está activo para agregarlo al listado de conceptos por glosa
                if (chk.Checked)
                {
                    int indexConcepto = this.ConceptSelected(Convert.ToInt32(this.gvConceptos.DataKeys[gr.RowIndex]["idconcept"]));
                    if (indexConcepto == -1)
                    {
                        oConcepto = new ConceptoGlosa()
                        {
                            idconcept = Convert.ToInt32(this.gvConceptos.DataKeys[gr.RowIndex]["idconcept"]),
                            conceptname = this.gvConceptos.DataKeys[gr.RowIndex]["conceptname"].ToString(),
                            conceptcode = this.gvConceptos.DataKeys[gr.RowIndex]["conceptcode"].ToString(),
                            conceptvalue = !string.IsNullOrEmpty((gr.FindControl("txtValorConcepto") as TextBox).Text) ? Convert.ToDecimal((gr.FindControl("txtValorConcepto") as TextBox).Text) : 0,
                            conceptobservations = (gr.FindControl("txtObservacionConcepto") as TextBox).Text,
                        };
                        lConcepto.Add(oConcepto);
                    }
                    else
                    {
                        lConcepto.Add(this.lConcepto[indexConcepto]);
                    }                    
                }
            }
            this.lConcepto = lConcepto;
            this.gvConceptos.AllowPaging = true;
            this.mpeValidar.Hide();
            this.BindCommentConceptGrid();
        }

        /// <summary>
        /// Evento del botón buscar concepto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbBuscar_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                //Muestra modal con la grilla
                this.mpeValidar.Show();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
        }

        /// <summary>
        /// Evento botón buscar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            //Declaración de variables
            FacadeFactura oFacade = new FacadeFactura(Configuration.GetStringValue("FNCFacturacion"));
            FacadeGlosa oFacadeGlosa = new FacadeGlosa();
            List<Invoice> lInvoice = null;
            Glosa oGlosa = new Glosa() { invoice = this.txtFactura.Text, company = this.ddlEmpresa.SelectedValue };
            Invoice oInvoice = new Invoice() { invoice = this.txtFactura.Text, source = this.ddlEmpresa.SelectedValue };
            try
            {
                //Obtiene listado de facturas
                lInvoice = oFacade.GetList(oInvoice);
                //Verifica si existen facturas
                if (lInvoice.Count == 0)
                {
                    //Mensaje de error
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('La factura ingresada no existe');", true);
                }
                else if (lInvoice[0].status != "RD" && this.ddlEmpresa.SelectedValue == "IN")
                {
                    //Mensaje de error
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('La factura ingresada aún no esta radicada');", true);
                }
                else
                {
                    oFacadeGlosa.sConnection = Configuration.GetStringValue("FNCFacturacion");
                    oGlosa = oFacadeGlosa.GetComment(oGlosa);
                    if (oGlosa.id == 0)
                    {
                        //Asigna valores de factura a labels
                        this.SetLabels(lInvoice[0]);
                    }
                    else
                    {
                        if (oGlosa.lConcept.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(oGlosa.lConcept[0].oResponse.responsename))
                            {
                                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('La factura ingresada ya tiene respuestas de glosa y no puede ser modificada');", true);
                                return;
                            }
                        }
                        this.SetLabels(lInvoice[0]);
                        this.LoadComment(oGlosa);
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
                oFacade.Dispose();
                oFacade = null;
                lInvoice = null;
                oInvoice = null;
            }
        }

        /// <summary>
        /// Evento del botón almacenar glosa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbSave_Click(object sender, ImageClickEventArgs e)
        {
            if (this.lConcepto.Count != 0)
            {
                if (this.IsHigherValue())
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('La suma de conceptos no puede ser mayor al valor de la factura');", true);
                    return;
                }
                this.CreateComment();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Glosa creada correctamente');", true);
                Response.Redirect("~/RecepcionGlosa.aspx");
            }            
            else 
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Debe agregar por lo menos un concepto de devolución de factura');", true);
            }
        }

        /// <summary>
        /// Evento del paginador de la grilla de conceptos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvConceptos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.RememberConceptValues();
            this.RememberOldValues();            
            this.gvConceptos.PageIndex = e.NewPageIndex;
            this.BindConceptGrid();
            this.RePopulateValues();            
            this.mpeValidar.Show();
        }

        /// <summary>
        /// Método que asigna variables de la factura a labels
        /// </summary>
        /// <param name="oInvoice">Objeto que contiene la información de las facturas</param>
        private void SetLabels(Invoice oInvoice)
        {
            this.lblFactura.Text = oInvoice.invoice;
            this.lblFecha.Text = oInvoice.invoicedate.ToString("dd/MM/yyyy");
            this.lblFuente.Text = oInvoice.source;
            this.lblObservaciones.Text = oInvoice.observations;
            this.lblEstado.Text = oInvoice.status;
            this.lblValor.Text = oInvoice.value.ToString("C");
            this.txtValor.Text = oInvoice.value.ToString();
            this.lblEps.Text = oInvoice.eps;
            this.lblUsuario.Text = oInvoice.user;
            this.pnlFactura.Visible = true;
        }

        /// <summary>
        /// Método que carga los valores iniciales para los controles
        /// </summary>
        private void LoadControls()
        {
            this.txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlEmpresa.DataSource = oFacade.GetList("company");
                this.ddlEmpresa.DataTextField = "name";
                this.ddlEmpresa.DataValueField = "name";
                this.ddlEmpresa.DataBind();
                this.ddlEmpresa.SelectedValue = string.Empty;
                this.BindConceptGrid();
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
        /// Método de carga los conceptos en la grilla para su selección
        /// </summary>
        private void BindConceptGrid()
        {
            //Inicializa la fachada
            using (FacadeGlosa oFacade = new FacadeGlosa())
            {
                //Asigna cadena de conexión
                oFacade.sConnection = Configuration.GetStringValue("FNCFacturacion");
                //Trae listado de conceptos de glosas y los pinta en la grilla
                this.gvConceptos.DataKeyNames = new string[] { "idconcept", "conceptname", "conceptcode" };
                this.gvConceptos.DataSource = oFacade.GetConcepts();
                this.gvConceptos.DataBind();
            }
        }

        /// <summary>
        /// Método para crear la glosa con sus conceptos
        /// </summary>
        /// <param name="lConcepto"></param>
        private void CreateComment()
        {
            Glosa oGlosa = null;
            FacadeGlosa oFacade = new FacadeGlosa();
            try
            {
                oFacade.sConnection = Configuration.GetStringValue("FNCFacturacion");
                oGlosa = new Glosa()
                {
                    invoice = this.txtFactura.Text,
                    transactdate = Convert.ToDateTime(this.txtFecha.Text),
                    type = Convert.ToInt32(this.ddlTipo.SelectedValue),
                    observations = this.txtObservaciones.Text,
                    lConcept = this.lConcepto,
                    company = this.ddlEmpresa.SelectedValue,
                    charge = string.Empty,
                    value = Convert.ToDecimal(this.txtValor.Text),
                    id = this.iComment,
                };
                this.iComment = oFacade.CreateOrEdit(oGlosa);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oGlosa = null;
                oFacade.Dispose();
                oFacade = null;
            }                     
        }

        /// <summary>
        /// Método que valida si el valor de los conceptos de la glosa es mayor a la factura
        /// </summary>
        /// <returns>Verdadero si el valor es mayor falso si es menor</returns>
        private bool IsHigherValue()
        {
            decimal iValue = this.lConcepto.Sum(x => x.conceptvalue);
            return (iValue > Convert.ToDecimal(this.txtValor.Text));
        }

        /// <summary>
        /// Método que muestra los conceptos seleccionados para la glosa
        /// </summary>
        private void BindCommentConceptGrid()
        {
            this.gvConceptosGlosa.DataSource = this.lConcepto;
            this.gvConceptosGlosa.DataBind();
        }

        /// <summary>
        /// Método que valida si un concepto ya existe en el listado de conceptos y retorna su índice
        /// </summary>
        /// <param name="idconcept">Id del concepto</param>
        /// <returns></returns>
        private int ConceptSelected(int idconcept)
        {
            if (this.lConcepto.Count(x => x.idconcept == idconcept) > 0)
            {
                return this.lConcepto.IndexOf(this.lConcepto.Find(x => x.idconcept == idconcept));
            }
            return -1;
        }

        /// <summary>
        /// Método que almacena en una lista los conceptos seleccionados para rellenar la paginación
        /// </summary>
        private void RememberConceptValues()
        {
            int iConcepto = 0;
            foreach (GridViewRow row in this.gvConceptos.Rows)
            {
                if (((CheckBox)row.FindControl("chkConcepto")).Checked)
                {
                    ConceptoGlosa oConcepto = new ConceptoGlosa()
                    {
                        idconcept = Convert.ToInt32(this.gvConceptos.DataKeys[row.RowIndex]["idconcept"]),
                        conceptvalue = !string.IsNullOrEmpty((row.FindControl("txtValorConcepto") as TextBox).Text) ? Convert.ToDecimal((row.FindControl("txtValorConcepto") as TextBox).Text) : 0,
                        conceptobservations = (row.FindControl("txtObservacionConcepto") as TextBox).Text,
                        conceptname = this.gvConceptos.DataKeys[row.RowIndex]["conceptname"].ToString(),
                        conceptcode = this.gvConceptos.DataKeys[row.RowIndex]["conceptcode"].ToString(),
                    };
                    iConcepto = this.ConceptSelected(oConcepto.idconcept);
                    if (iConcepto == -1)
                    {
                        this.lConcepto.Add(oConcepto);
                    }
                    else
                    {
                        this.lConcepto[iConcepto].conceptvalue = oConcepto.conceptvalue;
                        this.lConcepto[iConcepto].conceptobservations = oConcepto.conceptobservations;
                    }
                }
            }            
        }

        /// <summary>
        /// Almacena los conceptos en memoria
        /// </summary>
        private void RememberOldValues()
        {            
            ArrayList categoryIDList = new ArrayList();
            int index = -1;
            foreach (GridViewRow row in this.gvConceptos.Rows)
            {
                index = Convert.ToInt32(this.gvConceptos.DataKeys[row.RowIndex].Value);
                bool result = ((CheckBox)row.FindControl("chkConcepto")).Checked;
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
        /// Carga nuevamente los conceptos seleccionados
        /// </summary>
        private void RePopulateValues()
        {
            ArrayList categoryIDList = this.CheckedItems;
            if (categoryIDList != null && categoryIDList.Count > 0)
            {
                foreach (GridViewRow row in this.gvConceptos.Rows)
                {
                    int index = Convert.ToInt32(this.gvConceptos.DataKeys[row.RowIndex].Value);
                    if (categoryIDList.Contains(index))
                    {
                        CheckBox myCheckBox = (CheckBox)row.FindControl("chkConcepto");
                        TextBox tb = (TextBox)row.FindControl("txtValorConcepto");
                        TextBox tb1 = (TextBox)row.FindControl("txtObservacionConcepto");
                        myCheckBox.Checked = true;
                        tb.Visible = true;
                        tb1.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Método que desactiva la paginación de la grilla de conceptos
        /// </summary>
        private void DisablePaging()
        {
            this.gvConceptos.AllowPaging = false;
            this.RememberOldValues();
            this.BindConceptGrid();
            this.RePopulateValues();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbCancel_Click(object sender, ImageClickEventArgs e)
        {
            this.mpeValidar.Hide();
        }

        /// <summary>
        /// Evento para el checkbox de la grilla de los conceptos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void chkConcepto_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            GridViewRow gr = chk.NamingContainer as GridViewRow;
            (gr.FindControl("txtObservacionConcepto") as TextBox).Visible = (gr.FindControl("txtValorConcepto") as TextBox).Visible = chk.Checked;
            this.mpeValidar.Show();        
        }

        /// <summary>
        /// Método que retorna el valor del concepto
        /// </summary>
        /// <param name="idconcept">Id del concepto por glosa</param>
        /// <returns></returns>
        protected string GetValueText(object idconcept, object field)
        {
            ConceptoGlosa oConcepto = this.lConcepto.Find(x => x.idconcept == Convert.ToInt32(idconcept));
            if (field.ToString() == "Valor")
                return (oConcepto != null) ? Convert.ToInt32(oConcepto.conceptvalue).ToString() : string.Empty;
            else
                return (oConcepto != null) ? oConcepto.conceptobservations : string.Empty;
        }
        
        /// <summary>
        /// Método que carga los valores de la glosa
        /// </summary>
        /// <param name="oGlosa">Objeto con información de la glosa</param>
        private void LoadComment(Glosa oGlosa)
        {
            this.iComment = oGlosa.id;
            this.ddlTipo.SelectedValue = oGlosa.type.ToString();
            this.txtObservaciones.Text = oGlosa.observations;
            this.txtFecha.Text = oGlosa.transactdate.ToString("dd/MM/yyyy");
            this.lConcepto = oGlosa.lConcept;
            this.BindCommentConceptGrid();
            this.FillConceptGrid();
        }

        /// <summary>
        /// Método que selecciona los conceptos ya agregados a la glosa
        /// </summary>
        private void FillConceptGrid()
        {
            this.DisablePaging();
            ConceptoGlosa oConcept = null;
            int iConcepto = 0;
            foreach (GridViewRow row in this.gvConceptos.Rows)
            {
                iConcepto = Convert.ToInt32(this.gvConceptos.DataKeys[row.RowIndex]["idconcept"]);
                CheckBox myCheckBox = (CheckBox)row.FindControl("chkConcepto");
                oConcept = this.lConcepto.Find(x => x.idconcept == iConcepto);
                if (oConcept != null)
                {
                    myCheckBox.Checked = (row.FindControl("txtObservacionConcepto") as TextBox).Visible = (row.FindControl("txtValorConcepto") as TextBox).Visible = true;
                    (row.FindControl("txtObservacionConcepto") as TextBox).Text = oConcept.conceptobservations;
                    (row.FindControl("txtValorConcepto") as TextBox).Text = Convert.ToInt32(oConcept.conceptvalue).ToString();
                }                 
            }
            this.gvConceptos.AllowPaging = true;
            this.RememberConceptValues();
            this.RememberOldValues();            
            this.BindConceptGrid();
            this.RePopulateValues();         
        }
    }
}