# Arquitectura del sistema

## Diagrama de capas

```
┌─────────────────────────────────────────────────────────────────┐
│  PRESENTACIÓN                                                   │
│  Trazabilidad/  Reportes/  Nomina/  WServices/                 │
│  (ASP.NET WebForms ASPX + code-behind)  (WCF)                 │
└────────────────────────┬────────────────────────────────────────┘
                         │ referencia
┌────────────────────────▼────────────────────────────────────────┐
│  BATCH                                                          │
│  SendCompensarPDF  SendElyonFile  SendMessage                   │
│  SendPublicity     GeneraConsultas  GenerateAppointmentFile     │
│  (Consola .exe — se ejecutan como tareas programadas)          │
└────────────────────────┬────────────────────────────────────────┘
                         │ referencia
┌────────────────────────▼────────────────────────────────────────┐
│  FACHADA (Facade/)                                              │
│  FacadeCargo  FacadeFactura  FacadeRelacion  FacadeUser        │
│  FacadeCosto  FacadeGlosa   FacadeCompensar  FacadeSanitas     │
│  FacadeNomina FacadeSMS     FacadeSecurity   FacadeGenerico    │
│  FacadeBaul   FacadeDesmaterializacion  FacadePublicidad       │
│  + referencia FNCDAC (proyecto externo FNCESB)                 │
└──────────────┬──────────────────────────┬───────────────────────┘
               │                          │
┌──────────────▼──────────┐  ┌────────────▼──────────────────────┐
│  ACCESO A DATOS (DAC/)  │  │  SOPORTE                          │
│  CargosDAC  FacturaDAC  │  │  Config/    — lee Config.dll.config│
│  RelacionDAC UserDAC    │  │  Entity/    — objetos de dominio   │
│  CostosDAC  GlosaDAC    │  │  EventLog/  — Windows Event Log    │
│  NominaDAC  CompensarDAC│  │  Utils/     — herramientas static  │
│  SanitasDAC RelacionDAC │  │  + referencia FNCUtils (externo)  │
│  SQLDAC OracleDAC       │  └───────────────────────────────────┘
│  OLEDBDAC GenericDAC    │
└──────────────┬──────────┘
               │
┌──────────────▼──────────────────────────────────────────────────┐
│  BASES DE DATOS                                                 │
│  Oracle PRUTRAZA (principal)   SQL Server HEIMDALL              │
│  Oracle PRUINTEG / PRUFNEUM    FoxPro/OLEDB (legacy nómina)    │
└─────────────────────────────────────────────────────────────────┘
```

## Responsabilidad de cada capa

### Config
Lee todas las configuraciones desde un XML externo hardcodeado en `Configuration.cs`. Expone `GetStringValue`, `GetBoolValue`, `GetIntegerValue` y `UpdateKeyValue`. No tiene dependencias internas.

### Entity
Objetos de dominio planos serializables. Sin herencia, sin ORM, sin lógica de negocio. Solo propiedades. Referenciado por todas las demás capas.

### DAC (Data Access Components)
Una clase por dominio. Acceso directo a base de datos via ADO.NET puro. Cada clase:
- Acepta `string sConnection = ""` en el constructor; si vacío, lee de `Config`
- Construye SQL dinámico con `StringBuilder` y `List<OracleParameter>`
- Llama `oDAC.GetDataTable()` y mapea filas a entidades manualmente
- Implementa `IDisposable`
- Captura excepciones, las loguea via `LogError.WriteError` y relanza como `ApplicationException`

Tres conectores de infraestructura: `SQLDAC` (SQL Server), `OracleDAC` (Oracle), `OLEDBDAC` (FoxPro).

### Facade
Fina capa de lógica de negocio sobre DAC. Una clase por dominio. Acepta connection string opcional para permitir que la capa de presentación elija la BD. Implementa `IDisposable`. Referencia también `FNCDAC` (proyecto externo).

### Presentación (WebForms)
Páginas ASPX con code-behind en C#. Sin MVC, sin ViewModels formales. Estado manejado via `Session` y `ViewState`. Autenticación por Forms Auth (`Login.aspx`), sesión de 60 minutos.

### WServices
Servicio WCF con una operación: `SendSMS(Sms)`. Llama al gateway de SMS de Infobip via HTTP POST.

### Batch
Aplicaciones de consola independientes. Se ejecutan como tareas programadas. Referencian Facade directamente. Ver `docs/batch.md`.

## Reglas de dependencia

```
Presentación  →  Facade  →  DAC  →  Config
                         →  Entity
                         →  EventLog
                         →  Utils
              →  Entity
              →  Utils
              →  Config
Facade        →  FNCDAC  (externo — FNCESB)
DAC           →  FNCUtils (externo — FNCESB)
```

**Prohibido:** DAC no puede referenciar Facade. Entity no puede referenciar nadie. Config no puede referenciar nadie.

## Patrones aplicados

| Patrón | Dónde se ve |
|---|---|
| Facade | `Facade/` — simplifica el acceso al DAC para la presentación |
| Repository | `DAC/` — cada clase encapsula el acceso a su tabla/dominio |
| Disposable | Todas las clases DAC y Facade implementan `IDisposable` |
| Query Builder dinámico | Todo DAC — `StringBuilder` acumula cláusulas `WHERE` condicionales |
| LEFT JOIN con vista | `CargosDAC.GetData()` — `VCargos` (HIS) LEFT JOIN `cargo` (local) |
| Trigger de BD para PK | Tabla `cargo` — `ca_id` asignado por trigger Oracle, retornado con `RETURNING INTO` |
| Static helper | `Utils.Tools`, `EventLog.LogError` — métodos utilitarios sin estado |
| Forms Authentication | `Trazabilidad/` — `Login.aspx` con redirect automático |

## Configuración de ambientes

El flag `OnProduction` en `Config.dll.config` controla el ambiente:

| Setting | Pruebas | Producción |
|---|---|---|
| `OnProduction` | `False` | `True` |
| `SQLConnection` SID | `PRUTRAZA` | [POR CONFIRMAR] |
| `OracleIntegra` SID | `PRUINTEG` | [POR CONFIRMAR] |
| `ServinteOracle` SID | `PRUFNEUM` | [POR CONFIRMAR] |
| `CompensarConnection` BD | `FNCStats` | [POR CONFIRMAR] |
