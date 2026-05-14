<%@ Page Title="Reporte relaciones de envío" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="ReporteRelaciones.aspx.cs" Inherits="Trazabilidad.ReporteRelaciones" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Reporte Relaciones de Env&iacute;o</h1>
    <table style="width: 100%">
        <tr>
            <td>Relaci&oacute;n de env&iacute;o:</td>
            <td>
                <asp:TextBox ID="txtRelacion" runat="server" MaxLength="10"></asp:TextBox>
            </td>
            <td>Factura:</td>
            <td>
                <asp:TextBox ID="txtFactura" runat="server" MaxLength="10"></asp:TextBox>         
                <asp:FilteredTextBoxExtender ID="fteFactura" runat="server" TargetControlID="txtFactura" FilterType="Numbers"></asp:FilteredTextBoxExtender>
            </td>
            <td>Fecha inicial de env&iacute;o:</td>
            <td>
                <asp:TextBox ID="txtFechaInicio" runat="server" onblur="this.blur();"></asp:TextBox>
                <asp:ImageButton ID="imbFechaInicio" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                <asp:CalendarExtender ID="ceFechaInicio" runat="server" TargetControlID="txtFechaInicio" Format="dd/MM/yyyy" PopupButtonID="imbFechaInicio"></asp:CalendarExtender>
            </td>
            <td>
                Fecha final de env&iacute;o:
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
            <td>Estado Relaci&oacute;n de Env&iacute;o:</td>
            <td>
                <asp:DropDownList ID="ddlEstado" runat="server">
                    <asp:ListItem></asp:ListItem>                    
                    <asp:ListItem Text="Enviado a log&iacute;stica" Value="E"></asp:ListItem>
                    <asp:ListItem Text="Recibido en log&iacute;stica" Value="R"></asp:ListItem>                    
                    <asp:ListItem Text="En log&iacute;stica" Value="P"></asp:ListItem>
                    <asp:ListItem Text="En facturaci&oacute;n" Value="F"></asp:ListItem>
                    <asp:ListItem Text="Tramitado Completo" Value="T"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>Factura enviada a log&iacute;stica:</td>
            <td>
                <asp:DropDownList ID="ddlAsignado" runat="server">
                    <asp:ListItem Text="" Value=""></asp:ListItem>
                    <asp:ListItem Text="Si" Value="1"></asp:ListItem>
                    <asp:ListItem Text="No" Value="0"></asp:ListItem>
                </asp:DropDownList>
            </td> 
            <td>Factura en log&iacute;stica:</td>
            <td>
                <asp:DropDownList ID="ddlRecibido" runat="server">
                    <asp:ListItem Text="" Value=""></asp:ListItem>
                    <asp:ListItem Text="Si" Value="1"></asp:ListItem>
                    <asp:ListItem Text="No" Value="0"></asp:ListItem>
                </asp:DropDownList>
            </td>             
             <td>Factura en facturaci&oacute;n:</td>
            <td>
                <asp:DropDownList ID="ddlEnviado" runat="server">
                    <asp:ListItem Text="" Value=""></asp:ListItem>
                    <asp:ListItem Text="Si" Value="1"></asp:ListItem>
                    <asp:ListItem Text="No" Value="0"></asp:ListItem>
                </asp:DropDownList>
            </td>             
            <td>
                <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
            </td>
        </tr>
    </table>
     <br />
    <asp:GridView ID="gvRelaciones" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AllowPaging="true" PageSize="30" OnPageIndexChanging="gvRelaciones_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="Numero" HeaderText="Relaci&oacute;n" HeaderStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="EstadoTexto" HeaderText="Estado" HeaderStyle-HorizontalAlign="Center" />            
            <asp:BoundField DataField="Factura" HeaderText="Factura" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />               
            <asp:BoundField DataField="FechaFactura" HeaderText="Fecha Factura" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:dd/MM/yyyy}"  />               
            <asp:BoundField DataField="Valor" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:C}" />               
            <asp:BoundField DataField="Empresa" HeaderText="Asegurador" HeaderStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="FechaCompleto" HeaderText="F. Completo" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:dd/MM/yyyy}" />
            <asp:BoundField DataField="UsuarioCompleta" HeaderText="Usr. Completa" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center"  />            
            <asp:BoundField DataField="Observacion" HeaderText="Observaci&oacute;n" HeaderStyle-HorizontalAlign="Center" />            
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
        <asp:ImageButton ID="imbExportar" runat="server" ImageUrl="~/images/excel.png" ToolTip="Exportar a excel" OnClick="imbExportar_Click" Width="30" Height="30" />
    </div>
</asp:Content>
