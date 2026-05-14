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

namespace Trazabilidad
{
    public partial class Baul : System.Web.UI.Page
    {
        private User oUser
        {
            get { return Session["oUser"] as User; }
        }

        private int iidbaul
        {
            get { return (ViewState["iidbaul"] != null) ? Convert.ToInt32(ViewState["iidbaul"]): 0; }
            set { ViewState["iidbaul"] = value; }
        }

        private List<Entity.Baul> lBaul
        {
            get { return (ViewState["lBaul"] != null) ? ViewState["lBaul"] as List<Entity.Baul> : new List<Entity.Baul>(); }
            set { ViewState["lBaul"] = value; }

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (!Tools.HaveAccess(this.oUser.lSecurity, (int)Permissions.passwordtrunk))
                {
                    Response.Redirect("~/SinAcceso.aspx");
                }
                this.BindGrid();
            }
        }

        protected void btnBuscar_Click(object sender, ImageClickEventArgs e)
        {
            this.BindGrid();
        }

        protected void gvListado_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow gr = ((e.CommandSource) as Control).NamingContainer as GridViewRow;
            int iid = Convert.ToInt32(this.gvListado.DataKeys[gr.RowIndex]["iid"]);
            if (e.CommandName == "Editar")
            {
                this.LoadData(iid);
                this.mpeValidar.Show();
            }
            else if (e.CommandName == "Eliminar")
            {
                this.DeleteRecord(iid);
            }
        }

        protected void gvListado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvListado.PageIndex = e.NewPageIndex;
            this.BindGrid();
        }

        protected void imbAgregar_Click(object sender, ImageClickEventArgs e)
        {
            this.iidbaul = 0;
            this.txtUsuario.Text = this.txtPassword.Text = this.txtRole.Text = this.txtDetalle.Text = this.txtAcceso.Text = string.Empty;
            this.mpeValidar.Show();
        }

        protected void btnCancelar_Click(object sender, ImageClickEventArgs e)
        {
            this.txtUser.Text = this.txtAccess.Text = this.txtRol.Text = string.Empty;
            this.BindGrid();
        }

        protected void imbGuardar_Click(object sender, ImageClickEventArgs e)
        {
            Entity.Baul baul = new Entity.Baul();
            FacadeBaul facadeBaul = new FacadeBaul(Configuration.GetStringValue("FNCFacturacion"));
            string smessage = string.Empty;
            try
            {
                baul.suser = this.txtUsuario.Text;
                baul.spassword = this.txtPassword.Text;
                baul.srol = this.txtRole.Text;
                baul.sdetail = this.txtDetalle.Text;
                baul.saccess = this.txtAcceso.Text;
                baul.smodifiedby = this.oUser.username;
                if (this.iidbaul != 0)
                {
                    baul.iid = this.iidbaul;                    
                    facadeBaul.Update(baul);
                    smessage = "alert('El registro ha sido actualizado correctamente');";
                }
                else
                {
                    baul.screatedby = this.oUser.username;                    
                    facadeBaul.Insert(baul);
                    smessage = "alert('El registro ha sido creado correctamente');";
                }
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, smessage, true);
                this.BindGrid();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                smessage = "alert('" + ex.Message + "');";                
            }
            finally
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, smessage, true);
                facadeBaul.Dispose();
                baul = null;
            }
        }

        protected void imbClose_Click(object sender, ImageClickEventArgs e)
        {
            this.txtUsuario.Text = this.txtPassword.Text = this.txtRole.Text = this.txtDetalle.Text = this.txtAcceso.Text = string.Empty;
            this.iidbaul = 0;
            this.mpeValidar.Hide();
        }

        private void BindGrid()
        {
            Entity.Baul baul = new Entity.Baul();
            FacadeBaul facadeBaul = new FacadeBaul(Configuration.GetStringValue("FNCFacturacion"));
            try
            {
                baul.saccess = this.txtAccess.Text;
                baul.suser = this.txtUser.Text;
                baul.srol = this.txtRol.Text;
                this.lBaul = facadeBaul.GetBauls(baul);
                this.gvListado.DataKeyNames = new string[] { "iid", "saccess", "spassword", "suser", "srol", "sdetail" };
                this.gvListado.DataSource = this.lBaul;
                this.gvListado.DataBind();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                throw;
            }
            finally
            {
                facadeBaul.Dispose();
                facadeBaul = null;
                baul = null;
            }
        }

        private void LoadData(int iid)
        {
            Entity.Baul baul = this.lBaul.FirstOrDefault(x => x.iid == iid);
            if (baul != null)
            {
                this.iidbaul = iid;
                this.txtAcceso.Text = baul.saccess;
                this.txtDetalle.Text = baul.sdetail;
                this.txtPassword.Text = baul.spassword;
                this.txtRole.Text = baul.srol;
                this.txtUsuario.Text = baul.suser;
                this.mpeValidar.Show();
            }
        }

        private void DeleteRecord(int iid)
        {
            using(FacadeBaul facade = new FacadeBaul(Configuration.GetStringValue("FNCFacturacion")))
            {
                facade.Delete(iid);
            }
            this.BindGrid();
        }        
    }
}