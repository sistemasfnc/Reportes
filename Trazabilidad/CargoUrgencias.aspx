<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="CargoUrgencias.aspx.cs" Inherits="Trazabilidad.CargoUrgencias" %>
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
        function ConfirmCreate()
        {
            if (confirm('Operacion irreversible. Esta seguro que desea crear estos ingresos?'))
            {
                document.getElementById("loading").style.display = "";
                return true;
            }
            return false;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <h1 style="font-size:15px">Cargar plantilla urgencias</h1>
    <asp:Wizard ID="Wizard1" runat="server" DisplaySideBar="false" ActiveStepIndex="0" OnNextButtonClick="Wizard1_NextButtonClick" Width="70%">
        <StartNavigationTemplate>
            <div style="text-align: right">
                <br />
                <asp:Button Text="Siguiente" ID="StartNextButton" runat="server" CommandName="MoveNext" />
            </div>
        </StartNavigationTemplate>
        <StepNavigationTemplate>
            <br />
            <div style="text-align: right">
                <asp:Button Text="Anterior" ID="FinishPreviousButton" runat="server" CommandName="MovePrevious" CausesValidation="false" Style="display: none" />
                <asp:Button Text="Siguiente" ID="StepNextButton" runat="server" CommandName="MoveNext" OnClientClick="return ConfirmCreate();" />
            </div>
        </StepNavigationTemplate>
         <FinishNavigationTemplate>
            <br />
            <div class="formBtn" style="text-align: right">
                <asp:Button Text="ANTERIOR" ID="FinishPreviousButton" runat="server" CommandName="MovePrevious" CausesValidation="false" Style="display: none" />
                <asp:Button Text="Finalizar" ID="FinishButton" runat="server" OnClick="FinishButton_Click" />
            </div>
        </FinishNavigationTemplate>
        <WizardSteps>
            <asp:WizardStep ID="WizardStep1" runat="server">
                <div>
                    Cargar archivo de ingresos de urgencias</div>
                <br />
                <div style="text-align: justify">
                    <strong>PASO 1:</strong> Seleccione el archivo que desea cargar y haga clic en el bot&oacute;n "Siguiente".
                </div>
                <br />
                <div>
                    <asp:FileUpload ID="fuArchivo" runat="server" />
                    <asp:RequiredFieldValidator ID="rfvArchivo" runat="server" ControlToValidate="fuArchivo"
                        ErrorMessage="El archivo no puede ser vacio" Display="None"></asp:RequiredFieldValidator>
                    <asp:ValidatorCalloutExtender ID="vceArchivo" runat="server" TargetControlID="rfvArchivo">
                    </asp:ValidatorCalloutExtender>
                    <asp:RegularExpressionValidator ID="revArchivo" runat="server" ControlToValidate="fuArchivo"
                        ErrorMessage="El tipo de archivo es incorrecto" Display="None" ValidationExpression="(.*\.csv)"></asp:RegularExpressionValidator>
                    <asp:ValidatorCalloutExtender ID="vceArchivo1" runat="server" TargetControlID="revArchivo">
                    </asp:ValidatorCalloutExtender>
                </div>
                <br />
                <div>
                    <asp:Label ID="lblError" runat="server" ForeColor="Red"></asp:Label>
                </div>
            </asp:WizardStep>
             <asp:WizardStep ID="WizardStep2" runat="server">
                <div>
                    <strong>PASO 2:</strong> A continuaci&oacute;n encontrar&aacute; el detalle de los pacientes que desea cargar, haga clic en "Siguiente" para crear los ingresos. 
                    La operaci&oacute;n puede tardar unos minutos por favor espere el resultado.
                </div>
                <br />
                 <asp:GridView ID="gvIngresos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" AllowPaging="true" OnPageIndexChanging="gvIngresos_PageIndexChanging" PageSize="50">
                    <Columns>
                       <asp:BoundField DataField="sdocumenttype" HeaderText="Tipo de documento" />
                        <asp:BoundField DataField="sdocument" HeaderText="Documento" />                        
                        <asp:BoundField DataField="sfirstname" HeaderText="Nombre" />                        
                        <asp:BoundField DataField="ssurname" HeaderText="Apellido" />  
                        <asp:TemplateField HeaderText="Convenio">
                            <ItemTemplate>
                                <span><%# DataBinder.Eval(Container.DataItem, "lappointments[0].sagreementname") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>                                           
                        <asp:TemplateField HeaderText="Plan">
                            <ItemTemplate>
                                <span><%# DataBinder.Eval(Container.DataItem, "lappointments[0].splan") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>                                           
                        <asp:TemplateField HeaderText="Tarifa">
                            <ItemTemplate>
                                <span><%# DataBinder.Eval(Container.DataItem, "lappointments[0].srate") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>     
                        <asp:TemplateField HeaderText="Autorizaci&oacute;n">
                            <ItemTemplate>
                                <span><%# DataBinder.Eval(Container.DataItem, "lappointments[0].sauthorization") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>  
                        <asp:TemplateField HeaderText="Servicio">
                            <ItemTemplate>
                                <span><%# DataBinder.Eval(Container.DataItem, "lappointments[0].lservices[0].sservice") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>  
                        <asp:TemplateField HeaderText="Cantidad">
                            <ItemTemplate>
                                <span><%# DataBinder.Eval(Container.DataItem, "lappointments[0].lservices[0].iqty") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>  
                        <asp:TemplateField HeaderText="Valor">
                            <ItemTemplate>
                                <span><%# DataBinder.Eval(Container.DataItem, "lappointments[0].lservices[0].ivalue") %></span>
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
            </asp:WizardStep>
            <asp:WizardStep ID="WizardStep3" runat="server">
                <div>
                    <strong>FINAL:</strong> A continuaci&oacute;n encontrar&aacute; el resultado de la operaci&oacute;n:
                </div>
                <br />
                <asp:GridView ID="gvResultado" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="1" AllowPaging="true" PageSize="50" OnPageIndexChanging="gvResultado_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="sdocumenttype" HeaderText="Tipo de documento" />
                        <asp:BoundField DataField="sdocument" HeaderText="Documento" />
                        <asp:BoundField DataField="splan" HeaderText="Plan" />
                        <asp:BoundField DataField="srate" HeaderText="Tarifa" />
                        <asp:BoundField DataField="scostcenter" HeaderText="Centro de costos" />
                        <asp:BoundField DataField="sconcept" HeaderText="Concepto" />                        
                        <asp:BoundField DataField="sauthorization" HeaderText="Autorización" />
                        <asp:BoundField DataField="sservice" HeaderText="Servicio" />
                        <asp:BoundField DataField="iqty" HeaderText="Cantidad" />
                        <asp:BoundField DataField="dvalue" HeaderText="Valor" />
                        <asp:BoundField DataField="ientry" HeaderText="Ingreso" />
                        <asp:BoundField DataField="icharge" HeaderText="Cargo" />                        
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
                    <asp:ImageButton ID="imbExportar" runat="server" ImageUrl="~/images/excel.png" ToolTip="Exportar a excel" Width="30" Height="30" OnClick="imbExportar_Click" />
                </div>
            </asp:WizardStep>
        </WizardSteps>
    </asp:Wizard>
     <div style="width:100%; height:1024px; top:0; left:0; background-color:white; z-index:1000; opacity: 0.9; display:none; position:absolute" id="loading">
        <div style="position:absolute; height: 100px; width: 100px; position: fixed; z-index:1000; left: 50%; top: 50%; margin: -25px 0 0 -25px;">
            <asp:Image ID="imgLoader" runat="server" ImageUrl="~/images/ajax-loader.gif" />
        </div>
    </div>    
</asp:Content>
