<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="usuarios.aspx.cs" Inherits="Trazabilidad.usuarios" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div>
        <table style="width:100%">
            <tr>
                <td>
                    Usuario:
                </td>
                <td>
                    <asp:TextBox ID="txtUsuario" runat="server" MaxLength="20"></asp:TextBox>
                </td>
                 <td>
                    Nombres:
                </td>
                <td>
                    <asp:TextBox ID="txtNombre" runat="server" MaxLength="100"></asp:TextBox>
                </td>
                 <td>
                    Apellidos:
                </td>
                <td>
                    <asp:TextBox ID="txtApellido" runat="server" MaxLength="100"></asp:TextBox>
                </td>
                <td>
                    Pefil:
                </td>
                <td>
                    <asp:DropDownList ID="ddlPerfil" runat="server"></asp:DropDownList>
                </td>
                <td>
                    <asp:ImageButton ID="btnBuscar" runat="server" OnClick="btnBuscar_Click" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" />
                </td>
                <td>
                    <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
                </td>
            </tr>            
        </table>
    </div>
    <br />
    <asp:GridView ID="gvUsuarios" runat="server" Width="100%" AutoGenerateColumns="False" PageSize="20" OnPageIndexChanging="gvUsuarios_PageIndexChanging" AllowPaging="True" OnRowCommand="gvUsuarios_RowCommand" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1">
        <Columns>
            <asp:BoundField HeaderText="Usuario" DataField="username" />
            <asp:BoundField HeaderText="Nombres" DataField="firstname" />
            <asp:BoundField HeaderText="Apellidos" DataField="lastname" />
            <asp:BoundField HeaderText="Email" DataField="email" />
            
            <asp:BoundField HeaderText="Perfil" DataField="profilename" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:ImageButton ID="imbEditar" CommandName="Editar" runat="server" ImageUrl="~/images/edit.png" Width="20" Height="20" BorderWidth="0" ToolTip="Editar" />
                </ItemTemplate>
            </asp:TemplateField>
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
    <div>
        <asp:ImageButton ID="imbAgregar" runat="server" ImageUrl="~/images/admin.png" Width="20" Height="20" ToolTip="Agregar Usuario" OnClick="imbAgregar_Click" />
    </div>
</asp:Content>
