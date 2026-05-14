<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="CruceCargosProgramas.aspx.cs" Inherits="Trazabilidad.CruceCargosProgramas" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <h1 style="font-size:15px">Cruce de ingresos programas</h1>
    <div>
        A continuaci&oacute;n encontrar&aacute; el listado de ingresos de programas que a&uacute;n no tienen asignados factura. 
        El sistema le asigna una factura autom&aacute;ticamente, revise el n&uacute;mero de la factura y asigne el que sea correcto
    </div>
    <br />
    <div>
        <table>
            <tr>
                <td>
                    Fecha Inicial:
                </td>
                <td>
                    <asp:TextBox ID="txtFechaInicio" runat="server"></asp:TextBox>
                    <asp:ImageButton ID="imbFechaInicio" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                    <asp:CalendarExtender ID="ceFechaInicio" runat="server" TargetControlID="txtFechaInicio" Format="dd/MM/yyyy" PopupButtonID="imbFechaInicio"></asp:CalendarExtender>
                </td>
                <td>
                    Fecha Final:
                </td>
                <td>
                    <asp:TextBox ID="txtFechaFin" runat="server"></asp:TextBox>
                    <asp:ImageButton ID="imbFechaFin" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                    <asp:CalendarExtender ID="ceFechaFin" runat="server" TargetControlID="txtFechaFin" Format="dd/MM/yyyy" PopupButtonID="imbFechaFin"></asp:CalendarExtender>
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
    <asp:GridView ID="gvIngresos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" OnRowCommand="gvIngresos_RowCommand">
        <Columns>
            <asp:TemplateField HeaderText="Ingreso">
                <ItemTemplate>
                    <asp:LinkButton ID="lbtDetail" runat="server" Text='<%# Eval("ientry") %>' CommandName="Detail" ToolTip="Ver Detalle"></asp:LinkButton>
                    <asp:GridView ID="gvDetalle" runat="server">
                        <Columns>
                            <asp:BoundField DataField="sconceptname" HeaderText="Concepto" />
                            <asp:BoundField DataField="sconstcenter" HeaderText="Centro" />
                            <asp:BoundField DataField="sservince" HeaderText="Cups" />
                            <asp:BoundField DataField="sservicename" HeaderText="Servicio" />
                            <asp:BoundField DataField="iqty" HeaderText="Cantidad" />
                            <asp:BoundField DataField="dvalue" HeaderText="Valor" />
                        </Columns>
                    </asp:GridView>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="sdocumenttype" HeaderText="Tipo Documento" />
            <asp:BoundField DataField="sdocument" HeaderText="Documento" />
            <asp:BoundField DataField="spatient" HeaderText="Paciente" />
            <asp:BoundField DataField="sagreement" HeaderText="Convenio" />
            <asp:BoundField DataField="splan" HeaderText="Plan" />
            <asp:BoundField DataField="srate" HeaderText="Tarifa" />            
            <asp:BoundField DataField="ddate" HeaderText="Fecha" />
            <asp:BoundField DataField="dvalue" HeaderText="Valor" />
            <asp:TemplateField HeaderText="Factura">
                <ItemTemplate>
                    <asp:TextBox ID="txtInvoce" runat="server" Text='<%# Eval("iinvoice") %>'></asp:TextBox>
                    <asp:FilteredTextBoxExtender ID="fteInvioce" runat="server" TargetControlID="txtInvoce" FilterType="Numbers"></asp:FilteredTextBoxExtender>
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
    <div>
        <asp:ImageButton ImageUrl="~/images/diskette.png" ToolTip="Guardar" runat="server" ID="imbGuardar" OnClick="imbGuardar_Click" Width="30" Height="30" />
    </div>
</asp:Content>
