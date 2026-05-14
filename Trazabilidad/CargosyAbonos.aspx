<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="CargosyAbonos.aspx.cs" Inherits="Trazabilidad.CargosyAbonos" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Cargos y abonos facturados</h1>
    <table style="width: 100%">
        <tr>
            <td>Ingreso:</td>
            <td>
                <asp:TextBox ID="txtIngreso" runat="server" MaxLength="10"></asp:TextBox>
                <asp:FilteredTextBoxExtender ID="fteIngreso" runat="server" TargetControlID="txtIngreso" FilterType="Numbers"></asp:FilteredTextBoxExtender>
            </td>
            <td>Factura:</td>
            <td>
                <asp:TextBox ID="txtFactura" runat="server" MaxLength="10"></asp:TextBox>
                <asp:FilteredTextBoxExtender ID="fteFactura" runat="server" TargetControlID="txtFactura" FilterType="Numbers"></asp:FilteredTextBoxExtender>
            </td>
            <td>Fecha Inicial:
            </td>
            <td>
                <asp:TextBox ID="txtFechaInicio" runat="server"></asp:TextBox>
                <asp:ImageButton ID="imbFechaInicio" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                <asp:CalendarExtender ID="ceFechaInicio" runat="server" TargetControlID="txtFechaInicio" Format="dd/MM/yyyy" PopupButtonID="imbFechaInicio"></asp:CalendarExtender>
            </td>
            <td>Fecha Final:
            </td>
            <td>
                <asp:TextBox ID="txtFechaFin" runat="server"></asp:TextBox>
                <asp:ImageButton ID="imbFechaFin" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                <asp:CalendarExtender ID="ceFechaFin" runat="server" TargetControlID="txtFechaFin" Format="dd/MM/yyyy" PopupButtonID="imbFechaFin"></asp:CalendarExtender>
            </td>  
             <td>
                <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" OnClick="btnBuscar_Click" />
            </td>          
        </tr>
        <tr>
            <td>Servicio:</td>
            <td>
                <asp:ComboBox ID="ddlServicio" runat="server" AutoCompleteMode="SuggestAppend"></asp:ComboBox>
            </td>
            <td>Empresa:</td>
            <td>
                <asp:DropDownList ID="ddlEmpresa" runat="server"></asp:DropDownList>
            </td>
            <td>Plan:</td>
            <td>
                <asp:ComboBox ID="ddlPlan" runat="server" AutoCompleteMode="SuggestAppend"></asp:ComboBox>
            </td>            
            <td>Origen</td>
            <td>
                <asp:DropDownList ID="ddlOrigen" runat="server">
                    <asp:ListItem Value="" Text=""></asp:ListItem>
                    <asp:ListItem Value="ABO" Text="Abono"></asp:ListItem>
                    <asp:ListItem Value="CAR" Text="Cargo"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>
                <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
            </td>
        </tr>
    </table>
    <br />
    <asp:GridView ID="gvCargos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AllowPaging="true" PageSize="30" OnPageIndexChanging="gvCargos_PageIndexChanging">
        <Columns>            
            <asp:BoundField DataField="company" HeaderText="Empresa" />
            <asp:BoundField DataField="plan" HeaderText="Plan" />
            <asp:BoundField DataField="service" HeaderText="Servicio" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="notuser" HeaderText="Origen" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="costcenter" HeaderText="Centro de costos" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="subcenter" HeaderText="Subcentro" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="idadmission" HeaderText="Ingreso" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="date" HeaderText="Fecha Ingreso" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="value" HeaderText="Valor Cargo" ItemStyle-HorizontalAlign="Right" />            
            <asp:BoundField DataField="invoice" HeaderText="Factura" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="invoicedate" HeaderText="Fecha Factura" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />            
            <asp:BoundField DataField="invoicevalue" HeaderText="Valor Factura" ItemStyle-HorizontalAlign="Right" />            
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
    <br />
    <div>
        <asp:ImageButton ID="imbExportar" runat="server" ImageUrl="~/images/excel.png" ToolTip="Exportar a excel" OnClick="imbExportar_Click" Width="64" Height="64" />
    </div>
</asp:Content>
