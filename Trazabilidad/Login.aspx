<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Trazabilidad.Login" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Inicio de Sesi&oacute;n</title>
    <link href="css/newstyles.css" rel='stylesheet' type='text/css' />
    <script type="application/x-javascript"> addEventListener("load", function() { setTimeout(hideURLbar, 0); }, false); function hideURLbar(){ window.scrollTo(0,1); } </script>
    <!--webfonts-->
	<link href="http://fonts.googleapis.com/css?family=PT+Sans:400,700,400italic,700italic|Oswald:400,300,700" rel="stylesheet" type="text/css" />
	<link href="http://fonts.googleapis.com/css?family=Open+Sans:300italic,400italic,600italic,700italic,400,300,700,800" rel="stylesheet" type="text/css" />
    <!--//webfonts-->
</head>
<body>    
    <div class="login-10">
	    <div class="tenth-login">
		    <h4>Sistema de Trazabilidad de Cargos - Inicio de Sesi&oacute;n</h4>
		    <form id="frmIngreso" class="ten" runat="server">	
				<asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" CombineScripts="false"></asp:ToolkitScriptManager>
				 <asp:UpdateProgress ID="UpdateProgress1" runat="server">
					<ProgressTemplate>
                    
					</ProgressTemplate>
				</asp:UpdateProgress>        
                <asp:Label ID="lblError" runat="server" CssClass="error"></asp:Label>	
				<ul>
					<li class="cream">
                    <asp:TextBox ID="txtUsuario" runat="server" CssClass="text" MaxLength="20"></asp:TextBox><a href="#" class=" icon10 user10"></a>				    
					</li>
					<li class="cream">
						<asp:TextBox ID="txtPassword" TextMode="Password" CssClass="text" MaxLength="20" runat="server"></asp:TextBox><a href="#" class=" icon10 lock10"></a>				    
					</li>					
				</ul>			    				
			    <div class="submit-ten">
                    <asp:Button ID="btnIngreso" runat="server" Text="Iniciar Sesi&oacute;n" OnClick="btnIngreso_Click" />				    
			    </div>
				<asp:LinkButton ID="lbtValidar" runat="server"></asp:LinkButton>    
				<asp:Panel ID="pnValidar" runat="server" CssClass="modalPopup" Style="position: absolute; display:none;">
					<asp:Panel ID="pnlMensaje" runat="server">
						<div style="text-align:right"></div>
						<div style="font-weight:bold">Tipo de cajero</div>
						<div>
							Seleccione el tipo de cajero:
							<asp:DropDownList ID="ddlPerfil" runat="server">
								<asp:ListItem Text="Cajero" Value="1"></asp:ListItem>
								<asp:ListItem Text="Cajero RHB" Value="10"></asp:ListItem>
							</asp:DropDownList>							
						</div>					
						<div>
							<asp:Button ID="btnAceptar" runat="server" OnClick="btnAceptar_Click" Text="Aceptar" CssClass="submit" />
						</div>
						<br />
					</asp:Panel>
				</asp:Panel>
				<asp:ModalPopupExtender ID="mpeValidar" runat="server" PopupControlID="pnValidar" BackgroundCssClass="modalBackground" TargetControlID="lbtValidar" DropShadow="true" >        
				</asp:ModalPopupExtender>
		    </form>
	    </div>
    </div>   
</body>
</html>
