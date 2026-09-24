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

## Documentación completa

Toda la documentación descriptiva de este repositorio vive en
`C:\www\Pedro_Romero\Centralización_Documentación\Proyectos\BusDatos\docs\Reportes\`,
junto con la de `FNCESB` (carpeta hermana `..\FNCESB\` dentro de ese mismo
`docs\`, ver "Proyectos compartidos con FNCESB" abajo). No crear `docs/` en este
repositorio — agregar contenido nuevo allá y, si hace falta, enlazarlo desde
este archivo.

- **Esa carpeta no es un repositorio git:** los cambios de documentación no
  quedan versionados ni viajan en los commits de este repositorio.
- **Índice del proyecto:** `Proyectos\BusDatos\BusDatos.md`, dentro de
  `Centralización_Documentación`. `Proyectos\Trazabilidad\Trazabilidad.md` son
  notas rápidas del sitio; la documentación formal sigue siendo `docs\Reportes\`.
- **Pendientes** (cosas por hacer, no documentación): van en
  `Centralización_Documentación\Pendientes.md`, sección del proyecto, no en
  este archivo ni en `docs\`.
- Las reglas generales de la carpeta están en `Centralización_Documentación\CLAUDE.md`.

| Archivo (en `BusDatos\docs\Reportes\`) | Qué contiene |
|---|---|
| `arquitectura.md` | Capas del sistema, reglas de dependencia, patrones aplicados |
| `dominio.md` | Entidades, enumeraciones, estados y reglas de negocio |
| `flujos.md` | Flujos de negocio de punta a punta (cajero → facturador → EPS) |
| `paginas.md` | Todas las páginas ASPX, permisos requeridos y qué hacen (puntos de entrada de la app) |
| `externas.md` | Integraciones con EPS, SMS, FTP, bases de datos externas |
| `batch.md` | Aplicaciones de consola, cuándo se ejecutan y qué producen |
| `decisiones.md` | Decisiones técnicas no obvias y por qué se tomaron |
| `pruebas.md` | Estado de pruebas, convenciones y cobertura |
| `historial.md` | Registro cronológico completo de requerimientos cerrados |

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
| `SendCompensarPDF/`, `SendCompensarReport/`, `SendElyonFile/`, `SendMessage/`, `SendPublicity/`, `GeneraConsultas/`, `GenerateAppointmentFile/` | Aplicaciones de consola (batch), ver `batch.md` centralizado |
| `packages/` | Paquetes NuGet restaurados (generado, no editar a mano) |

> `Reportes.sln` lista además `FNCDAC`, `FNCEntity`, `FNCFacade` y `FNCUtils`.
> **No son carpetas de este repositorio** — son proyectos enlazados desde
> `..\FNCESB\`. Ver "Proyectos compartidos con FNCESB" más abajo.

## Proyectos compartidos con FNCESB

Esta solución y `FNCESB` (carpeta hermana, `..\FNCESB\`) **comparten proyectos en
las dos direcciones** y se referencian con rutas relativas. No son repositorios
independientes: mover cualquiera de las dos carpetas rompe ambos builds.

| Proyecto | Dueño | Lo consume |
|---|---|---|
| `FNCDAC`, `FNCEntity`, `FNCFacade`, `FNCUtils` | **`FNCESB`** | `DAC`, `Facade` y `Trazabilidad` de este repo |
| `EventLog` | **este repo** | 19 proyectos de `FNCESB` |

**La documentación de `FNCDAC`, `FNCEntity`, `FNCFacade` y `FNCUtils` no vive
en ningún repositorio de código.** Está en
`Centralización_Documentación\Proyectos\BusDatos\docs\FNCESB\proyectos-compartidos.md`
(carpeta hermana de `Reportes\` dentro de ese `docs\`) — qué contiene cada
proyecto, el grafo de dependencias, quién los consume de los dos lados, las
reglas al modificarlos y el incidente de AWSSDK del 2026-07-29.

En sentido inverso, `EventLog` sí es de este repositorio: se documenta en
`BusDatos\docs\Reportes\` y `FNCESB` apunta a esa documentación.

## Endpoints o puntos de entrada

No expone API REST. Los puntos de entrada son páginas ASPX — ver `paginas.md` centralizado.

## Zonas de peligro

- **`Config\Configuration.cs`** — lee desde un path **hardcodeado** (en este equipo: `C:\www\Pedro_Romero\Proyectos\Proyectos_NET\Reportes\Config\bin\Debug\Config.dll.config`); si no existe ahí, toda la aplicación falla al arrancar. Cambiar al migrar de máquina. Ver `arquitectura.md` centralizado para el esquema XML esperado.
  - **NUNCA copiar/desplegar `Config.dll` compilado en un equipo de desarrollo hacia otro ambiente (staging/producción).** El path que queda embebido en el `.dll` es el que estaba activo en `Configuration.cs` al momento de compilar en ESE equipo — no es relativo ni se ajusta solo al servidor destino. Pasar un `Config.dll` de dev a producción (incluso "sin querer", como parte de copiar varios `.dll` juntos) rompe **toda** lectura de configuración en el ambiente destino — contraseñas, connection strings, todo — y se manifiesta como fallos que parecen no tener relación (ej. login rechaza contraseña correcta), no como un error de configuración obvio. Pasó realmente el 2026-07-29 (ver `historial.md` centralizado).
  - Si hay que reconstruir `Config.dll` para producción, cambiar temporalmente el path activo en `Configuration.cs` al de destino (confirmando antes dónde vive realmente el `Config.dll.config` en ese servidor — no asumir), compilar, copiar solo ese `.dll`, y revertir el path local inmediatamente para no romper el entorno de desarrollo.
  - **Resuelto el 2026-07-29:** `Config\Configuration.cs` y `Config\Properties\AssemblyInfo.cs` habían quedado trackeados en git desde el commit inicial, antes de que la regla `Config/` existiera en `.gitignore` — por eso esa regla nunca los cubrió y seguían colisionando entre desarrolladores. Se sacaron del índice (`git rm --cached`, el archivo físico no se tocó) para que la regla de `.gitignore` finalmente aplique. **Consecuencia:** un clon nuevo del repo no trae `Config\Configuration.cs` — hay que crearlo a mano en cada equipo nuevo (mismo procedimiento que ya existe para `Config.dll.config`), con el `sXml` apuntando al path correcto de esa máquina/servidor.
- **`Trazabilidad\Web.config`** — contiene credenciales de impersonación (`fnc\vidar`). El Application Pool de `Trazabilidad` corre como `ApplicationPoolIdentity` (cuenta local sin credenciales de red), así que `<identity impersonate="true">` con esa cuenta y su contraseña vigente es **obligatorio** para que funcione el acceso a `\\Loki2\BACKUP\SOPORTES` (soportes de planes especiales en desmaterialización, ver `externas.md` centralizado → "Recursos de red compartidos"). Si `fnc\vidar` rota su contraseña de dominio y no se actualiza aquí, o si `impersonate` queda en `false`, el síntoma es `UnauthorizedAccessException`/"Acceso denegado" en ese share (aislado por factura en `errorlog.txt`, no tumba toda la relación — ver `decisiones.md` centralizado).
- **`Config\bin\Debug\Config.dll.config`** — contiene todas las contraseñas de BD, FTP, SMS y AWS en texto plano
- **`OnProduction = False`** en config — verificar antes de desplegar a producción; cuando es `False` apunta a SIDs de prueba (`PRUTRAZA`, `PRUINTEG`, `PRUFNEUM`)
- **Dependencia externa FNCESB** — `DAC.csproj` referencia `..\..\FNCESB\FNCUtils\FNCUtils.csproj`, `Facade.csproj` referencia `..\..\FNCESB\FNCDAC\FNCDAC.csproj` y `Trazabilidad.csproj` referencia los cuatro (`FNCDAC`, `FNCEntity`, `FNCFacade`, `FNCUtils`). Deben existir en `C:\...\FNCESB\` o el build falla. Además de ser dependencia de build, `FNCESB\FNCDAC\ServinteOracle.cs` y `FNCESB\FNCUtils\Tools.cs` contienen la lógica que escribe ingresos directamente en Servinte para toda la familia de páginas "Cargar plantilla ingresos..." — ver `externas.md` centralizado → "Creación de ingresos por plantilla". **Ese código no es de este repositorio: se edita y documenta en `FNCESB`** (ver "Proyectos compartidos con FNCESB" arriba), y un cambio allá puede romper este build o esta aplicación sin que nada cambie en este repo.
- **Tabla `cargo`** — los registros solo se crean desde la UI (modal de soportes en `Listado.aspx`); no existen al inicio aunque el ingreso aparezca en `VCargos`
- **Tabla `estadocargo`** — de solo lectura desde la app (no hay CRUD en la UI). Agregar un `ChargeStatus` nuevo al enum sin insertar antes la fila correspondiente (`es_id`) en esta tabla causa `ORA-02291` (FK `CARGOESTADO_FK`) al primer `UPDATE cargo`. Ver `dominio.md` centralizado → "Visibilidad de cargos por bandeja" para la lista de estados y filtros vigentes.
- **Despliegue a producción** — el publish de Visual Studio (FileSystem, `PublishUrl=C:\Temp\cargos`) puede omitir DLLs sin cambios aparentes en su caché incremental (visto con `DAC.dll` quedando desactualizado). Verificar fecha de cada `.dll` copiado contra el build local antes de subir a producción, y copiar también los `.aspx` modificados (no solo el `bin`).
- **Repo copiado/movido a un equipo nuevo** — Windows puede marcar todos los archivos con el stream NTFS `Zone.Identifier` (Mark of the Web), lo que no impide compilar ni correr en IIS Express pero sí rompe el publish: .NET Framework lanza `FileLoadException` con HRESULT `0x80131515` sobre algún DLL del `bin` (visto con `AjaxControlToolkit.dll`). Si aparece ese error al publicar pero el proyecto compila y corre bien, desbloquear todo el árbol con `Get-ChildItem -Path <ruta-del-repo> -Recurse -File | Unblock-File`.

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

La validación funcional es manual vía IIS o IIS Express (ver `pruebas.md` centralizado para el estado y la convención de pruebas unitarias adoptada).

## Requerimiento en curso

(vacío)

## Últimos cambios

(máximo 3 entradas, el historial completo está en `historial.md` centralizado)

- 2026-09-22 — Documento/cups en log de soportes faltantes de planes especiales (Facturacion2885) + documentación del proceso externo FNCCargoProgramas tras caída de servidores
- 2026-09-11 — Investigación de soporte clínico faltante (relación 28830) + cadena de incidentes de entorno local (IIS/Config.dll/impersonación Loki2) + aislamiento de errores por factura en GenerateFiles
- 2026-08-02 — Un solo HEV por factura Sanitas, sin sufijo de cédula (Facturacion2885); commit sin push por permiso denegado a DevNeumo

---

## INSTRUCCIONES DE MANTENIMIENTO — leer y respetar siempre

**1. Punto de control** (usar al cerrar una sesión intermedia de un requerimiento):
Cuando el usuario diga "punto de control", actualiza la sección "Requerimiento en curso" de este archivo con: qué se está implementando, qué ya está hecho (con archivos modificados), cuál es el siguiente paso exacto, contexto importante que no está en el código, y si se escribieron pruebas, cuáles y qué cubren.

**2. Retomar sesión** (usar al iniciar una sesión nueva):
Cuando el usuario diga "retomar", lee la sección "Requerimiento en curso" y resume en 3 líneas dónde estamos y cuál es el siguiente paso.

**3. Cierre de requerimiento** (usar al terminar un requerimiento completo):
Cuando el usuario diga "cerrar requerimiento", hacer en orden, todo sobre los
archivos en `Centralización_Documentación\Proyectos\BusDatos\docs\Reportes\`
(no en este repositorio):
   a. Agregar una entrada completa a `historial.md` con: fecha de hoy, título corto del requerimiento, qué se implementó, archivos modificados, decisiones tomadas que no son obvias, pruebas escritas (qué clases/módulos y qué escenarios cubren), y si quedó algo pendiente.
   b. Actualizar "Últimos cambios" en este archivo (`CLAUDE.md`, que sí queda en este repositorio): agregar la entrada nueva resumida en 1 línea; si ya hay 3 entradas, eliminar la más antigua.
   c. Actualizar `pruebas.md`: agregar las clases o módulos nuevos a "Qué se prueba en este proyecto", y agregar una entrada al "Historial de cobertura" con fecha, qué se cubrió y por qué.
   d. Si el requerimiento implicó una decisión técnica no obvia, agregar una entrada a `decisiones.md`.
   e. Si el requerimiento afectó endpoints, dominio, flujos o integraciones, actualizar el archivo correspondiente en ese mismo `docs\Reportes\`.
   f. Si quedó algo pendiente, registrarlo en `Centralización_Documentación\Pendientes.md` (sección del proyecto).
   g. Borrar el contenido de "Requerimiento en curso" y dejarlo como (vacío).

**4. Escribir pruebas** (comportamiento estándar siempre, adoptado 2026-07-08):
Cuando se implemente o modifique lógica de negocio, sin que el usuario tenga que pedirlo explícitamente:
   - Proponer qué pruebas unitarias corresponden a ese cambio.
   - Usar las herramientas de testing estándar del stack (ver `pruebas.md` centralizado — a definir/crear, ya que hoy no existe proyecto de tests en la solución).
   - Usar estructura AAA (Arrange / Act / Assert) con comentarios.
   - Nombrar los tests describiendo: método o función, escenario y resultado esperado.
   - Mockear solo dependencias externas (Oracle, SFTP, SMS, correo), nunca módulos propios del proyecto.
   - Cubrir mínimo: caso feliz + caso de error más probable.
