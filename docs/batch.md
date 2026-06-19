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
