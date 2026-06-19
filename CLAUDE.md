# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Propósito del sistema

Sistema enterprise de gestión hospitalaria para la **Fundación Neumológica Colombiana**. Administra el ciclo completo de facturación de servicios de salud: desde el registro de cargos por admisión hasta la generación de facturas y envío a EPS (Entidades Promotoras de Salud). Incluye módulos de nómina, costos, trazabilidad de estados y envío de archivos a más de 17 aseguradoras colombianas.

## Stack técnico

- **Framework:** .NET Framework 4.8 — C#
- **UI:** ASP.NET WebForms (ASPX + code-behind), sin MVC
- **Bases de datos:** Oracle (principal), SQL Server, FoxPro/OLEDB (legacy)
- **Servicios:** WCF (SMS), WinSCP/SFTP (envío de archivos a EPS)
- **Autenticación:** Forms Authentication, sesión 60 min, cultura `es-CO`

## Mapa de documentación

| Archivo | Qué buscar ahí |
|---|---|
| [docs/arquitectura.md](docs/arquitectura.md) | Capas del sistema, reglas de dependencia, patrones aplicados |
| [docs/dominio.md](docs/dominio.md) | Entidades, enumeraciones, estados y reglas de negocio |
| [docs/flujos.md](docs/flujos.md) | Flujos de negocio de punta a punta (cajero → facturador → EPS) |
| [docs/paginas.md](docs/paginas.md) | Todas las páginas ASPX, permisos requeridos y qué hacen |
| [docs/externas.md](docs/externas.md) | Integraciones con EPS, SMS, FTP, bases de datos externas |
| [docs/batch.md](docs/batch.md) | Aplicaciones de consola, cuándo se ejecutan y qué producen |

## Comandos del día a día

```powershell
# Restaurar paquetes NuGet (obligatorio antes del primer build)
nuget restore Reportes.sln

# Compilar toda la solución
msbuild Reportes.sln /p:Configuration=Debug

# Compilar un proyecto individual
msbuild DAC\DAC.csproj /p:Configuration=Debug
msbuild Trazabilidad\Trazabilidad.csproj /p:Configuration=Debug

# Build de release
msbuild Reportes.sln /p:Configuration=Release
```

No hay pruebas automatizadas. La validación es manual via IIS o IIS Express.

## Configuración crítica

`Config\Configuration.cs` lee desde un path **hardcodeado**:
```
C:\www\Pedro_Romero\Proyectos_NET\BusDatos\Reportes\Config\bin\Debug\Config.dll.config
```
Si el archivo no existe en esa ruta, toda la aplicación falla al arrancar. Ver `docs/arquitectura.md` para el esquema XML esperado.

## Dependencia externa: FNCESB

`DAC.csproj` referencia `..\..\FNCESB\FNCUtils\FNCUtils.csproj` y `Facade.csproj` referencia `..\..\FNCESB\FNCDAC\FNCDAC.csproj`. Deben existir en `C:\...\FNCESB\` o el build falla.

## Zonas de peligro

- **`Config\Configuration.cs`** — path hardcodeado; cambiar al migrar de máquina
- **`Trazabilidad\Web.config`** — contiene credenciales de impersonación (`fnc\vidar`)
- **`Config\bin\Debug\Config.dll.config`** — contiene todas las contraseñas de BD, FTP, SMS y AWS en texto plano
- **`OnProduction = False`** en config — verificar antes de desplegar a producción; cuando es `False` apunta a SIDs de prueba (`PRUTRAZA`, `PRUINTEG`, `PRUFNEUM`)
- **Tabla `cargo`** — los registros solo se crean desde la UI (modal de soportes en `Listado.aspx`); no existen al inicio aunque el ingreso aparezca en `VCargos`

## Historial de cambios

| Fecha | Cambio | Archivo(s) |
|---|---|---|
| 2026-06-19 | Corrección path Config.dll.config para nuevo equipo | `Config\Configuration.cs` |
| | | |
