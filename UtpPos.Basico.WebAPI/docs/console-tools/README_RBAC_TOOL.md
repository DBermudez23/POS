# README - UTP POS Console Tools para RBAC

## 1. Objetivo

> La gestión de roles, permisos por rol, roles por usuario y permisos efectivos se documenta en [README_RBAC_ROLE_MANAGEMENT.md](README_RBAC_ROLE_MANAGEMENT.md).

`UtpPos.Console.Tools` es una herramienta técnica interna de consola para administrar los catálogos base del módulo RBAC del Sistema de Ventas.

La herramienta permite:

```text
- Listar módulos existentes.
- Crear módulos.
- Listar acciones existentes.
- Crear acciones.
- Listar permisos existentes.
- Crear permisos combinando un módulo existente y una acción existente.
```

La consola no crea registros al iniciar. Toda operación de consulta o creación debe ser seleccionada explícitamente por el operador.

---

## 2. Ubicación del proyecto

El proyecto se encuentra en:

```text
UtpPos.Basico.WebAPI/
├── UtpPos.WebAPI/
└── UtpPos.Console.Tools/
```

`UtpPos.Console.Tools` está al mismo nivel que `UtpPos.WebAPI` y forma parte de la solución `UtpPos.WebAPI.sln`. No referencia el proyecto WebAPI como dependencia de compilación.

---

## 3. Por qué existe esta consola

Los módulos, las acciones y los permisos representan capacidades técnicas reales del backend. No son catálogos funcionales que deban quedar disponibles inicialmente mediante endpoints públicos ni administrarse con escrituras SQL manuales.

La consola proporciona un punto de administración controlado que:

* reutiliza las capas existentes del backend;
* ejecuta los stored procedures oficiales;
* valida duplicados antes de insertar;
* obliga a seleccionar módulos y acciones existentes para crear permisos;
* muestra el resultado transversal retornado por Application.

La herramienta no reemplaza el mecanismo final de autorización HTTP. Su responsabilidad actual es mantener los catálogos técnicos base de RBAC.

---

# 4. Arquitectura usada

El flujo de una operación es:

```text
UtpPos.Console.Tools
    ↓
Application
    ↓
Repository
    ↓
Dapper
    ↓
Stored Procedures
    ↓
SQL Server
```

La consola consume `IRbacApplication`. `RbacApplication` aplica validaciones ligeras y utiliza `IRbacRepository`. La implementación `RbacRepository` es la única parte de este flujo que usa Dapper y ejecuta stored procedures.

Por diseño, `UtpPos.Console.Tools`:

* no usa Dapper directamente;
* no usa `SqlConnection` directamente;
* no ejecuta sentencias SQL;
* no contiene lógica de acceso a datos;
* no contiene seeds ni listas hardcodeadas de módulos o acciones;
* no crea datos automáticamente al iniciar.

---

# 5. Estructura del proyecto

La estructura actual es:

```text
UtpPos.Console.Tools/
├── UtpPos.Console.Tools.csproj
├── Program.cs
├── Menus/
│   └── MainMenu.cs
└── Runners/
    ├── ModuleRunner.cs
    ├── ActionRunner.cs
    └── PermissionRunner.cs
```

No existe una carpeta `Seeds` dentro de este proyecto.

---

# 6. Responsabilidad de cada archivo

| Archivo | Responsabilidad |
| ------- | --------------- |
| `Program.cs` | Localiza la configuración de `UtpPos.WebAPI`, construye `IConfiguration`, registra las dependencias, crea el scope de ejecución y resuelve `MainMenu`. |
| `Menus/MainMenu.cs` | Presenta el menú principal y delega cada opción al runner correspondiente. |
| `Runners/ModuleRunner.cs` | Lista módulos, solicita los datos de un nuevo módulo, valida su existencia y solicita su creación. |
| `Runners/ActionRunner.cs` | Lista acciones, solicita los datos de una nueva acción, valida su existencia y solicita su creación. |
| `Runners/PermissionRunner.cs` | Lista permisos y coordina la selección de un módulo activo y una acción activa para crear un permiso. |

Los runners solo orquestan interacción con el operador y llamadas a `IRbacApplication`; no abren conexiones ni conocen detalles de Dapper.

---

# 7. Configuración

La consola localiza el directorio `UtpPos.WebAPI` desde el directorio actual o desde el directorio del ejecutable. Después carga la configuración en este orden:

```text
1. UtpPos.WebAPI/appsettings.json
2. UtpPos.WebAPI/appsettings.Development.json, si existe
3. Variables de entorno
```

Las variables de entorno se agregan al final y pueden sobrescribir los valores definidos en los archivos JSON.

La cadena de conexión utilizada es:

```text
ConnectionStrings:DefaultConnection
```

Ejemplo de sobrescritura mediante variable de entorno:

```text
ConnectionStrings__DefaultConnection
```

`UtpPos.Console.Tools` no tiene ni debe tener un `appsettings.json` propio. Utiliza la misma configuración de conexión que `UtpPos.WebAPI`.

---

# 8. Ejecución

Desde la raíz de `UtpPos.Basico.WebAPI`, en Windows:

```powershell
dotnet run --project .\UtpPos.Console.Tools\UtpPos.Console.Tools.csproj
```

En Linux o macOS:

```bash
dotnet run --project ./UtpPos.Console.Tools/UtpPos.Console.Tools.csproj
```

La configuración de `DefaultConnection` debe estar disponible en los archivos de `UtpPos.WebAPI` o mediante variables de entorno antes de iniciar la herramienta.

---

# 9. Menú principal

Al iniciar, la consola muestra:

```text
==============================================
             UTP POS CONSOLE TOOLS
==============================================
[1] Gestionar Módulos
[2] Gestionar Acciones
[3] Gestionar Permisos (Módulo + Acción)
[0] Salir
==============================================
Seleccione una opción:
```

Seleccionar `0`, o finalizar la entrada estándar, termina la ejecución sin consultar ni modificar datos.

---

# 10. Flujo para módulos

El submenú de módulos permite listar registros o crear uno nuevo.

## 10.1. Listado

El listado consulta `IRbacApplication.ListModulesAsync` sin filtros y muestra:

```text
MOD_ID
MOD_CODIGO
MOD_NOMBRE
MOD_DESCRIPCION
MOD_ESTADO
```

## 10.2. Creación

El flujo actual es:

1. Solicitar el código técnico del módulo.
2. Normalizar el código a minúscula.
3. Consultar su existencia mediante Application, Repository y `UspListaModulos`, sin filtrar por estado.
4. Si existe activo o inactivo, informar el duplicado y no insertar.
5. Si no existe, solicitar el nombre visible.
6. Solicitar una descripción opcional.
7. Solicitar confirmación al operador.
8. Crear el módulo mediante `UspCreaModulo`.
9. Mostrar el mensaje y el `MOD_ID` resultante.

La consola envía los siguientes valores técnicos por defecto:

```text
MOD_ES_SISTEMA = 'S'
MOD_ESTADO = 'A'
```

La validación de existencia también se repite en Application antes de ejecutar el SP de creación.

---

# 11. Flujo para acciones

El submenú de acciones permite listar registros o crear uno nuevo.

## 11.1. Listado

El listado consulta `IRbacApplication.ListActionsAsync` sin filtros y muestra:

```text
ACC_ID
ACC_CODIGO
ACC_NOMBRE
ACC_DESCRIPCION
ACC_ESTADO
```

## 11.2. Creación

El flujo actual es:

1. Solicitar el código técnico de la acción.
2. Normalizar el código a minúscula.
3. Consultar su existencia mediante Application, Repository y `UspListaAcciones`, sin filtrar por estado.
4. Si existe activa o inactiva, informar el duplicado y no insertar.
5. Si no existe, solicitar el nombre visible.
6. Solicitar una descripción opcional.
7. Solicitar confirmación al operador.
8. Crear la acción mediante `UspCreaAccion`.
9. Mostrar el mensaje y el `ACC_ID` resultante.

La consola envía los siguientes valores técnicos por defecto:

```text
ACC_ES_SISTEMA = 'S'
ACC_ESTADO = 'A'
```

La validación de existencia también se repite en Application antes de ejecutar el SP de creación.

---

# 12. Flujo para permisos

El submenú de permisos permite listar registros o crear una combinación válida entre módulo y acción.

## 12.1. Listado

El listado consulta `IRbacApplication.ListPermissionsAsync` sin filtros y muestra:

```text
PER_ID
PER_NOMBRE
MOD_CODIGO
ACC_CODIGO
PER_DESCRIPCION
PER_ESTADO
```

## 12.2. Creación

El flujo actual es:

1. Consultar módulos activos mediante `UspListaModulos` con estado `A`.
2. Consultar acciones activas mediante `UspListaAcciones` con estado `A`.
3. Mostrar los módulos activos numerados.
4. Seleccionar un módulo.
5. Mostrar las acciones activas numeradas.
6. Seleccionar una acción.
7. Construir el nombre lógico esperado:

   ```text
   MOD_CODIGO + "." + ACC_CODIGO
   ```

8. Validar la existencia mediante `UspListaPermisos`, usando los códigos seleccionados y sin filtrar por estado.
9. Si la combinación existe activa o inactiva, informar el duplicado y no insertar.
10. Solicitar una descripción opcional.
11. Si no se ingresa una descripción, generar:

    ```text
    Permiso para {MOD_CODIGO}.{ACC_CODIGO}
    ```

12. Solicitar confirmación al operador.
13. Crear el permiso mediante `UspCreaPermiso`, con `PER_ESTADO = 'A'`.
14. Mostrar `PER_ID` y `PER_NOMBRE` retornados por el SP.

Los identificadores `PER_MOD_ID` y `PER_ACC_ID` no se digitan. Se obtienen exclusivamente de los registros activos seleccionados desde la base de datos.

---

# 13. Stored Procedures usados

| Operación | Stored Procedure |
| --------- | ---------------- |
| Listar módulos | `dbo.UspListaModulos` |
| Crear módulo | `dbo.UspCreaModulo` |
| Listar acciones | `dbo.UspListaAcciones` |
| Crear acción | `dbo.UspCreaAccion` |
| Listar permisos | `dbo.UspListaPermisos` |
| Crear permiso | `dbo.UspCreaPermiso` |

Los nombres y parámetros SQL permanecen encapsulados en `RbacRepository`.

---

# 14. Reglas importantes

* No crear módulos duplicados.
* No crear acciones duplicadas.
* No crear permisos duplicados.
* La búsqueda de duplicados debe incluir registros activos e inactivos.
* No crear permisos digitando identificadores manualmente.
* Para crear permisos, seleccionar siempre un módulo activo y una acción activa consultados desde la base de datos.
* No usar listas hardcodeadas para módulos, acciones o permisos.
* Mantener los códigos técnicos en minúscula.
* No modificar estados desde la consola en la versión actual.
* Para modificar estados se requiere una operación controlada directa en base de datos o desarrollar una opción futura con sus correspondientes contratos y stored procedures.
* Si un stored procedure retorna `Exitoso = 0`, Repository propaga su `Mensaje` como una operación fallida y Application lo retorna mediante `Response<T>`.

---

# 15. Consideraciones para mantenimiento

Toda opción futura debe mantener el flujo:

```text
Console -> Application -> Repository -> Stored Procedure
```

Al ampliar la herramienta:

* agregar contratos en las capas correspondientes;
* mantener Dapper y los nombres SQL dentro de Infrastructure.Repository;
* consultar catálogos desde la base de datos en lugar de crear listas fijas;
* validar existencia antes de las operaciones de escritura;
* conservar `Response<T>` como respuesta transversal de Application;
* registrar únicamente las dependencias necesarias en `Program.cs`;
* evitar agregar controllers, middleware HTTP o dependencias hacia `UtpPos.WebAPI`.

La definición detallada de la vista y los stored procedures RBAC se encuentra en:

```text
docs/database/procedures/README_RBAC_PROCEDURES.md
```
