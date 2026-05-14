<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportarDevoluciones.aspx.cs" Inherits="Trazabilidad.ExportarDevoluciones" %>

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
                <asp:BoundField DataField="eps" HeaderText="EPS" />
                <asp:BoundField DataField="user" HeaderText="Usuario" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="centraluser" HeaderText="Auditor" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="date" HeaderText="Fecha" HtmlEncode="false" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />                        
                <asp:BoundField DataField="date" HeaderText="Mes" HtmlEncode="false" DataFormatString="{0:MM}" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="senddate" HeaderText="Fecha de env&iacute;o" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="centraldate" HeaderText="Fcha recepci&oacute;n en central" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="returndate" HeaderText="Fecha devoluci&oacute;n" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="recievedate" HeaderText="Fecha recepci&oacute;n devoluci&oacute;n" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="returnsenddate" HeaderText="Fecha respuesta devoluci&oacute;n" ItemStyle-HorizontalAlign="Center" />            
                <asp:BoundField DataField="readytoinvoicedate" HeaderText="Fecha listo facturar" ItemStyle-HorizontalAlign="Center" />            
                <asp:BoundField DataField="reasontext" HeaderText="Motivo devoluci&oacute;n" />
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
