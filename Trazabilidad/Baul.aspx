<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="Baul.aspx.cs" Inherits="Trazabilidad.Baul" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Ba&uacute;l de los recuerdos</h1>  
    <asp:UpdatePanel ID="upDatos" runat="server">
        <ContentTemplate>
            <div>
                <table>
                    <tr>
                        <td>
                            Acceso
                        </td>
                        <td>
                            <asp:TextBox ID="txtAccess" runat="server" MaxLength="150"></asp:TextBox>
                        </td>
                        <td>
                            Usuario
                        </td>
                        <td>
                            <asp:TextBox ID="txtUser" runat="server" MaxLength="100"></asp:TextBox>
                        </td>
                        <td>
                            Rol
                        </td>
                        <td>
                            <asp:TextBox ID="txtRol" runat="server" MaxLength="200"></asp:TextBox>
                        </td>
                        <td>
                            <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" OnClick="btnBuscar_Click" CausesValidation="false" />&nbsp;
                            <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click"  CausesValidation="false" />
                        </td>
                    </tr>
                </table>        
            </div>
            <br />
            <div>        
                <asp:GridView ID="gvListado" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" OnRowCommand="gvListado_RowCommand" OnPageIndexChanging="gvListado_PageIndexChanging" PageSize="50" AllowPaging="true">
                    <Columns>
                        <asp:TemplateField  ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:ImageButton ID="btnEditar" runat="server" Width="20" Height="20" ToolTip="Editar" CommandName="Editar" ImageUrl="~/images/edit.png" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:ImageButton ID="btnEliminar" runat="server" Width="20" Height="20" ToolTip="Eliminar" CommandName="Eliminar" ImageUrl="~/images/trash_can.png" OnClientClick="return confirm('Esta seguro que desea eliminar este registro?');" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField HeaderText="Acceso" DataField="saccess" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Usuario" DataField="suser" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Contrase&ntilde;a" DataField="spassword" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Rol" DataField="srol" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Detalles" DataField="sdetail" HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <HeaderStyle BackColor="#66b6b9" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="White" ForeColor="#000066" HorizontalAlign="Left" />
                    <RowStyle ForeColor="#000000" />
                    <SelectedRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <SortedAscendingCellStyle BackColor="#F1F1F1" />
                    <SortedAscendingHeaderStyle BackColor="#007DBB" />
                    <SortedDescendingCellStyle BackColor="#CAC9C9" />
                    <SortedDescendingHeaderStyle BackColor="#00547E" />
                </asp:GridView>
                <br />
                <asp:ImageButton ID="imbAgregar" runat="server" ImageUrl="~/images/add.png" Width="20" Height="20" ToolTip="Agregar" OnClick="imbAgregar_Click" CausesValidation="false"  />&nbsp;Agregar entrada
                <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>    
                <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                    <asp:Panel ID="pnlMensaje" runat="server">
                        <div style="text-align:right">
                            <asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" CausesValidation="false" /> 
                        </div>
                        <center>
                            <h3>Ingrese los datos del registro que desea crear</h3>
                        </center>
                        <table align="center">
                            <tr>
                                <td>
                                    Acceso:*
                                </td>
                                <td>
                                    <asp:TextBox ID="txtAcceso" runat="server" MaxLength="100" TabIndex="5" ValidationGroup="frmDatos"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvAcceso" runat="server" ControlToValidate="txtAcceso" SetFocusOnError="true" ErrorMessage="El acceso no puede ser vacío" ValidationGroup="frmDatos" Display="None"></asp:RequiredFieldValidator>
                                    <asp:ValidatorCalloutExtender ID="vceAcceso" runat="server" TargetControlID="rfvAcceso"></asp:ValidatorCalloutExtender>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Usuario:*
                                </td>                            
                                <td>
                                    <asp:TextBox ID="txtUsuario" runat="server" MaxLength="50" TabIndex="6" ValidationGroup="frmDatos"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvUsuario" runat="server" ControlToValidate="txtUsuario" SetFocusOnError="true" ErrorMessage="El usuario no puede ser vacío" ValidationGroup="frmDatos" Display="None"></asp:RequiredFieldValidator>
                                    <asp:ValidatorCalloutExtender ID="vceUsuario" runat="server" TargetControlID="rfvUsuario"></asp:ValidatorCalloutExtender>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Contrase&ntilde;a:*                                    
                                </td>
                                <td>
                                    <asp:TextBox ID="txtPassword" runat="server" MaxLength="50" TabIndex="7" ValidationGroup="frmDatos"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" SetFocusOnError="true" ErrorMessage="La contraseña no puede ser vacía" ValidationGroup="frmDatos" Display="None"></asp:RequiredFieldValidator>
                                    <asp:ValidatorCalloutExtender ID="vcePassword" runat="server" TargetControlID="rfvPassword"></asp:ValidatorCalloutExtender>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Rol:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtRole" runat="server" MaxLength="100" TabIndex="8" ValidationGroup="frmDatos"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Detalles:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtDetalle" runat="server" TextMode="MultiLine" Rows="3" MaxLength="1000" ValidationGroup="frmDatos"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" style="text-align:center">
                                    <asp:ImageButton ID="imbGuardar" runat="server" ImageUrl="~/images/diskette.png" Width="20" Height="20" OnClick="imbGuardar_Click" ValidationGroup="frmDatos" />&nbsp;
                                    <asp:ImageButton ID="imbClose" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" OnClick="imbClose_Click" CausesValidation="false"  />&nbsp;
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </asp:Panel>
                <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">        
                </asp:ModalPopupExtender>          
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
