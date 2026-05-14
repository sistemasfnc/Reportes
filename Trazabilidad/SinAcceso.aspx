<%@ Page Title="" Language="C#" MasterPageFile="~/Principal.master" AutoEventWireup="true" CodeBehind="SinAcceso.aspx.cs" Inherits="Trazabilidad.SinAcceso" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <p>
        <asp:Label ID="lblMensaje" runat="server" Text="Usted no tiene permisos necesarios para ingresar a esta secci&oacute;n. Si usted considera que debe tener permisos, favor comun&iacute;quese con el administrador del sistema"></asp:Label>
    </p>
    
</asp:Content>
