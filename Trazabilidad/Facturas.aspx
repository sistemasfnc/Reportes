<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="Facturas.aspx.cs" Inherits="Trazabilidad.Facturas" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <h1 style="font-size:15px">Estado de facturas</h1>
    <table style="width:100%">
        <tr>
            <td>Factura:</td>
            <td>
                <asp:TextBox ID="txtFactura" runat="server" MaxLength="10"></asp:TextBox>
                <asp:FilteredTextBoxExtender ID="fteFactura" runat="server" TargetControlID="txtFactura" FilterType="Numbers"></asp:FilteredTextBoxExtender>
            </td>
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
                Usuario:
            </td>
            <td>
                <asp:DropDownList ID="ddlUsuario" runat="server"></asp:DropDownList>
            </td>
            <td>
                <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" OnClick="btnBuscar_Click" />
            </td>
        </tr>
        <tr>
            <td>EPS:</td>
            <td>
                <asp:TextBox ID="txtEps" runat="server" MaxLength="50"></asp:TextBox>
            </td>
            <td>Estado:</td>
            <td>
                <asp:DropDownList ID="ddlEstado" runat="server">
                    <asp:ListItem Text="" Value=""></asp:ListItem>
                    <asp:ListItem Text="RD" Value="RD"></asp:ListItem>
                    <asp:ListItem Text="EV" Value="EV"></asp:ListItem>
                    <asp:ListItem Text="AP" Value="AP"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td colspan="4">&nbsp;</td>            
            <td>
                 <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
            </td>
        </tr>
    </table>
    <br />
    <asp:UpdatePanel ID="upDatos" runat="server">
        <ContentTemplate>
            <asp:GridView ID="gvFacturas" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" OnRowCommand="gvPendientes_RowCommand" AllowPaging="true" PageSize="50" OnPageIndexChanging="gvFacturas_PageIndexChanging">
                <Columns>                    
                    <asp:BoundField DataField="invoice" HeaderText="Factura" ItemStyle-HorizontalAlign="Center" />                    
                    <asp:BoundField DataField="invoicedate" HeaderText="Fecha Factura" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="user" HeaderText="Usuario" ItemStyle-HorizontalAlign="Center"  />
                    <asp:BoundField DataField="source" HeaderText="Fuente" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="eps" HeaderText="EPS" />                                        
                    <asp:BoundField DataField="value" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:C}"/>
                    <asp:BoundField DataField="status" HeaderText="Estado" ItemStyle-HorizontalAlign="Center" />
                    <%--<asp:BoundField DataField="dbstatus" HeaderText="Estado" ItemStyle-HorizontalAlign="Center" />--%>
                    <asp:BoundField DataField="observations" HeaderText="Observaciones" ItemStyle-HorizontalAlign="Left" />
                   <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>                                                       
                            <asp:ImageButton ID="imbPendiente" CommandName="Pendiente" runat="server" ImageUrl="~/images/document.png" Width="18" Height="18" BorderWidth="0" ToolTip="Pendiente" Visible='<%# !ViewField(Eval("status")) %>' />
                            <asp:Label ID="lblFechaRadicado" runat="server" Text='<%# Convert.ToDateTime(Eval("locateddate")).ToString("dd/MM/yyyy") %>' Visible='<%# ViewField(Eval("status")) %>'></asp:Label>                            
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
            <div>
                <asp:ImageButton ID="imbExportar" runat="server" ImageUrl="~/images/excel.png" ToolTip="Exportar a excel" Width="64" Height="64" OnClick="imbExportar_Click" />
            </div>
            <asp:LinkButton ID="lbtPendiente" runat="server"></asp:LinkButton>    
            <asp:ModalPopupExtender ID="mpePendiente" runat="server" PopupControlID="pnlPendiente" BackgroundCssClass="modalBackground" TargetControlID="lbtPendiente" DropShadow="true" OkControlID="imbCerrar">
            </asp:ModalPopupExtender>
            <asp:Panel ID="pnlPendiente" runat="server" CssClass="modalPopup" Style="position: absolute; display: none;">
                <asp:Panel ID="pnlDetalle" runat="server">
                    <div style="text-align: right">
                        <asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" />
                    </div>
                    <div style="font-weight: bold">Soportes pendientes</div>
                    <br />
                    <asp:GridView ID="gvPendientes" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%">
                        <Columns>
                            <asp:TemplateField HeaderText="Soporte">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSoporte" runat="server" OnCheckedChanged="chkSoporte_CheckedChanged" AutoPostBack="true" Checked='<%# CheckSupport(Eval("id")) %>' Text='<%# Eval("name") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Detalle">
                                <ItemTemplate>
                                    <asp:TextBox ID="txtObservacion" runat="server" TextMode="MultiLine" Visible='<%# ViewObservation(Eval("id")) %>' Text='<%# GetObservation(Eval("id")) %>'></asp:TextBox>
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
                    <br />
                    <div style="text-align: right">
                        <asp:ImageButton ID="imbSave" runat="server" ToolTip="Guardar" ImageUrl="~/images/diskette.png" Width="20" Height="20" OnClick="imbSave_Click" />
                        <asp:ImageButton ID="imbCancel" runat="server" ToolTip="Cerrar" ImageUrl="~/images/cancel.png" Width="20" Height="20" OnClick="imbCancel_Click" />
                    </div>
                </asp:Panel>
            </asp:Panel>            
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
