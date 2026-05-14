<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="TramitarRelacion.aspx.cs" Inherits="Trazabilidad.TramitarRelacion" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Tramitar relaci&oacute;n de env&iacute;o</h1>      
    <table style="width:100%">
        <tr>
            <td>Relaci&oacute;n:</td>
            <td>
                <asp:TextBox ID="txtRelacion" runat="server" MaxLength="10"></asp:TextBox>
            </td>
            <td>Fecha inicial de env&iacute;o</td>
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
            <td>Estado</td>
            <td>
                <asp:DropDownList ID="ddlEstado" runat="server">
                    <asp:ListItem></asp:ListItem>                    
                    <asp:ListItem Text="Enviado" Value="E"></asp:ListItem>
                    <asp:ListItem Text="Recibido" Value="R"></asp:ListItem>
                    <asp:ListItem Text="Con pendientes" Value="P"></asp:ListItem>
                    <asp:ListItem Text="Tramitado Completo" Value="T"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>
                <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" OnClick="btnBuscar_Click" />
            </td> 
            <td>
                <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click"/>
            </td> 
        </tr>
    </table>
    <br />
    <asp:UpdatePanel ID="upRelacion" runat="server">
        <ContentTemplate>
            <asp:GridView ID="gvRelaciones" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" OnRowCommand="gvRelaciones_RowCommand" AllowPaging="true" PageSize="50" OnPageIndexChanging="gvRelaciones_PageIndexChanging" >
                <Columns>
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRelacion" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="snumero" HeaderText="N&uacute;mero" />
                    <asp:BoundField DataField="dtfecha" HeaderText="Fecha de env&iacute;o" HtmlEncode="false" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-HorizontalAlign="Center" />
                    <asp:TemplateField HeaderText="Estado" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:Image ID="imgEstado" runat="server" ImageUrl='<%# GetImage(Eval("cestado")) %>' ToolTip="Estado" Width="18" Height="18" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderText="Tramitar" HeaderStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="imbTramitar" runat="server" ToolTip="Tramitar" ImageUrl="~/images/list.png" Width="18" Height="18" CommandName="Tramitar" Enabled='<%# !EnableButton(Eval("cestado")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderText="Recibir" HeaderStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="imbRecibir" runat="server" ToolTip="Recibir" ImageUrl="~/images/checkmark.png" Width="18" Height="18" CommandName="Recibir" Enabled='<%# EnableButton(Eval("cestado")) %>' />
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
        </ContentTemplate>
    </asp:UpdatePanel>
     <asp:UpdatePanel ID="upFacturas" runat="server">
        <ContentTemplate>
            <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>    
            <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                <asp:Panel ID="pnlMensaje" runat="server" style="font-size:small">
                    <div style="text-align:right"><asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> </div>
                    <div style="font-weight:bold">Facturas a enviar</div><br />
                    <asp:GridView ID="gvFacturas" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" AutoGenerateColumns="false" Width="100%" AllowPaging="true" PageSize="20" OnPageIndexChanging="gvFacturas_PageIndexChanging" OnRowDataBound="gvFacturas_RowDataBound">
                        <Columns>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                                <HeaderTemplate>
                                    <asp:CheckBox ID="checkAll" runat="server" OnCheckedChanged="checkAll_CheckedChanged" AutoPostBack="true" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSeleccionar" runat="server" Visible='<%# EnableCheck(Eval("ienviado")) %>' />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>          
                            <asp:BoundField DataField="ifactura" HeaderText="Factura" HeaderStyle-HorizontalAlign="Center" />
                            <asp:TemplateField HeaderText="Asignado" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblAsignado" runat="server" Text='<%# GetText(Eval("iasignado")) %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Fecha asignaci&oacute;n" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblFechaAsignado" runat="server" Text='<%# GetTextDate(Eval("dtfechaasignado")) %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Enviado" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblEnviado" runat="server" Text='<%# GetText(Eval("ienviado")) %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Fecha de env&iacute;o" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblFechaEnviado" runat="server" Text='<%# GetTextDate(Eval("dtfechaenviado")) %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Recibido" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblRecibido" runat="server" Text='<%# GetText(Eval("irecibido")) %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Fecha de recepci&oacute;n" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblFechaRecibido" runat="server" Text='<%# GetTextDate(Eval("dtrecibido")) %>'></asp:Label>
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
                    <table style="font-size:small">
                        <tr>
                            <td style="vertical-align:top">
                                <table>
                                    <tr>
                                        <td style="vertical-align:top">Observaciones:</td>
                                        <td>
                                            <asp:TextBox ID="txtObservacion" runat="server" TextMode="MultiLine" Rows="3"></asp:TextBox>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td style="vertical-align:top">
                                 <asp:GridView ID="gvObservaciones" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" AutoGenerateColumns="false" Width="100%">
                                    <Columns>
                                        <asp:BoundField DataField="dtfecha" HeaderText="Fecha" HtmlEncode="false" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-HorizontalAlign="Center" />
                                        <asp:BoundField DataField="sobservacion" HeaderText="Observaci&oacute;n" />
                                        <asp:BoundField DataField="susuario" HeaderText="Usuario" ItemStyle-HorizontalAlign="Center" />
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
                            </td>
                        </tr>
                    </table>
                    <br />
                    <div>
                        <asp:ImageButton ID="imbEnviar" runat="server" ImageUrl="~/images/envelope.png" ToolTip="Enviar" Width="30" Height="30" OnClick="imbEnviar_Click" />
                    </div>
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">        
            </asp:ModalPopupExtender>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upVer" runat="server">
        <ContentTemplate>
            <asp:LinkButton ID="lbtVer" runat="server"></asp:LinkButton>    
            <asp:Panel ID="pnlVer" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                <asp:Panel ID="pnlFacturas" runat="server" style="font-size:small">
                    <div style="text-align:right"><asp:ImageButton ID="imbCerrarModal" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> </div>
                    <asp:GridView ID="gvVisorFacturas" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" AutoGenerateColumns="false" Width="100%" AllowPaging="true" PageSize="20" OnPageIndexChanging="gvVisorFacturas_PageIndexChanging">
                        <Columns>                            
                            <asp:BoundField DataField="ifactura" HeaderText="Factura" HeaderStyle-HorizontalAlign="Center" />
                            <asp:TemplateField HeaderText="Asignado" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblAsignado" runat="server" Text='<%# GetText(Eval("iasignado")) %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Fecha asignaci&oacute;n" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblFechaAsignado" runat="server" Text='<%# GetTextDate(Eval("dtfechaasignado")) %>'></asp:Label>
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
                    <div style="text-align:right">
                        <asp:ImageButton ID="imbRecibir" runat="server" ImageUrl="~/images/checkmark.png" ToolTip="Recibir" Width="30" Height="30" OnClick="imbRecibir_Click" OnClientClick="return confirm('Esta seguro que desa recibir esta relación de envío?');"  />
                    </div>
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeFacturas" runat="server" PopupControlID="pnlVer" BackgroundCssClass="modalBackground" TargetControlID="lbtVer" DropShadow="true" OkControlID="imbCerrarModal">        
            </asp:ModalPopupExtender>
        </ContentTemplate>
    </asp:UpdatePanel>
    <br />
    <div>
        <asp:ImageButton ID="imbGenerarRelacion" runat="server" ImageUrl="~/images/file.png" ToolTip="Generar Planilla" OnClick="imbGenerarRelacion_Click" Width="30" Height="30" />
    </div>
</asp:Content>
