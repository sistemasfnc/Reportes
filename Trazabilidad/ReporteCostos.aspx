<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="ReporteCostos.aspx.cs" Inherits="Trazabilidad.ReporteCostos" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Reporte de distribuci&oacute;n de costos</h1>
    <table style="width: 100%">
        <tr>
            <td>Nombre:</td>
            <td>
                <asp:TextBox ID="txtNombre" runat="server"></asp:TextBox>
            </td>
            <td>Documento:</td>
            <td>
                <asp:TextBox ID="txtDocumento" runat="server" MaxLength="20"></asp:TextBox>                
            </td>
            <td>Mes:</td>
            <td>
                <asp:DropDownList ID="ddlMes" runat="server"></asp:DropDownList>
            </td>
            <td>A&ntilde;o:
            </td>
            <td>
                <asp:DropDownList ID="ddlAno" runat="server"></asp:DropDownList>
            </td>            
            <td>
                <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" OnClick="btnBuscar_Click" />
            </td>
        </tr>
        <tr>
            <td>Centro de costo:</td>
            <td>
                <asp:DropDownList ID="ddlCentro" runat="server"></asp:DropDownList>
            </td>
            <td>Completo:</td>
            <td>
                <asp:DropDownList ID="ddlCompleto" runat="server">
                    <asp:ListItem Text="" Value=""></asp:ListItem>
                    <asp:ListItem Text="Si" Value="1"></asp:ListItem>
                    <asp:ListItem Text="No" Value="0"></asp:ListItem>
                </asp:DropDownList>

            </td>            
            <td colspan="4">&nbsp;</td>            
            <td>
                <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click"  />
            </td>
        </tr>
    </table>
     <br />
    <asp:GridView ID="gvCostos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AllowPaging="true" PageSize="30" OnPageIndexChanging="gvCostos_PageIndexChanging">
        <Columns>            
            <asp:BoundField DataField="sdocument" HeaderText="Documento" ItemStyle-HorizontalAlign="Left" />
            <asp:BoundField DataField="sname" HeaderText="Nombre" ItemStyle-HorizontalAlign="Left" />
            <asp:BoundField DataField="smaincostcenter" HeaderText="Centro principal" ItemStyle-HorizontalAlign="Center"/>
            <asp:BoundField DataField="iyear" HeaderText="A&ntilde;o" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="smonth" HeaderText="Mes" ItemStyle-HorizontalAlign="Center" />                        
            <asp:BoundField DataField="sstatus" HeaderText="Cerrado" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="suser" HeaderText="Usuario" ItemStyle-HorizontalAlign="Left" />
            <asp:BoundField DataField="scode" HeaderText="Centro" />
            <asp:BoundField DataField="dvalue" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:F2}" />
            <asp:BoundField DataField="dtotal" HeaderText="% Completado" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:F2}" />            
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
        <asp:ImageButton ID="imbExportar" runat="server" ImageUrl="~/images/excel.png" ToolTip="Exportar a excel" Width="64" Height="64" OnClick="imbExportar_Click" />
    </div>
</asp:Content>
