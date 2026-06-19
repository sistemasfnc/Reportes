# Dominio del sistema

## Entidades principales

### Cargo (`Entity\Cargo.cs`)
Representa un expediente de admisión hospitalaria que debe ser tramitado y facturado. Es la entidad central del sistema. Vincula un ingreso del HIS (`idadmission`) con su estado de tramitación en este aplicativo (`status`).

Campos clave: `id` (ca_id), `idadmission` (número de ingreso del HIS), `status`, `service`, `plan`, `company`, `eps`, `value`, `iduser`, `date`.

**Importante:** un `Cargo` solo existe en la tabla `cargo` si fue procesado desde la UI. Los ingresos nuevos viven en `VCargos` (vista del HIS) y solo se persisten aquí al guardar soportes por primera vez.

### Invoice (`Entity\Invoice.cs`)
Factura generada a partir de uno o varios cargos. Tiene campos `status` (string) y `dbstatus` que mapea al estado de su `RelacionEnvio`.

### RelacionEnvio (`Entity\RelacionEnvio.cs`)
Agrupa facturas para envío a una EPS. Representa el sobre/remisión. Tiene estado `cestado` (char) y detalle por factura en `DetalleRelacion` con flags de asignación, envío y recepción.

### Glosa (`Entity\Glosa.cs`)
Disputa o devolución de cobro por parte de la EPS. Tiene `type` (1=Devolución, 2=Glosa), `transactdate` y `responsedate`.

### User (`Entity\User.cs`)
Usuario del sistema. Tiene `idprofile` (rol), `lSecurity` (lista de permisos), `otheruser` (usuarios auxiliares vinculados).

### Patient (`Entity\Patient.cs`)
Extiende `User` con datos del paciente: documento, tipo de documento, EPS del paciente.

### Cost / CostProcess / CostUser / CostReport (`Entity\Cost.cs`)
Entidades del módulo de costos. Manejan centros de costo, subcostros, distribución y reportes.

### Nomina / NominaStatus (`Entity\Nomina.cs`)
Registros de nómina e incapacidades del personal.

### Baul (`Entity\Baul.cs`)
Bóveda de credenciales cifradas para acceso a sistemas externos.

### Support (`Entity\Support.cs`)
Soporte documental asociado a un cargo (ej: autorización, historia clínica). Se registra en `soportecargo`. Tiene `observation` para los tipos 7 y 8.

### RelacionLog / RelacionEnvio
Observaciones de texto libre registradas al cambiar el estado de una relación. Tabla `relacionobservacion`.

### RegistroCargo (`Entity\RegistroCargo.cs`)
Entrada del historial de estados de un cargo. Una por cada cambio de estado. Tabla `cargolog`.

### Desmaterializacion (`Entity\Desmaterializacion.cs`)
Registro de facturas electrónicas enviadas al proceso de desmaterialización.

### Sms (`Entity\Sms.cs`)
Mensaje SMS con destinatario, plantilla y parámetros de envío.

### Employee (`Entity\Employee.cs`)
Empleado de la fundación, usado en módulo de nómina y costos.

### Generic (`Entity\Generic.cs`)
Contenedor genérico `{ id, code, name }` usado para poblar dropdowns y listas desde `FacadeGenerico`.

### PFP, Paquete, Sanitas, ACL
Entidades especializadas para integraciones con EPS específicas.

---

## Enumeraciones

### ChargeStatus — estados de un Cargo

| Valor | Nombre | Descripción visible |
|---|---|---|
| 1 | `incomplete` | Sin diligenciar y sin enviar a central de cuentas |
| 2 | `saved` | En caja con pendientes sin enviar a central de cuentas |
| 3 | `dispatched` | En caja sin recibir en central de cuentas |
| 4 | `recieved` | En central de cuentas recibido y auditado |
| 5 | `readytoinvoice` | Listo para facturar sin pendientes |
| 6 | `returned` | Devuelto a caja sin recibir |
| 7 | `recievedreturned` | Recibido devuelto de central de cuentas |
| 8 | `invoicedpending` | Con pendientes para facturar |
| 9 | `readytoinvoicepending` | Listo para facturar con pendientes |
| 10 | `intreatment` | En tratamiento |
| 11 | `intreatmentnoncentral` | En tratamiento sin recibir en central de cuentas |
| 12 | `intreatmentreturned` | En tratamiento devuelto |

Los textos visibles se definen en `Utils\Tools.GetStatus(int)`. Los estados 10–12 no tienen descripción aún en ese método.

### relationshipstatus — estado de RelacionEnvio (en código)

| Valor | Nombre |
|---|---|
| 0 | `none` |
| 1 | `sent` |
| 2 | `pending` |
| 3 | `completed` |

El estado real en BD se guarda como char: `S` (sin enviar), `P` (con pendientes), `E` (enviada total), `T` (tramitada).

### ProfileEnum — roles de usuario

| Valor | Nombre | Rol |
|---|---|---|
| 1 | `cashier` | Cajero |
| 2 | `invoicingaux` | Auxiliar de facturación |
| 3 | `director` | Director |
| 4 | `administrator` | Administrador |
| 5 | `healthcarecoordinator` | Coordinador asistencial |
| 6 | `rostercoordinator` | Coordinador de turnos |
| 7 | `educationcoordinator` | Coordinador de educación |
| 8 | `investigationcoordiator` | Coordinador de investigación |
| 10 | `rhbcashier` | Cajero RHB |
| 11 | `admincoordinator` | Coordinador administrativo |
| 12 | `adminaux` | Auxiliar administrativo |
| 13 | `pharmacycoordinator` | Coordinador de farmacia |

### Permissions — permisos granulares por funcionalidad

**Flujo de cargos (cajero):**
| Valor | Nombre | Habilita |
|---|---|---|
| 1 | `entrylist` | Ver `Listado.aspx` |
| 2 | `sendcentral` | Botón "Completos" — enviar a central |
| 11 | `supportsave` | Guardar soportes sin enviar |
| 3 | `returnreception` | Ver `RecibirDevolucion.aspx` |
| 4 | `returnresponse` | Tramitar devolución en `Devolucion.aspx` |
| 41 | `rhbcharges` | Ver solo cargos RHB (servicio 933500) |

**Flujo de cargos (facturador / central de cuentas):**
| Valor | Nombre | Habilita |
|---|---|---|
| 5 | `entryreception` | Ver `Central.aspx` — recibir cargos |
| 6 | `entryreturn` | Ver `Auditar.aspx` — devolver cargos |
| 7 | `sendreturn` | Confirmar devolución en `Auditar.aspx` |
| 25 | `readytoinvoice` | Marcar listo para facturar |

**Reportes:**
| Valor | Nombre |
|---|---|
| 8 | `supportpendingreport` |
| 9 | `pendingsendreport` |
| 10 | `returnreport` |
| 15 | `entryreport` |
| 16 | `chargesnotinvoicedreport` |
| 18 | `chargesnottransactedreport` |
| 19 | `chargestatusreport` |
| 20 | `invoicereport` |
| 21 | `logreport` |
| 23 | `chargescanceledreport` |
| 26 | `ripsreport` |
| 34 | `costreport` |

**Costos:**
| Valor | Nombre |
|---|---|
| 32 | `assigncost` |
| 33 | `savecost` |
| 35 | `generatecost` |
| 37 | `listcostmaster` |
| 38 | `editcost` |
| 39 | `viewspecialcost` |
| 40 | `addspecialcost` |
| 46 | `specialcostassign` |

**Relaciones / EPS:**
| Valor | Nombre |
|---|---|
| 42 | `relationshipgeneration` |
| 43 | `relationshiplist` |
| 44 | `relationshipvalidation` |
| 45 | `relationshipreport` |
| 48 | `compensardematerialize` |
| 47 | `programsinvoices` |

**Usuarios y otros:**
| Valor | Nombre |
|---|---|
| 12 | `listuser` |
| 13 | `createuser` |
| 14 | `edituser` |
| 29 | `createadmission` |
| 36 | `createhospadmission` |
| 49 | `passwordtrunk` |
| 50 | `pharmacylist` |
| 51 | `pharmacyedit` |

---

## Reglas de negocio del dominio

### Estados de Cargo
- Un cargo **solo existe en la tabla `cargo`** si fue tocado desde la UI. Los ingresos nuevos del HIS aparecen en `VCargos` pero con `ca_id = NULL` hasta que el cajero guarda soportes.
- Todo cambio de estado escribe atómicamente en `cargo` (UPDATE) y en `cargolog` (INSERT). Si falla uno, se hace rollback.
- No hay validación de transiciones permitidas — cualquier estado puede pasar a cualquier otro desde código.
- Los estados 10–12 (`intreatment*`) están definidos en el enum pero no tienen descripción en `GetStatus()` ni filas en `estadocargo`.

### Soportes de Cargo
- Al devolver un cargo, se debe seleccionar al menos un motivo (`motivodevolucion`). Sin motivo, el sistema bloquea la devolución.
- Los soportes del cajero (`soportecargo`) y los motivos del facturador (`motivocargo`) son tablas separadas.

### RelacionEnvio
- El estado global (`cestado`) se deriva de los flags individuales de `detallerelacion` (`dr_enviado`, `dr_recibido`).
- Las observaciones son opcionales; solo se insertan si `oRelacion.oLog != null`.

### Permisos
- Los permisos se asignan por perfil en BD y se cargan al login en `Session["oUser"].lSecurity`.
- `Tools.HaveAccess(lSecurity, permiso)` es el único punto de verificación — si devuelve `false`, el botón o página se oculta o redirige a `SinAcceso.aspx`.
- El perfil `cashier` (1) tiene restricción adicional en `Listado.aspx`: no puede cambiar el dropdown de usuario.
