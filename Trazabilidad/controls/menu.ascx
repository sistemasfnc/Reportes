<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="menu.ascx.cs" Inherits="Trazabilidad.controls.menu" %>
<div id="dvMenu" runat="server">
    <a href="#">Bienvenido <asp:Label ID="lblUsuario" runat="server"></asp:Label>!</a><a href="#"></a>    
    <a href="home.aspx">Inicio</a>
    <a href="#" data-flexmenu="menuusuarios">Usuarios <i class="arrow down"></i></a>
    <a href="#" data-flexmenu="menuprocesos ">Cargos <i class="arrow down"></i></a>    
    <a href="#" data-flexmenu="menuglosas">Glosas <i class="arrow down"></i></a>  
    <a href="#" data-flexmenu="menuingresos">Ingresos <i class="arrow down"></i></a>
    <a href="#" data-flexmenu="menureportes">Reportes Facturaci&oacute;n <i class="arrow down"></i></a>
    <a href="#" data-flexmenu="menucostos">Costos <i class="arrow down"></i></a>    
    <a href="#" data-flexmenu="menurelaciones">Relaciones de Env&iacute;o <i class="arrow down"></i></a> 
    <a href="#" data-flexmenu="menudesmaterializa">Desmaterializac&oacute;n<i class="arrow down"></i></a> 
    <a href="#" data-flexmenu="menuadministracion">Administraci&oacute;n<i class="arrow down"></i></a> 
    <a href="LogOut.aspx">Salir</a>
    <ul id="menuusuarios" class="flexdropdownmenu">                
        <li><a href="usuarios.aspx"><span>Listar</span></a></li>                
        <li><a href="editarusuario.aspx">Agregar</a></li>                                                
        <li><a href="#">Administrar Roles</a></li>                                                
    </ul>
    <ul id="menuprocesos" class="flexdropdownmenu">
        <li><a href="Listado.aspx"><span>Revisar Cargos</span></a></li>                
        <li><a href="Central.aspx"><span>Recibir Cargos</span></a></li>                        
        <li><a href="Auditar.aspx"><span>Registrar Cargos</span></a></li>                        
        <li><a href="RecibirDevolucion.aspx"><span>Recibir Devoluciones</span></a></li>  
        <li><a href="Devolucion.aspx"><span>Tramitar Devoluciones</span></a></li>  
        <li><a href="Trazabilidad.aspx"><span>Trazabilidad Cargos</span></a></li>
        <li><a href="ListoFacturar.aspx"><span>Retorno Listos para Facturar</span></a></li>
        <li><a href="Facturas.aspx"><span>Facturas</span></a></li>
        <li><a href="CargosFCI.aspx"><span>Cargos FCI</span></a></li>
        <li><a href="Farmacia.aspx"><span>Cargos Farmacia</span></a></li>
    </ul>    
    <ul id="menuglosas" class="flexdropdownmenu">
        <li><a href="RecepcionGlosa.aspx"><span>Recepcionar Glosas</span></a></li>                        
        <li><a href="Glosas.aspx"><span>Responder Glosas</span></a></li>
    </ul>    
    <ul id="menuingresos" class="flexdropdownmenu">
        <li><a href="CargoProgramas.aspx"><span>Cargar plantilla ingresos programas</span></a></li>                                
        <li><a href="CargoHospitalizacion.aspx"><span>Cargar plantilla ingresos hospitalizaci&oacute;n</span></a></li>                                
        <li><a href="CargoUrgencias.aspx"><span>Cargar plantilla ingresos urgencias</span></a></li>                                
        <li><a href="CargoFibrosis.aspx"><span>Cargar plantilla ingresos fibrosis</span></a></li>  
        <li><a href="CargosOtros.aspx"><span>Cargar otros servicios</span></a></li>  
        <li><a href="CargoParticular.aspx"><span>Cargar particulares</span></a></li>  
        <li><a href="CargarValoraciones.aspx"><span>Cargar plantilla ingreso inicial</span></a></li>  
        <li><a href="CruceCargosProgramas.aspx"><span>Cruce facturas cargos programas</span></a></li>
        <li><a href="GeneraPlantillaProgramas.aspx"><span>Generar plantilla programas</span></a></li>
    </ul>
    <ul id="menureportes" class="flexdropdownmenu">                
        <li><a href="CargosXFacturar.aspx"><span>Cargos sin facturar</span></a></li>        
        <li><a href="AbonosXFacturar.aspx"><span>Abonos sin facturar</span></a></li>        
        <li><a href="IngresosXLegalizar.aspx"><span>Ingresos por legalizar</span></a></li>        
        <li><a href="EstadoCargo.aspx"><span>Estados cargos</span></a></li>        
        <li><a href="CargosyAbonos.aspx"><span>Cargos y abonos facturados</span></a></li>        
        <li><a href="Devoluciones.aspx"><span>Devoluciones</span></a></li> 
        <li><a href="LogCargo.aspx"><span>Registro de Cargos</span></a></li> 
        <li><a href="IngresosAnulados.aspx"><span>Ingresos Anulados</span></a></li> 
        <li><a href="AbonosTarde.aspx"><span>Abonos posteriores al ingreso</span></a></li> 
        <li><a href="RipFamisanar.aspx"><span>RIPS PU Famisanar</span></a></li> 
    </ul>    
    <ul id="menucostos" class="flexdropdownmenu">                
        <li><a href="CostoXMedico.aspx"><span>Distribuir Costos Asistencial</span></a></li>                
        <li><a href="DistribuirCostos.aspx"><span>Distribuir Otros Costos</span></a></li>                
        <li><a href="ReporteCostos.aspx"><span>Reporte Costos</span></a></li>
        <li><a href="GenerarPlano.aspx"><span>Generar Plano</span></a></li>
        <li><a href="MaestroCostos.aspx"><span>Maestro de Costos</span></a></li>
    </ul>
    <ul id="menurelaciones" class="flexdropdownmenu">                        
        <li><a href="CrearRelacion.aspx"><span>Crear Relaci&oacute;n de Env&iacute;o</span></a></li>                
        <li><a href="TramitarRelacion.aspx"><span>Tramitar Relaciones de Env&iacute;o</span></a></li>                
        <li><a href="RecibirRelacion.aspx"><span>Recibir Relaciones de Env&iacute;o</span></a></li> 
        <li><a href="ReporteRelaciones.aspx"><span>Reporte Relaciones de Env&iacute;o</span></a></li> 
    </ul>    
    <ul id="menudesmaterializa" class="flexdropdownmenu">    
        <li><a href="DesmaterializaCompensar.aspx"><span>Por relaci&oacute;n de env&iacute;o</span></a></li>                
    </ul>
    <ul id="menuadministracion" class="flexdropdownmenu">                
        <li><a href="baul.aspx"><span>Ba&uacute;l</span></a></li>                        
    </ul>
</div>
<br />