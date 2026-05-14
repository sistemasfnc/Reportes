<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="ListoFacturar.aspx.cs" Inherits="Trazabilidad.ListoFacturar" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Cargos listos para facturar</h1>
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
            <asp:GridView ID="gvCargos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" OnRowCommand="gvCargos_RowCommand">
                <Columns>                    
                    <asp:BoundField DataField="idadmission" HeaderText="Ingreso" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="date" HeaderText="Fecha" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />                    
                    <asp:BoundField DataField="documenttype" HeaderText="T. Docum." />
                    <asp:BoundField DataField="patientdocument" HeaderText="Docum." />
                    <asp:BoundField DataField="patientfullname" HeaderText="Paciente" />                         
                    <asp:BoundField DataField="authorization" HeaderText="Autorizaci&oacute;n" />
                    <asp:BoundField DataField="fcharge" HeaderText="Enviado" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="user" HeaderText="Cajero" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="eps" HeaderText="Empresa" />
                    <asp:BoundField DataField="service" HeaderText="Servicio" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="plan" HeaderText="Plan" />
                    <asp:BoundField DataField="adding" HeaderText="Excedente" ItemStyle-HorizontalAlign="Right"  />
                    <asp:BoundField DataField="surplus" HeaderText="Abono" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="value" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" />
                     <asp:TemplateField>
                        <ItemTemplate>
                            <asp:ImageButton ID="imbEditar" CommandName="Editar" runat="server" ImageUrl="~/images/reload.png" Width="18" Height="18" BorderWidth="0" ToolTip="Devolver" />
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
            <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>
            <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                <asp:Panel ID="pnlMensaje" runat="server">
                    <div style="text-align:right"><asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> </div>
                    <div style="font-weight:bold">Motivos de devoluci&oacute;n</div><br />
                    <asp:GridView ID="gvSoportes" runat="server" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AutoGenerateColumns="false" Width="100%">
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkMotivo" runat="server" OnCheckedChanged="chkMotivo_CheckedChanged" AutoPostBack="true" />                            
                                </ItemTemplate>
                            </asp:TemplateField> 
                            <asp:BoundField HeaderText="C&oacute;digo" DataField="id" />
                            <asp:BoundField HeaderText="Motivo" DataField="name" />                            
                            <asp:TemplateField HeaderText="Detalle">
                                <ItemTemplate>
                                    <asp:TextBox ID="txtObservacion" runat="server" TextMode="MultiLine" Enabled="false"></asp:TextBox>
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
                        <asp:ImageButton ID="imbGuardar" runat="server" ToolTip="Guardar" ImageUrl="~/images/diskette.png" Width="20" Height="20" OnClick="imbGuardar_Click" OnClientClick="return confirm('Esta seguro que desea devolver este cargo');" />
                    </div>                
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">
            </asp:ModalPopupExtender>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
