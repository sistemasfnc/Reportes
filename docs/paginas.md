# Páginas del sistema (ASPX)

El sistema usa ASP.NET WebForms — no hay controllers REST. Cada página `.aspx` tiene un code-behind `.aspx.cs` que maneja los eventos.

**Autenticación:** Forms Authentication. Todas las páginas (excepto `Login.aspx`) requieren sesión activa. Timeout: 60 minutos. Cultura: `es-CO`.

**Control de acceso:** cada `Page_Load` verifica `Tools.HaveAccess(oUser.lSecurity, Permissions.X)`. Si falla, redirige a `SinAcceso.aspx`.

---

## Trazabilidad/ — Aplicación principal

### Autenticación
| Página | Permiso | Descripción |
|---|---|---|
| `Login.aspx` | ninguno | Autenticación con SHA256 |
| `LogOut.aspx` | ninguno | Limpia sesión y redirige a Login |
| `SinAcceso.aspx` | ninguno | Pantalla de acceso denegado |

### Flujo de Cargos — Cajero
| Página | Permiso | Descripción |
|---|---|---|
| `Listado.aspx` | `entrylist` (1) | Lista cargos pendientes de `VCargos`. Permite enviar a central (estado 3) o guardar soportes (estado 2). Botón "Completos" hace batch. |
| `RecibirDevolucion.aspx` | `returnreception` (3) | Cajero recibe cargos devueltos por el facturador (estado 6→7) |
| `Devolucion.aspx` | `returnresponse` (4) | Cajero tramita la devolución: responde motivos y reenvía (estado 7→4) |
| `Devoluciones.aspx` | [POR CONFIRMAR] | Lista de devoluciones |

### Flujo de Cargos — Facturador / Central de Cuentas
| Página | Permiso | Descripción |
|---|---|---|
| `Central.aspx` | `entryreception` (5) | Recibe cargos del cajero. Botón Devolver envía a `Auditar.aspx` |
| `Auditar.aspx` | `entryreturn` (6) | Registra y tramita devoluciones. Modal con motivos obligatorios. Botón listo para facturar (estado 5) |
| `ListoFacturar.aspx` | `readytoinvoice` (25) | Cargos listos para facturar. Botón Devolver (estado 6) |
| `CargosXFacturar.aspx` | [POR CONFIRMAR] | Cargos pendientes de facturación |

### Tipos de Cargos (creación por servicio)
| Página | Descripción |
|---|---|
| `CargoUrgencias.aspx` | Crea cargo de urgencias (estado inicial: incomplete=1) |
| `CargoHospitalizacion.aspx` | Crea cargo de hospitalización |
| `CargoParticular.aspx` | Crea cargo de paciente particular |
| `CargoProgramas.aspx` | Crea cargo de programas |
| `CargoFibrosis.aspx` | Crea cargo para pacientes de fibrosis |
| `CargosFCI.aspx` | Cargos FCI (requiere permiso `chargesfci` 24) |
| `CargosOtros.aspx` | Otros tipos de cargos |
| `Farmacia.aspx` | Cargos de farmacia (permiso `pharmacylist` 50) |

### Consulta y Trazabilidad
| Página | Permiso | Descripción |
|---|---|---|
| `Trazabilidad.aspx` | [POR CONFIRMAR] | Dashboard / vista principal de trazabilidad |
| `EstadoCargo.aspx` | `chargestatusreport` (19) | Historial de estados de un cargo |
| `LogCargo.aspx` | `logreport` (21) | Log completo de transacciones de cargo |
| `CargosyAbonos.aspx` | `invoicereport` (20) | Vista de cargos y abonos por factura |
| `AbonosTarde.aspx` | [POR CONFIRMAR] | Abonos registrados tarde |
| `AbonosXFacturar.aspx` | [POR CONFIRMAR] | Abonos pendientes de facturar |
| `IngresosXLegalizar.aspx` | [POR CONFIRMAR] | Ingresos pendientes de legalización |
| `IngresosAnulados.aspx` | [POR CONFIRMAR] | Ingresos anulados |

### Exportaciones
| Página | Descripción |
|---|---|
| `ExportarCargos.aspx` | Exporta cargos a archivo |
| `ExportarLogCargo.aspx` | Exporta log de cargos |
| `ExportarDevoluciones.aspx` | Exporta devoluciones |
| `ExportaFactura.aspx` | Exporta una factura |
| `ExportarFacturas.aspx` | Exporta múltiples facturas |
| `Exportar.aspx` | Exportador genérico |
| `GenerarReporte.aspx` | Generador de reportes |
| `GenerarPlano.aspx` | Genera archivo plano |

### Facturas y Relaciones
| Página | Permiso | Descripción |
|---|---|---|
| `Facturas.aspx` | `invoicelist` (22) | Lista y gestión de facturas |
| `FacturasXFacturar.aspx` | [POR CONFIRMAR] | Facturas pendientes |
| `CrearRelacion.aspx` | `relationshipgeneration` (42) | Crea relación de envío a EPS |
| `RecibirRelacion.aspx` | `relationshipvalidation` (44) | Recibe respuesta de la EPS |
| `TramitarRelacion.aspx` | [POR CONFIRMAR] | Tramita relación con EPS |
| `ReporteRelaciones.aspx` | `relationshipreport` (45) | Reporte de relaciones |

### Glosas / Disputas
| Página | Descripción |
|---|---|
| `Glosas.aspx` | Lista y gestión de glosas con EPS |
| `RecepcionGlosa.aspx` | Recepción de notificación de glosa |

### Costos
| Página | Permiso | Descripción |
|---|---|---|
| `MaestroCostos.aspx` | `listcostmaster` (37) | Maestro de costos |
| `DistribuirCostos.aspx` | `assigncost` (32) | Distribución de costos por centro |
| `ReporteCostos.aspx` | `costreport` (34) | Reporte de costos |
| `CostoXMedico.aspx` | [POR CONFIRMAR] | Costos por médico |
| `AsignarCentros.aspx` | [POR CONFIRMAR] | Asignación de centros de costo |
| `CargarValoraciones.aspx` | [POR CONFIRMAR] | Carga de valoraciones |
| `GeneraPlantillaProgramas.aspx` | [POR CONFIRMAR] | Plantilla de programas |

### EPS Especiales
| Página | Descripción |
|---|---|
| `DesmaterializaCompensar.aspx` | Factura electrónica para Compensar (S3) |
| `RipFamisanar.aspx` | RIPS para Famisanar |
| `ReporteRHB.aspx` | Reporte RHB (permiso `rhbcharges` 41) |
| `CruceCargos.aspx` | Cruce / conciliación de cargos |
| `CruceCargosProgramas.aspx` | Cruce de cargos de programas |

### Administración
| Página | Permiso | Descripción |
|---|---|---|
| `usuarios.aspx` | `listuser` (12) | Lista de usuarios del sistema |
| `editarusuario.aspx` | `edituser` (14) | Edición de usuario y permisos |
| `Baul.aspx` | `passwordtrunk` (49) | Bóveda de credenciales cifradas |

---

## Reportes/ — Aplicación de reportes

| Página | Descripción |
|---|---|
| `Default.aspx` | Placeholder (sin implementación activa) |
| `GeneraConsultas.aspx` | Consultas de citas desde FoxPro |
| `ReporteFacturados.aspx` | Reporte de elementos facturados |

---

## Nomina/ — Aplicación de nómina

| Página | Descripción |
|---|---|
| `Default.aspx` | Lista de registros de nómina |
| `exportnomina.aspx` | Exportación de nómina |
| `About.aspx` / `Contact.aspx` | Páginas informativas (sin lógica) |

---

## WServices/ — Servicio WCF

| Operación | Parámetros | Descripción |
|---|---|---|
| `SendSMS(Sms)` | `Sms` entity | Envía SMS via Infobip. Lee plantilla de archivo, reemplaza variables, HTTP POST al gateway |

Endpoint: configurado en IIS. WSDL disponible en `/WServices/Service1.svc?wsdl`.
