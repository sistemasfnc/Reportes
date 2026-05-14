<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="Central.aspx.cs" Inherits="Trazabilidad.Central" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Recibir cargos enviados por cajas</h1>
    <table style="width:100%">
        <tr>
            <td>Ingreso:</td>
            <td>
                <asp:TextBox ID="txtIngreso" runat="server" MaxLength="10"></asp:TextBox>
                <asp:FilteredTextBoxExtender ID="fteIngreso" runat="server" TargetControlID="txtIngreso" FilterType="Numbers"></asp:FilteredTextBoxExtender>
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
            <td>Documento:</td>
            <td>
                <asp:TextBox ID="txtDocumento" runat="server" MaxLength="20"></asp:TextBox>
            </td>
            <td>Autorizaci&oacute;n:</td>
            <td>
                <asp:TextBox ID="txtAutorizacion" runat="server" MaxLength="20"></asp:TextBox>
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
                <asp:TextBox ID="txtEPS" runat="server" MaxLength="50"></asp:TextBox>
                <asp:DropDownList ID="ddlEmpresa" runat="server" Visible="false"></asp:DropDownList>
            </td>
            <td>Plan:</td>
            <td>
                <asp:ComboBox ID="ddlPlan" runat="server" AutoCompleteMode="SuggestAppend"></asp:ComboBox>
            </td>
              <td>
                Usuario:
            </td>
            <td>
                <asp:ListBox ID="ddlUsuario" runat="server" SelectionMode="Multiple" Height="100" Width="140" />
            </td>    
            <td colspan="2">&nbsp;</td>                      
            <td>
                <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
            </td>     
        </tr>     
    </table>
    <br />
    <asp:UpdatePanel ID="upDatos" runat="server">
        <ContentTemplate>
            <asp:GridView ID="gvCargos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" OnRowCommand="gvCargos_RowCommand" OnPageIndexChanging="gvCargos_PageIndexChanging" PageSize="100" AllowPaging="true">
                <Columns>
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                        <HeaderTemplate>
                            <asp:CheckBox ID="checkAll" runat="server" AutoPostBack="true" OnCheckedChanged="checkAll_CheckedChanged" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="chkEnviar" runat="server" />                            
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:TemplateField>
                     <asp:BoundField DataField="idadmission" HeaderText="Ingreso" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="date" HeaderText="Fecha" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />                    
                    <asp:BoundField DataField="documenttype" HeaderText="T. Id" />
                    <asp:BoundField DataField="patientdocument" HeaderText="Id" />
                    <asp:BoundField DataField="patientfullname" HeaderText="Paciente" />                         
                    <asp:BoundField DataField="authorization" HeaderText="Autorizaci&oacute;n" />
                    <asp:BoundField DataField="fcharge" HeaderText="Enviado" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="user" HeaderText="Cajero" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="eps" HeaderText="Empresa" />
                    <asp:BoundField DataField="service" HeaderText="Servicio" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="plan" HeaderText="Plan" />
                    <asp:BoundField DataField="adding" HeaderText="Excedente" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C}" HtmlEncode="false"  />
                    <asp:BoundField DataField="surplus" HeaderText="Abono" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C}" HtmlEncode="false" />
                    <asp:BoundField DataField="value" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C}" HtmlEncode="false" />
                     <asp:TemplateField>
                        <ItemTemplate>
                            <asp:ImageButton ID="imbEditar" CommandName="Editar" runat="server" ImageUrl="~/images/list.png" Width="18" Height="18" BorderWidth="0" ToolTip="Incompletos" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Motivo" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="imbMotivo" CommandName="Ver" runat="server" ImageUrl="~/images/preview.png" Width="18" Height="18" BorderWidth="0" ToolTip="Ver motivos" />
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
            <div>
                <asp:ImageButton ID="imbRecibir" runat="server" ImageUrl="~/images/file.png" ToolTip="Recibir" OnClick="imbRecibir_Click" OnClientClick="return confirm('Esta seguro que desea recibir estos cargos?');" Width="30" Height="30" />&nbsp;
                <asp:ImageButton ID="imbDevolver" runat="server" ImageUrl="~/images/reload.png" ToolTip="Devolver" OnClick="imbDevolver_Click" Visible="false" Width="30" Height="30" />
                &nbsp;<asp:ImageButton ID="imbTramite" runat="server" ToolTip="En tratamiento devuelto" ImageUrl="~/images/list.png"  Width="30" Height="30" OnClick="imbTramite_Click" />
                &nbsp;<asp:ImageButton ID="imbFacturar" runat="server" ImageUrl="~/images/ready.png" ToolTip="Listo para Facturar" OnClientClick="return confirm('Esta seguro que desea facturar estos cargos');"  OnClick="imbFacturar_Click" Width="30" Height="30" />
            </div>
            <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>
            <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display: none;">
                <asp:Panel ID="pnlMensaje" runat="server">
                    <div style="text-align: right">
                        <asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" />
                    </div>
                    <div style="font-weight: bold">Soportes pendientes</div>
                    <br />
                    <asp:GridView ID="gvSoportes" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%">
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
                        <asp:ImageButton ID="imbEnviar" runat="server" ToolTip="Guardar y recibir" ImageUrl="~/images/file.png" OnClick="imbEnviar_Click" OnClientClick="return confirm('Esta seguro que desea recibir el cargo?');" Width="20" Height="20" />
                        &nbsp;<asp:ImageButton ID="imbGuardar" runat="server" ToolTip="Guardar sin recibir" ImageUrl="~/images/diskette.png" OnClick="imbGuardar_Click" Width="20" Height="20" />                        
                    </div>
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">
            </asp:ModalPopupExtender>

            <asp:LinkButton ID="lbtMotivos" runat="server"></asp:LinkButton>    
            <asp:Panel ID="pnlMotivos" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                <asp:Panel ID="pnlDetalles" runat="server">
                    <div style="text-align:right"><asp:ImageButton ID="imbClose" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> </div>
                    <div style="font-weight:bold">Motivos de devoluci&oacute;n</div><br />
                    <asp:GridView ID="gvMotivos" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%">
                        <Columns>                            
                            <asp:BoundField HeaderText="C&oacute;digo" DataField="id" />
                            <asp:BoundField HeaderText="Motivo" DataField="name" />                            
                            <asp:BoundField HeaderText="Observaci&oacute;n" DataField="observation" />                            
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
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeMotivos" runat="server" PopupControlID="pnlMotivos" BackgroundCssClass="modalBackground" TargetControlID="lbtMotivos" DropShadow="true" OkControlID="imbClose">        
            </asp:ModalPopupExtender>  
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
