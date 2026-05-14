<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="GeneraPlantillaProgramas.aspx.cs" Inherits="Trazabilidad.GeneraPlantillaProgramas" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size:15px">Genera plantilla programas</h1>
    <br />
    <table>
        <tr>
            <td>
                Fecha Inicial:
            </td>
            <td>
                <asp:TextBox ID="txtFechaInicio" runat="server"></asp:TextBox>
                <asp:ImageButton ID="imbFechaInicio" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                <asp:CalendarExtender ID="ceFechaInicio" runat="server" TargetControlID="txtFechaInicio" Format="dd/MM/yyyy" PopupButtonID="imbFechaInicio"></asp:CalendarExtender>
            </td>
            <td>
                Fecha Final:
            </td>
            <td>
                 <asp:TextBox ID="txtFechaFin" runat="server"></asp:TextBox>
                <asp:ImageButton ID="imbFechaFin" runat="server" ImageUrl="~/images/calendar.png" Width="16" Height="16" />
                <asp:CalendarExtender ID="ceFechaFin" runat="server" TargetControlID="txtFechaFin" Format="dd/MM/yyyy" PopupButtonID="imbFechaFin"></asp:CalendarExtender>
            </td>
            <td>
                Convenio
            </td>
            <td>
                <asp:DropDownList ID="ddlAgreement" runat="server">                    
                    
                </asp:DropDownList>
            </td>
            <td>
                Programa
            </td>
            <td>
                <asp:DropDownList ID="ddlPlan" runat="server">                    
                    <asp:ListItem Text="ASMAIRE" Value="ASMA"></asp:ListItem>
                    <asp:ListItem Text="AIREPOC" Value="AIREP"></asp:ListItem>
                    <asp:ListItem Text="VASCULAR PULMONAR" Value="HTP"></asp:ListItem>
                    <asp:ListItem Text="VENTILACION MECANICA" Value="VENTIL"></asp:ListItem>
                    <asp:ListItem Text="VMI" Value="VMI"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>
                Tiene valoraciones?
            </td>
            <td>
                <asp:DropDownList ID="ddlValoracion" runat="server">
                    <asp:ListItem Text="No" Selected="True">No</asp:ListItem>
                    <asp:ListItem Text="Si">Si</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>
                <asp:Button ID="btnGenerar" runat="server" Text="Generar" OnClick="btnGenerar_Click" />
            </td>
        </tr>
    </table>
    <br />
</asp:Content>
