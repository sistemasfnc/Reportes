# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Propósito

Sistema enterprise de gestión hospitalaria para la **Fundación Neumológica Colombiana**. Administra el ciclo completo de facturación de servicios de salud: desde el registro de cargos por admisión hasta la generación de facturas y envío a EPS (Entidades Promotoras de Salud). Incluye módulos de nómina, costos, trazabilidad de estados y envío de archivos a más de 17 aseguradoras colombianas.

## Stack técnico

- **Framework:** .NET Framework 4.8 — C#
- **UI:** ASP.NET WebForms (ASPX + code-behind), sin MVC
- **Bases de datos:** Oracle (principal), SQL Server, FoxPro/OLEDB (legacy)
- **Servicios:** WCF (SMS), WinSCP/SFTP (envío de archivos a EPS)
- **Autenticación:** Forms Authentication, sesión 60 min, cultura `es-CO`

## Arquitectura

Ver [docs/arquitectura.md](docs/arquitectura.md)

## Mapa de carpetas

| Carpeta | Qué vive ahí |
|---|---|
| `Config/` | Lectura de configuración desde XML externo (`Configuration.cs`) |
| `Entity/` | Objetos de dominio planos, sin lógica ni ORM |
| `DAC/` | Acceso a datos vía ADO.NET puro (Oracle/SQL Server/FoxPro) |
| `Facade/` | Lógica de negocio, una clase por dominio, sobre DAC |
| `Trazabilidad/` | Sitio WebForms principal — cargos, devoluciones, envíos a EPS |
| `Reportes/` | Sitio WebForms de reportes |
| `Nomina/` | Sitio WebForms de nómina |
| `WServices/` | Servicio WCF de envío de SMS |
| `Utils/` | Utilidades estáticas compartidas (correo, logging, helpers) |
| `EventLog/` | Wrapper de logging a Windows Event Log |
| `SendCompensarPDF/`, `SendCompensarReport/`, `SendElyonFile/`, `SendMessage/`, `SendPublicity/`, `GeneraConsultas/`, `GenerateAppointmentFile/` | Aplicaciones de consola (batch), ver `docs/batch.md` |
| `packages/` | Paquetes NuGet restaurados (generado, no editar a mano) |
| `docs/` | Documentación detallada del proyecto (ver tabla abajo) |

**Contenido de `docs/`:**

| Archivo | Qué contiene |
|---|---|
| [docs/arquitectura.md](docs/arquitectura.md) | Capas del sistema, reglas de dependencia, patrones aplicados |
| [docs/dominio.md](docs/dominio.md) | Entidades, enumeraciones, estados y reglas de negocio |
| [docs/flujos.md](docs/flujos.md) | Flujos de negocio de punta a punta (cajero → facturador → EPS) |
| [docs/paginas.md](docs/paginas.md) | Todas las páginas ASPX, permisos requeridos y qué hacen (puntos de entrada de la app) |
| [docs/externas.md](docs/externas.md) | Integraciones con EPS, SMS, FTP, bases de datos externas |
| [docs/batch.md](docs/batch.md) | Aplicaciones de consola, cuándo se ejecutan y qué producen |
| [docs/decisiones.md](docs/decisiones.md) | Decisiones técnicas no obvias y por qué se tomaron |
| [docs/pruebas.md](docs/pruebas.md) | Estado de pruebas, convenciones y cobertura |
| [docs/historial.md](docs/historial.md) | Registro cronológico completo de requerimientos cerrados |

## Flujos principales

Ver [docs/flujos.md](docs/flujos.md)

## Entidades del dominio

Ver [docs/dominio.md](docs/dominio.md)

## Endpoints o puntos de entrada

No expone API REST. Los puntos de entrada son páginas ASPX — ver [docs/paginas.md](docs/paginas.md)

## Dependencias externas

Ver [docs/externas.md](docs/externas.md)

## Decisiones técnicas no obvias

Ver [docs/decisiones.md](docs/decisiones.md)

## Zonas de peligro

- **`Config\Configuration.cs`** — lee desde un path **hardcodeado** (`C:\www\Pedro_Romero\Proyectos_NET\BusDatos\Reportes\Config\bin\Debug\Config.dll.config`); si no existe ahí, toda la aplicación falla al arrancar. Cambiar al migrar de máquina. Ver `docs/arquitectura.md` para el esquema XML esperado.
- **`Trazabilidad\Web.config`** — contiene credenciales de impersonación (`fnc\vidar`)
- **`Config\bin\Debug\Config.dll.config`** — contiene todas las contraseñas de BD, FTP, SMS y AWS en texto plano
- **`OnProduction = False`** en config — verificar antes de desplegar a producción; cuando es `False` apunta a SIDs de prueba (`PRUTRAZA`, `PRUINTEG`, `PRUFNEUM`)
- **Dependencia externa FNCESB** — `DAC.csproj` referencia `..\..\FNCESB\FNCUtils\FNCUtils.csproj` y `Facade.csproj` referencia `..\..\FNCESB\FNCDAC\FNCDAC.csproj`. Deben existir en `C:\...\FNCESB\` o el build falla.
- **Tabla `cargo`** — los registros solo se crean desde la UI (modal de soportes en `Listado.aspx`); no existen al inicio aunque el ingreso aparezca en `VCargos`
- **Tabla `estadocargo`** — de solo lectura desde la app (no hay CRUD en la UI). Agregar un `ChargeStatus` nuevo al enum sin insertar antes la fila correspondiente (`es_id`) en esta tabla causa `ORA-02291` (FK `CARGOESTADO_FK`) al primer `UPDATE cargo`. Ver `docs/dominio.md` → "Visibilidad de cargos por bandeja" para la lista de estados y filtros vigentes.
- **Despliegue a producción** — el publish de Visual Studio (FileSystem, `PublishUrl=C:\Temp\cargos`) puede omitir DLLs sin cambios aparentes en su caché incremental (visto con `DAC.dll` quedando desactualizado). Verificar fecha de cada `.dll` copiado contra el build local antes de subir a producción, y copiar también los `.aspx` modificados (no solo el `bin`).
- **Repo copiado/movido a un equipo nuevo** — Windows puede marcar todos los archivos con el stream NTFS `Zone.Identifier` (Mark of the Web), lo que no impide compilar ni correr en IIS Express pero sí rompe el publish: .NET Framework lanza `FileLoadException` con HRESULT `0x80131515` sobre algún DLL del `bin` (visto con `AjaxControlToolkit.dll`). Si aparece ese error al publicar pero el proyecto compila y corre bien, desbloquear todo el árbol con `Get-ChildItem -Path <ruta-del-repo> -Recurse -File | Unblock-File`.

## Pruebas

Ver [docs/pruebas.md](docs/pruebas.md)

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

La validación funcional es manual vía IIS o IIS Express (ver `docs/pruebas.md` para el estado y la convención de pruebas unitarias adoptada).

## Requerimiento en curso

(vacío)

## Últimos cambios

(máximo 3 entradas, el historial completo está en docs/historial.md)

- 2026-07-24 — Motivo de devolución 11 ("Paciente en tratamiento") oculto de las ventanas de selección en Auditar.aspx y ListoFacturar.aspx
- 2026-06-25 — Estados 11/13/14 agregados al flujo de devoluciones; nuevo botón "En tratamiento auditado" en Central.aspx
- 2026-06-19 — Corrección path Config.dll.config para nuevo equipo

---

## INSTRUCCIONES DE MANTENIMIENTO — leer y respetar siempre

**1. Punto de control** (usar al cerrar una sesión intermedia de un requerimiento):
Cuando el usuario diga "punto de control", actualiza la sección "Requerimiento en curso" de este archivo con: qué se está implementando, qué ya está hecho (con archivos modificados), cuál es el siguiente paso exacto, contexto importante que no está en el código, y si se escribieron pruebas, cuáles y qué cubren.

**2. Retomar sesión** (usar al iniciar una sesión nueva):
Cuando el usuario diga "retomar", lee la sección "Requerimiento en curso" y resume en 3 líneas dónde estamos y cuál es el siguiente paso.

**3. Cierre de requerimiento** (usar al terminar un requerimiento completo):
Cuando el usuario diga "cerrar requerimiento", hacer en orden:
   a. Agregar una entrada completa a `docs/historial.md` con: fecha de hoy, título corto del requerimiento, qué se implementó, archivos modificados, decisiones tomadas que no son obvias, pruebas escritas (qué clases/módulos y qué escenarios cubren), y si quedó algo pendiente.
   b. Actualizar "Últimos cambios" en este archivo: agregar la entrada nueva resumida en 1 línea; si ya hay 3 entradas, eliminar la más antigua.
   c. Actualizar `docs/pruebas.md`: agregar las clases o módulos nuevos a "Qué se prueba en este proyecto", y agregar una entrada al "Historial de cobertura" con fecha, qué se cubrió y por qué.
   d. Si el requerimiento implicó una decisión técnica no obvia, agregar una entrada a `docs/decisiones.md`.
   e. Si el requerimiento afectó endpoints, dominio, flujos o integraciones, actualizar el archivo `docs/` correspondiente.
   f. Borrar el contenido de "Requerimiento en curso" y dejarlo como (vacío).

**4. Escribir pruebas** (comportamiento estándar siempre, adoptado 2026-07-08):
Cuando se implemente o modifique lógica de negocio, sin que el usuario tenga que pedirlo explícitamente:
   - Proponer qué pruebas unitarias corresponden a ese cambio.
   - Usar las herramientas de testing estándar del stack (ver `docs/pruebas.md` — a definir/crear, ya que hoy no existe proyecto de tests en la solución).
   - Usar estructura AAA (Arrange / Act / Assert) con comentarios.
   - Nombrar los tests describiendo: método o función, escenario y resultado esperado.
   - Mockear solo dependencias externas (Oracle, SFTP, SMS, correo), nunca módulos propios del proyecto.
   - Cubrir mínimo: caso feliz + caso de error más probable.
