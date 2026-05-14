<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="Glosas.aspx.cs" Inherits="Trazabilidad.Glosas" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/javascript">
        function SelectRadio(iValue)
        {
            var rbt = document.getElementsByName("rbtConcepto");
            var flag = true;
            for (var i = 0; i < rbt.length && flag; i++)
            {
                if (parseInt(rbt[i].value) == parseInt(iValue))
                {
                    rbt[i].checked = "checked";
                    flag = false;
                }
            }
        }
    </script>
    <h1 style="font-size:15px">Listado de Glosas</h1>     
    <table style="width:100%">
        <tr>
            <td>Factura:</td>
            <td>
                <asp:TextBox ID="txtFactura" runat="server" MaxLength="10"></asp:TextBox>
            </td>
            <td>Empresa</td>
            <td>
                <asp:DropDownList ID="ddlEmpresa" runat="server"></asp:DropDownList>
            </td>
            <td>
                Fecha Tr&aacute;mite Inicial:
            </td>
            <td>
                <asp:TextBox ID="txtFechaInicio" runat="server" onblur="this.blur();"></asp:TextBox>
                <asp:ImageButton ID="imbFechaInicio" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                <asp:CalendarExtender ID="ceFechaInicio" runat="server" TargetControlID="txtFechaInicio" Format="dd/MM/yyyy" PopupButtonID="imbFechaInicio"></asp:CalendarExtender>
            </td>
            <td>
                Fecha Tr&aacute;mite Final:
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
            <td>Tipo</td>
            <td>
                <asp:DropDownList ID="ddlTipo" runat="server">
                    <asp:ListItem Text="" Value="0"></asp:ListItem>
                    <asp:ListItem Text="Devolución" Value="1"></asp:ListItem>
                    <asp:ListItem Text="Glosa" Value="2"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td colspan="6">&nbsp;</td>
            <td>
                <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
            </td>   
        </tr>
    </table>
    <br />
    <asp:UpdatePanel ID="upGlosas" runat="server">
        <ContentTemplate>
            <asp:GridView ID="gvGlosas" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AllowPaging="true" PageSize="50" OnPageIndexChanging="gvGlosas_PageIndexChanging" OnRowCommand="gvGlosas_RowCommand">
                <Columns>            
                    <asp:BoundField DataField="company" HeaderText="Empresa" ItemStyle-HorizontalAlign="Center" />                    
                    <asp:BoundField DataField="invoice" HeaderText="Factura" ItemStyle-HorizontalAlign="Center" />                    
                    <asp:BoundField DataField="invoicedate" HeaderText="Fecha Factura" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="source" HeaderText="Fuente" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="eps" HeaderText="EPS" />                                        
                    <asp:BoundField DataField="invoicevalue" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:C}"/>
                    <asp:BoundField DataField="status" HeaderText="Estado" ItemStyle-HorizontalAlign="Center" />
                    <asp:TemplateField HeaderText="Tipo">
                        <ItemTemplate>
                            <asp:Label ID="lblTipo" runat="server" Text='<%# GetVal(Eval("type")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <%--<asp:BoundField DataField="commentvalue" HeaderText="V. Glosa" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:C}"/>
                    <asp:BoundField DataField="acceptedvalue" HeaderText="V. Aceptado" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:C}"/>--%>
                    <asp:BoundField DataField="transactdate" HeaderText="F. recepci&oacute;n" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="responsedate" HeaderText="F. respuesta" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                    <%--<asp:BoundField DataField="observations" HeaderText="Observaciones" />
                    <asp:BoundField DataField="concepts" HeaderText="Conceptos Devoluci&oacute;n" />--%>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:ImageButton ID="imbRespuesta" CommandName="Respuesta" runat="server" ImageUrl="~/images/list.png" Width="18" Height="18" BorderWidth="0" ToolTip="Respuesta Glosa" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:ImageButton ID="imbExportar" CommandName="Descargar" runat="server" ImageUrl="~/images/pdf.jpg" Width="18" Height="18" BorderWidth="0" ToolTip="Descargar PDF" />
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
        </ContentTemplate>
    </asp:UpdatePanel>    
    <asp:UpdatePanel ID="upConceptos" runat="server">
        <ContentTemplate>
            <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>    
            <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                <asp:Panel ID="pnlMensaje" runat="server">
                    <div style="text-align:right"><asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> </div>
                    <div style="font-weight:bold">Conceptos de respuesta glosa</div><br />
                    <asp:GridView ID="gvConceptosGlosa" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%" OnRowCommand="gvConceptosGlosa_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="conceptcode" HeaderText="C&oacute;digo" />
                            <asp:BoundField DataField="conceptname" HeaderText="Concepto" />
                            <asp:BoundField DataField="conceptvalue" HeaderText="Vr. Glosa" HtmlEncode="false" DataFormatString="{0:C}" ItemStyle-Wrap="false" />                            
                            <%--<asp:BoundField DataField="conceptobservations" HeaderText="Observaciones" />                            --%>
                            <asp:TemplateField HeaderText="Rpta" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:ImageButton ID="btnResponder" ToolTip="Responder" CommandName="Responder" ImageUrl="~/images/response.png" Width="16" Height="16" runat="server" />
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
                    <asp:Panel ID="pnlRespuestas" runat="server" Visible="false">
                        <div style="font-weight:bold">Escoja la respuesta para el concepto seleccionado:<br /><asp:Label ID="lblConcepto" runat="server"></asp:Label></div><br />
                        <asp:GridView ID="gvConceptos" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%" AllowPaging="false" PageSize="20">
                            <Columns>
                                <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <input type="radio" name="rbtConcepto" <%# SelectGridRadio(Eval("idconcept")) %> value='<%# Eval("idconcept") %>' />                                    
                                        <%--<asp:CheckBox ID="chkConcepto" runat="server" />                            --%>
                                    </ItemTemplate>
                                </asp:TemplateField> 
                                <asp:BoundField HeaderText="C&oacute;digo" DataField="conceptcode" />
                                <asp:BoundField HeaderText="Respuesta" DataField="conceptname" />                                                
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
                        <p>
                            <asp:Label ID="lblError" runat="server" style="color:red;"></asp:Label>                  
                        </p>                                                  
                        <table>
                            <tr style="display:none">
                                <td>Valor de la glosa:</td>
                                <td>
                                    <asp:Label ID="lblValue" runat="server"></asp:Label>
                                </td>
                            </tr>                           
                            <tr>
                                <td>Ingrese el valor aceptado:*</td>
                                <td>
                                    <asp:TextBox ID="txtValor" runat="server" MaxLength="10" Text="0"></asp:TextBox>
                                    <asp:FilteredTextBoxExtender ID="fteValor" runat="server" TargetControlID="txtValor" FilterType="Numbers"></asp:FilteredTextBoxExtender>                                    
                                </td>
                            </tr>
                            <tr>
                                <td>Ingrese la fecha de respuesta:</td>
                                <td>
                                    <asp:TextBox ID="txtFechaRespuesta" runat="server" onfocus="this.blur();"></asp:TextBox>
                                    <asp:ImageButton ID="imbFechaRespuesta" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                                    <asp:CalendarExtender ID="ceFechaRespuesta" runat="server" TargetControlID="txtFechaRespuesta" Format="dd/MM/yyyy" PopupButtonID="imbFechaRespuesta"></asp:CalendarExtender>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align:top">Ingrese las observaciones o comentarios:</td>
                                <td>
                                    <asp:TextBox ID="txtObservaciones" runat="server" TextMode="MultiLine" Rows="6" Height="107px" Width="306px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align:top">Seleccione la categor&iacute;a del responsable:*</td>
                                <td>
                                    <asp:DropDownList ID="ddlCategoria" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoria_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align:top">Seleccione el servicio del responsable:*</td>
                                <td>
                                    <asp:DropDownList ID="ddlServicio" runat="server">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align:top">Ingrese el responsable:*</td>
                                <td>
                                    <asp:TextBox ID="txtResponsable" runat="server" MaxLength="80"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                        <div style="text-align:right">
                            <asp:ImageButton ID="imbGuardar" runat="server" ToolTip="Guardar" ImageUrl="~/images/diskette.png" Width="20" Height="20" OnClick="imbGuardar_Click" />&nbsp;
                            <asp:ImageButton ID="imbCancel" runat="server" ToolTip="Cerrar" ImageUrl="~/images/cancel.png" Width="20" Height="20" OnClick="imbCancel_Click" />
                        </div> 
                    </asp:Panel>                     
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">        
            </asp:ModalPopupExtender>
        </ContentTemplate>
    </asp:UpdatePanel>
    <br />
    <div>
        <asp:ImageButton ID="imbExportar" runat="server" ImageUrl="~/images/excel.png" ToolTip="Exportar a excel" OnClick="imbExportar_Click" Width="64" Height="64" />
    </div>
</asp:Content>
