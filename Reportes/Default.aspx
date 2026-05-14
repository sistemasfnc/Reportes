<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Reportes.Default" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Fundaci&oacute; Neumol&oacute;gica Colombiana</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>        
    <link rel="stylesheet" href="~/css/style.css" type="text/css" media="all" runat="server" id="cssMain" />
    <script type="text/javascript" src="js/jquery-1.4.2.min.js"></script>
    <script type="text/javascript" src="js/jquery.jcarousel.js"></script>
    <script src="js/cufon-yui.js" type="text/javascript" charset="utf-8"></script>
    <script src="js/Chaparral_Pro.font.js" type="text/javascript" charset="utf-8"></script>
    <script type="text/javascript" src="js/jquery-func.js"></script>
    <link rel="shortcut icon" type="image/x-icon" href="images/favicon.ico" />
    <!--[if IE 6]><link rel="stylesheet" href="css/ie.css" type="text/css" media="all" /><![endif]--></head>
<body>
    <form id="form1" runat="server">
        <div id="header">
            <asp:ImageButton ID="imgLogo" runat="server" ImageUrl="~/images/logo tri.jpg" Width="200" Height="300" />                    
        </div>        
        <div></div>
        <div id="main-boxes">
            <div class="box box-last">
                <h3>Ingreso</h3> 
                <div class="box-content">
                    <table>
                        <tr>
                            <td>Usuario:</td>
                            <td><asp:TextBox ID="txtUsuario" runat="server" MaxLength="50" CssClass="blink"></asp:TextBox></td>                
                        </tr>
                        <tr>
                            <td>Contrase&ntilde;a:</td>
                            <td><asp:TextBox ID="txtPassword" runat="server" MaxLength="100" TextMode="Password" CssClass="blink"></asp:TextBox></td>                
                        </tr>
                        <tr><td colspan="2">&nbsp;</td></tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td><asp:Button ID="btnLogin" runat="server" Text="Ingresar" OnClick="btnLogin_Click" /> </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>                     
    </form>          
</body>
</html>
