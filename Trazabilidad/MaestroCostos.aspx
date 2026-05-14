<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="MaestroCostos.aspx.cs" Inherits="Trazabilidad.MaestroCostos" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function ValidateDelete()
        {            
            return confirm("Esta seguro que desea eliminar este registro?");
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Maestro de centros de costo</h1>
    <asp:UpdatePanel ID="upGrid" runat="server">
        <ContentTemplate>
            <table style="width: 100%">
                <tr>
                    <td>Nombre:</td>
                    <td>
                        <asp:TextBox ID="txtNombre" runat="server" MaxLength="50"></asp:TextBox>
                    </td>
                    <td>C&oacute;digo centro de costos:</td>
                    <td>
                        <asp:TextBox ID="txtCentro" runat="server" MaxLength="5"></asp:TextBox>                
                    </td>
                    <td>Tipo:</td>
                    <td>
                        <asp:DropDownList ID="ddlTipo" runat="server">
                            <asp:ListItem></asp:ListItem>
                            <asp:ListItem Text="Normal" Value="N"></asp:ListItem>
                            <asp:ListItem Text="Investigaci&oacute;n" Value="I"></asp:ListItem>
                            <asp:ListItem Text="Educaci&oacute;n" Value="E"></asp:ListItem>
                        </asp:DropDownList>
                    </td>            
                    <td>
                        <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Buscar" OnClick="btnBuscar_Click" />
                    </td>
                    <td>
                        <asp:ImageButton ID="btnCancelar" runat="server" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Limpiar" OnClick="btnCancelar_Click" />
                    </td>
                </tr>        
            </table>
            <br />
            <asp:GridView ID="gvCostos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" AllowPaging="true" PageSize="30" OnPageIndexChanging="gvCostos_PageIndexChanging" OnRowCommand="gvCostos_RowCommand">
                <Columns>            
                    <asp:BoundField DataField="scode" HeaderText="C&oacute;digo" ItemStyle-HorizontalAlign="Left" />
                    <asp:BoundField DataField="sname" HeaderText="Nombre" ItemStyle-HorizontalAlign="Left" />
                    <asp:BoundField DataField="ctype" HeaderText="Tipo" ItemStyle-HorizontalAlign="Center"/>         
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="imbEditar" CommandName="Editar" runat="server" ImageUrl="~/images/edit.png" Width="18" Height="18" BorderWidth="0" ToolTip="Editar" />
                        </ItemTemplate>
                    </asp:TemplateField>   
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="imbEliminar" CommandName="Eliminar" runat="server" ImageUrl="~/images/trash_can.png" Width="18" Height="18" BorderWidth="0" ToolTip="Eliminar" OnClientClick="ValidateDelete();" />
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
    <br />
    <div>
        <asp:ImageButton ID="imbAgregar" runat="server" ImageUrl="~/images/add.png" ToolTip="Agregar" OnClick="imbAgregar_Click" />
    </div>
     <asp:UpdatePanel ID="upConceptos" runat="server">
        <ContentTemplate>
            <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>    
            <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                <asp:Panel ID="pnlMensaje" runat="server">
                    <div style="text-align:right"><asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> </div>
                    <div style="font-weight:bold">Agregar / Editar Centro de Costo</div><br />
                    <table>
                        <tr>
                            <td>C&oacute;digo del centro:*</td>
                            <td>                                
                                <asp:TextBox ID="txtCodigo" runat="server" MaxLength="4" Width="300"></asp:TextBox> 
                                <asp:RequiredFieldValidator ID="rfvCodigo" runat="server" ControlToValidate="txtCodigo" ErrorMessage="El c&oacute;digo del centro no puede ser vacio" SetFocusOnError="true" ValidationGroup="Costos" Display="None"></asp:RequiredFieldValidator>
                                <asp:ValidatorCalloutExtender ID="vceCodigo" runat="server" TargetControlID="rfvCodigo"></asp:ValidatorCalloutExtender>
                            </td>                            
                        </tr>
                        <tr>
                            <td>Nombre del centro:*</td>
                            <td>
                                <asp:TextBox ID="txtNombre1" runat="server" MaxLength="100" Width="300"></asp:TextBox> 
                                <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre1" ErrorMessage="El nombre del centro no puede ser vacio" SetFocusOnError="true" ValidationGroup="Costos" Display="None"></asp:RequiredFieldValidator>
                                <asp:ValidatorCalloutExtender ID="vceNombre" runat="server" TargetControlID="rfvNombre"></asp:ValidatorCalloutExtender>
                            </td>
                        </tr>
                        <tr>
                            <td>Tipo:</td>
                            <td>
                                <asp:DropDownList ID="ddlTipo1" runat="server" Width="300">                                    
                                    <asp:ListItem Text="Normal" Value="N"></asp:ListItem>
                                    <asp:ListItem Text="Investigaci&oacute;n" Value="I"></asp:ListItem>
                                    <asp:ListItem Text="Educaci&oacute;n" Value="E"></asp:ListItem>
                                </asp:DropDownList>
                            </td>     
                        </tr>                        
                        <tr>
                            <td style="text-align:center">&nbsp;</td>
                            <td>
                                <br />
                                <asp:ImageButton ID="imbGuardar" ToolTip="Guardar" ImageUrl="~/images/diskette.png" Height="30" Width="30" runat="server" OnClick="imbGuardar_Click" ValidationGroup="Costos" />
                                <asp:ImageButton ID="imbCancelar" ToolTip="Cancelar" ImageUrl="~/images/cancel.png" Height="30" Width="30" runat="server" OnClick="imbCancelar_Click" />
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">        
            </asp:ModalPopupExtender>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
