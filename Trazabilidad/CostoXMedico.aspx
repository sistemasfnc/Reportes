<%@ Page Title="Distribuir Costos" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="CostoXMedico.aspx.cs" Inherits="Trazabilidad.CostoXMedico" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
    #dvLoading
    {
        background: url(images/ajax-loader.gif) no-repeat center center;
        height: 100px;
        width: 100px;
        position: fixed;
        z-index: 1000;
        left: 50%;
        top: 50%;
        margin: -25px 0 0 -25px;
    }       
    .ui-widget-overlay
    {
        background-color:white;
        left: 0;
        opacity: 0.9;
        position: absolute;
        top: 0;
    }
    </style>   
    <script type="text/javascript">
        function ShowMessage(sMessage)
        {
            alert(sMessage);
        }        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Asignar Tiempos</h1>
    <div>
        <table>
            <tr>
                <td>
                    Seleccione el centro de costo:
                </td>
                <td>
                    <asp:DropDownList ID="ddlCentro" runat="server">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>
                    Seleccione el a&ntilde;o:
                </td>
                <td>
                    <asp:DropDownList ID="ddlAno" runat="server"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>
                    Seleccione el mes:
                </td>
                <td>
                    <asp:DropDownList ID="ddlMes" runat="server"></asp:DropDownList>
                </td>
            </tr>
             <tr>
                <td>&nbsp;</td>
                <td>
                    <asp:ImageButton ID="btnBuscar" runat="server" OnClick="btnBuscar_Click" ImageUrl="~/images/binoculars.png" Width="32" Height="32" ToolTip="Generar" />                    
                </td>
                </tr>
        </table>         
    </div>
    <asp:UpdatePanel ID="upDatos" runat="server">
        <ContentTemplate>
            <div>
                <asp:Panel ID="pnlTabla" runat="server" Visible="false">
                    <h2>Distribuci&oacute;n de centros de costo por empleado</h2>                    
                    <asp:PlaceHolder ID="plhTabla" runat="server" EnableViewState="true"></asp:PlaceHolder>            
                    <asp:Panel ID="pnlFormulario" runat="server">
                        <p>Agregue centros de costo adicionales:</p>                
                        <table style="width:30%;">
                            <tr>
                                <td>Centro de costos:</td>
                                <td>
                                    <asp:DropDownList ID="ddlCentroCosto" runat="server"></asp:DropDownList>
                                </td>                        
                                <td>
                                    <asp:ImageButton ID="btnAgregar" runat="server" ToolTip="Agregar" ImageUrl="~/images/add.png" Width="20" Height="20" OnClick="btnAgregar_Click" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                    <br />
                    <asp:ImageButton ID="imbGuardar" runat="server" ImageUrl="~/images/diskette.png" OnClick="imbGuardar_Click" ToolTip="Guardar" Width="32" Height="32" />           
                </asp:Panel>       
            </div>
            <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>                
                <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                    <asp:Panel ID="pnlMensaje" runat="server">
                        <div style="text-align:center">
                            <p>
                                A continuaci&oacute;n encontrar&aacute; el listado de inconsistencias en horas asignadas:
                            </p>
                            
                            <br />
                            <br />
                            <asp:GridView ID="gvResultado" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" >
                                <Columns>
                                    <asp:BoundField DataField="sname" HeaderText="Empleado" />     
                                    <asp:BoundField DataField="dextra2" HeaderText="Horas Ingresadas" />     
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
                            Desea almacenar la informaci&oacute;n?
                            <div style="text-align:center">
                                <asp:ImageButton ID="imbAceptar" runat="server" ImageUrl="~/images/diskette.png" ToolTip="Continuar" OnClick="imbAceptar_Click" OnClientClick="return confirm('Esta seguro que desea almacenar la información?');" Width="32" Height="32" />&nbsp;
                                <asp:ImageButton ID="imbCancelar" runat="server" ImageUrl="~/images/cancel.png" ToolTip="Cancelar" Width="32" Height="32" />
                            </div>
                        </div>
                    </asp:Panel>
                </asp:Panel>        
                <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" CancelControlID="imbCancelar">        
                </asp:ModalPopupExtender>  
            <div style="width:100%; height:1024px; top:0; left:0; background-color:white; z-index:1000; opacity: 0.9; display:none; position:absolute" id="loading">
                <div style="position:absolute; height: 100px; width: 100px; position: fixed; z-index:1000; left: 50%; top: 50%; margin: -25px 0 0 -25px;">
                    <asp:Image ID="imgLoader" runat="server" ImageUrl="~/images/ajax-loader.gif" />
                </div>
            </div>    
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
