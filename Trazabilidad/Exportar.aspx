<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Exportar.aspx.cs" Inherits="Trazabilidad.Exportar" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:GridView ID="gvCargos" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3">
            <Columns>            
                <asp:BoundField DataField="idadmission" HeaderText="Ingreso" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="date" HeaderText="Fecha" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="company" HeaderText="Empresa" />
                <asp:BoundField DataField="user" HeaderText="Usuario" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="service" HeaderText="Servicio" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="plan" HeaderText="Plan" />
                <asp:BoundField DataField="documenttype" HeaderText="T. Docum." />
                <asp:BoundField DataField="patientdocument" HeaderText="Docum." />
                <asp:BoundField DataField="patientfullname" HeaderText="Paciente" />                    
                <asp:BoundField DataField="costcenter" HeaderText="Centro de costos" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="costname" HeaderText="Centro de costos" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="subcenter" HeaderText="Subcentro" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="subcentername" HeaderText="Subcentro" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="authorization" HeaderText="Autorizaci&oacute;n" />
                <asp:BoundField DataField="surplus" HeaderText="Abono" ItemStyle-HorizontalAlign="Right" />
                <asp:BoundField DataField="adding" HeaderText="Excedente" ItemStyle-HorizontalAlign="Right"  />
                <asp:BoundField DataField="surplus" HeaderText="Abono" ItemStyle-HorizontalAlign="Right" />
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
    </form>
</body>
</html>
