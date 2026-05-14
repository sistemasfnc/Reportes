using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using Entity;
using EventLog;
using Utils;
using Config;
using System.Data;
using OfficeOpenXml;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using System.IO;
using System.Collections;

namespace Trazabilidad
{
    public partial class Glosas : System.Web.UI.Page
    {
        /// <summary>
        /// Almacena datatable con la consulta de glosas
        /// </summary>
        private DataTable dtComment
        {
            get { return (Session["dtComment"] != null) ? Session["dtComment"] as DataTable : null; }
            set { Session["dtComment"] = value; }
        }

        /// <summary>
        /// Almacena listado de conceptos por glosa
        /// </summary>
        private List<ConceptoGlosa> lConcept
        {
            get { return (ViewState["lConcept"] != null) ? ViewState["lConcept"] as List<ConceptoGlosa> : null; }
            set { ViewState["lConcept"] = value; }
        }

        /// <summary>
        /// Id de la glosa seleccionada
        /// </summary>
        private int iComment
        {
            get { return (ViewState["iComment"] != null) ? Convert.ToInt32(ViewState["iComment"]) : 0; }
            set { ViewState["iComment"] = value; }
        }

        /// <summary>
        /// Id del concepto seleccionado
        /// </summary>
        private int iConcept
        {
            get { return (ViewState["iConcept"] != null) ? Convert.ToInt32(ViewState["iConcept"]) : 0; }
            set { ViewState["iConcept"] = value; }
        }

        /// <summary>
        /// Listado de respuestas por concepto
        /// </summary>
        private List<RespuestaGlosa> lRespuesta
        {
            get { return (ViewState["lRespuesta"] != null) ? ViewState["lRespuesta"] as List<RespuestaGlosa> : new List<RespuestaGlosa>(); }
            set { ViewState["lRespuesta"] = value; }
        }

        /// <summary>
        /// Usuario logueado en el sistema
        /// </summary>
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        /// <summary>
        /// Valor del concepto
        /// </summary>
        private int iConceptValue
        {
            get { return (ViewState["iConceptValue"] != null) ? Convert.ToInt32(ViewState["iConceptValue"]) : 0; }
            set { ViewState["iConceptValue"] = value; }
        }

        /// <summary>
        /// Método que carga la página Inicial
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.responsecomment))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                //Carga controles inciales
                this.LoadControls();
            }
        }

        /// <summary>
        /// Retorna el texto del tipo de glosa para pintarlo en la grilla
        /// </summary>
        /// <param name="oValue">Valor del tipo en BD</param>
        /// <returns>Texto del tipo</returns>
        protected string GetVal(object oValue)
        {
            return (oValue.ToString() == "1" ? "Devoluci&oacute;n" : "Glosa");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvGlosas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvGlosas.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        /// <summary>
        /// Evento que captura los comandos en la grilla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvGlosas_RowCommand(object sender, GridViewCommandEventArgs e)
        {            
            //Si el comando es responder la glosa
            if (e.CommandName == "Respuesta")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.iComment = Convert.ToInt32(this.gvGlosas.DataKeys[gr.RowIndex]["id"]);
                this.pnlRespuestas.Visible = false;
                this.BindConceptCommentGrid(iComment);
                this.txtFechaRespuesta.Text = (!string.IsNullOrEmpty(this.gvGlosas.DataKeys[gr.RowIndex]["responsedate"].ToString())) ? Convert.ToDateTime(this.gvGlosas.DataKeys[gr.RowIndex]["responsedate"]).ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy");
                /*this.txtObservaciones.Text = this.gvGlosas.DataKeys[gr.RowIndex]["analysis"].ToString();
                this.txtValor.Text = Convert.ToInt32(this.gvGlosas.DataKeys[gr.RowIndex]["acceptedvalue"]).ToString();
                this.lblValue.Text = Convert.ToInt32(this.gvGlosas.DataKeys[gr.RowIndex]["commentvalue"]).ToString();
                this.txtFechaRespuesta.Text = (!string.IsNullOrEmpty(this.gvGlosas.DataKeys[gr.RowIndex]["responsedate"].ToString())) ? Convert.ToDateTime(this.gvGlosas.DataKeys[gr.RowIndex]["responsedate"]).ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy");
                this.BindConceptGrid();*/
                this.mpeValidar.Show();
            }
            //Si el comando es generar PDF
            else if (e.CommandName == "Descargar")
            {
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                this.iComment = Convert.ToInt32(this.gvGlosas.DataKeys[gr.RowIndex]["id"]);
                if (this.gvGlosas.DataKeys[gr.RowIndex]["responsedate"] != null)
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"window.open('GenerarReporte.aspx?iComment=" + this.iComment.ToString() + "');", true);
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('La glosa no tiene respuesta por lo tanto el archivo no puede ser generado');", true);
                }

            }
        }

        /// <summary>
        /// Evento botón guardar los conceptos de respuesta de la glosa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            string sValidation = this.GetError();
            if (string.IsNullOrEmpty(sValidation))
            {
                RespuestaGlosa oRespuesta = null;
                FacadeGlosa oFacade = new FacadeGlosa();
                Glosa oGlosa = null;
                try
                {
                    oFacade.sConnection = Configuration.GetStringValue("FNCFacturacion");
                    oGlosa = new Glosa()
                    {
                        accepted = 0,
                        id = this.iComment,
                        responsedate = Convert.ToDateTime(this.txtFechaRespuesta.Text),
                        lConcept = new List<ConceptoGlosa>(),
                    };
                    oFacade.CreateOrEdit(oGlosa, false);
                    oRespuesta = new RespuestaGlosa()
                    {
                        idcomment = this.iComment,
                        idresponse = Convert.ToInt32(Request.Form["rbtConcepto"]),
                        idconcept = this.iConcept,
                        acceptedvalue = !string.IsNullOrEmpty(this.txtValor.Text) ? Convert.ToInt32(this.txtValor.Text) : 0,
                        observations = this.txtObservaciones.Text,
                        idservice = Convert.ToInt32(this.ddlServicio.SelectedValue),
                        responsible = this.txtResponsable.Text,
                    };
                    oFacade.InsertConceptResponse(oRespuesta);
                    this.lRespuesta = oFacade.GetConceptResponse(new RespuestaGlosa() { idcomment = this.iComment });
                    this.pnlRespuestas.Visible = false;
                    this.mpeValidar.Show();
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
                    oRespuesta = null;
                }
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"alert('Respuesta de glosa agregada correctamente');", true);
            }
            else
            {
                this.CheckRadios();
                this.lblError.Text = sValidation;
                this.mpeValidar.Show();
            }                       
        }

        /// <summary>
        /// Método que verifica los conceptos por glosa y los selecciona en el checkbox
        /// </summary>
        /// <param name="iConcept"></param>
        /// <returns></returns>
        protected bool CheckConcept(object iConcept)
        {
            return (this.lConcept != null) ? (this.lConcept.Count(x => x.idcomment == this.iComment && x.idconcept == (int)iConcept) != 0) : false;
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
                this.gvConceptos.DataSource = oFacade.GetConcepts("9");
                this.gvConceptos.DataBind();
            }
        }

        /// <summary>
        /// Método que carga los DropDown list
        /// </summary>
        private void LoadDropDown()
        {
            FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                this.ddlEmpresa.DataSource = oFacade.GetList("company");
                this.ddlEmpresa.DataTextField = "name";
                this.ddlEmpresa.DataValueField = "name";
                this.ddlEmpresa.DataBind();
                this.ddlEmpresa.SelectedValue = string.Empty;
                this.ddlCategoria.DataSource = oFacade.GetList("category");
                this.ddlCategoria.DataTextField = "name";
                this.ddlCategoria.DataValueField = "code";
                this.ddlCategoria.DataBind();
                this.ddlCategoria.SelectedValue = string.Empty;
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
        /// Método que asigna valores iniciales a los controles
        /// </summary>
        private void LoadControls()
        {
            FacadeGlosa oFacade = new FacadeGlosa();
            try
            {
                oFacade.sConnection = Configuration.GetStringValue("FNCFacturacion");
                this.lConcept = oFacade.GetCoceptsByComment();
                this.BindGrid();
                this.txtFechaRespuesta.Text = DateTime.Now.ToString("dd/MM/yyyy");
                this.LoadDropDown();
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
        /// Método que carga la grilla de glosas
        /// </summary>
        private void BindGrid()
        {
            Glosa oEntity = null;
            using (FacadeGlosa oFacade = new FacadeGlosa())
            {
                oEntity = new Glosa()
                {
                    invoice = this.txtFactura.Text,
                    finaldate = (!string.IsNullOrEmpty(this.txtFechaFin.Text)) ? Convert.ToDateTime(this.txtFechaFin.Text) : (Nullable<DateTime>)null,
                    initialdate = (!string.IsNullOrEmpty(this.txtFechaInicio.Text)) ? Convert.ToDateTime(this.txtFechaInicio.Text) : (Nullable<DateTime>)null,
                    type = Convert.ToInt32(this.ddlTipo.SelectedValue),
                    company = this.ddlEmpresa.SelectedValue,
                };
                oFacade.sConnection = Configuration.GetStringValue("FNCFacturacion");
                this.dtComment = oFacade.GetCommentViewData(oEntity);
                this.gvGlosas.DataKeyNames = new string[] { "id", "observations", "acceptedvalue", "responsedate", "analysis", "commentvalue" };
                this.gvGlosas.DataSource = this.dtComment;
                this.gvGlosas.DataBind();
                oEntity = null;
            }
        }

        /// <summary>
        /// Método que obtiene los conceptos seleccionados
        /// </summary>
        /// <returns>Lista de conceptos seleccionados</returns>
        private List<ConceptoGlosa> GetSelectedConcept()
        {
            //Declaración de variables
            List<ConceptoGlosa> lConcept = new List<ConceptoGlosa>();
            ConceptoGlosa oEntity = null;
            //Recorre el grid de conceptos
            foreach (GridViewRow gr in this.gvConceptos.Rows)
            {
                //Verifica si el concepto fue seleccionado
                if ((gr.FindControl("chkConcepto") as CheckBox).Checked)
                {
                    oEntity = new ConceptoGlosa()
                    {
                        idconcept = Convert.ToInt32(this.gvConceptos.DataKeys[gr.RowIndex]["idconcept"]),
                        idcomment = this.iComment,                        
                    };
                    lConcept.Add(oEntity);
                }
            }
            return lConcept;
        }

        /// <summary>
        /// Método que carga el grid de conceptos por glosa
        /// </summary>
        private void BindConceptCommentGrid(int idcomment)
        {
            FacadeGlosa oFacade = new FacadeGlosa();
            try
            {                
                oFacade.sConnection = Configuration.GetStringValue("FNCFacturacion");
                this.gvConceptosGlosa.DataKeyNames = new string[] { "idconcept", "idcomment", "conceptvalue", "conceptname" };
                this.gvConceptosGlosa.DataSource = oFacade.GetCoceptsByComment(idcomment);
                this.gvConceptosGlosa.DataBind();
                this.lRespuesta = oFacade.GetConceptResponse(new RespuestaGlosa() { idcomment = idcomment });
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbCancel_Click(object sender, ImageClickEventArgs e)
        {
            this.mpeValidar.Hide();
            this.pnlRespuestas.Visible = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.ddlEmpresa.SelectedValue = string.Empty;
            this.ddlTipo.SelectedValue = "0";
            this.txtFechaFin.Text = string.Empty;
            this.txtFechaInicio.Text = string.Empty;
            this.txtFactura.Text = string.Empty;
            this.BindGrid();
        }

        /// <summary>
        /// Evento del botón de exportar a Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbExportar_Click(object sender, ImageClickEventArgs e)
        {
            ExcelPackage oExcel = new ExcelPackage();
            ExcelWorksheet ws = oExcel.Workbook.Worksheets.Add("Glosas");
            ws.Cells["A1"].LoadFromDataTable(this.dtComment, true);
            Response.AddHeader("content-disposition", "attachment; filename=glosas.xlsx");
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
        /// Evento de clic en botón de la grilla de conceptos por glosa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvConceptosGlosa_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;                        
            //Si el comando es responder la glosa
            if (e.CommandName == "Responder")
            {
                this.pnlRespuestas.Visible = true;
                this.BindConceptGrid();
                this.iConcept = Convert.ToInt32(this.gvConceptosGlosa.DataKeys[gr.RowIndex]["idconcept"]);
                this.iConceptValue = Convert.ToInt32(this.gvConceptosGlosa.DataKeys[gr.RowIndex]["conceptvalue"]);
                this.lblConcepto.Text = this.gvConceptosGlosa.DataKeys[gr.RowIndex]["conceptname"].ToString();
                this.lblError.Text = string.Empty;
                this.SetResponseValues();
                this.BindConceptGrid();
                this.mpeValidar.Show();                
            }
        }

        /// <summary>
        /// Método que selecciona el radio button de la grilla con la respuesta seleccionada
        /// </summary>
        /// <param name="idconcept"></param>
        /// <returns></returns>
        protected string SelectGridRadio(object idconcept)
        {
            return (this.lRespuesta.Count(x => x.idresponse == (int)idconcept && x.idconcept == this.iConcept) > 0) ? "checked" : string.Empty;            
        }

        /// <summary>
        /// Método que setea los valores de los campos según la respuesta seleccionada
        /// </summary>
        private void SetResponseValues()
        {
            RespuestaGlosa oRespuesta = this.lRespuesta.Find(x => x.idconcept == this.iConcept && x.idcomment == this.iComment);
            if (oRespuesta != null)
            {
                this.txtValor.Text = oRespuesta.acceptedvalue.ToString();
                this.txtObservaciones.Text = oRespuesta.observations;
                this.txtResponsable.Text = oRespuesta.responsible;
                this.ddlCategoria.SelectedValue = oRespuesta.idcategory.ToString();
                ddlCategoria_SelectedIndexChanged(this.ddlCategoria, null);
                this.ddlServicio.SelectedValue = oRespuesta.idservice.ToString();
            }
            else
            {
                this.txtValor.Text = string.Empty;
                this.txtObservaciones.Text = string.Empty;
                this.ddlCategoria.SelectedValue = string.Empty;
                ddlCategoria_SelectedIndexChanged(this.ddlCategoria, null);
                this.txtResponsable.Text = string.Empty;
            }
        }

        /// <summary>
        /// Método que retorna los mensajes de error de validación de los campos 
        /// </summary>
        /// <returns></returns>
        private string GetError()
        {
            string sError = string.Empty;
            if (Request.Form["rbtConcepto"] == null)
            {
                sError = "Debe seleccionar una respuesta para el concepto";
            }
            else if (string.IsNullOrEmpty(this.txtValor.Text))
            {
                sError = "Debe ingresar un valor aceptado para la glosa";
                this.txtValor.Focus();
            }
            else if (Convert.ToInt32(this.txtValor.Text) > this.iConceptValue)
            {
                sError = "El valor aceptado no puede ser mayor al valor de lal glosa";
                this.txtValor.Focus();
            }
            else if (string.IsNullOrEmpty(this.ddlServicio.SelectedValue))
            {
                sError = "Debe seleccionar un servicio responsable";
                this.ddlServicio.Focus();
            }
            else if (string.IsNullOrEmpty(this.txtResponsable.Text))
            {
                sError = "Debe ingresar un responsable";
                this.txtResponsable.Focus();
            }
            return sError;            
        }

        /// <summary>
        /// Evento de selección del dropdown de categorias de responsables
        /// </summary>
        /// <param name="sender">DropDown List</param>
        /// <param name="e">Evento</param>
        protected void ddlCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.CheckRadios();
            if (!string.IsNullOrEmpty(this.ddlCategoria.SelectedValue))
            {
                FacadeGenerico oFacade = new FacadeGenerico(Configuration.GetStringValue("FNCFacturacion"));
                try
                {
                    this.ddlServicio.DataSource = oFacade.GetList("services", Convert.ToInt32(this.ddlCategoria.SelectedValue));
                    this.ddlServicio.DataTextField = "name";
                    this.ddlServicio.DataValueField = "code";
                    this.ddlServicio.DataBind();

                    this.mpeValidar.Show();
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
                this.ddlServicio.DataSource = new List<Generic>();
                this.ddlServicio.DataBind();
            }            
        }

        /// <summary>
        /// Método que ejecuta js que checkea los radios con la respuesta seleccionada
        /// </summary>
        private void CheckRadios()
        {
            if (Request.Form["rbtConcepto"] != null)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", @"SelectRadio(" + Request.Form["rbtConcepto"] + ");", true);
            }
        }

        /// <summary>
        /// Evento botón buscar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }
    }
}