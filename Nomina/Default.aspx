<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Nomina._Default" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1>Incapacidades</h1>                
            </hgroup>
            <p style="text-align:center">
              Estado de incapacidades  
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <script type="text/javascript">
        function ShowValueRow(obj)
        {
            document.getElementById("trValor").style.display = (obj.value == "4") ? "" : "none";
        }
    </script>
    <table width="100%">
        <tr>
            <td>Documento:</td>
            <td><asp:TextBox ID="txtDocument" runat="server" MaxLength="12" Height="16px" Width="130px"></asp:TextBox></td>
            <td>C&oacute;digo:</td>
            <td>
                <asp:DropDownList ID="ddlCode" runat="server">
                    <asp:ListItem Text=""></asp:ListItem>
                    <asp:ListItem Text="INCAPACIDAD ARL ENF. PROFESIO" Value="1110"></asp:ListItem>
                    <asp:ListItem Text="INCAP. ASUMIDA POR LA EMPRESA" Value="1111"></asp:ListItem>
                    <asp:ListItem Text="INCAP.POR ENFERMEDAD GRAL" Value="1112"></asp:ListItem>
                    <asp:ListItem Text="MATERNIDAD Y PATERNIDAD" Value="1113"></asp:ListItem>
                    <asp:ListItem Text="INCAPACIDAD ARP APRENDIZ" Value="1195"></asp:ListItem>
                    <asp:ListItem Text="INCAPACIDAD SALUD APRENDIZ" Value="1196"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>Eps:</td>
            <td>
                <asp:DropDownList ID="ddlEps" runat="server"></asp:DropDownList>
            </td>               
            <td>
                <asp:Button ID="btnBuscar" runat="server" Text="Buscar" OnClick="btnBuscar_Click" />
            </td>
        </tr>
        <tr>
            <td>
                Inicio:
            </td>
            <td>
                <asp:TextBox ID="txtInitialDate" runat="server" Height="30px" Width="97px" onfocus="javascript:this.blur();"></asp:TextBox>
                <asp:ImageButton ID="imbInitialDate" runat="server" ImageUrl="~/Images/Calendar_scheduleHS.png" Width="16" Height="16" />
                <cc1:CalendarExtender ID="ceInitialDate" runat="server" TargetControlID="txtInitialDate" Format="dd/MM/yyyy" PopupButtonID="imbInitialDate"></cc1:CalendarExtender>
            </td>
            <td>
                Fin:
            </td>
            <td nowrap="nowrap">
                <asp:TextBox ID="txtFinalDate" runat="server" Height="23px" Width="102px" onfocus="javascript:this.blur();"></asp:TextBox>
                <asp:ImageButton ID="imbFinalDate" runat="server" ImageUrl="~/Images/Calendar_scheduleHS.png" Width="16" Height="16" />
                <cc1:CalendarExtender ID="ceFinalDate" runat="server" TargetControlID="txtFinalDate" Format="dd/MM/yyyy" PopupButtonID="imbFinalDate"></cc1:CalendarExtender>
            </td>
            <td>Estado:</td>
            <td>
                <asp:DropDownList ID="ddlStatus" runat="server">
                    <asp:ListItem Text=""></asp:ListItem>
                    <asp:ListItem Text="Transcripci&oacute;n" Value="1"></asp:ListItem>
                    <asp:ListItem Text="Liquidada" Value="2"></asp:ListItem>
                    <asp:ListItem Text="Cuenta de cobro" Value="3"></asp:ListItem>
                    <asp:ListItem Text="Valor pagado" Value="4"></asp:ListItem>
                    <asp:ListItem Text="No reconocimiento" Value="5"></asp:ListItem>
                </asp:DropDownList>
            </td>  
            <td>                
                <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar" OnClick="btnLimpiar_Click" />
            </td>                   
        </tr>
    </table>
    <br />
    <asp:GridView ID="gvDatos" runat="server" BackColor="White" BorderColor="#336666" BorderStyle="Double" BorderWidth="3px" CellPadding="4" GridLines="Horizontal" Width="100%" 
        AutoGenerateColumns="false" OnRowCommand="gvDatos_RowCommand" OnPageIndexChanging="gvDatos_PageIndexChanging" AllowPaging="true" PageSize="25">
        <FooterStyle BackColor="White" ForeColor="#333333" />
        <HeaderStyle BackColor="#336666" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#336666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="White" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#339966" Font-Bold="True" ForeColor="White" />
        <SortedAscendingCellStyle BackColor="#F7F7F7" />
        <SortedAscendingHeaderStyle BackColor="#487575" />
        <SortedDescendingCellStyle BackColor="#E5E5E5" />
        <SortedDescendingHeaderStyle BackColor="#275353" />
        <Columns>
            <asp:BoundField DataField="document" HeaderText="Documento" />
            <asp:BoundField DataField="name" HeaderText="Nombre" />
            <asp:BoundField DataField="eps" HeaderText="Eps" />
            <asp:BoundField DataField="costcenter" HeaderText="C. Cost" />
            <asp:BoundField DataField="inccodename" HeaderText="C&oacute;digo" />
            <asp:BoundField DataField="incdate" HeaderText="Fe. Inc." HtmlEncode="false" DataFormatString="{0:d}" />
            <asp:BoundField DataField="incdays" HeaderText="D&iacute;as" />
            <asp:BoundField DataField="incvalue" HeaderText="Valor" HtmlEncode="false" DataFormatString="{0:C}" />
            <asp:TemplateField HeaderText="Diagn&oacute;stico">
                <ItemTemplate>
                    <asp:Label ID="lblDiagnostico" runat="server" Text='<%# GetDiagnosis(Eval("document"), Eval("inccode"), Eval("incdate"), Eval("incdays")) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Estado">
                <ItemTemplate>
                    <asp:Label ID="lblEstado" runat="server" Text='<%# GetStatus(Convert.ToInt32(Eval("status"))) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>            
            <asp:TemplateField HeaderText="Valor pagado">
                <ItemTemplate>
                    <asp:Label ID="lblValor" runat="server" Text='<%# GetValue(Eval("document"), Eval("inccode"), Eval("incdate"), Eval("incdays")) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>        
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="lbtEstado" runat="server" Text="Detalle" CommandName="Editar"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView><br />
    <asp:Button ID="btnExportar" runat="server" Text="Exportar a Excel" OnClick="btnExportar_Click" />
    <asp:LinkButton ID="lbtModal" runat="server"></asp:LinkButton>
    <asp:Panel ID="pnEstados" runat="server" CssClass="modalPopup" Style="position: absolute; display:none">
        <asp:Panel ID="pnlMensaje" runat="server">
            <div style="text-align: right">
                <asp:LinkButton ID="lbtCerrar" runat="server" Text="Cerrar [x]"></asp:LinkButton>
            </div>
            <asp:GridView ID="gvEstados" runat="server" AutoGenerateColumns="false" BackColor="White" BorderColor="#336666" BorderStyle="Double" BorderWidth="3px" CellPadding="4" GridLines="Horizontal" HorizontalAlign="Center" Width="80%">
                <FooterStyle BackColor="White" ForeColor="#333333" />
                <HeaderStyle BackColor="#336666" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#336666" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="White" ForeColor="#333333" />
                <SelectedRowStyle BackColor="#339966" Font-Bold="True" ForeColor="White" />
                <SortedAscendingCellStyle BackColor="#F7F7F7" />
                <SortedAscendingHeaderStyle BackColor="#487575" />
                <SortedDescendingCellStyle BackColor="#E5E5E5" />
                <SortedDescendingHeaderStyle BackColor="#275353" />
                <Columns>
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate>
                            <asp:Label ID="lblEstado" runat="server" Text='<%# GetStatus(Convert.ToInt32(Eval("status"))) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="statusdate" HeaderText="Fecha" HtmlEncode="false" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="diagnosis" HeaderText="Diagn&oacute;stico" />
                    <asp:BoundField DataField="observations" HeaderText="Observaciones" />                                        
                </Columns>
            </asp:GridView>
            <br />
            <table align="center">
                <tr>
                    <td>Estado:</td>
                    <td>
                        <asp:DropDownList ID="ddlEstado" runat="server" onchange="ShowValueRow(this);">                            
                            <asp:ListItem Text="Transcripci&oacute;n" Value="1"></asp:ListItem>
                            <asp:ListItem Text="Autorizaci&oacute;n" Value="6"></asp:ListItem>
                            <asp:ListItem Text="Liquidada" Value="2"></asp:ListItem>
                            <asp:ListItem Text="Cuenta de cobro" Value="3"></asp:ListItem>
                            <asp:ListItem Text="Valor pagado" Value="4"></asp:ListItem>
                            <asp:ListItem Text="No reconocimiento" Value="5"></asp:ListItem>  
                            <asp:ListItem Text="Solicitud orden de pago" Value="7"></asp:ListItem>                          
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Fecha:</td>
                    <td>
                        <asp:TextBox ID="txtFecha" runat="server" Width="150" onfocus="this.blur();"></asp:TextBox>
                        <asp:ImageButton ID="ibtFechaInicial" runat="server" ImageUrl="~/Images/Calendar_scheduleHS.png" Width="16" Height="16" Style="vertical-align: bottom" />
                        <cc1:CalendarExtender ID="ceFecha" runat="server" TargetControlID="txtFecha" Format="dd/MM/yyyy" PopupButtonID="ibtFechaInicial"></cc1:CalendarExtender>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:top">Diagn&oacute;stico:</td>
                    <td>
                        <asp:TextBox ID="txtDiagnostico" runat="server" Width="150" MaxLength="10"></asp:TextBox>                        
                    </td>
                </tr>
                <tr id="trValor" style="display:none">
                    <td>Valor:</td>
                    <td>
                        <asp:TextBox ID="txtValor" runat="server" MaxLength="7" Width="150"></asp:TextBox>
                        <cc1:FilteredTextBoxExtender ID="fteValor" runat="server" FilterType="Numbers" TargetControlID="txtValor"></cc1:FilteredTextBoxExtender>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:top">Observaciones:</td>
                    <td>
                        <asp:TextBox ID="txtObservacion" runat="server" Width="150" TextMode="MultiLine" Rows="4"></asp:TextBox>                        
                    </td>
                </tr>                
                <tr>
                    <td></td>
                    <td>
                        <asp:Button ID="btnEstado" runat="server" Text="Guardar" OnClick="btnEstado_Click" />
                        <asp:Button ID="btnCerrar" runat="server" Text="Cerrar" OnClick="btnCerrar_Click" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </asp:Panel>
    <cc1:ModalPopupExtender ID="mpEstados" runat="server" TargetControlID="lbtModal" BackgroundCssClass="modalBackground" PopupControlID="pnEstados" DropShadow="true" CancelControlID="lbtCerrar">
    </cc1:ModalPopupExtender>
</asp:Content>
