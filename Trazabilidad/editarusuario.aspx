<%@ Page Title="Editar Usuario" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="editarusuario.aspx.cs" Inherits="Trazabilidad.editarusuario" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table>
        <tr>
            <td>Usuario:</td>        
            <td>
                <asp:TextBox ID="txtUsuario" runat="server" MaxLength="50"></asp:TextBox><span class="error">*</span>
                <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/zoom.png" Width="20" Height="20" OnClick="btnBuscar_Click" />
                <asp:RequiredFieldValidator ID="rfvUsuario" runat="server" ControlToValidate="txtUsuario" Display="None" ErrorMessage="El nombre de usuario es obligatorio" SetFocusOnError="true" ValidationGroup="frmUsuario"></asp:RequiredFieldValidator>
                <asp:ValidatorCalloutExtender ID="vceUsuario" runat="server" TargetControlID="rfvUsuario"></asp:ValidatorCalloutExtender>
            </td>
        </tr>
        <tr>
            <td>Nombres:</td>                
            <td>
                <asp:TextBox ID="txtNombre" runat="server" MaxLength="100"></asp:TextBox><span class="error">*</span>
                <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre" Display="None" ErrorMessage="Los nombres son obligatorios" SetFocusOnError="true" ValidationGroup="frmUsuario"></asp:RequiredFieldValidator>
                <asp:ValidatorCalloutExtender ID="vceNombre" runat="server" TargetControlID="rfvNombre"></asp:ValidatorCalloutExtender>
            </td>
        </tr>
        <tr>
            <td>Apellidos:</td>
            <td>
                <asp:TextBox ID="txtApellido" runat="server" MaxLength="100"></asp:TextBox><span class="error">*</span>
                <asp:RequiredFieldValidator ID="rfvApellido" runat="server" ControlToValidate="txtApellido" Display="None" ErrorMessage="Los nombres son obligatorios" SetFocusOnError="true" ValidationGroup="frmUsuario"></asp:RequiredFieldValidator>
                <asp:ValidatorCalloutExtender ID="vceApellido" runat="server" TargetControlID="rfvApellido"></asp:ValidatorCalloutExtender>
            </td>
        </tr>
        <tr>
            <td>Correo Electr&oacute;nico:</td>
            <td>
                <asp:TextBox ID="txtEmail" runat="server" MaxLength="80"></asp:TextBox><span class="error">*</span>
               <%-- <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ErrorMessage="El correo electr&oacute;nico no puede ser vacio"
                            ControlToValidate="tbEmail" Display="None"></asp:RequiredFieldValidator>
                <asp:ValidatorCalloutExtender ID="vceEmail" runat="server" TargetControlID="rfvEmail">
                </asp:ValidatorCalloutExtender>--%>
                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                    Display="None" ErrorMessage="El formato del correo es incorrecto. <i><b>micorreo@dominio.com</b></i>"
                    ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"  ValidationGroup="frmUsuario">
                </asp:RegularExpressionValidator>
                <asp:ValidatorCalloutExtender ID="vceEmail1" runat="server" TargetControlID="revEmail" />
            </td>
        </tr>        
        <tr>
            <td>Perfil:</td>
            <td>
                <asp:DropDownList ID="ddlPerfil" runat="server"></asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td style="vertical-align:top">Usuario Gestor:</td>
            <td>
                <asp:ListBox ID="ddlUsuario" runat="server" SelectionMode="Multiple" Height="100" Width="140"></asp:ListBox>                                
            </td>
        </tr>
        <tr>
            <td style="vertical-align:top">Centro de costos:</td>
            <td>
                <asp:ListBox ID="ddlCentro" runat="server" SelectionMode="Multiple" Height="100" Width="300"></asp:ListBox>
            </td>
        </tr>
        <tr style="text-align:center">
            <td>
                <asp:ImageButton ID="imbCancelar" runat="server" ToolTip="Cancelar" ImageUrl="~/images/cancel.png" Height="30" Width="30" OnClick="imbCancelar_Click" />
            </td>
            <td>
                <asp:ImageButton ID="imbAceptar" runat="server" ToolTip="Aceptar" ImageUrl="~/images/diskette.png" Height="30" Width="30" OnClick="imbAceptar_Click"  ValidationGroup="frmUsuario"/>
            </td>
        </tr>        
    </table>
    <asp:ModalPopupExtender ID="mpeUsuarios" runat="server" PopupControlID="pnUsuarios" TargetControlID="btnBuscar" BackgroundCssClass="modalBackground" OkControlID="lbtCerrar">        
    </asp:ModalPopupExtender>
    <asp:Panel ID="pnUsuarios" runat="server" CssClass="modalPopup" Style="position: absolute; display:none">
        <asp:Panel ID="pnlMensaje" runat="server">
            <div style="text-align: right">
                <asp:LinkButton ID="lbtCerrar" runat="server" Text="Cerrar [x]"></asp:LinkButton>
            </div>
            <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="False" HorizontalAlign="Center" Width="100%" OnRowCommand="gvUsuarios_RowCommand" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AllowPaging="true" PageSize="20" OnPageIndexChanging="gvUsuarios_PageIndexChanging">
                <Columns>
                    <asp:TemplateField ItemStyle-Wrap="false">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbtUsuario" runat="server" Text='<%# Eval("username") %>' CommandName="Seleccionar"></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:BoundField HeaderText="Nombres" DataField="firstname" />
                    <asp:BoundField HeaderText="Apellidos" DataField="lastname" />
                    <asp:BoundField HeaderText="Email" DataField="email" />
                </Columns>
                <FooterStyle BackColor="White" ForeColor="#000066" />
                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="White" ForeColor="#000066" HorizontalAlign="Left" />
                <RowStyle ForeColor="#000066" />
                <SelectedRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                <SortedAscendingCellStyle BackColor="#F1F1F1" />
                <SortedAscendingHeaderStyle BackColor="#007DBB" />
                <SortedDescendingCellStyle BackColor="#CAC9C9" />
                <SortedDescendingHeaderStyle BackColor="#00547E" />
            </asp:GridView>
        </asp:Panel>
    </asp:Panel>    
</asp:Content>
