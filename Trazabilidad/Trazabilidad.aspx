<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="Trazabilidad.aspx.cs" Inherits="Trazabilidad.Trazabilidad" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Reporte de trazabilidad del cargo</h1>
    <table cellpading="2" cellspacing="2">
        <tr>
            <td>Digite el n&uacute;mero del ingreso:</td>
            <td>
                <asp:TextBox ID="txtIngreso" runat="server" MaxLength="15"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvIngreso" runat="server" ControlToValidate="txtIngreso" Display="None" ErrorMessage="El ingreso obligatorio" SetFocusOnError="true" ValidationGroup="frmIngreso"></asp:RequiredFieldValidator>
                <asp:ValidatorCalloutExtender ID="vceIngreso" runat="server" TargetControlID="rfvIngreso"></asp:ValidatorCalloutExtender>
            </td>
            <td width="5%">&nbsp;</td> 
            <td>
                Seleccione la compa&ntilde;&iacute;a:
            </td>
            <td>
                <asp:DropDownList ID="ddlEmpresa" runat="server"></asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvEmpresa" runat="server" ControlToValidate="ddlEmpresa" Display="None" ErrorMessage="Debe seleccionar una empresa" SetFocusOnError="true" ValidationGroup="frmIngreso"></asp:RequiredFieldValidator>
                <asp:ValidatorCalloutExtender ID="vceEmpresa" runat="server" TargetControlID="rfvEmpresa"></asp:ValidatorCalloutExtender>
            </td>
            <td width="5%">&nbsp;</td>
            <td>
                <asp:ImageButton ID="imbBuscar" runat="server" ToolTip="Buscar" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ValidationGroup="frmIngreso" OnClick="imbBuscar_Click" />
            </td>
        </tr>
    </table>      
    <asp:ListView ID="lvIngresos" runat="server" OnPagePropertiesChanging="lvIngresos_PagePropertiesChanging">
        <LayoutTemplate>
            <div id="itemPlaceholder" runat="server">
            </div>
            <br />
            <div align="center">
                <asp:DataPager runat="server" ID="DataPager1" PageSize="1" PagedControlID="lvIngresos">                    
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Image" ShowFirstPageButton="false" ShowPreviousPageButton="true" ShowNextPageButton="false"  PreviousPageImageUrl="~/images/arrow_left.png" ButtonCssClass="imgsize" />
                        <asp:NumericPagerField ButtonType="Link" />
                        <asp:NextPreviousPagerField ButtonType="Image" ShowNextPageButton="true" ShowLastPageButton="false" NextPageImageUrl="~/images/arrow_right.png" ShowPreviousPageButton="false" ButtonCssClass="imgsize" />
                    </Fields>
                </asp:DataPager>
            </div>
        </LayoutTemplate>
        <ItemTemplate>
            <table runat="server" width="100%">
                <tr>
                    <td style="vertical-align:top" width="25%">
                        <h1 style="font-size:13px">Datos Generales del Cargo</h1>
                        <table border="0" cellpading="1" cellspacing="1" width="100%">
                            <tr>                                
                                <th style="text-align:left">Usuario:</th>
                                <td><asp:Label ID="lblUsuario" runat="server" Text='<%# Eval("user") %>' /></td>
                                <th style="text-align:left">Servicio:</th>
                                <td><%# Eval("service") %></td>
                            </tr>
                            <tr>                                
                                <th style="text-align:left">Fecha:</th>
                                <td><%# String.Format("{0:d}", Eval("date")) %></td>
                                <th style="text-align:left">Valor:</th>
                                <td><%# String.Format("{0:C}", Eval("value")) %></td>
                            </tr>
                            <tr>                                
                                <th style="text-align:left">Excedente:</th>
                                <td><%# String.Format("{0:C}", Eval("adding")) %></td>
                                <th style="text-align:left">Abono:</th>
                                <td><%# String.Format("{0:C}", Eval("surplus")) %></td>
                            </tr>                            
                            <tr>                                
                                <th style="text-align:left">Factura:</th>
                                <td><%# Eval("invoice") %></td>
                                <th style="text-align:left">Valor Facturado:</th>
                                <td><%# String.Format("{0:C}", Eval("invoicevalue")) %></td>
                            </tr>
                            <tr>                                
                                <th style="text-align:left">Estado Cargo:</th>
                                <td><%# Utils.Tools.GetChargeStatus(Convert.ToInt32(Eval("canceled"))) %></td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                            </tr>
                        </table>
                    </td>                    
                    <td width="1%">&nbsp;</td>
                    <td width="64%" valign="top">
                        <h1 style="font-size:15px">Ubicaci&oacute;n y resultados</h1>
                        <table border="0" cellpading="1" cellspacing="1" width="50%">
                            <tr>
                                <th style="text-align:left">Ubicaci&oacute;n actual:</th>
                                <td><%# Utils.Tools.GetStatus(Convert.ToInt32(Eval("status")))  %></td>
                            </tr>
                            <tr>
                                <th style="text-align:left">En caja:</th>
                                <td><%# GetStatusDate(Eval("id"), 1)  %></td>
                            </tr>
                            <tr>
                                <th style="text-align:left">Central de cuentas:</th>
                                <td><%# GetStatusDate(Eval("id"), 4)  %></td>
                            </tr>
                            <tr>
                                <th style="text-align:left">Facturada:</th>
                                <td><%# GetStatusDate(Eval("id"), 5)  %></td>
                            </tr>
                            <tr>
                                <th style="text-align:left">M&aacute;s Informaci&oacute;n:</th>
                                <td>
                                    <asp:ImageButton ID="imbSoportes" runat="server" ImageUrl="~/images/list.png" ToolTip="Ver Soportes" Width="20" Height="20" OnClick="imbSoportes_Click" CommandArgument='<%# Eval("id") %>' />&nbsp;
                                    <asp:ImageButton ID="imbMotivos" runat="server" ImageUrl="~/images/reload.png" ToolTip="Motivos Devoluci&oacute;n" Width="20" Height="20" OnClick="imbMotivos_Click" CommandArgument='<%# Eval("id") %>' />
                                </td>
                            </tr>
                        </table>                        
                    </td>  
                    <td width="10%">&nbsp;</td>                                      
                </tr>
            </table>    
            <br />
            <h1 style="font-size:13px">Estados</h1>
            <asp:GridView ID="gvLog" runat="server" DataSource='<%# Eval("lLog") %>' BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" AutoGenerateColumns="false" Width="55%">
                <Columns>
                    <asp:BoundField DataField="status" HeaderText="Estado" />
                    <asp:BoundField DataField="date" HeaderText="Fecha" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="user" HeaderText="Usuario" ItemStyle-HorizontalAlign="Center" />
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
        </ItemTemplate>
    </asp:ListView>
    <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>
    <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display: none;">
        <asp:Panel ID="pnlMensaje" runat="server">
            <div style="text-align: right">
                <asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" />
            </div>            
            <br />
            <asp:GridView ID="gvSoportes" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%" Visible="false">
                <Columns>
                    <asp:BoundField DataField="name" HeaderText="Soporte" />
                    <asp:BoundField DataField="observation" HeaderText="Observaci&oacute;n" />                    
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
            <asp:GridView ID="gvMotivos" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%" Visible="false">
                <Columns>
                    <asp:BoundField DataField="name" HeaderText="Motivo" />
                    <asp:BoundField DataField="observation" HeaderText="Observaci&oacute;n" />                    
                    <asp:BoundField DataField="response" HeaderText="Respuesta" />                    
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
            <div align="center">
                <asp:Label ID="lblError" Text="No se encontraron registros" runat="server" Visible="false"></asp:Label> 
            </div>
        </asp:Panel>
    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">
    </asp:ModalPopupExtender>
</asp:Content>
