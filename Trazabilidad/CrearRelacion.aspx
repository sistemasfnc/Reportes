<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="CrearRelacion.aspx.cs" Inherits="Trazabilidad.CrearRelacion" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Crear relaci&oacute;n de env&iacute;o</h1>  
    <div style="float:left; width: 500px;">
        <table>
            <tr>
                <td>
                    Ingrese el n&uacute;mero de radicado que desea tramitar:
                </td>
                <td>
                    <asp:TextBox ID="txtRadicado" runat="server" MaxLength="10" ValidationGroup="frmRadicado"></asp:TextBox>                    
                    <asp:RequiredFieldValidator ID="rfvRadicado" runat="server" ControlToValidate="txtRadicado" Display="None" ErrorMessage="El radicado es obligatorio" SetFocusOnError="true" ValidationGroup="frmRadicado"></asp:RequiredFieldValidator>
                    <asp:ValidatorCalloutExtender ID="vceRadicado" runat="server" TargetControlID="rfvRadicado"></asp:ValidatorCalloutExtender>
                </td>                
                <td>
                     <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" ValidationGroup="frmRadicado" OnClick="btnBuscar_Click" />
                </td>
            </tr>
        </table>
    </div>
    <div style="float:none; display:inline-block">
        <asp:Panel ID="pnlTitulo" runat="server" style="margin-left: 24px; font-weight:bold" Visible="false">
            Listado de facturas a enviar
        </asp:Panel>        
         <br />
         <asp:UpdatePanel ID="upFacturas" runat="server">
            <ContentTemplate>
                <asp:GridView ID="gvFacturas" runat="server" AutoGenerateColumns="False" Width="150%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" style="margin-left: 24px" >
                    <Columns>
                        <asp:BoundField DataField="invoice" HeaderText="Factura" ItemStyle-HorizontalAlign="Center" />                    
                        <asp:BoundField DataField="invoicedate" HeaderText="Fecha Factura" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:d}" />
                        <asp:BoundField DataField="user" HeaderText="Usuario" ItemStyle-HorizontalAlign="Center"  />
                        <asp:BoundField DataField="source" HeaderText="Fuente" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="eps" HeaderText="EPS" />                                        
                        <asp:BoundField DataField="value" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:C}"/>
                        <asp:BoundField DataField="status" HeaderText="Estado" ItemStyle-HorizontalAlign="Center" />                                                
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
            </ContentTemplate>
        </asp:UpdatePanel>
        <br />
        <div>
            <asp:ImageButton ID="imbEnviar" runat="server" ImageUrl="~/images/envelope.png" ToolTip="Enviar" OnClick="imbEnviar_Click" OnClientClick="return confirm('Esta seguro que desea crear la relación de envío? Esta operaciónes irreversible');" Visible="false" style="margin-left: 24px" Width="30" Height="30"  />
        </div>
    </div>
</asp:Content>
