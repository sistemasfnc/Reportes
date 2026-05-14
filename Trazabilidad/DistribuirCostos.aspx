<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="DistribuirCostos.aspx.cs" Inherits="Trazabilidad.DistribuirCostos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function OpenWindow(sdocument)
        {
            var sPage = "AsignarCentros.aspx?document=" + sdocument;
            window.open(sPage, "_blank", "width=900,height=600,dependent=no,resizable=yes,toolbar=no,status=no,directories=no,menubar=no,scrollbars=yes,top=100px,left=400px");
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Asignar Tiempo</h1>
    <div>
        <table style="width:100%">
            <tr>
                <td>Documento:</td>
                <td>
                    <asp:TextBox ID="txtDocumento" runat="server" MaxLength="20"></asp:TextBox>
                </td>
                <td>Nombre:</td>
                <td>
                    <asp:TextBox ID="txtNombre" runat="server" MaxLength="100"></asp:TextBox>
                </td>
                <td>Apellido:</td>
                <td>
                    <asp:TextBox ID="txtApellido" runat="server" MaxLength="100"></asp:TextBox>
                </td>
                <td>Centro de costos:</td>
                <td>
                    <asp:DropDownList ID="ddlCentro" runat="server"></asp:DropDownList>
                </td>
                <td>
                    <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" OnClick="btnBuscar_Click"  />
                </td>
                <td>
                    <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
                </td>
            </tr>
        </table>
    </div>
    <br /> 
    <div>
        <asp:GridView ID="gvEmpleados" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AllowPaging="true" PageSize="30" OnPageIndexChanging="gvEmpleados_PageIndexChanging" OnRowCommand="gvEmpleados_RowCommand">
            <Columns>            
                <asp:BoundField DataField="sdocument" HeaderText="Documento" />
                <asp:BoundField DataField="sname" HeaderText="Nombres" />
                <asp:BoundField DataField="slastname" HeaderText="Apellidos" />
                <asp:BoundField DataField="smaincostcenter" HeaderText="Centro de costos" />
                <asp:TemplateField>
                    <ItemTemplate>                        
                        <asp:ImageButton ID="imbAsignar" runat="server" CommandName="Assign" ImageUrl="~/images/admin.png" Width="16" Height="16" />
                    </ItemTemplate>
                </asp:TemplateField>
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
    </div>
</asp:Content>
