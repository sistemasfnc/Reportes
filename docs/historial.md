# Historial de cambios

Registro cronológico de todos los requerimientos completados. Se agrega una entrada nueva al cerrar cada requerimiento (ver "INSTRUCCIONES DE MANTENIMIENTO" en `CLAUDE.md`).

### 2026-06-19 — Corrección de path de configuración para equipo nuevo

- **Qué se implementó:** ajuste del path hardcodeado que lee `Configuration.cs` para que apunte a `Config.dll.config` en la ruta del nuevo equipo de desarrollo.
- **Archivos modificados:** `Config\Configuration.cs`
- **Decisiones tomadas:** ninguna decisión de diseño; es un ajuste puntual de entorno. Se documentó como "zona de peligro" porque el path sigue hardcodeado (no leído desde variable de entorno ni relativo).
- **Pruebas escritas:** no aplica (no había proyecto de tests en ese momento).
- **Pendientes:** ninguno registrado.

### 2026-06-25 — Estados intermedios de tratamiento en el flujo de devoluciones

- **Qué se implementó:** se distinguió cuándo una devolución se hace por el motivo "En tratamiento Devuelto" (12) del resto de motivos, agregando los estados 11 (`intreatmentaudited`, redefinición de `intreatmentnoncentral`), 13 (`intreatmentpendingreception`) y 14 (`returnedpendingreception`). Se ajustaron los filtros de bandeja en `Central.aspx`, `Auditar.aspx` y `RecibirDevolucion.aspx` para que cada estado aparezca en la pantalla correcta, y se agregó el botón "En tratamiento auditado" en `Central.aspx`. Un fix posterior el mismo día aisló los estados 13/14 (vía `Cargo.bIncludeReturnFlow`) para que solo afectaran la bandeja "Recibir Cargos" y no arrastraran cargos a `CargosFCI.aspx`/`ReporteRHB.aspx`, que comparten el mismo método de consulta.
- **Archivos modificados:** `Entity\Generic.cs` (hoy `Entity\Cargo.cs`), `DAC\CargosDAC.cs`, `Utils\Tools.cs`, `Trazabilidad\Central.aspx(.cs)`, `Trazabilidad\Devolucion.aspx.cs`, `Trazabilidad\Auditar.aspx.cs`.
- **Decisiones tomadas que no son obvias:** no existe un mapeo centralizado estado→bandeja; cada página de "Cargos" filtra `ca_es_id` con su propio hardcode en `CargosDAC.cs`, así que agregar o reutilizar un estado obliga a revisar manualmente los cinco filtros (ver `docs/dominio.md` → "Visibilidad de cargos por bandeja"). Para el caso de `status == dispatched (3)` compartido entre `Central.aspx`, `CargosFCI.aspx` y `ReporteRHB.aspx`, se optó por un flag explícito (`Cargo.bIncludeReturnFlow`) en vez de inferir la página que llama a partir del valor de `status`.
- **Pruebas escritas:** no aplica (no había proyecto de tests en ese momento).
- **Pendientes:** ninguno registrado en el commit original. Nota post-cierre: el despliegue a producción de este requerimiento quedó incompleto por más de un mes (el `.aspx` de `Central.aspx` y el `DAC.dll` actualizado no llegaron a producción en el primer intento de publish) — ver `docs/decisiones.md` si aplica una entrada sobre el proceso de despliegue.

### 2026-07-24 — Ocultar motivo de devolución 11 ("Paciente en tratamiento") de las ventanas de selección

- **Qué se implementó:** se excluyó el motivo `mo_id = 11` ("Paciente en tratamiento") de la lista seleccionable en la ventana modal "Motivos de devolución", agregando `WHERE mo_id <> 11` a la consulta de `GenericDAC.GetReasonsData()`. Esta ventana es compartida por `Auditar.aspx` (botón "Devolver") y `ListoFacturar.aspx` (botón "Editar"/"Devolver"), así que el cambio la oculta en ambas a la vez.
- **Archivos modificados:** `DAC\GenericDAC.cs`.
- **Decisiones tomadas que no son obvias:** se optó por filtrar en el `SELECT` (no por `DELETE` de la fila en `motivodevolucion`) para no romper el `INNER JOIN` que usan `CargosDAC.GetReasonData()`/`GetReasonDataLog()` contra cargos históricos que ya usaron ese motivo, y para evitar un posible error de FK si `motivocargo.mc_mo_id` referencia `motivodevolucion.mo_id` (mismo patrón de riesgo que `estadocargo`/`CARGOESTADO_FK`, ver "Zonas de peligro" en `CLAUDE.md`). El motivo 11 no tenía ninguna lógica de código asociada (a diferencia del motivo 12, que sí dispara la transición a `intreatmentreturned`), así que ocultarlo no afecta transiciones de estado.
- **Pruebas escritas:** no aplica — el cambio vive enteramente en la capa `DAC` (acceso directo a Oracle), ya documentada en `docs/pruebas.md` → "Qué NO se prueba y por qué" como fuera del alcance de pruebas automatizadas.
- **Pendientes:** ninguno.
