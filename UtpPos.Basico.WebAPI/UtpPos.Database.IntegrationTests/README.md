# UTP POS Database Integration Tests

## Objetivo

Este proyecto contiene pruebas de integracion contra SQL Server para validar infraestructura de base de datos controlada.

El proyecto contiene smoke tests no destructivos:

- Apertura de conexion.
- Validacion de nombre de base de datos de pruebas.
- Existencia de tablas criticas.
- Existencia de stored procedures criticos.
- Existencia de table-valued parameter types criticos.

Tambien contiene pruebas funcionales P0 de terceros globales y relacion negocio-tercero. Esas pruebas crean datos con prefijo `TEST_` y los limpian por `RunId` al finalizar.

## Configuracion obligatoria

Estos tests requieren una base de datos de pruebas explicita mediante variable de entorno:

```powershell
$env:UTPPOS_TEST_SQL_CONNECTION_STRING="Server=localhost;Database=VENTAS_POS_TEST;User Id=...;Password=...;TrustServerCertificate=True;"
```

El proyecto no lee `appsettings.json`, no lee `appsettings.Development.json`, no usa `DefaultConnection` y no usa `UtpPosConnection`.

Si `UTPPOS_TEST_SQL_CONNECTION_STRING` no esta configurada, los tests se omiten con un mensaje claro.

## Restaurar BD test desde backup

Existe un backup local para preparar la base de datos de pruebas:

```text
UtpPos.Basico.WebAPI/Docs/database/bak/VENTAS-POS.bak
```

Aunque el archivo se llame `VENTAS-POS.bak`, nunca debe restaurarse como `VENTAS-POS` para pruebas. Restaurar siempre con un nombre que contenga `TEST`, por ejemplo:

```text
VENTAS_POS_TEST
```

El `.bak` no debe subirse al repositorio. La carpeta `Docs/database/bak` se conserva con `.gitkeep` y los backups estan ignorados por git.

Ejemplo con Windows Authentication:

```powershell
.\UtpPos.Basico.WebAPI\Docs\database\testing\restore-test-db.ps1 `
  -ServerInstance "localhost" `
  -DatabaseName "VENTAS_POS_TEST" `
  -BackupPath "C:\Users\UTP POS\Desktop\POS-pruebas-glo\UtpPos.Basico.WebAPI\Docs\database\bak\VENTAS-POS.bak"
```

Ejemplo con SQL Authentication:

```powershell
.\UtpPos.Basico.WebAPI\Docs\database\testing\restore-test-db.ps1 `
  -ServerInstance "localhost" `
  -DatabaseName "VENTAS_POS_TEST" `
  -BackupPath "C:\Users\UTP POS\Desktop\POS-pruebas-glo\UtpPos.Basico.WebAPI\Docs\database\bak\VENTAS-POS.bak" `
  -UseSqlAuth `
  -UserId "sa" `
  -Password "..."
```

Configurar variable con Windows Authentication:

```powershell
$env:UTPPOS_TEST_SQL_CONNECTION_STRING="Server=localhost;Database=VENTAS_POS_TEST;Trusted_Connection=True;TrustServerCertificate=True;"
```

O con SQL Authentication:

```powershell
$env:UTPPOS_TEST_SQL_CONNECTION_STRING="Server=localhost;Database=VENTAS_POS_TEST;User Id=sa;Password=***;TrustServerCertificate=True;"
```

## Seguridad

Nunca apuntar estos tests a `VENTAS-POS`.

Las pruebas abortan si la cadena apunta a una base de datos llamada exactamente `VENTAS-POS` o si el nombre de la base de datos no parece ser de pruebas.

El nombre de la base de datos debe contener un indicador como `TEST`, `_TEST`, `UtpPosTest` o `VENTAS_POS_TEST`.

## Comandos

Ejecutar tests:

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Database.IntegrationTests\UtpPos.Database.IntegrationTests.csproj"
```

Si la WebAPI esta corriendo y bloquea DLLs, usar un output temporal:

```powershell
dotnet test "UtpPos.Basico.WebAPI\UtpPos.Database.IntegrationTests\UtpPos.Database.IntegrationTests.csproj" -p:OutputPath="$env:TEMP\utppos-db-integration-tests-build\"
```
