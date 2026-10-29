# UTP POS

Sistema de punto de venta (ventas, compras, inventario, kardex, terceros, usuarios con roles y permisos, dashboard).

| Parte | Tecnología | Carpeta |
|---|---|---|
| **API (backend)** | ASP.NET Core Web API, .NET 6, Dapper, SQL Server | `UtpPos.Basico.WebAPI/` |
| **Frontend** | Blazor (MudBlazor), .NET 8 | `UtpPos.Basico.MudBlazorWeb/` |
| **Base de datos** | SQL Server 2022 en Docker | `UtpPos.Basico.WebAPI/docker/` y `database/` |

---

## 1. Puesta en marcha (≈ 15 minutos)

### Requisitos

- [Git](https://git-scm.com/), [Docker Desktop](https://www.docker.com/products/docker-desktop/) (encendido) y PowerShell.
- **.NET 8 SDK** y **.NET 6 Runtime** (la API es `net6.0`, el frontend `net8.0`). Verifica con `dotnet --list-sdks` y `dotnet --list-runtimes`.
- Un IDE: Visual Studio 2022, Rider o VS Code.

### Paso a paso

**1. Clona el repo y consigue el `.bak`.**
La base de datos no está en git (por tamaño y por contener datos de ejemplo). Pídele al equipo el archivo `UtpPos-VENTAS-POS.bak` y cópialo en:

```
UtpPos.Basico.WebAPI/Docs/database/bak/UtpPos-VENTAS-POS.bak
```

**2. Levanta SQL Server y restaura la BD.**

```powershell
cd UtpPos.Basico.WebAPI\docker
copy .env.example .env          # opcional: cambia la clave de sa
.\restore-db.ps1
```

Qué hace: levanta el contenedor `utppos-sqlserver` (solo accesible desde tu máquina, puerto **14335**) y restaura la BD `VENTAS-POS`. Es seguro repetirlo: restaura de nuevo desde cero.

El `.bak` trae datos **ficticios y anonimizados**. Usuarios de ejemplo (clave de todos: **`UtpPos#Dev2026`**):

| Correo | Rol |
|---|---|
| `admin.global@utppos.local` | Administrador global (sin negocio) |
| `admin.negocio@utppos.local` | Administrador del negocio 5 |
| `vendedor@utppos.local` | Vendedor del negocio 5 (no ve finanzas) |

> `-SetDevPassword` solo hace falta si usas un `.bak` que *no* sea el anonimizado. Es solo para tu máquina; nunca lo apliques a una BD compartida.

**3. Configura la API.**

Los secretos **no van en archivos del repo**: `appsettings.json` solo trae valores vacíos o `CHANGE_ME`. En desarrollo se guardan con [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (quedan fuera del proyecto, en tu perfil de usuario):

```powershell
cd ..                                   # UtpPos.Basico.WebAPI
$p = "UtpPos.WebAPI\UtpPos.WebApi.csproj"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1,14335;Database=VENTAS-POS;User Id=sa;Password=<clave de docker/.env>;TrustServerCertificate=True;" --project $p
dotnet user-secrets set "Jwt:SecretKey" "<cadena aleatoria de 32+ caracteres>" --project $p
dotnet user-secrets set "Jwt:RefreshTokenHashKey" "<otra cadena aleatoria de 32+ caracteres>" --project $p
```

- Usa `127.0.0.1` y no `localhost` en la cadena de conexión (en Windows `localhost` puede resolver a IPv6 y fallar).
- Para subir imágenes de productos (opcional) agrega también `Cloudinary:CloudName`, `Cloudinary:ApiKey` y `Cloudinary:ApiSecret` con los datos de tu cuenta gratuita de Cloudinary.
- Para ver o quitar claves: `dotnet user-secrets list --project $p` y `dotnet user-secrets remove "<clave>" --project $p`. **No uses `clear`**: borra todos los secretos del proyecto.
- En producción usa variables de entorno (`ConnectionStrings__DefaultConnection`, `Jwt__SecretKey`, ...) o el gestor de secretos de tu nube. Nunca archivos con valores reales en git.

**4. Arranca la API** (terminal 1, desde `UtpPos.Basico.WebAPI`):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project UtpPos.WebAPI\UtpPos.WebApi.csproj --urls http://localhost:5211
```

Swagger: <http://localhost:5211/swagger>

**5. Arranca el frontend** (terminal 2, desde `UtpPos.Basico.MudBlazorWeb\UtpPos.MudBlazorWeb\UtpPos.MudBlazorWeb`):

```powershell
dotnet run --launch-profile http
```

Abre <http://localhost:5229> e inicia sesión con uno de los usuarios de la tabla del paso 2.

**6. Comprueba que todo funciona.** Entra al Dashboard: debe cargar sin errores. Los datos de ejemplo son de **junio de 2026** (negocio 5), así que para ver números usa un rango de fechas de ese mes.

---

## 2. Mapa del repositorio

```
UtpPos.Basico.WebAPI/
├─ UtpPos.WebAPI/                          Controllers, Program.cs (arranque, JWT, Swagger)
├─ UtpPos.Framework.Application.DTO/       Objetos que viajan por la API
├─ UtpPos.Framework.Application.Interface/ + .Main/   Casos de uso (validación, mapeo, Response<T>)
├─ UtpPos.Framework.Domain.Entity/         Entidades
├─ UtpPos.Framework.Domain.Interface/ + .Core/        Reglas de dominio
├─ UtpPos.Framework.InfraStructure.Interface/ + .Repository/   Repositorios (Dapper → stored procedures)
├─ UtpPos.Framework.InfraStructure.Data/   Fábrica de conexión
├─ UtpPos.Framework.Transversal.*          Mapper (AutoMapper), seguridad/BCrypt, logging, utilidades
├─ UtpPos.Console.Tools/                   Herramienta de consola para administrar RBAC
├─ UtpPos.Backend.Tests/                   Tests unitarios
├─ UtpPos.Database.IntegrationTests/       Tests contra SQL Server real
├─ UtpPos.WebAPI.IntegrationTests/         Tests de la API completa
├─ database/                               Scripts SQL (procedures/ y seeds/)
├─ docker/                                 docker-compose + restore-db.ps1
└─ docs/                                   Documentación funcional (auth, rbac, dashboard, testing...)

UtpPos.Basico.MudBlazorWeb/
├─ UtpPos.MudBlazorWeb/                    App Blazor (Server + Client WASM): páginas y componentes
├─ UtpPos.Framework.Interface/ + .Main/    Contratos y servicios que llaman a la API
└─ UtpPos.Framework.Shared/                DTOs compartidos con el frontend
```

### Cómo viaja una petición

```
Controller → Application (Interface/Main) → Domain (Interface/Core)
          → Repository (Dapper) → Stored Procedure en SQL Server
```

El mejor ejemplo para leer y copiar es el módulo **Business** (negocios): `BusinessController` → `BusinessApplication` → `BusinessDomain` → `BusinessRepository`. El detalle está en `UtpPos.Basico.WebAPI/README.md`.

---

## 3. Reglas del proyecto (léelas antes de tocar código)

1. **Solo stored procedures.** Nada de SQL inline en C#. Si necesitas datos nuevos, crea o modifica un SP.
2. **Todo cambio de BD va como script en `database/`** (ver sección 5). La BD de tu máquina no es la fuente de verdad: los scripts sí.
3. **Permisos en backend.** Cada endpoint protegido usa `RequirePermission(...)`. Ocultar un botón en el frontend no es seguridad.
4. **El negocio sale del token**, no de lo que envíe el frontend (salvo usuarios globales con `dashboard-global.ver`).
5. **Nada de secretos en git.** Usa user-secrets (paso 3); `docker/.env` y `appsettings.Development.json` están ignorados, déjalos así.
6. Las respuestas de la API usan `Response<T>` (`IsSuccess`, `Data`, `Message`).

---

## 4. Pruebas

Los tests de integración necesitan una BD de pruebas **con "TEST" en el nombre** (protección para no tocar datos reales).

```powershell
cd UtpPos.Basico.WebAPI\docker
.\restore-db.ps1 -DatabaseName VENTAS_POS_TEST

cd ..
$env:UTPPOS_TEST_SQL_CONNECTION_STRING = "Server=127.0.0.1,14335;Database=VENTAS_POS_TEST;User Id=sa;Password=UtpPos_Dev#2026;TrustServerCertificate=True;"
dotnet test UtpPos.Backend.Tests\UtpPos.Backend.Tests.csproj
dotnet test UtpPos.Database.IntegrationTests\UtpPos.Database.IntegrationTests.csproj
dotnet test UtpPos.WebAPI.IntegrationTests\UtpPos.WebAPI.IntegrationTests.csproj
```

Estado actual: 160 tests (115 + 12 + 33), todos en verde. Detalle en `UtpPos.Basico.WebAPI/docs/testing.md`.
Si `dotnet test` dice que un `.exe` está en uso, detén primero la API que tengas corriendo.

---

## 5. Cambios de base de datos

| Carpeta | Contenido | Cuándo se ejecuta |
|---|---|---|
| `database/procedures/` | Un `.sql` por stored procedure (`CREATE OR ALTER`, repetible) | Al crear o modificar un SP |
| `database/seeds/` | Datos base idempotentes (p. ej. permisos del dashboard) | Al agregar módulos, acciones o permisos |

Ejemplo para aplicar un script a tu BD local:

```powershell
docker cp database\procedures\MiSp.sql utppos-sqlserver:/tmp/MiSp.sql
docker exec utppos-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -I -b -S localhost -U sa -P "<clave>" -d VENTAS-POS -i /tmp/MiSp.sql
```

Usa siempre `-I` (QUOTED_IDENTIFIER). Si agregas un SP, súbelo en el mismo PR que el código C# que lo usa.

### Generar un `.bak` nuevo para compartir

Un `UPDATE` **no** borra los datos viejos: quedan en páginas libres y en tablas internas del backup. Por eso, para compartir una BD con datos reales anonimizados:

1. Restaura una copia (`restore-db.ps1 -DatabaseName VENTAS_POS_ANON`) y ejecuta `database/tools/AnonymizeDevData.sql` sobre ella.
2. Genera esquema + datos con `database/tools/ScriptDatabase` (`dotnet run --no-build -- "<cadena>" salida.sql VENTAS_POS_ANON`, tras un `dotnet build`).
3. Crea una BD **nueva y vacía**, ejecuta `salida.sql` y haz el backup de *esa* BD (renombra sus archivos lógicos a `POS-VENTAS` y `POS-VENTAS_log` para que `restore-db.ps1` funcione).
4. Verifica sin comprimir (`BACKUP ... WITH NO_COMPRESSION`) y busca texto real con `grep -a`. Debe dar 0 coincidencias.

Nunca subas a git un `.bak`: está en `.gitignore`.

---

## 6. Tu primera funcionalidad (checklist)

1. SP en `database/procedures/` y aplicarlo a tu BD.
2. Entidad en `Domain.Entity` y DTOs en `Application.DTO`.
3. Interfaz + implementación en `Infrastructure` (Dapper llamando al SP).
4. Dominio y Application (validaciones ligeras, `Response<T>`).
5. Mapeo en `Transversal.Mapper/MappingProfile.cs` y registro de dependencias (busca dónde se registra `BusinessApplication` en `Program.cs` y haz lo mismo).
6. Controller con `RequirePermission` y, si es un permiso nuevo, su seed en `database/seeds/`.
7. Test unitario en `Backend.Tests` (copia uno de `Dashboard/`) y, si toca BD, uno de integración.
8. Frontend: servicio en `Framework.Interface`/`Main` y página o componente en `MudBlazorWeb`.

---

## 7. Problemas frecuentes

| Síntoma | Causa y solución |
|---|---|
| `restore-db.ps1` dice que falta el `.bak` | Cópialo en `UtpPos.Basico.WebAPI/Docs/database/bak/` (paso 1). |
| La API no conecta a SQL | Revisa puerto **14335**, que uses `127.0.0.1` (no `localhost`) y la clave en tus user-secrets (`dotnet user-secrets list`); `docker ps` debe mostrar `utppos-sqlserver` *healthy*. |
| Puerto 14335 ocupado | Cambia `UTPPOS_SQL_PORT` en `docker/.env` y la cadena de conexión. |
| Login: "Correo o contraseña incorrectos" | Restauraste sin `-SetDevPassword`. Vuelve a correr el script con ese parámetro. |
| `403 No tiene permisos` | El usuario no tiene el permiso en su rol. Revisa `docs/rbac/` o usa `UtpPos.Console.Tools`. |
| Dashboard vacío | Los datos de ejemplo son de junio de 2026: filtra por ese rango. |
| `You must install or update .NET` | Falta el runtime .NET 6 (API) o el SDK 8 (frontend). |
| Error escribiendo backups en el contenedor | `docker exec -u 0 utppos-sqlserver chown mssql:mssql /var/opt/mssql/backup` (un `docker compose down -v` borra los volúmenes). |

---

## 8. Documentación existente

- `UtpPos.Basico.WebAPI/README.md` — arquitectura por capas y cómo replicar un módulo.
- `UtpPos.Basico.WebAPI/docs/auth/` y `docs/rbac/` — autenticación, JWT, roles y permisos.
- `UtpPos.Basico.WebAPI/docs/database/procedures/` — documentación de cada SP.
- `UtpPos.Basico.WebAPI/docs/dashboard.md` y `UtpPos.Basico.MudBlazorWeb/Docs/dashboard.md` — dashboard.
- `UtpPos.Basico.WebAPI/docs/testing.md` — estrategia y comandos de pruebas.
- `UtpPos.Basico.WebAPI/docs/console-tools/` — herramienta de consola para RBAC.
