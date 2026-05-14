<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AsignarCentros.aspx.cs" Inherits="Trazabilidad.AsignarCentros" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <link href="~/css/style.css" rel="stylesheet" type="text/css" runat="server" />
    <script type="text/javascript">
        function ShowMessage(sMessage)
        {
            alert(sMessage);
        }

        function ValidateAdd()
        {
            return confirm("Esta seguro que desea almacenar esta informacion? Recuerde que afectara todos los porcentajes asignados");
        }
    </script>
</head>
<body style="font-size:small">
    <form id="form1" runat="server">
        <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" CombineScripts="false"></asp:ToolkitScriptManager>
        <asp:Panel ID="pnlDatos" runat="server">
            <h1 style="font-size:15px">Asignaci&oacute;n de centros de costos para: <asp:Label ID="lblNombre" runat="server"></asp:Label></h1>
            <h2>Total horas contratadas: <asp:Label ID="lblHoras" runat="server"></asp:Label></h2>            
            <br />
            <table>
                <tr>
                    <td>Seleccione el a&ntilde;o a generar:</td>
                    <td>
                        <asp:DropDownList ID="ddlAno" runat="server"></asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Seleccione el mes a generar:</td>
                    <td>
                        <asp:DropDownList ID="ddlMes" runat="server"></asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>
                        <asp:ImageButton ID="btnBuscar" runat="server" OnClick="btnBuscar_Click" ImageUrl="~/images/edit.png" Width="20" Height="20" ToolTip="Generar" />
                    </td>
                </tr>
            </table>            
        </asp:Panel>
        <br />
        <asp:Panel ID="pnlProceso" runat="server" Visible="false">
            <asp:Panel ID="pnlInvestiga" runat="server" Visible="false">
                <h2>Horas asignadas por coordinador asistencial: <asp:Label ID="lblInvestiga" runat="server"></asp:Label></h2>
            </asp:Panel>
            <asp:Panel ID="pnlEducacion" runat="server" Visible="false">
                <h2>Horas asignadas por coordinador asistencial: <asp:Label ID="lblEduca" runat="server"></asp:Label></h2>
            </asp:Panel>
            <h2>Porcentaje asignado sobre el total: <asp:Label ID="lblAsignado" runat="server"></asp:Label></h2>
            <div>
                Seleccione el tipo de liquidaci&oacute;n que desea generar:
                <asp:RadioButton ID="rbtPorcentaje" Checked="true" GroupName="liquidacion" runat="server" Text="Porcentaje" OnCheckedChanged="rbtPorcentaje_CheckedChanged" AutoPostBack="true" />
                <asp:RadioButton ID="rbtHoras" GroupName="liquidacion" runat="server" Text="Horas" OnCheckedChanged="rbtPorcentaje_CheckedChanged" AutoPostBack="true" />
            </div>
            <br />
            <p>Asigne los valores a los centros de costo: </p>
            <asp:GridView ID="gvCostos" runat="server" AutoGenerateColumns="False" Width="50%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" OnRowCommand="gvCostos_RowCommand">
                <Columns>            
                    <asp:BoundField DataField="scode" HeaderText="Centro de Costos" />                    
                    <asp:TemplateField HeaderText="Valor" HeaderStyle-HorizontalAlign="Center">
                        <ItemTemplate>                        
                            <asp:TextBox ID="txtValor" runat="server" MaxLength="7" Text='<%# FormatValue(Eval("dvalue")) %>'></asp:TextBox>
                            <asp:FilteredTextBoxExtender ID="fteValor" runat="server" FilterMode="ValidChars" ValidChars="0123456789," TargetControlID="txtValor"></asp:FilteredTextBoxExtender>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate> 
                            <asp:ImageButton ID="btnEliminar" runat="server" CommandName="DeleteItem" ImageUrl="~/images/cancel.png" Width="20" Height="20" ToolTip="Eliminar" OnClientClick="return confirm('Esta seguro que desea eliminar este registro?');" Enabled='<%# EnableDeleteButton(Eval("iuser"), Eval("scode")) %>' />
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
            <asp:Panel ID="pnlAdicional" runat="server" Visible="false">
                <p>Agregue centro de costo y porcentaje adicional para rec&aacute;lculo de la distribuci&oacute;n:</p>
                <table style="width:50%;" border="1" cellspacing="1" cellpading="1">
                    <tr>
                        <td>Centro de costos:</td>
                        <td>
                            <asp:DropDownList ID="ddlCostoAdicional" runat="server" ValidationGroup="adicional"></asp:DropDownList>
                        </td>
                        <td>Valor:</td>
                        <td>
                            <asp:TextBox ID="txtValorAdicional" runat="server" MaxLength="7" ValidationGroup="adicional"></asp:TextBox>
                            <asp:FilteredTextBoxExtender ID="fteValorAdicional" runat="server" FilterMode="ValidChars" ValidChars="0123456789," TargetControlID="txtValorAdicional"></asp:FilteredTextBoxExtender>
                            <asp:RequiredFieldValidator ID="rfvValorAdicional" runat="server" ControlToValidate="txtValorAdicional" SetFocusOnError="true" Display="None" ErrorMessage="Debe ingresar un valor" ValidationGroup="adicional"></asp:RequiredFieldValidator>
                            <asp:ValidatorCalloutExtender ID="vceValorAdicional" runat="server" TargetControlID="rfvValorAdicional"></asp:ValidatorCalloutExtender>
                        </td>
                        <td>
                            <asp:ImageButton ID="imbAdicional" runat="server" ToolTip="Guardar valor adicional" ImageUrl="~/images/diskette.png" Width="20" Height="20" OnClick="imbAdicional_Click" OnClientClick="ValidateAdd();" ValidationGroup="adicional" />
                        </td>
                    </tr>
                </table>
                <br />
            </asp:Panel>
            <asp:Panel ID="pnlFormulario" runat="server">
                <p>Agregue centros de costo adicionales:</p>
                <table style="width:50%;" border="1" cellspacing="1" cellpading="1">
                    <tr>
                        <td>Centro de costos:</td>
                        <td>
                            <asp:DropDownList ID="ddlCentro" runat="server"></asp:DropDownList>
                        </td>
                        <td>Valor:</td>
                        <td>
                            <asp:TextBox ID="txtValor1" runat="server" MaxLength="7"></asp:TextBox>
                            <asp:FilteredTextBoxExtender ID="fteValor1" runat="server" FilterMode="ValidChars" ValidChars="0123456789," TargetControlID="txtValor1"></asp:FilteredTextBoxExtender>
                        </td>
                        <td>
                            <asp:ImageButton ID="btnAgregar" runat="server" ToolTip="Agregar" ImageUrl="~/images/add.png" Width="20" Height="20" OnClick="btnAgregar_Click" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <br />
            <div style="text-align:left">
                <asp:ImageButton ID="btnGuardar" runat="server" ToolTip="Salvar Parcialmente" ImageUrl="~/images/diskette.png" OnClick="btnGuardar_Click" Width="20" Height="20" />&nbsp;
                <asp:ImageButton ID="btnCerrar" runat="server" ToolTip="Salvar Definitivamente" ImageUrl="~/images/file.png" OnClick="btnCerrar_Click" OnClientClick="return confirm('Esta seguro que desea cerrar la distribucion?');" Width="20" Height="20"/>
                <asp:ImageButton ID="btnCerrarEspecial" runat="server" ToolTip="Salvar Definitivamente" ImageUrl="~/images/file.png" OnClick="btnCerrarEspecial_Click" OnClientClick="return confirm('Esta seguro que desea cerrar la distribucion?');" Width="20" Height="20"/>
            </div>
        </asp:Panel> 
        <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>                
        <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
            <asp:Panel ID="pnlMensaje" runat="server">
                <div style="text-align:center">
                    No se ha encontrado en el sistema informaci&oacute;n de distribuci&oacute;n de costos para el mes y a&ntilde;o seleccionados. Desea cargar la &uacute;ltima informaci&oacute;n disponible? <br /><br />
                    <asp:Button ID="btnSi" runat="server" Text="Si" OnClick="btnSi_Click" />&nbsp;<asp:Button ID="btnNo" runat="server" Text="No" OnClick="btnNo_Click"/>
                </div>
            </asp:Panel>
        </asp:Panel>        
        <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true">        
        </asp:ModalPopupExtender>        
    </form>
</body>
</html>
