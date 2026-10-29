# README - Parametrización del Sistema POS

## 1. Objetivo

Este documento describe la lógica de parametrización usada en el Sistema POS / Programa de Ventas Básico.

La parametrización permite manejar listas maestras simples y reutilizables del sistema sin crear una tabla específica para cada caso.
Para esto se usan principalmente dos tablas:

```text
TBL_CLASE
TBL_PARAMETROS
```

La tabla `TBL_CLASE` actúa como agrupador o categoría de parámetros.

La tabla `TBL_PARAMETROS` contiene los valores concretos asociados a cada clase.

Ejemplo:

```text
Clase: TIPOS DE DOCUMENTO
  Parámetros:
    CC
    NIT
    CE
    PASAPORTE

Clase: MÉTODOS DE PAGO
  Parámetros:
    EFECTIVO
    TRANSFERENCIA
    TARJETA DÉBITO
    NEQUI
```

---

## 2. Base de datos

Base de datos principal:

```sql
[VENTAS-POS]
```

---

## 3. Tablas principales

## 3.1. TBL_CLASE

Representa el grupo, categoría o tipo de parametrización.

### Estructura conocida

```sql
CLA_ID              INT IDENTITY(1,1) PRIMARY KEY
CLA_DESCRIPCION     VARCHAR(100) NOT NULL
CLA_ES_EDITABLE     CHAR(1) NOT NULL
CLA_ESTADO          CHAR(1) NOT NULL
CLA_FECHA_CREACION  DATETIME NOT NULL
```

### Campos importantes

| Campo                | Descripción                                               |
| -------------------- | --------------------------------------------------------- |
| `CLA_ID`             | Identificador de la clase.                                |
| `CLA_DESCRIPCION`    | Nombre de la clase o agrupador.                           |
| `CLA_ES_EDITABLE`    | Indica si la clase puede modificarse. Valores: `S` o `N`. |
| `CLA_ESTADO`         | Estado de la clase. Valores: `A` o `I`.                   |
| `CLA_FECHA_CREACION` | Fecha de creación del registro.                           |

### Ejemplos de clases

```text
DEPARTAMENTOS
CIUDADES
UNIDADES DE MEDIDA
MÉTODOS DE PAGO
TIPOS DE DOCUMENTO
CATEGORÍAS DE PRODUCTO
TIPOS DE TERCERO
TIPOS DE MOVIMIENTO KARDEX
CAUSAS DE ANULACIÓN
CANALES DE VENTA
MOTIVOS DE AJUSTE DE INVENTARIO
```

---

## 3.2. TBL_PARAMETROS

Representa los valores pertenecientes a una clase.

### Estructura conocida

```sql
PARAM_ID              INT IDENTITY(1,1) PRIMARY KEY
PARAM_CLASE_ID        INT NOT NULL
PARAM_DESCRIPCION     VARCHAR(100) NOT NULL
PARAM_ESTADO          CHAR(1) NOT NULL
PARAM_FECHA_CREACION  DATETIME NOT NULL
```

### Relación principal

```text
TBL_CLASE.CLA_ID
  -> TBL_PARAMETROS.PARAM_CLASE_ID
```

Un parámetro siempre pertenece a una clase.

Ejemplo:

```text
TBL_CLASE
  CLA_ID = 1
  CLA_DESCRIPCION = TIPOS DE DOCUMENTO

TBL_PARAMETROS
  PARAM_CLASE_ID = 1
  PARAM_DESCRIPCION = CC

  PARAM_CLASE_ID = 1
  PARAM_DESCRIPCION = NIT
```

---

# 4. Concepto funcional

La parametrización se usa para manejar listas simples, configurables y reutilizables dentro del POS.

Debe usarse para datos como:

```text
Tipos de documento
Categorías de producto
Tipos de tercero
Métodos de pago
Unidades de medida
Ciudades
Departamentos
Causas de anulación
Canales de venta
Motivos de ajuste de inventario
```

No debe usarse para entidades que tienen comportamiento propio o mucha información adicional.

No deben ir en parametrización:

```text
Productos
Clientes
Usuarios
Negocios
Inventario
Compras
Ventas
Kardex
```

Esas entidades tienen tablas propias porque poseen reglas, relaciones y operaciones específicas.

---

# 5. Reglas de TBL_CLASE

## 5.1. Estado

El campo `CLA_ESTADO` puede tener estos valores:

```text
A = Activa
I = Inactiva
```

Una clase inactiva no debe usarse para crear nuevos parámetros asociados.

---

## 5.2. Editable

El campo `CLA_ES_EDITABLE` puede tener estos valores:

```text
S = Sí editable
N = No editable
```

Una clase no editable no debería modificarse desde el backend ni desde la interfaz de administración.

Ejemplos de clases no editables:

```text
TIPOS DE DOCUMENTO
TIPOS DE MOVIMIENTO KARDEX
MÉTODOS DE PAGO
DEPARTAMENTOS
CIUDADES
```

Ejemplos de clases editables:

```text
CATEGORÍAS DE PRODUCTO
CAUSAS DE ANULACIÓN
CANALES DE VENTA
MOTIVOS DE AJUSTE DE INVENTARIO
```

---

## 5.3. Restricciones funcionales

Una clase no editable:

```text
No puede modificarse.
No puede inactivarse.
```

Una clase con parámetros activos:

```text
No debería inactivarse si eso afecta datos relacionados.
```

Esta regla evita romper módulos como terceros, productos, ventas, compras, inventario o Kardex.

---

# 6. Reglas de TBL_PARAMETROS

## 6.1. Clase activa

Un parámetro debe pertenecer a una clase activa.

No debe permitirse crear parámetros para clases inexistentes o inactivas.

---

## 6.2. Estado

El campo `PARAM_ESTADO` puede tener estos valores:

```text
A = Activo
I = Inactivo
```

Los módulos del sistema deben usar preferiblemente parámetros activos.

---

## 6.3. Duplicados

No se permite duplicar la descripción de un parámetro dentro de la misma clase.

Ejemplo no permitido:

```text
Clase: TIPOS DE DOCUMENTO
  Parámetro: CC
  Parámetro: CC
```

Pero sí podrían existir descripciones iguales en clases diferentes si funcionalmente tiene sentido.

Ejemplo permitido:

```text
Clase: MOTIVOS DE AJUSTE DE INVENTARIO
  Parámetro: OTRO

Clase: CAUSAS DE ANULACIÓN
  Parámetro: OTRO
```

---

# 7. Procedures de TBL_CLASE

Los procedimientos almacenados principales para clases son:

```text
UspListaClases
UspConsultaClase
UspCreaClase
UspActualizaClase
UspEliminaClase
```

## 7.1. Convención esperada

```text
UspListaClases      -> Lista clases con filtros opcionales.
UspConsultaClase    -> Consulta una clase por ID.
UspCreaClase        -> Crea una nueva clase.
UspActualizaClase   -> Actualiza parcialmente una clase.
UspEliminaClase     -> Inactiva lógicamente una clase.
```

## 7.2. Reglas esperadas

Los SP de escritura deben:

```text
Usar TRY/CATCH.
Usar transacciones.
Retornar una estructura estándar.
No eliminar físicamente registros.
```

Respuesta estándar esperada:

```sql
SELECT
    1 AS Exitoso,
    'Mensaje...' AS Mensaje,
    @CLA_ID AS CLA_ID;
```

En error:

```sql
SELECT
    0 AS Exitoso,
    ERROR_MESSAGE() AS Mensaje,
    @CLA_ID AS CLA_ID;
```

---

# 8. Procedures de TBL_PARAMETROS

Los procedimientos almacenados principales para parámetros son:

```text
UspListaParametros
UspConsultaParametro
UspCreaParametro
UspActualizaParametro
UspEliminaParametro
```

## 8.1. Convención esperada

```text
UspListaParametros      -> Lista parámetros con filtros opcionales.
UspConsultaParametro    -> Consulta un parámetro por ID.
UspCreaParametro        -> Crea un nuevo parámetro.
UspActualizaParametro   -> Actualiza parcialmente un parámetro.
UspEliminaParametro     -> Inactiva lógicamente un parámetro.
```

## 8.2. Creación de parámetro

`UspCreaParametro` recibe:

```sql
@PARAM_CLASE_ID INT,
@PARAM_DESCRIPCION VARCHAR(100),
@PARAM_ESTADO CHAR(1) = 'A'
```

Reglas principales:

```text
La clase del parámetro es obligatoria.
La descripción del parámetro es obligatoria.
El estado debe ser A o I.
La clase asociada debe existir y estar activa.
No se permite duplicar PARAM_DESCRIPCION dentro de la misma clase.
```

Respuesta estándar esperada:

```sql
SELECT
    1 AS Exitoso,
    'Parámetro creado correctamente.' AS Mensaje,
    @PARAM_ID AS PARAM_ID;
```

En error:

```sql
SELECT
    0 AS Exitoso,
    ERROR_MESSAGE() AS Mensaje,
    NULL AS PARAM_ID;
```

---

# 9. Clases iniciales del sistema

Actualmente se manejan o se recomiendan las siguientes clases base:

| Clase                             | Editable | Uso                                                            |
| --------------------------------- | -------: | -------------------------------------------------------------- |
| `DEPARTAMENTOS`                   |      `N` | Ubicación geográfica de terceros, usuarios o negocios.         |
| `CIUDADES`                        |      `N` | Ubicación geográfica de terceros, usuarios o negocios.         |
| `UNIDADES DE MEDIDA`              |      `S` | Unidad en la que se vende o compra un producto.                |
| `MÉTODOS DE PAGO`                 |      `N` | Métodos disponibles para pagos en compras y ventas.            |
| `TIPOS DE DOCUMENTO`              |      `N` | Identificación de terceros y usuarios.                         |
| `CATEGORÍAS DE PRODUCTO`          |      `S` | Clasificación de productos.                                    |
| `TIPOS DE TERCERO`                |      `N` | Clasificación de terceros: cliente, proveedor, empleado, etc.  |
| `TIPOS DE MOVIMIENTO KARDEX`      |      `N` | Causas técnicas de movimientos de inventario.                  |
| `CAUSAS DE ANULACIÓN`             |      `S` | Motivos funcionales para anular compras, ventas u operaciones. |
| `CANALES DE VENTA`                |      `S` | Origen o canal por donde se realiza una venta.                 |
| `MOTIVOS DE AJUSTE DE INVENTARIO` |      `S` | Razones para ajustes manuales o administrativos de inventario. |

---

# 10. Parámetros sugeridos por clase

## 10.1. TIPOS DE DOCUMENTO

```text
CC
NIT
CE
TI
PASAPORTE
PEP
PPT
```

Uso:

```text
Terceros
Usuarios
Clientes
Proveedores
```

---

## 10.2. CATEGORÍAS DE PRODUCTO

```text
ALIMENTOS
BEBIDAS
ASEO
PAPELERÍA
TECNOLOGÍA
ROPA
SERVICIOS
OTROS
```

Uso:

```text
Productos
Reportes de ventas
Filtros de inventario
```

---

## 10.3. TIPOS DE TERCERO

```text
CLIENTE
PROVEEDOR
EMPLEADO
DISTRIBUIDOR
ALIADO
```

Uso:

```text
TBL_TERCEROS
Compras
Ventas
Reportes
```

---

## 10.4. TIPOS DE MOVIMIENTO KARDEX

```text
COMPRA
VENTA
ANULACIÓN COMPRA
ANULACIÓN VENTA
AJUSTE ENTRADA
AJUSTE SALIDA
INVENTARIO INICIAL
DEVOLUCIÓN CLIENTE
DEVOLUCIÓN PROVEEDOR
```

Uso:

```text
Kardex
Inventario
Compras
Ventas
Anulaciones
Ajustes
```

Esta clase debe tratarse con cuidado porque afecta la trazabilidad del inventario.

---

## 10.5. CAUSAS DE ANULACIÓN

```text
ERROR DE DIGITACIÓN
CLIENTE DESISTE
PRODUCTO EQUIVOCADO
PAGO INCORRECTO
VENTA DUPLICADA
COMPRA DUPLICADA
DEVOLUCIÓN
OPERACIÓN DE PRUEBA
OTRO
```

Uso:

```text
Anulación de ventas
Anulación de compras
Auditoría
Reportes administrativos
```

Una causa de anulación explica por qué una operación fue reversada.

Ejemplo:

```text
Venta #152
Estado: Anulada
Causa: PAGO INCORRECTO
Usuario: Admin
Fecha: 2026-06-18
```

Esto evita que las anulaciones queden sin trazabilidad funcional.

---

## 10.6. CANALES DE VENTA

```text
MOSTRADOR
WHATSAPP
WEB
DOMICILIO
REDES SOCIALES
MARKETPLACE
TELÉFONO
```

Uso:

```text
Ventas
Reportes comerciales
Análisis de canales
```

Ejemplo:

```text
Venta realizada por canal: WHATSAPP
Venta realizada por canal: MOSTRADOR
```

---

## 10.7. MOTIVOS DE AJUSTE DE INVENTARIO

```text
CONTEO FÍSICO
PÉRDIDA
DAÑO
VENCIMIENTO
ROBO
SOBRANTE
ERROR DE REGISTRO
```

Uso:

```text
Ajustes manuales de inventario
Kardex
Auditoría de stock
```

Ejemplo:

```text
Ajuste de salida
Motivo: DAÑO

Ajuste de entrada
Motivo: SOBRANTE
```

---

# 11. Uso en Backend .NET + Dapper

## 11.1. Arquitectura esperada

La parametrización debe seguir la arquitectura por capas del backend:

```text
Controller
  -> Application
    -> Domain
      -> Repository
        -> Stored Procedure
```

El repositorio es la única capa que debe conocer:

```text
Dapper
SQL Server
Nombres de procedures
Nombres de columnas SQL
Nombres de parámetros SQL
```

---

## 11.2. Entidades sugeridas

Para `TBL_CLASE`:

```csharp
public class Class
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IsEditable { get; set; } = "S";
    public string Status { get; set; } = "A";
    public DateTime CreatedAt { get; set; }
    public int TotalParameters { get; set; }
}
```

También es válido manejar `IsEditable` como `bool` en dominio, siempre que el repositorio convierta:

```text
S -> true
N -> false
```

y al enviar a SQL:

```text
true -> S
false -> N
```

---

Para `TBL_PARAMETROS`:

```csharp
public class Parameter
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string ClassDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "A";
    public DateTime CreatedAt { get; set; }
}
```

---

## 11.3. DTOs sugeridos para clases

```text
ClassDto
ClassListFilterDto
ClassCreateDto
ClassUpdateDto
```

### ClassListFilterDto

```csharp
public class ClassListFilterDto
{
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? IsEditable { get; set; }
}
```

### ClassCreateDto

```csharp
public class ClassCreateDto
{
    public string Description { get; set; } = string.Empty;
    public string IsEditable { get; set; } = "S";
    public string Status { get; set; } = "A";
}
```

### ClassUpdateDto

```csharp
public class ClassUpdateDto
{
    public string? Description { get; set; }
    public string? IsEditable { get; set; }
    public string? Status { get; set; }
}
```

---

## 11.4. DTOs sugeridos para parámetros

```text
ParameterDto
ParameterListFilterDto
ParameterCreateDto
ParameterUpdateDto
```

### ParameterListFilterDto

```csharp
public class ParameterListFilterDto
{
    public int? ClassId { get; set; }
    public string? ClassDescription { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}
```

### ParameterCreateDto

```csharp
public class ParameterCreateDto
{
    public int ClassId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "A";
}
```

### ParameterUpdateDto

```csharp
public class ParameterUpdateDto
{
    public int? ClassId { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}
```

---

# 12. Endpoints sugeridos

## 12.1. Clases

```text
GET    /api/classes
GET    /api/classes/{id}
POST   /api/classes
PATCH  /api/classes/{id}
DELETE /api/classes/{id}
```

También podría usarse ruta en español si el proyecto decide exponer endpoints orientados al cliente/admin en español:

```text
GET    /api/clases
GET    /api/clases/{id}
POST   /api/clases
PATCH  /api/clases/{id}
DELETE /api/clases/{id}
```

La decisión debe ser consistente en todo el backend.

---

## 12.2. Parámetros

```text
GET    /api/parameters
GET    /api/parameters/{id}
POST   /api/parameters
PATCH  /api/parameters/{id}
DELETE /api/parameters/{id}
GET    /api/classes/{classId}/parameters
```

O en español:

```text
GET    /api/parametros
GET    /api/parametros/{id}
POST   /api/parametros
PATCH  /api/parametros/{id}
DELETE /api/parametros/{id}
GET    /api/clases/{classId}/parametros
```

---

# 13. Reglas para implementación en Repository

## 13.1. No mapear directamente nombres SQL al dominio

La base de datos usa nombres como:

```text
CLA_ID
CLA_DESCRIPCION
PARAM_ID
PARAM_CLASE_ID
PARAM_DESCRIPCION
```

El dominio debe usar nombres limpios:

```text
Id
Description
ClassId
Status
CreatedAt
```

El repositorio puede usar modelos privados para Dapper.

Ejemplo:

```csharp
private sealed class ParameterDbModel
{
    public int PARAM_ID { get; set; }
    public int PARAM_CLASE_ID { get; set; }
    public string? PARAM_DESCRIPCION { get; set; }
    public string? PARAM_ESTADO { get; set; }
    public DateTime PARAM_FECHA_CREACION { get; set; }
}
```

Y luego mapear a dominio:

```csharp
private static Parameter MapToDomain(ParameterDbModel model)
{
    return new Parameter
    {
        Id = model.PARAM_ID,
        ClassId = model.PARAM_CLASE_ID,
        Description = model.PARAM_DESCRIPCION ?? string.Empty,
        Status = model.PARAM_ESTADO ?? string.Empty,
        CreatedAt = model.PARAM_FECHA_CREACION
    };
}
```

---

## 13.2. Usar SP, no tablas directas

El backend no debe consumir directamente:

```sql
SELECT * FROM dbo.TBL_CLASE
SELECT * FROM dbo.TBL_PARAMETROS
```

Debe consumir los stored procedures definidos.

---

## 13.3. Escrituras

Los procedimientos de escritura retornan una respuesta estándar.

El repositorio debe leer esa respuesta y convertirla en un resultado claro para las capas superiores.

Ejemplo:

```csharp
private sealed class ParameterWriteResult
{
    public int Exitoso { get; set; }
    public string? Mensaje { get; set; }
    public int? PARAM_ID { get; set; }
}
```

Si `Exitoso = 0`, el backend debe responder un error controlado.

---

# 14. Reglas para Domain

La capa de dominio puede validar reglas ligeras antes de llamar al repositorio.

Ejemplos:

```text
Id mayor que cero.
Description obligatoria en creación.
Status debe ser A o I.
IsEditable debe ser S o N.
ClassId mayor que cero al crear parámetro.
```

La base de datos sigue siendo la última defensa para:

```text
Duplicados.
Existencia real de registros.
Clase activa.
Clase no editable.
Clase con parámetros activos.
Consistencia transaccional.
```

---

# 15. Reglas para Application

La capa Application debe:

```text
Recibir DTOs.
Mapear DTOs hacia entidades de dominio.
Llamar la capa Domain.
Mapear entidades de dominio hacia DTOs de respuesta.
Retornar Response<T>.
```

No debe conocer:

```text
Dapper
SQL Server
Nombres de stored procedures
Nombres de columnas SQL
```

---

# 16. Reglas para Controller

El controller debe:

```text
Recibir peticiones HTTP.
Usar [FromQuery] para filtros.
Usar [FromBody] para creación y actualización.
Usar [FromRoute] para IDs.
Llamar únicamente Application.
Retornar IActionResult.
```

No debe:

```text
Usar Dapper.
Abrir conexiones.
Ejecutar SQL.
Conocer tablas directamente.
Llamar repositories directamente.
```

---

# 17. Seed de parametrización

Para datos base del sistema se recomienda usar scripts seed que:

```text
Creen clases si no existen.
Creen parámetros si no existen.
Usen los stored procedures.
No inserten directamente en tablas.
Sean idempotentes.
```

Idempotente significa que el script puede ejecutarse varias veces sin duplicar información.

Flujo recomendado:

```text
1. Buscar clase por CLA_DESCRIPCION.
2. Si no existe, crearla con UspCreaClase.
3. Buscar parámetro por PARAM_CLASE_ID y PARAM_DESCRIPCION.
4. Si no existe, crearlo con UspCreaParametro.
```

---

# 18. Cuándo crear una clase nueva

Crear una clase nueva cuando se necesite una lista simple de opciones.

Ejemplos válidos:

```text
REGÍMENES TRIBUTARIOS
TIPOS DE IMPUESTO
TIPOS DE PRECIO
FORMAS DE ENTREGA
TIPOS DE DESCUENTO
```

No crear clase nueva cuando el dato necesite:

```text
Muchas columnas propias.
Relaciones complejas.
Histórico propio.
Reglas transaccionales.
Operaciones propias.
Reportes complejos.
```

En esos casos, crear una tabla propia.

---

# 19. Decisión importante sobre métodos de pago

Actualmente `MÉTODOS DE PAGO` puede manejarse como parametrización.

Esto sirve si solo se necesita:

```text
Nombre del método.
Estado.
```

Pero si más adelante se necesita:

```text
Comisión.
Cuenta bancaria.
Integración externa.
Código contable.
QR.
Referencia de pasarela.
Reglas de conciliación.
```

Entonces debe migrarse a una tabla propia, por ejemplo:

```text
TBL_METODOS_PAGO
```

---

# 20. Decisión importante sobre categorías de producto

Actualmente `CATEGORÍAS DE PRODUCTO` puede manejarse como parametrización.

Esto sirve si solo se necesita:

```text
Nombre.
Estado.
```

Pero si más adelante se necesita:

```text
Imagen.
Color.
Categoría padre.
Orden.
Impuesto por defecto.
Margen por defecto.
Configuración por negocio.
```

Entonces debe migrarse a tabla propia, por ejemplo:

```text
TBL_CATEGORIAS_PRODUCTO
```

---

# 21. Flujo general de parametrización

```text
TBL_CLASE
  Define el grupo de parámetros

TBL_PARAMETROS
  Define los valores del grupo

Módulos del POS
  Usan PARAM_ID para validar o clasificar información
```

Ejemplo:

```text
TBL_CLASE
  CLA_ID = 1
  CLA_DESCRIPCION = TIPOS DE DOCUMENTO

TBL_PARAMETROS
  PARAM_ID = 10
  PARAM_CLASE_ID = 1
  PARAM_DESCRIPCION = CC

TBL_TERCEROS
  TER_TIPO_DOCUMENTO_ID = 10
```

---

# 22. Módulos que dependen de parametrización

La parametrización sirve como base para:

```text
Terceros
Usuarios
Productos
Inventario
Kardex
Compras
Ventas
Reportes
Auditoría
```

El ciclo general del sistema es:

```text
Parametrización
  ↓
Productos
  ↓
Inventario
  ↓
Kardex
  ↓
Compras
  ↓
Ventas
```

---

# 23. Resumen para modelos de IA

El sistema POS usa `TBL_CLASE` y `TBL_PARAMETROS` para manejar listas maestras simples.

`TBL_CLASE` es el agrupador.

`TBL_PARAMETROS` contiene los valores de cada agrupador.

La relación principal es:

```text
TBL_CLASE.CLA_ID -> TBL_PARAMETROS.PARAM_CLASE_ID
```

Las clases tienen estado `A/I` y bandera de edición `S/N`.

Los parámetros tienen estado `A/I`.

No se deben eliminar físicamente registros; se debe usar inactivación lógica.

El backend debe consumir stored procedures mediante Dapper, no consultar tablas directamente.

La capa Repository conoce los nombres SQL.

La capa Domain usa nombres limpios y reglas de negocio.

La capa Application mapea DTOs y retorna `Response<T>`.

La capa Controller solo expone HTTP.

Las clases y parámetros son la base de configuración del POS para terceros, usuarios, productos, inventario, Kardex, compras, ventas y reportes.
