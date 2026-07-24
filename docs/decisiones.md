# Decisiones técnicas no obvias

### Flag explícito `Cargo.bIncludeReturnFlow` en vez de inferir la página por `status`

- **Contexto:** el filtro `ca_es_id IN (2, 3, 13, 14)` para `status = dispatched (3)` lo comparten `Central.aspx`, `CargosFCI.aspx` y `ReporteRHB.aspx` a través del mismo método de consulta en `CargosDAC.cs`. Al agregar los estados 13/14 al flujo de devoluciones (2026-06-25), los dos reportes empezaron a traer cargos en esos estados sin que se hubiera pedido, porque el método no tenía forma de distinguir quién lo estaba llamando.
- **Decisión tomada:** agregar la propiedad `Cargo.bIncludeReturnFlow` (`Entity\Cargo.cs`). Solo `Central.aspx.cs` la pone en `true` al armar el filtro; el resto de los llamadores la dejan en su valor por defecto (`false`), preservando el comportamiento original `IN (2, 3)`.
- **Alternativas descartadas:** identificar la página que llama a partir del valor de `status` u otro dato ya presente en `Cargo` — se descartó porque `status` es el mismo valor (3) para los tres llamadores; no hay forma de diferenciarlos sin un flag nuevo.
- **Consecuencias:** cualquier filtro futuro de tipo "mismo estado, distinto significado según la página" debe repetir este patrón (un flag explícito en `Cargo`), no asumir que el valor de `status` identifica al llamador. Ver `docs/dominio.md` → "Visibilidad de cargos por bandeja".

### Path de configuración hardcodeado en `Configuration.cs`

- **Contexto:** `Config\Configuration.cs` necesita ubicar `Config.dll.config` para leer todas las credenciales de BD/FTP/SMS/AWS del sistema. Esto ya causó al menos dos incidentes de migración de equipo (2026-06-19 y julio 2026) donde la aplicación fallaba al arrancar hasta corregir el path a mano.
- **Decisión tomada:** mantener el path como constante hardcodeada en el código en vez de leerlo de una variable de entorno o de un archivo de configuración relativo al ensamblado.
- **Alternativas descartadas:** no hay evidencia en el repo de que se haya evaluado una alternativa (variable de entorno, `AppDomain.CurrentDomain.BaseDirectory`, etc.); se documenta como deuda técnica conocida, no como decisión deliberada con trade-offs evaluados.
- **Consecuencias:** cada vez que el proyecto se mueve a un equipo o carpeta nueva, hay que actualizar `Config\Configuration.cs` a mano antes de poder compilar o correr la aplicación. Ver "Zonas de peligro" en `CLAUDE.md`.

### Bloqueo NTFS (Zone.Identifier) al copiar el repo a un equipo nuevo

- **Contexto:** el 2026-07-03, tras mover el proyecto a un equipo nuevo, el publish de Visual Studio empezó a fallar con `FileLoadException` / HRESULT `0x80131515` sobre un DLL del `bin` (`AjaxControlToolkit.dll`), aunque el proyecto compilaba y corría bien en IIS Express. La causa fue que Windows marcó todos los archivos del repo con el stream NTFS `Zone.Identifier` (Mark of the Web) al copiarlos desde red/zip.
- **Decisión tomada:** documentar el diagnóstico y la solución (`Get-ChildItem -Recurse -File | Unblock-File` sobre la raíz del repo) en vez de cambiar el proceso de copia/distribución del código.
- **Alternativas descartadas:** ninguna evaluada; es un efecto secundario de Windows al copiar archivos de un origen no confiable, no un problema del proyecto en sí.
- **Consecuencias:** repetir el `Unblock-File` cada vez que el repo se copie o mueva a una máquina o carpeta nueva. Ver "Zonas de peligro" en `CLAUDE.md`.

### Ocultar un motivo de devolución filtrando el SELECT en vez de borrar la fila

- **Contexto:** se pidió quitar el motivo `mo_id = 11` ("Paciente en tratamiento") de la ventana "Motivos de devolución". Esa ventana es alimentada por `GenericDAC.GetReasonsData()` (`SELECT mo_id, mo_nombre FROM motivodevolucion`), consumida a la vez por `Auditar.aspx` y `ListoFacturar.aspx`, mientras que el historial de motivos ya usados por un cargo (`CargosDAC.GetReasonData()`/`GetReasonDataLog()`) hace un `INNER JOIN` contra la misma tabla `motivodevolucion`.
- **Decisión tomada:** agregar `WHERE mo_id <> 11` a `GetReasonsData()` en vez de eliminar la fila de `motivodevolucion`.
- **Alternativas descartadas:** `DELETE FROM motivodevolucion WHERE mo_id = 11` — se descartó porque cualquier cargo histórico con ese motivo ya guardado en `motivocargo`/`motivocargolog` dejaría de aparecer en las pantallas de historial (el `INNER JOIN` lo filtraría en silencio), y porque es probable que exista una FK `motivocargo.mc_mo_id` → `motivodevolucion.mo_id` que rompería el `DELETE` con un error de integridad referencial (mismo patrón de riesgo que `estadocargo`/`CARGOESTADO_FK`).
- **Consecuencias:** para "retirar" un motivo de devolución de la selección, filtrar en el `SELECT` de `GetReasonsData()` (o agregar un flag `activo` a `motivodevolucion` si se necesita administrar esto desde UI), nunca borrar la fila. Como `GetReasonsData()` es compartido, cualquier filtro ahí afecta a la vez a `Auditar.aspx` y `ListoFacturar.aspx`; si en el futuro se necesita ocultar un motivo en una sola de las dos pantallas, el filtro debe aplicarse en el code-behind de esa página puntual, no en el DAC compartido.
