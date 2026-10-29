# README - Procedures de Productos

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para la gestión de la tabla `TBL_PRODUCTOS` dentro de la base de datos del Sistema de Ventas.

La tabla `TBL_PRODUCTOS` permite registrar el catálogo de productos que serán utilizados en procesos de ventas, compras, inventario y Kardex.

Los procedimientos almacenados definidos permiten listar, consultar, crear, actualizar e inactivar lógicamente registros de productos.

---

## 2. Tabla relacionada

### Tabla principal

```sql
TBL_PRODUCTOS
```

### Estructura base

```sql
PROD_ID                       INT IDENTITY(1,1) PRIMARY KEY
PROD_NOMBRE                   VARCHAR(100) NOT NULL
PROD_DESCRIPCION              VARCHAR(100) NOT NULL
PROD_CATEGORIA_ID             INT NOT NULL
PROD_IMAGEN                   VARCHAR(500) NULL
PROD_VALOR_VENTA              DECIMAL(18,2) DEFAULT 0 NOT NULL
PROD_PROCENTAJE_IVA           DECIMAL(5,2) DEFAULT 0 NOT NULL
PROD_PORCENTAJE_MARGEN_GAN    DECIMAL(5,2) DEFAULT 0 NOT NULL
PROD_EXTERNAL_ID              VARCHAR(100) NULL
PROD_NEG_ID                   INT NOT NULL
PROD_CREADO_POR_USUARIO_ID    INT NOT NULL
PROD_ESTADO                   CHAR(1) DEFAULT 'A' NOT NULL
PROD_FECHA_CREADO             DATETIME NOT NULL
```

---

## 3. Relaciones principales

La tabla `TBL_PRODUCTOS` se relaciona con las siguientes tablas:

```sql
TBL_PRODUCTOS.PROD_CATEGORIA_ID -> TBL_PARAMETROS.PARAM_ID
TBL_PRODUCTOS.PROD_NEG_ID -> TBL_NEGOCIO.NEG_ID
TBL_PRODUCTOS.PROD_CREADO_POR_USUARIO_ID -> TBL_USUARIOS.USR_ID
```

Estas relaciones permiten asociar cada producto a una categoría parametrizada, a un negocio obligatorio y a un usuario creador.

---

## 4. Procedures creados

Para la entidad `Productos`, se utilizan los siguientes procedimientos almacenados:

```text
UspListaProductos
UspConsultaProducto
UspCreaProducto
UspActualizaProducto
UspEliminaProducto
```

---

# 5. Consideraciones generales

* No se realiza eliminación física de registros.

* La eliminación se maneja mediante borrado lógico usando el campo `PROD_ESTADO`.

* Los estados válidos son:

  * `A`: Activo.
  * `I`: Inactivo.

* El procedimiento de creación exige los campos obligatorios para registrar un nuevo producto.

* El procedimiento de actualización permite edición parcial.

* Si un campo no se envía en la actualización, conserva su valor actual.

* El campo `PROD_NEG_ID` es obligatorio y no permite valores `NULL`.

* Cada producto debe pertenecer a un negocio.

* El campo `PROD_VALOR_VENTA` no debe ser definido manualmente por el usuario.

* El campo `PROD_VALOR_VENTA` representa el valor base de venta antes de IVA.

* El campo `PROD_VALOR_VENTA` inicia en `0` al crear el producto.

* El valor de venta se recalcula a partir del costo ponderado del inventario y el margen de ganancia.

* El IVA no se incluye dentro de `PROD_VALOR_VENTA`; se calcula posteriormente en ventas.

* El campo `PROD_IMAGEN` permite valores `NULL`.

* El campo `PROD_EXTERNAL_ID` permite valores `NULL`.

* Los procedures de escritura usan `TRY/CATCH`.

* Los procedures de escritura usan transacciones para garantizar atomicidad.

* Se valida que la categoría exista y esté activa.

* Se valida que el negocio exista, esté activo y sea obligatorio.

* Se valida que el usuario creador exista y esté activo al crear un producto.

* Se evita duplicar productos con el mismo nombre dentro del mismo negocio.

* Se evita duplicar productos con el mismo identificador externo dentro del mismo negocio.

---

# 6. Regla de valor de venta

## 6.1. Concepto

El valor de venta del producto no es un dato manual.

El flujo correcto es:

```text
Compra / Entrada de inventario
        ↓
Kardex actualiza stock y costo ponderado
        ↓
Producto recalcula valor de venta base
        ↓
Venta usa ese valor base
        ↓
Venta calcula IVA y total
```

---

## 6.2. Fórmula

```text
PROD_VALOR_VENTA = INV_COSTO_PONDERADO * (1 + PROD_PORCENTAJE_MARGEN_GAN / 100)
```

Ejemplo:

```text
Costo ponderado: 2.700.800
Margen: 8%

PROD_VALOR_VENTA = 2.700.800 * 1.08
PROD_VALOR_VENTA = 2.916.864
```

Si el producto tiene IVA del 19%, el precio final de venta se calcula en el flujo de venta:

```text
Precio base: 2.916.864
IVA 19%: 554.204,16
Total: 3.471.068,16
```

---

## 6.3. Responsabilidad del campo `PROD_VALOR_VENTA`

`PROD_VALOR_VENTA` debe tratarse como un campo calculado/persistido por backend o procedimientos almacenados.

No debe ser editado directamente desde el frontend.

No debe enviarse como fuente de verdad desde formularios de creación o actualización.

---

# 7. Detalle de procedures

---

## 7.1. UspListaProductos

### Objetivo

Listar los productos registrados en el sistema, permitiendo aplicar filtros opcionales por negocio, categoría, estado, nombre o identificador externo.

### Nombre

```sql
dbo.UspListaProductos
```

### Parámetros de entrada

| Parámetro            | Tipo           | Obligatorio | Descripción                                                   |
| -------------------- | -------------- | ----------- | ------------------------------------------------------------- |
| `@PROD_NEG_ID`       | `INT`          | No          | Permite filtrar productos por negocio.                        |
| `@PROD_CATEGORIA_ID` | `INT`          | No          | Permite filtrar productos por categoría.                      |
| `@PROD_ESTADO`       | `CHAR(1)`      | No          | Permite filtrar por estado: `A` o `I`.                        |
| `@PROD_NOMBRE`       | `VARCHAR(100)` | No          | Permite buscar productos por coincidencia parcial del nombre. |
| `@PROD_EXTERNAL_ID`  | `VARCHAR(100)` | No          | Permite buscar productos por identificador externo.           |

### Resultado

Retorna un listado de productos junto con información descriptiva de categoría, negocio y usuario creador.

### Campos retornados

| Campo                        | Descripción                                             |
| ---------------------------- | ------------------------------------------------------- |
| `PROD_ID`                    | Identificador del producto.                             |
| `PROD_NOMBRE`                | Nombre del producto.                                    |
| `PROD_DESCRIPCION`           | Descripción del producto.                               |
| `PROD_CATEGORIA_ID`          | Identificador de la categoría.                          |
| `CATEGORIA`                  | Descripción de la categoría.                            |
| `PROD_IMAGEN`                | URL o ruta de imagen del producto.                      |
| `PROD_VALOR_VENTA`           | Valor base de venta antes de IVA.                       |
| `PROD_PROCENTAJE_IVA`        | Porcentaje de IVA aplicado al producto.                 |
| `PROD_PORCENTAJE_MARGEN_GAN` | Porcentaje de margen de ganancia usado para el cálculo. |
| `PROD_EXTERNAL_ID`           | Identificador externo del producto.                     |
| `PROD_NEG_ID`                | Identificador del negocio asociado.                     |
| `NEG_NOMBRE`                 | Nombre del negocio asociado.                            |
| `PROD_CREADO_POR_USUARIO_ID` | Identificador del usuario creador.                      |
| `CREADO_POR_USUARIO`         | Nombre del usuario creador.                             |
| `PROD_ESTADO`                | Estado del producto.                                    |
| `PROD_FECHA_CREADO`          | Fecha de creación del producto.                         |

### Consideración de negocio

Para usuarios globales, el backend puede consultar sin enviar `@PROD_NEG_ID`.

Para usuarios asociados a un negocio, el backend debe enviar el `BusinessId` del usuario como filtro para garantizar que solo vea productos de su negocio.

### Ejemplos de uso

Listar todos los productos:

```sql
EXEC dbo.UspListaProductos;
```

Listar productos activos:

```sql
EXEC dbo.UspListaProductos
    @PROD_ESTADO = 'A';
```

Buscar por nombre:

```sql
EXEC dbo.UspListaProductos
    @PROD_NOMBRE = 'Camiseta';
```

Listar productos por categoría:

```sql
EXEC dbo.UspListaProductos
    @PROD_CATEGORIA_ID = 1;
```

Listar productos por negocio:

```sql
EXEC dbo.UspListaProductos
    @PROD_NEG_ID = 1;
```

---

## 7.2. UspConsultaProducto

### Objetivo

Consultar la información de un producto específico mediante su identificador.

### Nombre

```sql
dbo.UspConsultaProducto
```

### Parámetros de entrada

| Parámetro  | Tipo  | Obligatorio | Descripción                             |
| ---------- | ----- | ----------- | --------------------------------------- |
| `@PROD_ID` | `INT` | Sí          | Identificador del producto a consultar. |

### Resultado

Retorna la información completa del producto consultado, incluyendo información descriptiva de categoría, negocio y usuario creador.

### Consideración de negocio

Si el usuario autenticado pertenece a un negocio específico, el backend debe validar que el producto consultado pertenezca a ese mismo negocio.

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaProducto
    @PROD_ID = 1;
```

---

## 7.3. UspCreaProducto

### Objetivo

Crear un nuevo producto dentro del catálogo del sistema.

Este procedimiento se utiliza únicamente para inserción. No actualiza registros existentes.

### Nombre

```sql
dbo.UspCreaProducto
```

### Parámetros de entrada

| Parámetro                     | Tipo            | Obligatorio | Descripción                                                              |
| ----------------------------- | --------------- | ----------- | ------------------------------------------------------------------------ |
| `@PROD_NOMBRE`                | `VARCHAR(100)`  | Sí          | Nombre del producto.                                                     |
| `@PROD_DESCRIPCION`           | `VARCHAR(100)`  | Sí          | Descripción del producto.                                                |
| `@PROD_CATEGORIA_ID`          | `INT`           | Sí          | Identificador de la categoría del producto.                              |
| `@PROD_IMAGEN`                | `VARCHAR(500)`  | No          | URL o ruta de imagen del producto.                                       |
| `@PROD_VALOR_VENTA`           | `DECIMAL(18,2)` | No          | Deprecated. Se conserva por compatibilidad, pero el procedure lo ignora. |
| `@PROD_PROCENTAJE_IVA`        | `DECIMAL(5,2)`  | No          | Porcentaje de IVA. Por defecto: `0`.                                     |
| `@PROD_PORCENTAJE_MARGEN_GAN` | `DECIMAL(5,2)`  | No          | Porcentaje de margen de ganancia. Por defecto: `0`.                      |
| `@PROD_EXTERNAL_ID`           | `VARCHAR(100)`  | No          | Identificador externo del producto.                                      |
| `@PROD_NEG_ID`                | `INT`           | Sí          | Identificador del negocio asociado.                                      |
| `@PROD_CREADO_POR_USUARIO_ID` | `INT`           | Sí          | Identificador del usuario que crea el producto.                          |
| `@PROD_ESTADO`                | `CHAR(1)`       | No          | Estado del producto. Por defecto: `A`.                                   |

### Validaciones realizadas

El procedimiento valida que:

1. El nombre del producto sea obligatorio.
2. La descripción del producto sea obligatoria.
3. La categoría del producto sea obligatoria.
4. El porcentaje de IVA no sea negativo.
5. El porcentaje de margen de ganancia no sea negativo.
6. El negocio asociado sea obligatorio.
7. El usuario creador sea obligatorio.
8. El estado sea válido: `A` o `I`.
9. La categoría exista y esté activa.
10. El negocio exista y esté activo.
11. El usuario creador exista y esté activo.
12. No exista otro producto con el mismo nombre dentro del mismo negocio.
13. No exista otro producto con el mismo identificador externo dentro del mismo negocio.

### Comportamiento sobre `PROD_VALOR_VENTA`

El procedimiento no recibe el valor de venta como dato de negocio.

Aunque el parámetro `@PROD_VALOR_VENTA` pueda existir temporalmente por compatibilidad, no se valida ni se usa.

Al crear un producto, `PROD_VALOR_VENTA` se registra en `0`.

El valor será recalculado posteriormente cuando exista un costo ponderado de inventario.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `PROD_ID` | Identificador del producto creado.                        |

### Ejemplo de creación

```sql
EXEC dbo.UspCreaProducto
    @PROD_NOMBRE = 'Camiseta básica',
    @PROD_DESCRIPCION = 'Camiseta de algodón',
    @PROD_CATEGORIA_ID = 1,
    @PROD_IMAGEN = 'https://example.com/camiseta.jpg',
    @PROD_PROCENTAJE_IVA = 19,
    @PROD_PORCENTAJE_MARGEN_GAN = 30,
    @PROD_EXTERNAL_ID = 'fake-store-1',
    @PROD_NEG_ID = 1,
    @PROD_CREADO_POR_USUARIO_ID = 1,
    @PROD_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                       | PROD_ID
--------|-------------------------------|--------
1       | Producto creado correctamente. | 1
```

---

## 7.4. UspActualizaProducto

### Objetivo

Actualizar un producto existente.

Este procedimiento permite edición parcial. Si un campo no se envía, conserva el valor actual almacenado en la base de datos.

### Nombre

```sql
dbo.UspActualizaProducto
```

### Parámetros de entrada

| Parámetro                     | Tipo            | Obligatorio | Descripción                                                                       |
| ----------------------------- | --------------- | ----------- | --------------------------------------------------------------------------------- |
| `@PROD_ID`                    | `INT`           | Sí          | Identificador del producto a actualizar.                                          |
| `@PROD_NOMBRE`                | `VARCHAR(100)`  | No          | Nuevo nombre del producto.                                                        |
| `@PROD_DESCRIPCION`           | `VARCHAR(100)`  | No          | Nueva descripción del producto.                                                   |
| `@PROD_CATEGORIA_ID`          | `INT`           | No          | Nueva categoría del producto.                                                     |
| `@PROD_IMAGEN`                | `VARCHAR(500)`  | No          | Nueva URL o ruta de imagen.                                                       |
| `@PROD_VALOR_VENTA`           | `DECIMAL(18,2)` | No          | Deprecated. Se conserva por compatibilidad, pero el procedure lo ignora.          |
| `@PROD_PROCENTAJE_IVA`        | `DECIMAL(5,2)`  | No          | Nuevo porcentaje de IVA.                                                          |
| `@PROD_PORCENTAJE_MARGEN_GAN` | `DECIMAL(5,2)`  | No          | Nuevo porcentaje de margen de ganancia.                                           |
| `@PROD_EXTERNAL_ID`           | `VARCHAR(100)`  | No          | Nuevo identificador externo.                                                      |
| `@PROD_NEG_ID`                | `INT`           | No          | Nuevo negocio asociado. Si se envía, debe ser válido y activo.                    |
| `@PROD_ESTADO`                | `CHAR(1)`       | No          | Nuevo estado del producto: `A` o `I`.                                             |
| `@LIMPIAR_IMAGEN`             | `BIT`           | No          | Si es `1`, limpia la imagen y la deja en `NULL`. Por defecto: `0`.                |
| `@LIMPIAR_EXTERNAL_ID`        | `BIT`           | No          | Si es `1`, limpia el identificador externo y lo deja en `NULL`. Por defecto: `0`. |
| `@LIMPIAR_NEGOCIO`            | `BIT`           | No          | Deprecated. No se permite limpiar negocio porque `PROD_NEG_ID` es obligatorio.    |

### Validaciones realizadas

El procedimiento valida que:

1. El identificador del producto sea obligatorio.
2. No se intente limpiar el negocio asociado.
3. El producto exista.
4. Si se envía una nueva categoría, exista y esté activa.
5. Si se envía un nuevo porcentaje de IVA, no sea negativo.
6. Si se envía un nuevo porcentaje de margen, no sea negativo.
7. Si se envía un nuevo negocio, exista y esté activo.
8. El negocio final del producto sea obligatorio.
9. Si se envía un nuevo estado, sea válido: `A` o `I`.
10. No exista otro producto con el mismo nombre dentro del mismo negocio final.
11. No exista otro producto con el mismo identificador externo dentro del mismo negocio final.

### Comportamiento de actualización parcial

Si un parámetro llega como `NULL`, no se modifica el valor actual.

Ejemplo: actualizar solo el nombre del producto.

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @PROD_NOMBRE = 'Camiseta básica premium';
```

### Comportamiento sobre `PROD_VALOR_VENTA`

`PROD_VALOR_VENTA` no se actualiza manualmente.

El parámetro `@PROD_VALOR_VENTA` se conserva temporalmente por compatibilidad, pero el procedure lo ignora.

Después de actualizar los datos del producto, el procedure recalcula `PROD_VALOR_VENTA` usando el costo ponderado actual del inventario y el margen de ganancia.

Si no existe inventario para el producto y negocio, o si el costo ponderado es `0`, el valor venta queda en `0`.

Fórmula aplicada:

```text
PROD_VALOR_VENTA = INV_COSTO_PONDERADO * (1 + PROD_PORCENTAJE_MARGEN_GAN / 100)
```

### Limpieza de campos opcionales

Como `NULL` significa "no actualizar este campo", se usan parámetros explícitos para limpiar campos opcionales.

Limpiar imagen:

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @LIMPIAR_IMAGEN = 1;
```

Limpiar identificador externo:

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @LIMPIAR_EXTERNAL_ID = 1;
```

No se permite limpiar negocio:

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @LIMPIAR_NEGOCIO = 1;
```

Esta operación debe retornar error porque el negocio asociado es obligatorio.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `PROD_ID` | Identificador del producto actualizado.                   |

### Ejemplo de actualización de margen

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @PROD_PORCENTAJE_MARGEN_GAN = 30;
```

Después de esta operación, el valor venta base se recalcula con el costo ponderado vigente del inventario.

### Ejemplo de actualización de varios campos

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @PROD_NOMBRE = 'Camiseta básica premium',
    @PROD_DESCRIPCION = 'Camiseta de algodón premium',
    @PROD_PROCENTAJE_IVA = 19,
    @PROD_PORCENTAJE_MARGEN_GAN = 30,
    @PROD_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                             | PROD_ID
--------|-------------------------------------|--------
1       | Producto actualizado correctamente. | 1
```

---

## 7.5. UspEliminaProducto

### Objetivo

Inactivar un producto mediante borrado lógico.

Este procedimiento no elimina físicamente el registro de la tabla. En su lugar, actualiza el campo `PROD_ESTADO` a `I`.

### Nombre

```sql
dbo.UspEliminaProducto
```

### Parámetros de entrada

| Parámetro  | Tipo  | Obligatorio | Descripción                             |
| ---------- | ----- | ----------- | --------------------------------------- |
| `@PROD_ID` | `INT` | Sí          | Identificador del producto a inactivar. |

### Validaciones realizadas

El procedimiento valida que el producto exista antes de inactivarlo.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `PROD_ID` | Identificador del producto inactivado.                    |

### Ejemplo de uso

```sql
EXEC dbo.UspEliminaProducto
    @PROD_ID = 1;
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                            | PROD_ID
--------|------------------------------------|--------
1       | Producto inactivado correctamente. | 1
```

---

# 8. Flujo recomendado de prueba

Para validar el correcto funcionamiento de los procedimientos, se recomienda ejecutar el siguiente flujo.

## 8.1. Crear un producto

```sql
EXEC dbo.UspCreaProducto
    @PROD_NOMBRE = 'Camiseta básica',
    @PROD_DESCRIPCION = 'Camiseta de algodón',
    @PROD_CATEGORIA_ID = 1,
    @PROD_IMAGEN = 'https://example.com/camiseta.jpg',
    @PROD_PROCENTAJE_IVA = 19,
    @PROD_PORCENTAJE_MARGEN_GAN = 30,
    @PROD_EXTERNAL_ID = 'fake-store-1',
    @PROD_NEG_ID = 1,
    @PROD_CREADO_POR_USUARIO_ID = 1,
    @PROD_ESTADO = 'A';
```

Resultado esperado:

```text
PROD_VALOR_VENTA = 0
```

El producto queda pendiente de costo ponderado hasta que exista inventario.

---

## 8.2. Listar productos

```sql
EXEC dbo.UspListaProductos;
```

---

## 8.3. Consultar un producto específico

```sql
EXEC dbo.UspConsultaProducto
    @PROD_ID = 1;
```

---

## 8.4. Actualizar margen de ganancia

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @PROD_PORCENTAJE_MARGEN_GAN = 35;
```

Si el producto ya tiene inventario con costo ponderado, se recalcula `PROD_VALOR_VENTA`.

---

## 8.5. Actualizar varios campos

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @PROD_NOMBRE = 'Camiseta básica premium',
    @PROD_DESCRIPCION = 'Camiseta de algodón premium',
    @PROD_PROCENTAJE_IVA = 19,
    @PROD_PORCENTAJE_MARGEN_GAN = 30;
```

---

## 8.6. Limpiar imagen

```sql
EXEC dbo.UspActualizaProducto
    @PROD_ID = 1,
    @LIMPIAR_IMAGEN = 1;
```

---

## 8.7. Inactivar producto

```sql
EXEC dbo.UspEliminaProducto
    @PROD_ID = 1;
```

---

## 8.8. Listar solo productos activos

```sql
EXEC dbo.UspListaProductos
    @PROD_ESTADO = 'A';
```

---

# 9. Reglas de negocio aplicadas

## 9.1. Producto por negocio

Todo producto debe estar asociado a un negocio mediante el campo `PROD_NEG_ID`.

`PROD_NEG_ID` es obligatorio y no permite valores `NULL`.

Esto evita ambigüedades en inventario, costo ponderado y valor de venta, ya que el costo del producto depende del negocio y su inventario.

---

## 9.2. Categoría parametrizada

El campo `PROD_CATEGORIA_ID` apunta a `TBL_PARAMETROS`.

Esto permite administrar categorías como:

```text
Categoría Producto:
- Ropa
- Tecnología
- Alimentos
- Servicios
```

---

## 9.3. Prevención de duplicados por nombre

No se permite crear dos productos con el mismo nombre dentro del mismo negocio.

Ejemplo no permitido:

```text
Negocio: 1
Producto: Camiseta básica

Negocio: 1
Producto: Camiseta básica
```

Como `PROD_NEG_ID` es obligatorio, la validación se realiza directamente por negocio.

---

## 9.4. Identificador externo

El campo `PROD_EXTERNAL_ID` permite guardar un identificador proveniente de una integración externa.

Ejemplos:

```text
Código de ecommerce
Código de plataforma externa
Código de importación
SKU externo
```

No se permite duplicar el mismo `PROD_EXTERNAL_ID` dentro del mismo negocio.

---

## 9.5. Valores monetarios y porcentajes

`PROD_VALOR_VENTA` no puede ser editado manualmente.

Los porcentajes de IVA y margen de ganancia no pueden ser negativos.

```text
PROD_VALOR_VENTA >= 0
PROD_PROCENTAJE_IVA >= 0
PROD_PORCENTAJE_MARGEN_GAN >= 0
```

`PROD_VALOR_VENTA` es el valor base antes de IVA.

El IVA se calcula en el proceso de venta.

---

## 9.6. Imagen del producto

El campo `PROD_IMAGEN` debe almacenar una URL o ruta de la imagen del producto.

La base de datos no debe almacenar directamente el archivo de imagen.

Ejemplo válido:

```text
https://cdn.midominio.com/productos/camiseta.jpg
```

---

## 9.7. Borrado lógico

Los productos no se eliminan físicamente de la base de datos.

Cuando se ejecuta `UspEliminaProducto`, el registro queda con estado `I`.

Esto permite conservar trazabilidad y evitar inconsistencias con inventario, compras, ventas o Kardex.

---

## 9.8. Separación entre creación y actualización

La creación y la actualización se manejan con procedures separados:

```text
UspCreaProducto
UspActualizaProducto
```

Esto evita ambigüedad entre insertar un nuevo registro y modificar uno existente.

`UspCreaProducto` exige los datos necesarios para crear un producto nuevo.

`UspActualizaProducto` permite modificar parcialmente un producto existente, conservando los valores actuales de los campos que no se envían.

---

# 10. Manejo de errores

Los procedures de escritura utilizan `TRY/CATCH`.

Si ocurre un error:

1. Se revierte la transacción con `ROLLBACK`.
2. Se retorna una respuesta con `Exitoso = 0`.
3. Se muestra el mensaje del error en el campo `Mensaje`.

Ejemplo:

```text
Exitoso = 0
Mensaje = Ya existe un producto con el mismo nombre para el negocio seleccionado.
```

---

# 11. Consideraciones para Backend

Desde el backend, estos procedures pueden ser consumidos desde la capa de repositorio usando Dapper.

## 11.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales si se requieren
Controller -> Expone endpoint REST
```

---

## 11.2. Endpoints sugeridos

| Método HTTP | Endpoint sugerido     | Procedure              |
| ----------- | --------------------- | ---------------------- |
| `GET`       | `/api/productos`      | `UspListaProductos`    |
| `GET`       | `/api/productos/{id}` | `UspConsultaProducto`  |
| `POST`      | `/api/productos`      | `UspCreaProducto`      |
| `PATCH`     | `/api/productos/{id}` | `UspActualizaProducto` |
| `DELETE`    | `/api/productos/{id}` | `UspEliminaProducto`   |

> Si el backend decide trabajar con actualización completa mediante `PUT`, debe enviar todos los campos. Sin embargo, este procedure está diseñado para actualización parcial, por eso encaja mejor con `PATCH`.

---

## 11.3. Consideraciones sobre `SaleValue`

En backend, `SaleValue` o su equivalente puede mantenerse en DTOs de salida para mostrar el valor base de venta en listados, detalle y selección de producto.

No debe tratarse como dato editable de entrada en creación o actualización.

Si se conserva temporalmente en DTOs de entrada por compatibilidad, el backend no debe usarlo como fuente de verdad.

---

## 11.4. Consideraciones por usuario y negocio

Si el usuario autenticado tiene un `BusinessId`, el backend debe limitar operaciones de producto al negocio del usuario.

Un usuario no global no debería poder:

```text
crear productos para otro negocio
listar productos de otro negocio
consultar productos de otro negocio
actualizar productos de otro negocio
eliminar productos de otro negocio
```

Esta validación debe estar en backend y no depender solo del frontend.

---

# 12. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` o `ALTER PROCEDURE` por separado en DBeaver si la herramienta presenta problemas con separadores de lote.
* Evitar el uso de `GO` en DBeaver si genera errores de sintaxis.
* Mantener los nombres de procedures bajo la convención definida.
* Mantener el borrado lógico como estándar del sistema.
* No consumir directamente la tabla `TBL_PRODUCTOS` desde el backend si ya existen procedures definidos para su operación.
* Crear primero las categorías mediante `TBL_CLASE` y `TBL_PARAMETROS` antes de crear productos.
* Crear primero negocios activos antes de crear productos, porque `PROD_NEG_ID` es obligatorio.
* Crear primero usuarios activos antes de crear productos, porque `PROD_CREADO_POR_USUARIO_ID` es obligatorio.
* Se recomienda que `PROD_IMAGEN` sea `VARCHAR(500)` para guardar URL o ruta de imagen.
* `UspCreaProducto` y `UspActualizaProducto` siguen el estándar de separación entre creación y actualización.
* El recálculo automático de `PROD_VALOR_VENTA` también debe revisarse en los procedures de Kardex, inventario y compras, para garantizar que el valor cambie cuando cambie el costo ponderado.
