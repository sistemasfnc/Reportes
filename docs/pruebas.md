# Pruebas

## Estado actual

**Sin tests.** No existe ningún proyecto de pruebas unitarias en `Reportes.sln`. La validación histórica ha sido manual, vía IIS o IIS Express, requerimiento por requerimiento.

Desde 2026-07-08 se adoptó como comportamiento estándar proponer/escribir pruebas unitarias para todo cambio de lógica de negocio (ver "INSTRUCCIONES DE MANTENIMIENTO" → punto 4 en `CLAUDE.md`). Como no existe proyecto de tests todavía, el primer requerimiento que aplique esta regla debe crear uno.

## Herramientas recomendadas

- **Framework de tests:** MSTest o NUnit (cualquiera se integra bien con .NET Framework 4.8 y MSBuild sin cambios de infraestructura). Definir cuál se usa la primera vez que se cree el proyecto de tests, y mantener consistencia después.
- **Mocking:** Moq — para aislar `DAC`/`Facade` de dependencias externas reales (Oracle, SFTP, SMS, correo) sin mockear clases propias del dominio.
- **Ejecución:** `vstest.console.exe` o el Test Explorer de Visual Studio; no hay integración CI configurada en este repo.

## Convención de nombres

`NombreMetodo_Escenario_ResultadoEsperado` — por ejemplo: `GetCharges_EstadoDoce_NoAparecEnCentral`.

## Qué se prueba en este proyecto

(vacío — se llena a medida que se cierren requerimientos que agreguen pruebas)

## Qué NO se prueba y por qué

- **Capa de acceso a datos directa a Oracle/SQL Server/FoxPro (`DAC/`)** — no hay tests de integración contra bases de datos reales; el ambiente de pruebas (`OnProduction = False`, SIDs `PRUTRAZA`/`PRUINTEG`/`PRUFNEUM`) se usa para validación manual en su lugar.
- **Envío real a EPS/SFTP/SMS (`SendCompensarPDF`, `SendElyonFile`, `SendMessage`, etc.)** — dependen de credenciales y servidores externos reales; no hay entorno de sandbox para estos servicios.
- **Páginas ASPX (code-behind WebForms)** — sin un patrón MVC/MVVM que separe presentación de lógica, probar el code-behind directamente requiere levantar el pipeline de ASP.NET; se prueba manualmente vía IIS Express.

## Historial de cobertura

- **2026-07-24** — Sin pruebas nuevas. Cambio: filtro `WHERE mo_id <> 11` en `GenericDAC.GetReasonsData()` para ocultar un motivo de devolución. No se escribió prueba porque cae enteramente en la capa `DAC` de acceso directo a Oracle, ya excluida arriba en "Qué NO se prueba y por qué".
- **2026-07-24** — Sin pruebas nuevas. Cambio: 6 códigos de empresa nuevos en `Trazabilidad\clases\Facturacion2885.GetInvoiceName()`. A diferencia del caso anterior, esta sí es lógica pura testeable sin mocks; se decidió con el usuario diferir la creación del primer proyecto de tests de la solución. **Pendiente:** cuando se cree, cubrir `GetInvoiceName()` para los códigos 277-282 (caso feliz, heredan formato de su empresa de referencia) y el caso `else` genérico (incluye 27/281).
