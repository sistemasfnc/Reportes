<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="DesmaterializaCompensar.aspx.cs" Inherits="Trazabilidad.DesmaterializaCompensar" %>
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
        background-color: white;
        left: 0;
        opacity: 0.9;
        position: absolute;
        top: 0;
    }
    </style>
    <script type="text/javascript">
        function ShowLoader()
        {
            document.getElementById("loading").style.display = "";
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Desmaterializaci&oacute;n Facturas</h1>  
    <br />
     <!--<asp:UpdatePanel ID="upDatos" runat="server">
        <ContentTemplate>-->
            <asp:GridView ID="gvRelaciones" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" OnRowCommand="gvRelaciones_RowCommand" PageSize="50" AllowPaging="true" OnPageIndexChanging="gvRelaciones_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="snumero" HeaderText="N&uacute;mero" />
                    <asp:BoundField DataField="dtfecha" HeaderText="Fecha de env&iacute;o" HtmlEncode="false" DataFormatString="{0:yyyy/MM/dd}" ItemStyle-HorizontalAlign="Center" />                    
                    <asp:BoundField DataField="sestado" HeaderText="Estado" ItemStyle-HorizontalAlign="Center" />
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>                            
                            <asp:ImageButton ID="imbProcesar" runat="server" ImageUrl="~/images/compensar.png" Width="30" Height="30" ToolTip="Generar Compensar" CommandName="Generar" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "21") %>' />                                                        
                            <asp:ImageButton ID="imbSura" runat="server" ImageUrl="~/images/sura.png" Width="30" Height="30" ToolTip="Generar Sura" CommandName="GenerarSura" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "83") %>' />
                            <asp:ImageButton ID="imgSuramericana" runat="server" ImageUrl="~/images/suramericana.png" Width="30" Height="30" ToolTip="Generar Suramericana" CommandName="GenerarSura" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "53") %>' />
                            <asp:ImageButton ID="imbBolivar" runat="server" ImageUrl="~/images/bolivar.png" Width="30" Height="30" ToolTip="Generar Bolívar" CommandName="GenerarBolivar" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "19") %>' />
                            <asp:ImageButton ID="imgAxaColpatria" runat="server" ImageUrl="~/images/axacolpatria.png" Width="30" Height="30" ToolTip="Generar Axa Colpatria" CommandName="GenerarAxaColpatria" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "04") %>' />
                            <asp:ImageButton ID="imgAxaColpatria1" runat="server" ImageUrl="~/images/axacolpatria.png" Width="30" Height="30" ToolTip="Generar Axa Colpatria" CommandName="GenerarAxaColpatria" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "05") %>' />
                             <asp:ImageButton ID="imgAxaColpatria2" runat="server" ImageUrl="~/images/axacolpatria.png" Width="30" Height="30" ToolTip="Generar Axa Colpatria" CommandName="GenerarAxaColpatria" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "215") %>' />
                            <asp:ImageButton ID="imbSanitas" runat="server" ImageUrl="~/images/eps-android.jpg" Width="30" Height="30" ToolTip="Generar Sanitas" CommandName="GenerarSanitas" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "25") %>' />
                            <asp:ImageButton ID="imbCoomevaPrep" runat="server" ImageUrl="~/images/coomevaprep.jpg" Width="30" Height="30" ToolTip="Generar Coomeva Prepagada" CommandName="GenerarCoomevaPrepagada" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "23") %>' />
                            <asp:ImageButton ID="imbAllianz" runat="server" ImageUrl="~/images/allianz.png" Width="30" Height="30" ToolTip="Generar Allianz Seguros" CommandName="GenerarAxaColpatria" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "02") %>' />
                            <asp:ImageButton ID="imbCafam" runat="server" ImageUrl="~/images/cafam.png" Width="50" Height="30" ToolTip="Generar Cafam" CommandName="GenerarSanitas" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "08") %>' />
                            <asp:ImageButton ID="imbLiberty" runat="server" ImageUrl="~/images/liberty.png" Width="30" Height="30" ToolTip="Generar Liberty" CommandName="GenerarSanitas" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "36") %>' />
                            <asp:ImageButton ID="imbLiberty1" runat="server" ImageUrl="~/images/liberty.png" Width="30" Height="30" ToolTip="Generar Liberty" CommandName="GenerarSanitas" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "70") %>' />
                            <asp:ImageButton ID="imbMapfre" runat="server" ImageUrl="~/images/mapfre.jpg" Width="30" Height="30" ToolTip="Generar Mapfre" CommandName="GenerarSanitas" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "37") %>' />
                            <asp:ImageButton ID="imbPanAmerican" runat="server" ImageUrl="~/images/panamericanlife.png" Width="30" Height="30" ToolTip="Generar Pan American Life" CommandName="GenerarSanitas" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "43") %>' />
                            <asp:ImageButton ID="imbPositiva" runat="server" ImageUrl="~/images/positiva.png" Width="30" Height="30" ToolTip="Generar Positiva" CommandName="GenerarPositiva" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "44") %>' />
                            <asp:ImageButton ID="imbFamisanar" runat="server" ImageUrl="~/images/famisanar.png" Width="30" Height="30" ToolTip="Generar Famisanar" CommandName="GenerarFamisanar" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "26") %>' />    
                            <asp:ImageButton ID="imbSaludTotal" runat="server" ImageUrl="~/images/saludtotal.png" Width="30" Height="30" ToolTip="Generar Salud Total" CommandName="GenerarSaludTotal" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "50") %>' />     
                            <asp:ImageButton ID="imbEcopetrol" runat="server" ImageUrl="~/images/ecopetrol.png" Width="30" Height="30" ToolTip="Generar Ecopetrol" CommandName="GenerarAxaColpatria" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "114") %>' />    
                            <asp:ImageButton ID="imbColmenaARL" runat="server" ImageUrl="~/images/colmena.png" Width="30" Height="30" ToolTip="Generar Colmena ARL" CommandName="GenerarColmenaARL" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "17") %>' />
                            <asp:ImageButton ID="imgColsanitas" runat="server" ImageUrl="~/images/colsanitas.png" Width="30" Height="30" ToolTip="Generar Colsanitas" CommandName="GenerarColsanitas" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "18") %>' />
                            <asp:ImageButton ID="imbMedisanitas" runat="server" ImageUrl="~/images/colsanitas.png" Width="30" Height="30" ToolTip="Generar Medisanitas" CommandName="GenerarColsanitas" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "39") %>' /> 
                            <asp:ImageButton ID="imbPolicia" runat="server" ImageUrl="~/images/policia.png" Width="30" Height="30" ToolTip="Generar Policía" CommandName="GenerarPolicia" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "112") %>' /> 
                            <asp:ImageButton ID="imbAliansalud" runat="server" ImageUrl="~/images/aliansalud.png" Width="30" Height="30" ToolTip="Generar Aliansalud" CommandName="GenerarAliansalud" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "01") %>' /> 
                            <asp:ImageButton ID="imbColmedica" runat="server" ImageUrl="~/images/colmedica.png" Width="30" Height="30" ToolTip="Generar Colmedica" CommandName="GenerarColmedica" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "16") %>' /> 
                            <asp:ImageButton ID="imbArmada" runat="server" ImageUrl="~/images/armada.png" Width="30" Height="30" ToolTip="Generar Armada" CommandName="GenerarArmada" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "255") %>' /> 

                            <asp:ImageButton ID="imbNuevaEps" runat="server" ImageUrl="~/images/nuevaeps.jpg" Width="30" Height="30" ToolTip="Generar NuevaEPS" CommandName="GenerarNuevaEPS" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "64") %>' />
                            &nbsp;<asp:ImageButton ID="imbCargar" runat="server" ImageUrl="~/images/arrow-up.svg.png" Width="30" Height="30" ToolTip="Cargar a FTP" CommandName="Cargar" Visible='<%# (EnableView(Eval("snumero")) && ViewCompany(Eval("sempresa"), "21")) %>' />
                            <asp:ImageButton ID="imbMedPlus" runat="server" ImageUrl="~/images/medplus.png" Width="30" Height="30" ToolTip="Generar Medplus" CommandName="GenerarMedplus" OnClientClick="ShowLoader();" Visible='<%# ViewCompany(Eval("sempresa"), "40") %>' />
                            &nbsp;<asp:ImageButton ID="imbCargarSaludTotal" runat="server" ImageUrl="~/images/arrow-up.svg.png" Width="30" Height="30" ToolTip="Cargar a FTP" CommandName="CargarSaludTotal" Visible='<%# (EnableView(Eval("snumero")) && ViewCompany(Eval("sempresa"), "50")) %>' OnClientClick="ShowLoader();" />
                            &nbsp;<asp:ImageButton ID="imbCargarFamisanar" runat="server" ImageUrl="~/images/arrow-up.svg.png" Width="30" Height="30" ToolTip="Cargar a FTP" CommandName="CargarFamisanar" Visible='<%# (EnableView(Eval("snumero")) && ViewCompany(Eval("sempresa"), "26")) %>' OnClientClick="ShowLoader();" />
                            &nbsp;<asp:ImageButton ID="imbCargarNuevaEPS" runat="server" ImageUrl="~/images/arrow-up.svg.png" Width="30" Height="30" ToolTip="Cargar a SFTP" CommandName="CargarNuevaEPS" Visible='<%# (EnableView(Eval("snumero")) && ViewCompany(Eval("sempresa"), "64")) %>' OnClientClick="ShowLoader();" />
                            &nbsp;<asp:ImageButton ID="imbCargarMedPlus" runat="server" ImageUrl="~/images/arrow-up.svg.png" Width="30" Height="30" ToolTip="Cargar a SFTP" CommandName="CargarMedplus" Visible='<%# (EnableView(Eval("snumero")) && ViewCompany(Eval("sempresa"), "40")) %>' OnClientClick="ShowLoader();" />
                            <asp:ImageButton ID="imb2885" runat="server" ImageUrl="~/images/fncarbol.png" Width="30" Height="30" ToolTip="Desmaterializar 2885" CommandName="Generar2885" OnClientClick="ShowLoader();" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="imbVer" runat="server" ImageUrl="~/images/binoculars.png" Width="30" Height="30" ToolTip="Ver archivos" CommandName="VerArchivos" Visible='<%# EnableView(Eval("snumero")) %>' />
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
            <asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>    
            <asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
                <asp:Panel ID="pnlMensaje" runat="server">
                    <div style="text-align:right">
                        <asp:ImageButton ID="imbCerrar" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> 
                    </div>
                    <br />
                    <div>
                        Lista de archivos generados
                    </div>
                    <br />
                    <asp:GridView runat="server" ID="gvArchivos" AutoGenerateColumns="true" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1">                        
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
                </asp:Panel>
            </asp:Panel>
            <asp:LinkButton ID="lbtSoportes" runat="server"></asp:LinkButton>    
            <asp:Panel ID="pnSoportes" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">                
                <div style="text-align:right"><asp:ImageButton ID="imbCerrarModal" runat="server" ImageUrl="~/images/close.png" Height="20" Width="20" ToolTip="Cerrar" /> </div>
                <asp:Panel ID="pnlSoporte" runat="server">                    
                    <br />
                    <div style="text-align:center">
                        Desea generar los soportes para esta relaci&oacute;n de env&iacute;o?
                    </div>
                    <br />
                    <div style="text-align:center">
                        <asp:Button ID="btnSi" runat="server" Text="Si" OnClick="btnSi_Click" />&nbsp;
                        <asp:Button ID="btnNo" runat="server" Text="No" OnClick="btnNo_Click" />
                    </div>
                    <br />                     
                </asp:Panel>
                <asp:Panel ID="pnlTipo" runat="server">
                    <br />
                    <div style="text-align:center">
                        Por favor indique el tipo de facturas de la relaci&oacute;n de env&iacute;o
                    </div>
                    <br />
                    <div style="text-align:center">
                        <asp:DropDownList ID="ddlPBS" runat="server">
                            <asp:ListItem Value="PBS" Text="PBS"></asp:ListItem>
                            <asp:ListItem Value="NPBS" Text="No PBS"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <br />
                    <div style="text-align:center">
                        <asp:Button ID="btnAceptar" runat="server" Text="Generar" OnClick="btnAceptar_Click" />
                    </div>
                    <br />                     
                </asp:Panel>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" OkControlID="imbCerrar">        
            </asp:ModalPopupExtender>
             <asp:ModalPopupExtender ID="mpeSoportes" runat="server" PopupControlID="pnSoportes" BackgroundCssClass="modalBackground" TargetControlID="lbtSoportes" DropShadow="true" OkControlID="imbCerrarModal">        
            </asp:ModalPopupExtender>
            <div style="width:100%; height:1024px; top:0; left:0; background-color:white; z-index:1000; opacity: 0.9; display:none; position:absolute" id="loading">
                <div style="position:absolute; height: 100px; width: 100px; position: fixed; z-index:1000; left: 50%; top: 50%; margin: -25px 0 0 -25px;">
                    <asp:Image ID="imgLoader" runat="server" ImageUrl="~/images/ajax-loader.gif" />
                </div>
            </div>    
        <!--</ContentTemplate>
    </asp:UpdatePanel>-->
</asp:Content>
