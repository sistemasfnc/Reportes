# Integraciones externas

## Bases de datos

### Oracle — Base de datos principal
- **Host:** `192.168.101.20`, Puerto `1521`
- **SID pruebas:** `PRUTRAZA` (facturación/trazabilidad), `PRUINTEG` (integración), `PRUFNEUM` (Servinte)
- **Usuarios:** `FACTU` (trazabilidad), `FNCSistemas` (integración), `SERVINTE`, `INTEGRABUS`
- **Código:** `DAC\OracleDAC.cs` — usa `Oracle.ManagedDataAccess v2.19.80`
- **Nota:** parámetros con sintaxis `:param` (no `@param`)

### SQL Server — Bases auxiliares
- **Servidor:** `HEIMDALL`
- **Bases:** `FNCStats` (Compensar), `ServinteFNC_Apruebas2` (Servinte)
- **Código:** `DAC\SQLDAC.cs` — `System.Data.SqlClient`
- **Config keys:** `CompensarConnection`, `ServinteConnection`

### FoxPro / OLEDB — Legacy
- **Nómina:** `C:\backup\DATOS6\NOM.F\BASE\EMPLEA.dbf` (Novasoft)
- **HIS histórico:** `c:\Temp\Data\aghis.dbc`
- **Código:** `DAC\OLEDBDAC.cs` — Provider `vfpoledb`
- **Config keys:** `FoxConnection`, `DBNovasoft`

### Excel via OLEDB
- **Uso:** plantillas Sanitas (lectura de tarifas)
- **Archivo:** `C:\Config\plantillasanitas.xls`
- **Config key:** `SanitasConnection`

---

## EPS (Entidades Promotoras de Salud)

El sistema integra con 17+ EPS colombianas. Cada una tiene su propio formato de archivo y canal de entrega.

### Compensar
- **Canal:** FTP propio (`CARPETAREMOTA` en config)
- **Formato:** archivo TXT delimitado por punto y coma
- **Batch:** `SendCompensarPDF.exe` — genera y sube via WinSCP
- **BD:** SQL Server `FNCStats` (`CompensarConnection`)
- **Código:** `DAC\CompensarDAC.cs`, `Facade\FacadeCompensar.cs`
- **Desmaterialización:** `Trazabilidad\DesmaterializaCompensar.aspx` → S3

### Sanitas / Colsanitas
- **Canal:** email a `programadosbogota2@colsanitas.com`
- **Formato:** Excel (plantillasanitas.xls)
- **Batch:** `SendMessage.exe` → `SendSanitasMail()`
- **Código:** `DAC\SanitasDAC.cs`, `Facade\FacadeSanitas.cs`

### Elyon
- **Canal:** FTP `ftp.elyon.com.co`
- **Credenciales:** `ElyonUser` = `800180553`, `ElyonPassword` en config
- **Batch:** `SendElyonFile.exe` — sube via WinSCP
- **Config keys:** `ElyonFtp`, `ElyonUser`, `ElyonPassword`, `WinSCP`

### NuevaEPS (Aciel)
- **Canal:** SFTP `sftpext04.acielcolombia.com`, puerto `4758`
- **Credenciales:** `NuevaEPSUser` = `Aciel_800180553`
- **Carpeta destino:** `uploads`
- **Config keys:** `NuevaEPSSFTP`, `NuevaEPSUser`, `NuevaEPSPassword`, `NuevaEPSPort`

### Famisanar
- **Canal:** FTP `190.143.65.21`
- **Credenciales:** `FamisanarUser` = `funneucol`
- **Página:** `Trazabilidad\RipFamisanar.aspx`
- **Config keys:** `FamisanarFTP`, `FamisanarUser`, `FamisanarPassword`

### Salud Total
- **Canal:** FTP `ftp://transaccional.saludtotal.com.co/`
- **Credenciales:** `SaludTotalUser` = `F800180553`
- **Config keys:** `SaludTotalFtp`, `SaludTotalUser`, `SaludTotalPassword`

### MedPlus (Nexus Merak)
- **Canal:** SFTP `cuentasmedicas.nexusmerak.com`, puerto `22`
- **Credenciales:** `MedPlusUser` = `800180553`
- **Config keys:** `MedPlusHost`, `MedPlusUser`, `MedPlusPassword`, `MedPlusPort`

### Consorcio Salud (SFTP genérico)
- **Canal:** SFTP `ftp.aseguramientosalud.com`, puerto `990`
- **Credenciales:** `SftpUser` = `consorciosalud\usuftpfneumologica`
- **Config keys:** `SftpHost`, `SftpUser`, `SftpPassword`, `SftpPort`

### Otras EPS
Las siguientes tienen integración en `Trazabilidad/` pero sin batch dedicado (generación manual desde UI):
- NuevaEPS, Colmedica, Coomeva, Sura, Cafesalud, Aliansalud, Cruz Blanca, Golden Group, Humana Vivir, Medimas, Policia Nacional, Magisterio, entre otras.

---

## Servicios de mensajería

### SMS — Infobip
- **URL:** `http://api.infobip.com/api/v3/sendsms/plain`
- **Credenciales:** `SMSUser` = `NeuFund45`, `SMSPassword` en config
- **Implementación:** `WServices\Service1.svc` → `SendSMS(Sms)` → HTTP POST
- **Plantilla:** archivo de texto en ruta `SMSTemplate` del config
- **Código cliente:** `Utils\Tools.SendHttpPostRequest()`

### Email — SMTP interno
- **Servidor:** `192.168.101.86`, puerto `4065`
- **Usuario:** `sistemas@neumologica.org`
- **Uso:** envío de plantillas Sanitas, publicidad
- **Código:** `Utils\SendMail.Send()`

---

## Almacenamiento en nube

### Amazon S3 — Factura electrónica Compensar
- **Bucket:** `fnc-facturacionelectronica`
- **Prefijos:** `CUV/` (documentos actuales), `CUVHistorico/` (histórico)
- **Credenciales:** `AWSKey`, `AWSSecret` en config (texto plano — zona de peligro)
- **Uso:** `Trazabilidad\DesmaterializaCompensar.aspx`
- **Config keys:** `S3BucketName`, `CuvPrefix`, `CuvHistPrefix`, `AWSKey`, `AWSSecret`

---

## Herramientas de transferencia

### WinSCP
- **Ruta local:** `C:\Program Files (x86)\WinSCP\winscp.com`
- **Uso:** subida de archivos a Compensar, Elyon y otros FTPs que no soportan FluentFTP
- **Log:** `C:\Temp\winscp.xml`
- **Config keys:** `WinSCP`, `WinSCPLog`

### FluentFTP (`FluentFTP v53.0.2`)
- **Uso:** operaciones FTP en `Utils\FTP.cs`
- **Métodos:** `GetFileList`, `GetFile`, `UploadFile`, `DeleteFile`
- **Soporta:** SSL/TLS con validación de certificado deshabilitada

### SSH.NET / Renci (`SSH.NET 2025.1.0`)
- **Uso:** conexiones SFTP cuando el servidor requiere SSH
- **Código:** `Utils\FTP.cs` o directamente en batch apps

---

## Sistema HIS — Servinte / Integra (Oracle)

- **Conexión:** `ServinteIntegra` → Oracle SID `FNEUMB`, usuario `INTEGRABUS`
- **Vista principal:** `VCargos` — fuente de todos los ingresos hospitalarios que aparecen en `Listado.aspx`
- **Otras vistas:** `VCargosDevueltos`, `VTrazabilidad`, `VAbonos`, `VEstadoCargo`, `VLogEstadoCargo`, `VCARGOSVENTAFARMACIA`, `VRIPSFAMISANAR`
- **Relación:** este sistema solo lee del HIS via vistas; no escribe en él

## Sistema Digiturno (WCF externo)
- **URL:** `http://localhost:8082/WSDigiturno.asmx`
- **Uso:** [POR CONFIRMAR] — referenciado en `Trazabilidad\Web.config`
- **Código:** referencia web `WSDigiturno` en `Trazabilidad/`
