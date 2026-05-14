<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="exportnomina.aspx.cs" Inherits="Nomina.exportnomina" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
   <asp:GridView ID="gvDatos" runat="server" AutoGenerateColumns="false">        
        <Columns>
            <asp:BoundField DataField="document" HeaderText="Documento" />
            <asp:BoundField DataField="name" HeaderText="Nombre" />
            <asp:BoundField DataField="eps" HeaderText="Eps" />
            <asp:BoundField DataField="costcenter" HeaderText="Centro de Costos" />
            <asp:BoundField DataField="inccode" HeaderText="C&oacute;digo" />
            <asp:BoundField DataField="date" HeaderText="Fecha" HtmlEncode="false" DataFormatString="{0:d}" />
            <asp:BoundField DataField="incdate" HeaderText="Inicio Incapacidad" HtmlEncode="false" DataFormatString="{0:d}" />
            <asp:BoundField DataField="incfdate" HeaderText="Fin Incapacidad" HtmlEncode="false" DataFormatString="{0:d}" />
            <asp:BoundField DataField="incdays" HeaderText="D&iacute;as" />
            <asp:BoundField DataField="incvalue" HeaderText="Valor" />
            <asp:BoundField DataField="diagnosis" HeaderText="Diagn&oacute;stico" />
            <asp:TemplateField HeaderText="Estado">
                <ItemTemplate>
                    <asp:Label ID="lblEstado" runat="server" Text='<%# GetStatus(Convert.ToInt32(Eval("status"))) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>          
            <asp:BoundField DataField="value" HeaderText="Valor Pagado" />
        </Columns>
    </asp:GridView>
    </form>
</body>
</html>
