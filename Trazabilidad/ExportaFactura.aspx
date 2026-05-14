<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportaFactura.aspx.cs" Inherits="Trazabilidad.ExportaFactura" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:GridView ID="gvFacturas" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3">
            <Columns>                    
                <asp:BoundField DataField="invoice" HeaderText="Factura" ItemStyle-HorizontalAlign="Center" />                    
                <asp:BoundField DataField="invoicedate" HeaderText="Fecha Factura" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:d}" />
                <asp:BoundField DataField="user" HeaderText="Usuario" ItemStyle-HorizontalAlign="Center"  />
                <asp:BoundField DataField="source" HeaderText="Fuente" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="eps" HeaderText="EPS" />                                        
                <asp:BoundField DataField="value" HeaderText="Valor" ItemStyle-HorizontalAlign="Right" HtmlEncode="false" DataFormatString="{0:C}"/>                
                <asp:BoundField DataField="status" HeaderText="Estado" ItemStyle-HorizontalAlign="Center" />                
                <asp:BoundField DataField="dbstatus" HeaderText="Estado Pendientes" ItemStyle-HorizontalAlign="Center" />                
                <asp:BoundField DataField="observations" HeaderText="Observaciones" ItemStyle-HorizontalAlign="Left" />
                <asp:BoundField DataField="locateddate" HeaderText="Fecha Radicado" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" DataFormatString="{0:d}"/>                                
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
