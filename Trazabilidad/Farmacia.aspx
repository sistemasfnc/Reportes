<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="Farmacia.aspx.cs" Inherits="Trazabilidad.Farmacia" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Cargos de farmacia recibidos</h1>
    <table style="width: 100%">
        <tr>
            <td>
                Ingreso:
            </td>
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
            <td>Fecha Final:
            </td>
            <td>
                <asp:TextBox ID="txtFechaFin" runat="server"></asp:TextBox>
                <asp:ImageButton ID="imbFechaFin" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                <asp:CalendarExtender ID="ceFechaFin" runat="server" TargetControlID="txtFechaFin" Format="dd/MM/yyyy" PopupButtonID="imbFechaFin"></asp:CalendarExtender>
            </td>
            <td>
                Documento Paciente:
            </td>
            <td>
                <asp:TextBox ID="txtDocmento" runat="server" MaxLength="12"></asp:TextBox>
            </td>        
            <td>
                Paciente:
            </td>
            <td>
                <asp:TextBox ID="txtPaciente" runat="server" MaxLength="100"></asp:TextBox>
            </td>        
            <td>
                <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Filrar" OnClick="btnBuscar_Click" />
            </td>
        </tr>
        <tr>
            <td>Estado:</td>
            <td>
                <asp:DropDownList ID="ddlEstado" runat="server">
                    <asp:ListItem Text="" Value=""></asp:ListItem>
                    <asp:ListItem Text="Auditado" Value="Auditado"></asp:ListItem>
                    <asp:ListItem Text="Devuelto" Value="Devuelto"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>Art&iacute;culo:</td>
            <td>
                <asp:TextBox ID="txtInsumo" runat="server" MaxLength="50"></asp:TextBox>
            </td>
            <td colspan="6">&nbsp;</td>
            <td>
                <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
            </td>
        </tr>        
    </table>
    <br />
    <asp:UpdatePanel ID="upDatos" runat="server">
        <ContentTemplate>
            <br />
            <asp:GridView ID="gvCargos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" OnRowCommand="gvCargos_RowCommand" OnPageIndexChanging="gvCargos_PageIndexChanging" PageSize="50" AllowPaging="true">
                <Columns>
                    <asp:BoundField DataField="idadmission" HeaderText="Ingreso" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="date" HeaderText="Fecha" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="patientdocument" HeaderText="Documento" />
                    <asp:BoundField DataField="patientfullname" HeaderText="Paciente" />                    
                    <asp:BoundField DataField="ssource" HeaderText="C&oacute;digo" ItemStyle-HorizontalAlign="Center" />   
                    <asp:BoundField DataField="service" HeaderText="Art&iacute;culo" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="iqty" HeaderText="Cantidad" ItemStyle-HorizontalAlign="Center" />             
                    <asp:BoundField DataField="costcenter" HeaderText="C&oacute;digo centro" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="costname" HeaderText="Centro de costo" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="subcenter" HeaderText="C&oacute;digo almacen" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="subcentername" HeaderText="Almacen" ItemStyle-HorizontalAlign="Center" />
                    <asp:TemplateField HeaderText="Estado" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlEstadoGrid" runat="server" OnDataBinding="ddlEstadoGrid_DataBinding">
                                <asp:ListItem Text="" Value=""></asp:ListItem>
                                <asp:ListItem Text="Auditado" Value="Auditado"></asp:ListItem>
                                <asp:ListItem Text="Devuelto" Value="Devuelto"></asp:ListItem>
                            </asp:DropDownList>                              
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actualizar" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="btnGuardar" runat="server" ImageUrl="~/images/diskette.png" Height="20" Width="20" Enabled='<%# EnableButton() %>' CommandName="Guardar" />
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
</asp:Content>
