# Flujos de negocio

## Flujo 1 — Tramitación de un cargo (flujo principal)

Este es el flujo central del sistema. Un ingreso hospitalario del HIS pasa por cajero y facturador hasta quedar listo para facturar.

```
CAJERO — Listado.aspx
  │
  ├─ [1] Busca ingreso → VCargos LEFT JOIN cargo (GetData en CargosDAC)
  │       Si ca_id = NULL: el cargo no ha sido tocado aún
  │
  ├─ [2] Clic en ícono Editar (RowCommand "Editar")
  │       → Carga modal "Soportes pendientes" (tabla soporte)
  │       → Si ca_id = 0: NO llamar GetSupports() (evitar cuelgue)
  │
  ├─ [3a] Guardar sin enviar (imbGuardar) → estado saved (2)
  │        → FacadeCargo.CreateCharge() → INSERT INTO cargo (si nuevo)
  │                                     → UPDATE cargo (si existe)
  │        → INSERT INTO cargolog (auditoría)
  │
  ├─ [3b] Guardar y enviar (imbEnviar) → estado dispatched (3)
  │        → mismo flujo que 3a pero con status = 3
  │
  └─ [4] Botón "Completos" (btnEnviar) — batch para múltiples cargos
          → selecciona checkboxes → estado dispatched (3)
          → FacadeCargo.ChangeStatus(lista) → UPDATE + cargolog por cada uno
          → Redirect a Listado.aspx (cargos enviados desaparecen de la lista)

FACTURADOR — Central.aspx
  │
  ├─ [5] Recibe cargos despachados (estado 3)
  │       → Puede marcar como recibido → estado recieved (4)
  │
  └─ [6] Puede devolver → va a Auditar.aspx

FACTURADOR — Auditar.aspx
  │
  ├─ [7] Ve cargos en estado recieved (4)
  │
  ├─ [8a] Devolver (RowCommand "Devolver")
  │        → Abre modal con lista de motivos (tabla motivodevolucion)
  │        → Debe seleccionar al menos un motivo (obligatorio)
  │        → SaveReasons() → INSERT INTO motivocargo
  │        → UpdateStatus() → estado returned (6) + INSERT cargolog
  │        → "Cargo devuelto al cajero correctamente"
  │
  └─ [8b] Marcar listo para facturar (imbFacturar)
           → estado readytoinvoice (5) + INSERT cargolog

CAJERO — RecibirDevolucion.aspx
  │
  └─ [9] Recibe cargo devuelto (estado 6 → 7 recievedreturned)

CAJERO — Devolucion.aspx
  │
  └─ [10] Tramita la devolución
           → Agrega respuesta a cada motivo (txtRespuesta)
           → UpdateReasonsResponse() → UPDATE motivocargo
           → Estado vuelve a recieved (4) si confirma
           → Estado queda en recievedreturned (7) si solo guarda
```

**Resultado final:** cargo en estado `readytoinvoice` (5) queda disponible para generación de factura.

---

## Flujo 2 — Creación de relación y envío a EPS

```
FACTURADOR — CrearRelacion.aspx
  │
  ├─ [1] Agrupa facturas listas → crea RelacionEnvio (estado S - sin enviar)
  │       → INSERT INTO relacionenvio
  │       → INSERT INTO detallerelacion por cada factura
  │
  ├─ [2] Marca facturas como enviadas
  │       → UPDATE detallerelacion SET dr_enviado = 1, dr_fechaenviado = SYSDATE
  │       → UPDATE relacionenvio SET re_estado = 'P' o 'E'
  │       → INSERT INTO relacionobservacion (opcional)
  │
  └─ [3] EPS devuelve / recibe
          → UPDATE detallerelacion SET dr_recibido = 1
          → UPDATE relacionenvio SET re_estado = 'T' (tramitada)
```

---

## Flujo 3 — Envío de archivo a EPS (batch)

```
Tarea programada ejecuta SendCompensarPDF.exe / SendElyonFile.exe
  │
  ├─ [1] Consulta registros pendientes de envío via Facade
  ├─ [2] Genera archivo (TXT delimitado / Excel / RIPS)
  ├─ [3] Sube archivo via WinSCP / FluentFTP / SSH.NET al servidor de la EPS
  └─ [4] Actualiza estado del registro en BD (enviado / procesado)
```

Ver `docs/batch.md` para detalle por EPS.

---

## Flujo 4 — Desmaterialización (factura electrónica)

```
Trazabilidad/DesmaterializaCompensar.aspx
  │
  ├─ [1] Selecciona facturas para desmaterializar
  │       (solo empresas configuradas en EmpresasDesmaterializacion del config)
  ├─ [2] FacadeDesmaterializacion genera el archivo
  ├─ [3] Sube a bucket S3 (AWSKey / AWSSecret en config)
  └─ [4] Registra en tabla desmaterializacion
```

---

## Flujo 5 — Login y control de acceso

```
Navegador → Login.aspx
  │
  ├─ [1] FacadeUser.Authenticate(usuario, SHA256(password))
  ├─ [2] Carga User con idprofile y lSecurity (lista de permisos)
  ├─ [3] Guarda en Session["oUser"]
  └─ [4] Redirect a página solicitada

En cada página:
  ├─ Page_Load verifica Tools.HaveAccess(oUser.lSecurity, Permissions.X)
  └─ Si false → Response.Redirect("~/SinAcceso.aspx")
```

---

## Flujo 6 — Generación y envío de SMS

```
WServices/Service1.svc → SendSMS(Sms oEntity)
  │
  ├─ [1] Lee plantilla desde SMSTemplate (ruta en config)
  ├─ [2] Reemplaza variables (nombre, fecha, hora)
  └─ [3] HTTP POST a SMSUrl (Infobip) con credenciales SMSUser/SMSPassword
```

---

## Flujos de error relevantes

### Cargo sin fila en tabla `cargo`
- **Trigger:** cajero hace clic en Editar sobre un ingreso nuevo del HIS
- **Síntoma:** página se cuelga sin mensaje de error
- **Causa:** `idcgharge = 0` → `GetSupports(0)` hace consulta que puede fallar silenciosamente dentro del UpdatePanel
- **Solución:** validar `idcgharge > 0` antes de llamar `GetSupports()`

### Tabla `estadocargo` inexistente
- **Trigger:** dropdown de estados en `Listado.aspx` no carga
- **Causa:** la tabla no existe en el ambiente de pruebas
- **Solución:** crear la tabla e insertar los estados, o verificar que existan los datos

### Config no encontrada
- **Trigger:** cualquier operación que lea de BD
- **Síntoma:** `DirectoryNotFoundException` al arrancar
- **Causa:** path hardcodeado en `Config\Configuration.cs` no existe en la máquina
- **Solución:** actualizar `sXml` en `Configuration.cs` con el path correcto
