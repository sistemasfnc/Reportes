<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="GenerarPlano.aspx.cs" Inherits="Trazabilidad.GenerarPlano" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Generar plano de distribuci&oacute;n de costos</h1>
    <table style="width:100%">
        <tr>            
            <td>Seleccione el a&ntilde;o a generar:</td>
            <td>
                <asp:DropDownList ID="ddlAno" runat="server"></asp:DropDownList>
            </td>
            <td>Seleccione el mes a generar:</td>
            <td>
                <asp:DropDownList ID="ddlMes" runat="server"></asp:DropDownList>
            </td>         
            <td>
                <asp:ImageButton ID="btnBuscar" runat="server" ImageUrl="~/images/binoculars.png" Width="20" Height="20" ToolTip="Generar" OnClick="btnBuscar_Click"/>
            </td>                  
            <td>&nbsp;</td>     
        </tr>   
    </table>
</asp:Content>
