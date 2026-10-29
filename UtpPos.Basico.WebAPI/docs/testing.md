# Testing

## Objetivo

Este documento resume la estrategia actual de pruebas del backend, cómo ejecutarlas, qué valida cada suite y qué queda pendiente para una fase posterior.

La estrategia está separada por responsabilidades. No todos los proyectos de pruebas validan las mismas capas ni tienen el mismo propósito.

Actualmente existen tres niveles principales:

1. **Unit Tests**  
   Validan reglas de negocio, Application, Domain, Auth y RBAC sin infraestructura externa.

2. **WebAPI Pipeline / Component Tests**  
   Validan el pipeline HTTP real de la WebAPI usando fakes para aislar autenticación, autorización, rutas, permisos y guards de negocio.

3. **Database Integration Tests**  
   Validan integración real contra SQL Server de pruebas usando una base aislada llamada `VENTAS_POS_TEST`.

Las pruebas **End-to-End completas API + Base de datos** no están implementadas todavía y quedan para una fase posterior.

---

## Proyectos

| Proyecto | Tipo | Usa BD real | Estado |
|---|---|---:|---|
| `UtpPos.Backend.Tests` | Unit tests de Application, Auth/RBAC y Domain funcional | No | Activo |
| `UtpPos.WebAPI.IntegrationTests` | WebAPI pipeline/component tests con fakes | No | Activo |
| `UtpPos.Database.IntegrationTests` | Integration tests contra SQL Server test controlado | Sí, solo BD test | Activo para smoke tests |

---

# Tipos de pruebas

## 1. Unit Tests

### Proyecto

```text
UtpPos.Basico.WebAPI/UtpPos.Backend.Tests
```

### Objetivo

Validar reglas de negocio y comportamiento interno de Application, Domain, Auth y RBAC sin depender de infraestructura externa.

Estos tests no usan:

- SQL Server
- Dapper
- Stored procedures
- WebApplicationFactory
- HTTP real
- Contenedor Docker
- Archivos `appsettings.json`

### Cobertura actual

| Área | Cobertura |
|---|---|
| Auth | `AuthApplication` |
| RBAC | `RbacApplication.UserHasPermissionAsync` |
| Authorization | `PermissionAuthorizationHandler`, `PermissionAuthorizationPolicyProvider` |
| Domain | `ThirdPartyDomain`, `ProductDomain`, `PurchaseDomain`, `SaleDomain` |
| Dashboard | `DashboardApplication` validations, global business resolution, `BusinessId`, date range and `top` normalization |

### Comando

Desde la raíz del repositorio:

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Backend.Tests\UtpPos.Backend.Tests.csproj"
```

### Estado actual conocido

```text
115 passed
0 failed
0 skipped
```

---

## 2. WebAPI Pipeline / Component Tests

### Proyecto

```text
UtpPos.Basico.WebAPI/UtpPos.WebAPI.IntegrationTests
```

### Objetivo

Validar el pipeline HTTP de la WebAPI con dependencias controladas mediante fakes.

Estos tests prueban la integración entre:

```text
Request HTTP
-> Routing
-> Authentication
-> [Authorize]
-> [RequirePermission]
-> Claims
-> Controller
-> Fake Application/Repository
-> Response HTTP
```

### Qué sí validan

- Routing real de ASP.NET Core.
- `[Authorize]`.
- `[RequirePermission]`.
- Claims del usuario autenticado.
- Permisos RBAC.
- Guards por negocio.
- Respuestas HTTP esperadas:
  - `401 Unauthorized`
  - `403 Forbidden`
  - `200 OK`
  - `201 Created`
  - otros códigos según endpoint.
- Comportamiento de endpoints frente a permisos.

### Qué no validan

Estos tests no validan:

- SQL Server.
- Dapper.
- Stored procedures.
- Repositorios reales.
- Transacciones de base de datos.
- Datos persistidos.
- Integración completa API + BD.

### Por qué usan fakes

Estos tests usan fakes porque su propósito no es probar la base de datos, sino aislar y validar el comportamiento del pipeline HTTP de la WebAPI.

La integración real con SQL Server se valida en el proyecto:

```text
UtpPos.Database.IntegrationTests
```

Por lo tanto, estos tests no deben venderse como pruebas end-to-end completas. Son pruebas de integración del pipeline WebAPI con dependencias controladas.

### Infraestructura

| Componente | Función |
|---|---|
| `TestAuthHandler` | Simula usuarios autenticados mediante headers de prueba |
| `FakeRbacApplication` | Autoriza permisos declarados en `X-Test-Permissions` |
| `CustomWebApplicationFactory` | Levanta la WebAPI en memoria y reemplaza servicios reales por fakes |
| Fakes de Application | Devuelven respuestas controladas para endpoints bajo prueba |

### Headers de prueba

Los tests pueden simular usuarios mediante headers como:

| Header | Propósito |
|---|---|
| `X-Test-UserId` | Identificador del usuario autenticado |
| `X-Test-Email` | Correo del usuario |
| `X-Test-Name` | Nombre del usuario |
| `X-Test-SessionId` | Identificador de sesión |
| `X-Test-BusinessId` | Negocio asociado al usuario |
| `X-Test-Permissions` | Permisos disponibles para el usuario |

Si no se envía `X-Test-UserId`, el usuario se considera no autenticado y los endpoints protegidos deben responder `401 Unauthorized`.

### Cobertura actual

| Módulo | Casos |
|---|---|
| Terceros | `401` sin usuario, `403` sin permiso, `200` con permiso, permiso `terceros-global.listar`, permisos operativos por negocio, guard de `BusinessId`, asociar/inactivar relación |
| Productos | `401` sin usuario, `403` sin permiso, `200` con permiso, permisos `productos.listar` y `productos.crear`, paso de filtro `BusinessId` al Application fake |
| Dashboard | `401` sin usuario, `400` global sin negocio seleccionado, `403` sin permiso, `200` con permiso, permisos `dashboard.ver`, `dashboard-finanzas.ver`, `dashboard-global.ver`, `BusinessId` desde claim `usr_neg_id` o selección global validada |

### Permisos relevantes

| Módulo | Permisos |
|---|---|
| Terceros globales | `terceros-global.listar` |
| Terceros por negocio | `terceros.listar`, `terceros.crear`, `terceros.editar`, según endpoint |
| Productos | `productos.listar`, `productos.crear`, según endpoint |
| Dashboard | `dashboard.ver`, `dashboard-finanzas.ver`, `dashboard-global.ver` |

> Nota: el permiso correcto para catálogo global de terceros es `terceros-global.listar`, con guion. No usar `terceros.global.listar`.
> Nota: el permiso financiero correcto para Dashboard es `dashboard-finanzas.ver`, con guion. No usar `dashboard.finanzas.ver` ni `dashboard-finanzas-ver`.
> Nota: el permiso global correcto para Dashboard es `dashboard-global.ver`, con guion en el módulo. No usar `dashboard.global.ver` ni `dashboard-global-ver`.

### Comando

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.WebAPI.IntegrationTests\UtpPos.WebAPI.IntegrationTests.csproj"
```

### Si la WebAPI está corriendo y bloquea DLLs

Usar un `OutputPath` temporal:

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.WebAPI.IntegrationTests\UtpPos.WebAPI.IntegrationTests.csproj" `
  -p:OutputPath="$env:TEMP\utppos-webapi-integration-tests-build\"
```

### Estado actual conocido

```text
33 passed
0 failed
0 skipped
```

---

## 3. Database Integration Tests

### Proyecto

```text
UtpPos.Basico.WebAPI/UtpPos.Database.IntegrationTests
```

### Objetivo

Validar la integración real contra SQL Server de pruebas.

Estos tests sí usan una base de datos real y controlada:

```text
VENTAS_POS_TEST
```

### Qué sí validan

Actualmente validan:

- Conexión real a SQL Server.
- Existencia de una BD de pruebas explícita.
- Guardas para evitar ejecutar contra BD productiva/desarrollo.
- Existencia de tablas críticas.
- Existencia de stored procedures críticos.
- Existencia de table-valued parameters críticos, si aplica.

En fases posteriores deben validar también:

- Repositorios Dapper reales.
- Ejecución de stored procedures.
- Inserciones, consultas y actualizaciones reales.
- Reglas persistidas en base de datos.
- Efectos sobre inventario, kardex, compras y ventas.

### Qué no validan todavía

Actualmente no son pruebas end-to-end completas API + BD.

No validan todavía el flujo completo:

```text
HTTP
→ WebAPI
→ Application
→ Domain
→ Repository
→ SQL Server
→ Response
```

Ese tipo de prueba queda para una fase posterior.

---

# Configuración de Database Integration Tests

## Variable obligatoria

Los tests de base de datos solo se ejecutan si existe la variable:

```text
UTPPOS_TEST_SQL_CONNECTION_STRING
```

Si la variable no existe, los tests se omiten con un mensaje claro.

---

## Configuración para SQL Server Docker

Este es el flujo principal usado en desarrollo.

```powershell
$env:UTPPOS_TEST_SQL_CONNECTION_STRING="Server=localhost,1433;Database=VENTAS_POS_TEST;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"
```

> No usar `Trusted_Connection=True` contra SQL Server Docker.  
> Docker debe usar SQL Authentication con `User Id=sa`.

---

## Configuración para SQL Server local con Windows Authentication

Solo aplica si se usa una instancia local de SQL Server instalada en Windows, no Docker.

```powershell
$env:UTPPOS_TEST_SQL_CONNECTION_STRING="Server=localhost;Database=VENTAS_POS_TEST;Trusted_Connection=True;TrustServerCertificate=True;"
```

---

## Reglas de seguridad

| Regla | Detalle |
|---|---|
| No `appsettings` | Los DB integration tests no leen `appsettings.json` ni `appsettings.Development.json` |
| No fallback | No usan `DefaultConnection` ni `UtpPosConnection` |
| BD test obligatoria | El nombre de la BD debe contener `TEST` |
| BD prohibida | Se aborta si la BD es `VENTAS-POS` o `VENTAS_POS` |
| Sin password en logs | No se debe imprimir la contraseña real |
| SQL Server controlado | La BD debe ser aislada de desarrollo y producción |

---

## Comando

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Database.IntegrationTests\UtpPos.Database.IntegrationTests.csproj"
```

### Si la WebAPI está corriendo y bloquea DLLs

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Database.IntegrationTests\UtpPos.Database.IntegrationTests.csproj" `
  -p:OutputPath="$env:TEMP\utppos-db-integration-tests-build\"
```

### Solo smoke tests

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Database.IntegrationTests\UtpPos.Database.IntegrationTests.csproj" `
  --filter "FullyQualifiedName~DatabaseSmokeTests" `
  -p:OutputPath="$env:TEMP\utppos-db-integration-tests-build\"
```

### Estado actual conocido

```text
DB smoke tests:
5 passed
0 failed
0 skipped
```

---

# SQL Server Docker

## Base esperada

La BD de pruebas debe llamarse:

```text
VENTAS_POS_TEST
```

La BD de desarrollo puede llamarse:

```text
VENTAS-POS
```

Pero los tests nunca deben ejecutarse directamente contra:

```text
VENTAS-POS
```

---

## Ver contenedores

Desde PowerShell:

```powershell
docker ps
```

El contenedor esperado es similar a:

```text
sqlserver
```

---

## Entrar al contenedor

```powershell
docker exec -it sqlserver /bin/bash
```

Dentro del contenedor, usar `clear` para limpiar pantalla. No usar `cls`.

---

## Guardar password de `sa` dentro del contenedor

Dentro del contenedor:

```bash
read -s -p "Password sa: " SAPASS
echo
```

---

## Probar conexión dentro del contenedor

```bash
/opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$SAPASS" \
  -C \
  -Q "SELECT @@SERVERNAME AS ServerName, name FROM sys.databases WHERE name IN ('VENTAS-POS', 'VENTAS_POS_TEST');"
```

---

## Validar objetos críticos en `VENTAS-POS`

```bash
/opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$SAPASS" \
  -C \
  -Q "
USE [VENTAS-POS];

SELECT name
FROM sys.tables
WHERE name IN ('TBL_TERCEROS', 'TBL_NEGOCIO_TERCEROS');

SELECT name
FROM sys.objects
WHERE type = 'P'
AND name IN (
    'UspAsociaTerceroNegocio',
    'UspListaTercerosPorNegocio',
    'UspValidaTerceroNegocio',
    'UspInactivaTerceroNegocio'
);

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TBL_TERCEROS'
AND COLUMN_NAME IN ('TER_NEG_ID', 'TER_NUMERO_DOCUMENTO', 'TER_NUM_DOCUMENTO');
"
```

Resultado esperado en `VENTAS-POS` actualizado:

```text
TBL_NEGOCIO_TERCEROS
TBL_TERCEROS

UspAsociaTerceroNegocio
UspInactivaTerceroNegocio
UspListaTercerosPorNegocio
UspValidaTerceroNegocio

TER_NUMERO_DOCUMENTO
```

No debería aparecer:

```text
TER_NEG_ID
```

---

## Validar objetos críticos en `VENTAS_POS_TEST`

```bash
/opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$SAPASS" \
  -C \
  -Q "
USE [VENTAS_POS_TEST];

SELECT name
FROM sys.tables
WHERE name IN ('TBL_TERCEROS', 'TBL_NEGOCIO_TERCEROS');

SELECT name
FROM sys.objects
WHERE type = 'P'
AND name IN (
    'UspAsociaTerceroNegocio',
    'UspListaTercerosPorNegocio',
    'UspValidaTerceroNegocio',
    'UspInactivaTerceroNegocio'
);

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TBL_TERCEROS'
AND COLUMN_NAME IN ('TER_NEG_ID', 'TER_NUMERO_DOCUMENTO', 'TER_NUM_DOCUMENTO');
"
```

Resultado esperado en `VENTAS_POS_TEST` actualizado:

```text
TBL_NEGOCIO_TERCEROS
TBL_TERCEROS

UspAsociaTerceroNegocio
UspInactivaTerceroNegocio
UspListaTercerosPorNegocio
UspValidaTerceroNegocio

TER_NUMERO_DOCUMENTO
```

---

# Preparar `VENTAS_POS_TEST` desde Docker

## Crear backup actualizado desde `VENTAS-POS`

Dentro del contenedor:

```bash
cat > /tmp/backup-current.sql <<'SQL'
BACKUP DATABASE [VENTAS-POS]
TO DISK = N'/var/opt/mssql/backup/VENTAS-POS-current.bak'
WITH INIT, STATS = 5;
GO
SQL
```

Ejecutar:

```bash
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SAPASS" -C -b -i /tmp/backup-current.sql
```

---

## Verificar logical names del backup

```bash
/opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$SAPASS" \
  -C \
  -Q "RESTORE FILELISTONLY FROM DISK = N'/var/opt/mssql/backup/VENTAS-POS-current.bak';"
```

Logical names esperados:

```text
POS-VENTAS
POS-VENTAS_log
```

---

## Restaurar backup sobre `VENTAS_POS_TEST`

Dentro del contenedor:

```bash
cat > /tmp/restore-test.sql <<'SQL'
IF DB_ID(N'VENTAS_POS_TEST') IS NOT NULL
BEGIN
    ALTER DATABASE [VENTAS_POS_TEST] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END;
GO

RESTORE DATABASE [VENTAS_POS_TEST]
FROM DISK = N'/var/opt/mssql/backup/VENTAS-POS-current.bak'
WITH
    MOVE N'POS-VENTAS' TO N'/var/opt/mssql/data/VENTAS_POS_TEST.mdf',
    MOVE N'POS-VENTAS_log' TO N'/var/opt/mssql/data/VENTAS_POS_TEST_log.ldf',
    REPLACE,
    RECOVERY,
    STATS = 5;
GO

ALTER DATABASE [VENTAS_POS_TEST] SET MULTI_USER;
GO
SQL
```

Ejecutar:

```bash
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SAPASS" -C -b -i /tmp/restore-test.sql
```

---

## Validar que `VENTAS_POS_TEST` quedó actualizada

```bash
cat > /tmp/validate-test.sql <<'SQL'
USE [VENTAS_POS_TEST];
GO

SELECT name
FROM sys.tables
WHERE name IN ('TBL_TERCEROS', 'TBL_NEGOCIO_TERCEROS');

SELECT name
FROM sys.objects
WHERE type = 'P'
AND name IN (
    'UspAsociaTerceroNegocio',
    'UspListaTercerosPorNegocio',
    'UspValidaTerceroNegocio',
    'UspInactivaTerceroNegocio'
);

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TBL_TERCEROS'
AND COLUMN_NAME IN ('TER_NEG_ID', 'TER_NUMERO_DOCUMENTO', 'TER_NUM_DOCUMENTO');
SQL
```

Ejecutar:

```bash
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SAPASS" -C -b -i /tmp/validate-test.sql
```

---

# Backup local

El backup local documentado para preparar una BD test puede estar en:

```text
UtpPos.Basico.WebAPI/Docs/database/bak/VENTAS-POS.bak
```

Ese archivo no debe subirse al repositorio.

Agregar o mantener en `.gitignore`:

```gitignore
# SQL Server backups
*.bak
*.trn
*.dif

# Local database backups
UtpPos.Basico.WebAPI/Docs/database/bak/*
!UtpPos.Basico.WebAPI/Docs/database/bak/.gitkeep
```

Si el `.bak` fue agregado al staging por error:

```powershell
git restore --staged "UtpPos.Basico.WebAPI/Docs/database/bak/VENTAS-POS.bak"
```

Si ya quedó trackeado:

```powershell
git rm --cached "UtpPos.Basico.WebAPI/Docs/database/bak/VENTAS-POS.bak"
```

---

# Script de restore PowerShell

Existe un script de restore manual:

```text
UtpPos.Basico.WebAPI/Docs/database/testing/restore-test-db.ps1
```

Este script es útil principalmente para restaurar desde rutas visibles por PowerShell/Windows o por una instancia local de SQL Server.

## Restore con Windows Authentication

```powershell
.\UtpPos.Basico.WebAPI\Docs\database\testing\restore-test-db.ps1 `
  -ServerInstance "localhost" `
  -DatabaseName "VENTAS_POS_TEST" `
  -BackupPath "C:\SqlBackups\VENTAS-POS.bak"
```

## Restore con SQL Auth

```powershell
.\UtpPos.Basico.WebAPI\Docs\database\testing\restore-test-db.ps1 `
  -ServerInstance "localhost,1433" `
  -DatabaseName "VENTAS_POS_TEST" `
  -BackupPath "C:\SqlBackups\VENTAS-POS.bak" `
  -UseSqlAuth `
  -UserId "sa" `
  -Password "TU_PASSWORD"
```

## Nota para Docker

Cuando SQL Server corre en Docker, las rutas como:

```text
/var/opt/mssql/backup/VENTAS-POS.bak
```

existen dentro del contenedor, no en Windows.

Por eso, para Docker es más seguro ejecutar backup/restore directamente dentro del contenedor con `sqlcmd`, como se documenta en la sección **Preparar `VENTAS_POS_TEST` desde Docker**.

---

# End-to-End API + Database Tests

Actualmente no implementado.

Este tipo de prueba validaría el flujo completo:

```text
HTTP Request
→ WebAPI real
→ Application real
→ Domain real
→ Repository real
→ SQL Server test real
→ HTTP Response
```

## Por qué queda para fase posterior

Este tipo de prueba requiere mayor control de:

- Datos semilla.
- Limpieza de datos.
- Orden de ejecución.
- Transacciones.
- Aislamiento entre tests.
- Estados iniciales de inventario.
- Restauración de BD o rollback por test.
- Tiempo de ejecución.
- Estabilidad del ambiente.

Por eso actualmente se separa la estrategia:

| Pregunta | Suite |
|---|---|
| ¿La regla de negocio funciona? | Unit tests |
| ¿La WebAPI protege y enruta bien? | WebAPI pipeline/component tests |
| ¿La BD y los SPs existen y responden? | Database integration tests |
| ¿Todo el sistema funciona junto de punta a punta? | E2E API + DB, fase posterior |

---

# Estado actual

| Suite | Último estado conocido |
|---|---|
| Unit tests backend | `104 passed` |
| WebAPI pipeline/component tests | `21 passed` |
| DB smoke tests | `5 passed` cuando `VENTAS_POS_TEST` está alineada con el schema actual |

---

# Pendientes Fase 2

| Prioridad | Pendiente |
|---|---|
| P0 | Mantener `VENTAS_POS_TEST` alineada con el schema actual |
| P0 | Ejecutar DB smoke tests antes de cualquier prueba funcional contra SQL Server |
| P0 | Completar pruebas DB funcionales de terceros globales + `TBL_NEGOCIO_TERCEROS` |
| P1 | Pruebas DB de productos y `PROD_VALOR_VENTA` |
| P1 | Pruebas DB de compras, inventario, Kardex y costo ponderado |
| P1 | Pruebas DB de ventas, descuento de inventario y bloqueos |
| P1 | Definir estrategia para E2E API + DB |
| P1 | Extender WebAPI pipeline/component tests a más módulos con fakes, sin BD real |
| P2 | Revisar vulnerabilidad de `AutoMapper 9.0.0` reportada por NuGet |
| P2 | Revisar impacto de licencia de FluentAssertions si el proyecto se usa comercialmente |

---

# Comandos rápidos

## Unit tests

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Backend.Tests\UtpPos.Backend.Tests.csproj"
```

## WebAPI pipeline/component tests sin BD

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.WebAPI.IntegrationTests\UtpPos.WebAPI.IntegrationTests.csproj" `
  -p:OutputPath="$env:TEMP\utppos-webapi-integration-tests-build\"
```

## Configurar DB integration contra Docker

```powershell
$env:UTPPOS_TEST_SQL_CONNECTION_STRING="Server=localhost,1433;Database=VENTAS_POS_TEST;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"
```

## DB smoke tests

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Database.IntegrationTests\UtpPos.Database.IntegrationTests.csproj" `
  --filter "FullyQualifiedName~DatabaseSmokeTests" `
  -p:OutputPath="$env:TEMP\utppos-db-integration-tests-build\"
```

## Todos los DB integration tests

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Database.IntegrationTests\UtpPos.Database.IntegrationTests.csproj" `
  -p:OutputPath="$env:TEMP\utppos-db-integration-tests-build\"
```

## Correr Unit + WebAPI sin BD

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Backend.Tests\UtpPos.Backend.Tests.csproj"

dotnet test "UtpPos.Basico.WebAPI\UtpPos.WebAPI.IntegrationTests\UtpPos.WebAPI.IntegrationTests.csproj" `
  -p:OutputPath="$env:TEMP\utppos-webapi-integration-tests-build\"
```

---

# Recomendación para la entrega actual

Para la primera entrega del software, la prioridad recomendada es:

1. Mantener unit tests en verde.
2. Mantener WebAPI pipeline/component tests en verde.
3. Mantener DB smoke tests en verde.
4. Documentar claramente que las pruebas E2E API + BD quedan para fase posterior.
5. No profundizar todavía en pruebas funcionales de BD si hay pendientes funcionales del software.

La infraestructura de integración con base de datos ya quedó iniciada y validada con smoke tests. La ampliación a repositorios reales por módulo debe manejarse como fase posterior.
