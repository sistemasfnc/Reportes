<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="RecepcionGlosa.aspx.cs" Inherits="Trazabilidad.RecepcionGlosa" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            text-align: left;
        }
        .auto-style2 {
            width: 310px;
        }
        .auto-style5 {
            width: 295px;
        }
        .auto-style12 {
            width: 324px;
        }
        .auto-style13 {
            width: 511px;
        }
        .auto-style14 {
            width: 302px;
        }
        .auto-style15 {
            width: 461px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Recepci&oacute;n de Glosas</h1>    
    <div style="float:left; width: 963px;">
        <table>
            <tr>
                <td>
                    Ingrese el n&uacute;mero de factura que desea tramitar:
                </td>
                <td>
                    <asp:TextBox ID="txtFactura" runat="server" MaxLength="10" ValidationGroup="frmFactura"></asp:TextBox>
                    <asp:FilteredTextBoxExtender ID="fteFactura" runat="server" FilterType="Numbers" TargetControlID="txtFactura"></asp:FilteredTextBoxExtender>
                    <asp:RequiredFieldValidator ID="rfvFactura" runat="server" ControlToValidate="txtFactura" Display="None" ErrorMessage="La factura es obligatoria" SetFocusOnError="true" ValidationGroup="frmFactura"></asp:RequiredFieldValidator>
                    <asp:ValidatorCalloutExtender ID="vceFactura" runat="server" TargetControlID="rfvFactura"></asp:ValidatorCalloutExtender>
                </td>
                <td>
                    Seleccione la compa&ntilde;&iacute;a:
                </td>
                <td>
                    <asp:DropDownList ID="ddlEmpresa" runat="server"></asp:DropDownList>
                    <asp:RequiredFieldValidator ID="rfvEmpresa" runat="server" ControlToValidate="ddlEmpresa" Display="None" ErrorMessage="Debe seleccionar una empresa" SetFocusOnError="true" ValidationGroup="frmFactura"></asp:RequiredFieldValidator>
                    <asp:ValidatorCalloutExtender ID="vceEmpresa" runat="server" TargetControlID="rfvEmpresa"></asp:ValidatorCalloutExtender>
                </td>
                <td>
                     <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" OnClick="btnBuscar_Click" ValidationGroup="frmFactura" />
                </td>
            </tr>
        </table>
        <asp:Panel ID="pnlFactura" runat="server" Visible="false" Width="594px">
            <asp:UpdatePanel ID="upTabla" runat="server">
                <ContentTemplate>
                    <table style="width: 719px">
                        <tr>
                            <th class="auto-style1">
                                Factura:
                            </th>
                            <td class="auto-style5">
                                <asp:Label ID="lblFactura" runat="server"></asp:Label>
                            </td>
                            <th class="auto-style1">
                                Fecha:
                            </th>
                            <td class="auto-style2">
                                <asp:Label ID="lblFecha" runat="server"></asp:Label>
                            </td>                
                        </tr>
                        <tr>                     
                            <th class="auto-style1">
                                Usuario:
                            </th>
                            <td class="auto-style5">
                                <asp:Label ID="lblUsuario" runat="server"></asp:Label>
                            </td>
                            <th class="auto-style1">
                                Fuente:
                            </th>
                            <td class="auto-style2">
                                <asp:Label ID="lblFuente" runat="server"></asp:Label>
                            </td>                
                        </tr>
                        <tr>
                            <th class="auto-style1">
                                Estado:
                            </th>            
                            <td class="auto-style5">
                                <asp:Label ID="lblEstado" runat="server"></asp:Label>
                            </td>           
                            <th class="auto-style1">
                                Valor:
                            </th>
                            <td class="auto-style2">
                                <asp:Label ID="lblValor" runat="server"></asp:Label>
                            </td> 
                        </tr>
                        <tr>
                            <th class="auto-style1">
                                EPS:
                            </th>
                            <td class="auto-style5">
                                <asp:Label ID="lblEps" runat="server"></asp:Label>
                            </td>                
                            <th class="auto-style1">
                                Observaciones:
                            </th>
                            <td class="auto-style2">
                                <asp:Label ID="lblObservaciones" runat="server"></asp:Label>
                            </td>                
                        </tr>
                    </table>
                    <br />
                    <table style="width: 633px">
                        <tr>
                            <td class="auto-style15">Seleccione el tipo de devoluci&oacute;n:</td>
                            <td class="auto-style14">
                                <asp:DropDownList ID="ddlTipo" runat="server">
                                    <asp:ListItem Text="Devolución" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="Glosa" Value="2"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td class="auto-style15">Ingrese la fecha de respuesta:</td>
                            <td class="auto-style14">
                                <asp:TextBox ID="txtFecha" runat="server" OnFocus="this.blur();"></asp:TextBox>
                                <asp:ImageButton ID="imbFecha" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                                <asp:CalendarExtender ID="ceFecha" runat="server" TargetControlID="txtFecha" Format="dd/MM/yyyy" PopupButtonID="imbFecha"></asp:CalendarExtender>                        
                            </td>
                        </tr>
                        <tr style="display:none">
                            <td class="auto-style15">Ingrese el valor para la glosa:</td>
                            <td class="auto-style14">
                                <asp:TextBox ID="txtValor" runat="server" MaxLength="10"></asp:TextBox>                                
                                <asp:FilteredTextBoxExtender ID="fteValor" runat="server" TargetControlID="txtValor" FilterType="Numbers"></asp:FilteredTextBoxExtender>
                            </td>
                        </tr>                        
                        <tr>
                            <td style="vertical-align:top" class="auto-style15">Ingrese observaciones o comentarios:</td>
                            <td class="auto-style14">
                                <asp:TextBox ID="txtObservaciones" runat="server" TextMode="MultiLine" Rows="6" Height="107px" Width="306px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="auto-style15">
                                Seleccione los conceptos de devoluci&oacute;n:
                            </td>
                            <td class="auto-style14">
                                <asp:ImageButton ID="imbBuscar" runat="server" ImageUrl="~/images/zoom.png" Width="20" Height="20" OnClick="imbBuscar_Click" ToolTip="Seleccionar Conceptos" />
                            </td>                
                        </tr>
                        <tr>
                            <td class="auto-style15">
                                <asp:ImageButton ID="imbSave" runat="server" OnClick="imbSave_Click" ImageUrl="~/images/diskette.png" ToolTip="Salvar glosa" OnClientClick="return confirm('Esta seguro que desea almacenar esta glosa?');" Height="30" Width="30" />
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>
    </div>
    <div style="float:none; display:inline-block">
        <asp:UpdatePanel ID="upConceptosGlosa" runat="server">
            <ContentTemplate>
                <asp:GridView ID="gvConceptosGlosa" runat="server" AutoGenerateColumns="False" Width="150%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" >
                    <Columns>
                        <asp:BoundField DataField="conceptcode" HeaderText="C&oacute;digo" />
                        <asp:BoundField DataField="conceptname" HeaderText="Concepto" />
                        <asp:BoundField DataField="conceptvalue" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:C}" />
                        <asp:BoundField DataField="conceptobservations" HeaderText="Observaciones" />
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
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>    
    <asp:UpdatePanel ID="upConceptos" runat="server">
        <ContentTemplate>
            <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>    
            <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                <asp:Panel ID="pnlMensaje" runat="server">
                    <div style="text-align:right"><asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> </div>
                    <div style="font-weight:bold">Conceptos de glosa</div><br />
                    <asp:GridView ID="gvConceptos" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvConceptos_PageIndexChanging">
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkConcepto" runat="server" OnCheckedChanged="chkConcepto_CheckedChanged" AutoPostBack="true" />                            
                                </ItemTemplate>
                            </asp:TemplateField> 
                            <asp:BoundField HeaderText="C&oacute;digo" DataField="conceptcode" />
                            <asp:BoundField HeaderText="Concepto" DataField="conceptname" /> 
                            <asp:TemplateField HeaderText="Valor" ItemStyle-VerticalAlign="Top">
                                <ItemTemplate>
                                    <asp:TextBox ID="txtValorConcepto" runat="server" MaxLength="10" Visible="false" Text='<%# GetValueText(Eval("idconcept"), "Valor") %>'></asp:TextBox>
                                    <asp:FilteredTextBoxExtender ID="fteValorConcepto" FilterType="Numbers" TargetControlID="txtValorConcepto" runat="server"></asp:FilteredTextBoxExtender>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Observaciones">
                                <ItemTemplate>
                                    <asp:TextBox ID="txtObservacionConcepto" runat="server" TextMode="MultiLine" Visible="false" Text='<%# GetValueText(Eval("idconcept"), "Observacion") %>'></asp:TextBox>
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
                    <div style="text-align:right">
                        <asp:ImageButton ID="imbGuardar" runat="server" ToolTip="Guardar" ImageUrl="~/images/diskette.png" Width="20" Height="20" OnClick="imbGuardar_Click" />&nbsp;
                        <asp:ImageButton ID="imbCancel" runat="server" ToolTip="Cerrar" ImageUrl="~/images/cancel.png" Width="20" Height="20" OnClick="imbCancel_Click" />
                    </div>  
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">        
            </asp:ModalPopupExtender>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
