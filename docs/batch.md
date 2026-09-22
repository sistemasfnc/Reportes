# Aplicaciones batch

Seis aplicaciones de consola `.exe` que se ejecutan como tareas programadas. Todas referencian `Facade` directamente y leen configuración de `Config.dll.config`.

---

## SendCompensarPDF

**Proyecto:** `SendCompensarPDF\`
**Propósito:** genera archivo TXT con registros PFP de Compensar y lo sube al servidor SFTP de la EPS.

**Flujo:**
1. Consulta registros pendientes via `FacadeDesmaterializacion` o `FacadeCompensar`
2. Genera archivo TXT con formato delimitado por punto y coma (NIT, autorización, tipo documento, etc.)
3. Sube via WinSCP al servidor configurado en `CompensarRemotePath`
4. Actualiza estado del registro en BD

**Configuración relevante:** `CompensarConnection`, `WinSCP`, `WinSCPLog`, `TextPath`, `CompensarRemotePath`

---

## SendCompensarReport

**Proyecto:** `SendCompensarReport\`
**Propósito:** genera reporte Excel para Compensar y lo sube.

**Estado actual:** los métodos `Main()` y `GenerateExcel()` están sin implementar (stubs vacíos). Tiene boilerplate de WinSCP.

**Configuración relevante:** `CompensarConnection`, `WinSCP`

---

## SendElyonFile

**Proyecto:** `SendElyonFile\`
**Propósito:** sube archivos de desmaterialización al FTP de Elyon (EPS).

**Flujo:**
1. Consulta archivos pendientes via `FacadeDesmaterializacion`
2. Sube cada archivo via WinSCP al FTP `ftp.elyon.com.co`
3. Actualiza estado del archivo en BD (enviado)

**Configuración relevante:** `ElyonFtp`, `ElyonUser`, `ElyonPassword`, `WinSCP`, `WinSCPLog`, `ProgramPath`

---

## SendMessage

**Proyecto:** `SendMessage\`
**Propósito:** envía SMS y correos de Sanitas. Tiene dos modos de operación.

**Modo 1 — SMS (`SendTextMessage`):**
1. Obtiene lista de destinatarios y mensajes desde BD
2. Llama `WServices` via HTTP para enviar SMS a cada número
3. Usa plantilla configurada en `SMSTemplate`

**Modo 2 — Email Sanitas (`SendSanitasMail`):**
1. Genera archivo Excel con procedimientos Sanitas via `FacadeSanitas.GenerateExcelFile()`
2. Envía email a `SanitasRecipient` con el Excel adjunto via `Utils.SendMail`

**Configuración relevante:** `SMSUrl`, `SMSUser`, `SMSPassword`, `SMSTemplate`, `SanitasRecipient`, `MailServer`, `MailPort`, `MailUser`, `MailPassword`

---

## SendPublicity

**Proyecto:** `SendPublicity\`
**Propósito:** envía correos masivos de publicidad/marketing.

**Flujo:**
1. Obtiene lista de correos desde BD via `FacadePublicidad`
2. Complementa con lista desde archivo CSV (`DataFile` en config)
3. Envía email HTML a cada destinatario via `Utils.SendMail`
4. Comienza desde el índice 3000 de la lista (evita reenvíos ya procesados)

**Configuración relevante:** `DataFile`, `MailServer`, `MailPort`, `MailUser`, `MailPassword`

---

## GeneraConsultas

**Proyecto:** `GeneraConsultas\`
**Propósito:** genera consultas/citas desde la base de datos FoxPro del HIS histórico.

**Flujo:**
1. Conecta a FoxPro via OLEDB (`FoxConnection`)
2. Llama `ConsultasDAC.Generate()`
3. Produce archivo de consultas [POR CONFIRMAR formato de salida]

**Configuración relevante:** `FoxConnection`

---

## GenerateAppointmentFile

**Proyecto:** `GenerateAppointmentFile\`
**Propósito:** [POR CONFIRMAR] — genera archivo de citas.

**Configuración relevante:** [POR CONFIRMAR]

---

## Procesos de `FNCESB` que alimentan datos de `Reportes`

Los siguientes procesos **no viven en este repo** sino en `FNCESB` (repo hermano, ver "Dependencia externa FNCESB" en `CLAUDE.md`), pero los informes de `Reportes` dependen de que corran. Se documentan aquí porque una caída de servidores los deja sin ejecutar y el síntoma aparece como "el informe no trae datos".

### FNCCargoProgramas (carga de programas — alimenta `GeneraPlantillaProgramas.aspx`)

**Proyecto (fuente):** `FNCESB\FNCCargoProgramas\CargoProgramas.cs` (consola, .NET 4.8).
**Dónde corre:** servidor **201.58**, carpeta `D:\Apps\CrearCargoProgramas`, ejecutable `FNCCargoProgramas.exe`. Según el equipo se ejecuta **a mano** con las fechas editadas en el `.config`; en el código no hay programador propio (`FNCESB\FNCServicioProgramas` es un esqueleto con el `OnTimer` vacío y no hace nada). No se verificó si además hay una tarea programada en ese servidor.
**Propósito:** traer desde Salesforce las citas de programas atendidas y registrarlas en `INSPIRASERVINTE` (tabla de sincronización de Inspira/Servinte) con `IS_TIPO = 'Programas solo estadistica'`.

**Dónde vive cada cosa (verificado en Oracle el 2026-09-21):**

| Objeto | Base de datos | Esquema |
|---|---|---|
| Tabla `INSPIRASERVINTE` (donde escribe el proceso) | **`INTEGRA`** (`INTEGRA.FNC.NEUMOLOGICA.ORG`, host `oda01`, `192.168.101.20:1521`) | `FNCSISTEMAS` |
| Vista `VDATOSPROGRAMAS` / `VDATOSVALORACIONPROGRAMAS` (donde lee el informe) | **`FNEUMB`** (`FNEUMB.FNC.NEUMOLOGICA.ORG`, mismo host) | `INTEGRABUS` |
| Tablas de Servinte usadas por la vista (`ABPAC`, `INEMP`, `INTAR`, `INPLA`, …) | `FNEUMB` | `SERVINTE` |

La vista de `FNEUMB` lee `INSPIRASERVINTE` a través del DB link `LNKINTEGRA`, que se conecta a `INTEGRA` como `FNCSISTEMAS`. Existen también `LNKINTEGRABUS` (mismo destino) y `LNKINTEGRAPRU` (apunta a `PRUINTEG`, pruebas). Por eso `ServinteIntegra` (usuario `INTEGRABUS`, SID `FNEUMB`) puede leer la tabla, y cualquier `DELETE`/`UPDATE` sobre `INSPIRASERVINTE@LNKINTEGRA` se ejecuta en la práctica con los privilegios de `FNCSISTEMAS` sobre `INTEGRA`. Los datos de la vista `VDATOSPROGRAMAS` salen de ahí, y de ahí sale el Excel de `GeneraPlantillaProgramas.aspx`.

**Cómo se ejecuta:**
1. En `D:\Apps\CrearCargoProgramas\FNCCargoProgramas.exe.config` fijar `InitialDate` y `FinalDate` (formato `yyyy-MM-dd`, ambas inclusive, filtran `ActivityDate__c` de la cita en Salesforce).
2. Ejecutar desde PowerShell en esa carpeta:
   ```powershell
   .\FNCCargoProgramas.exe False
   ```
   El argumento es obligatorio (sin argumento el programa termina sin hacer nada) y se convierte con `Convert.ToBoolean`.
3. Si `InitialDate`/`FinalDate` están vacías, usa **el día anterior** para ambas (pensado para una corrida diaria).

**Qué significa el argumento** (según `SalesforceViaRestApi.GetPatientsforPrograms`, `FNCESB\FNCSalesforce`):

| Argumento | Filtro que aplica en Salesforce |
|---|---|
| `False` | Rama general: `PlanId__r.HealthCarePlanId__r.Code__c = '434'`, excluyendo documentos `INVEST` y `BLOQUEO`, dentro del rango de fechas. |
| `True` | Solo planes `FAMISANAR AIREPOC` con `GroupId__r.Statistics__c = true`, dentro del rango de fechas. |

> **[POR CONFIRMAR]** El equipo indica que `False` es la ejecución para Famisanar, pero el fuente dice lo contrario por nombre (`bIsFamisanar`), y los datos cargados con `False` el 2026-09-21 son mayoritariamente de la empresa `25` (Sanitas) más algunas de `114`, `21`, `83` y `260`. Confirmar a qué EPS corresponde el código de plan de salud `434` en Salesforce y si el `.exe` desplegado en 201.58 coincide con este fuente (puede ser una versión distinta).

**Flujo interno:** `GetPatients` (Salesforce REST) → `GetProductsByRate` y `GetGroups` (Servinte) → `GroupPatients` → `CreateCharges` (`ServinteOracle.CreateChargesForPrograms`) → `CreateStatistics` (`Integrador.InsertRecord`, escribe en `INSPIRASERVINTE`). Los errores se registran con `LogError.WriteError("FNCProgramas", ...)` en el Visor de eventos.

**Trampas conocidas:**
- **No es idempotente.** Volver a correr un rango ya cargado **inserta las mismas filas otra vez** (misma cita + servicio + fecha con otro `IS_ID`), y el informe las muestra duplicadas. Antes de correr, verificar qué días faltan realmente (consulta abajo) y fijar el rango solo a esos días.
- **Fechas viejas en el config.** El `App.config` del fuente trae `InitialDate`/`FinalDate` de otro periodo (2025-08-31 a 2025-09-30). Si se copia al servidor sin editarlas, recarga ese rango. Tras una recarga manual, **dejar las fechas vacías** para que el proceso diario tome "ayer".
- El `.config` guarda credenciales de Salesforce y correo en texto plano (misma zona de peligro que `Config.dll.config`); no copiarlo a otros equipos ni al repo.

**Cómo validar que la carga funcionó** (solo lectura, conexión `ServinteIntegra`):
```sql
-- ¿Hasta qué fecha hay datos? (la vista del informe debe llegar a "ayer")
SELECT MAX(FECHA), COUNT(*) FROM VDATOSPROGRAMAS;

-- Filas por día del tipo que carga este proceso (buscar días faltantes o muy bajos)
SELECT TRUNC(IS_FECHA) dia, COUNT(*) filas
FROM INSPIRASERVINTE@LNKINTEGRA
WHERE IS_TIPO = 'Programas solo estadistica' AND IS_FECHA >= TRUNC(SYSDATE) - 30
GROUP BY TRUNC(IS_FECHA) ORDER BY 1;

-- Duplicados (misma cita + servicio + fecha en más de una fila)
SELECT COUNT(*) grupos, SUM(n) filas FROM (
  SELECT IS_CITA, IS_SERVICIO, TRUNC(IS_FECHA) f, COUNT(*) n
  FROM INSPIRASERVINTE@LNKINTEGRA
  WHERE IS_TIPO = 'Programas solo estadistica' AND IS_FECHA >= TRUNC(SYSDATE) - 30
  GROUP BY IS_CITA, IS_SERVICIO, TRUNC(IS_FECHA) HAVING COUNT(*) > 1);
```
Un día hábil normal trae unas 600–900 filas; sábados, unas 250; domingos y festivos, cero.

**Incidente 2026-09-21 (caída de servidores):** los datos de este proceso quedaron cortados al 2026-09-14 (`VDATOSPROGRAMAS` y `INSPIRASERVINTE` sin filas desde el 15/09), lo que hacía que `GeneraPlantillaProgramas.aspx` devolviera un Excel vacío. Además hubo un hueco previo del 28/08 al 31/08 (6, 114 y 13 filas en esos días). El 21/09 se ejecutó `FNCCargoProgramas.exe False` en 201.58. En la consulta de las 18:07–18:09 las filas nuevas (`IS_ID > 4390896`) correspondían a fechas de servicio del **21/08 al 28/08** (rango de arranque inferido de los datos, no leído del `.config`). Los días 21–27/08 ya estaban cargados, así que quedaron ~3.860 grupos cita+servicio duplicados (filas de ejemplo con `IS_INGRESO = 0` e `IS_CARGO = 0`, es decir, sin ingreso ni cargo asociado en Servinte; no se revisó `AYMOV`). En esos minutos el `IS_ID` máximo dejó de crecer, por lo que la corrida pudo haber terminado o estar entre fases. **Pendiente al momento de escribir esto:** limpiar los duplicados de 21–27/08 y cargar solo los días realmente faltantes (28/08, 29/08, 31/08 y 15/09 en adelante).

---

## Notas operativas

- Todos los batch leen el mismo `Config.dll.config` que las aplicaciones web.
- Los errores se loguean en Windows Event Log con fuente `FNCPortal`. Crear el source antes de ejecutar:
  ```powershell
  # Ejecutar como Administrador
  New-EventLog -LogName Application -Source "FNCPortal"
  ```
- `SendCompensarReport` está incompleto — no ejecutar en producción.
- `SendPublicity` inicia desde índice 3000; si la lista cambia de tamaño esto puede saltar o repetir destinatarios.
- WinSCP debe estar instalado en `C:\Program Files (x86)\WinSCP\winscp.com` para que funcionen `SendCompensarPDF` y `SendElyonFile`.
