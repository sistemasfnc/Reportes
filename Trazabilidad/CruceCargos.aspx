<%@ Page Title="Cruce de cargos" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="CruceCargos.aspx.cs" Inherits="Trazabilidad.CruceCargos" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Actualización estadística cargos programas</h1>  
    <br />
    <table>
        <tr>
            <td>Seleccione el a&ntilde;o</td>
            <td>
                <asp:DropDownList ID="ddlYear" runat="server">
                    <asp:ListItem Text="2020" Value="2020"></asp:ListItem>
                    <asp:ListItem Text="2021" Value="2021" Selected="True"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>
                Seleccione el mes
            </td>
            <td>
                <asp:DropDownList ID="ddlMonth" runat="server">
                    <asp:ListItem Text="Enero" Value="1"></asp:ListItem>
                    <asp:ListItem Text="Febrero" Value="2"></asp:ListItem>
                    <asp:ListItem Text="Marzo" Value="3"></asp:ListItem>
                    <asp:ListItem Text="Abril" Value="4"></asp:ListItem>
                    <asp:ListItem Text="Mayo" Value="5"></asp:ListItem>
                    <asp:ListItem Text="Junio" Value="6"></asp:ListItem>
                    <asp:ListItem Text="Julio" Value="7"></asp:ListItem>
                    <asp:ListItem Text="Agosto" Value="8"></asp:ListItem>
                    <asp:ListItem Text="Septiempre" Value="9"></asp:ListItem>
                    <asp:ListItem Text="Octubre" Value="10"></asp:ListItem>
                    <asp:ListItem Text="Noviembre" Value="11"></asp:ListItem>
                    <asp:ListItem Text="Diciembre" Value="12"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>
                <asp:ImageButton ID="imbProcesar" runat="server" ImageUrl="~/images/process.png" Height="30" Width="30" OnClientClick="return confirm('Esta seguro que desea procesar? esta operación es irreversible');" OnClick="imbProcesar_Click"/>
            </td>
        </tr>
    </table>
</asp:Content>
