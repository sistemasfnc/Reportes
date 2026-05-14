using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facade;
using Config;
using EventLog;
using Utils;
using System.Collections;

namespace Trazabilidad
{
    public partial class Farmacia : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.pharmacylist))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.txtFechaInicio.Text = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy");
                this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
                this.BindGrid();
            }
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtDocmento.Text = string.Empty;
            this.txtFechaInicio.Text = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy");
            this.txtFechaFin.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.txtIngreso.Text = string.Empty;
            this.txtPaciente.Text = string.Empty;
            this.txtInsumo.Text = string.Empty;
            this.ddlEstado.ClearSelection();
            this.BindGrid();
        }

        private void BindGrid()
        {
            Cargo oCargo = null;
            using (FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion")))
            {
                oCargo = new Cargo()
                {
                    idadmission = this.txtIngreso.Text,
                    initialdate = (!string.IsNullOrEmpty(this.txtFechaInicio.Text)) ? Convert.ToDateTime(this.txtFechaInicio.Text) : new DateTime(),
                    finaldate = (!string.IsNullOrEmpty(this.txtFechaFin.Text)) ? Convert.ToDateTime(this.txtFechaFin.Text) : new DateTime(),
                    patientdocument = this.txtDocmento.Text,
                    patientfullname = this.txtPaciente.Text,
                    sattentiontype = this.ddlEstado.SelectedValue,
                    service = this.txtInsumo.Text,
                    lastuser = (this.oUser.idprofile == (int)ProfileEnum.cashier || this.oUser.idprofile == (int)ProfileEnum.rhbcashier) ? String.Join("','", this.oUser.otheruser.ToArray()) : string.Empty,
                };
                this.gvCargos.DataKeyNames = new string[]
                {
                    "idadmission",
                    "id",
                    "service",
                    "patientdocument",
                    "scharge",
                    "sattentiontype"
                };
                try
                {
                    this.gvCargos.DataSource = oFacade.GetPhamacyCharges(oCargo);
                    this.gvCargos.DataBind();
                }
                catch (Exception ex)
                {

                    LogError.WriteError("Facturacion", "Aplicacion", ex); 
                }                
            }
        }

        protected bool EnableButton()
        {
            return Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.pharmacyedit);
        }

        protected void gvCargos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            Cargo cargo = null;
            if (e.CommandName == "Guardar")
            {                 
                GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
                GridViewRow row = gvCargos.Rows[gr.RowIndex];
                DropDownList ddlEstadoGrid = (DropDownList)row.FindControl("ddlEstadoGrid");
                if (!string.IsNullOrEmpty(ddlEstadoGrid.SelectedValue))
                {
                    cargo = new Cargo()
                    {
                        id = Convert.ToInt32(this.gvCargos.DataKeys[gr.RowIndex]["id"]),
                        scharge = this.gvCargos.DataKeys[gr.RowIndex]["scharge"].ToString(),
                        sattentiontype = ddlEstadoGrid.SelectedValue,
                    };
                    this.UpdateCharge(cargo);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Debe seleccionar un valor en el estado para actualizar el cargo');", true);
                }
            }
        }

        protected void gvCargos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvCargos.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void ddlEstadoGrid_DataBinding(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddl.NamingContainer;
            string currentValue = DataBinder.Eval(row.DataItem, "sattentiontype").ToString();
            ddl.ClearSelection();
            ListItem item = ddl.Items.FindByValue(currentValue);
            if (item != null)
            {
                item.Selected = true;
            }
        }

        private void UpdateCharge(Cargo cargo)
        {
            using (FacadeCargo oFacade = new FacadeCargo(Configuration.GetStringValue("FNCFacturacion")))
            {
                try
                {
                    oFacade.UpdatePharmacyEstatus(cargo);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Cargo actualizado correctamente');", true);
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", ex);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, "alert('Ha ocurrido un error en la actualización del estado del cargo');", true);
                }                                
            }
        }
    }
}