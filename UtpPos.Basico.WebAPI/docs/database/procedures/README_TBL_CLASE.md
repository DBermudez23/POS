# README - Procedures de Clases

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para la gestión de la tabla `TBL_CLASE` dentro de la base de datos del Sistema de Ventas.

La tabla `TBL_CLASE` permite definir los grupos o categorías de parametrización del sistema. Cada clase representa un conjunto de valores configurables que posteriormente son almacenados en `TBL_PARAMETROS`.

Ejemplos de clases:

```text
Tipo Documento
Medio de Pago
Categoría Producto
Departamento
Ciudad
Tipo Movimiento Inventario
```

Los procedimientos almacenados definidos permiten listar, consultar, crear, actualizar e inactivar lógicamente registros de clases.

---

## 2. Tabla relacionada

### Tabla principal

```sql
TBL_CLASE
```

### Estructura base

```sql
CLA_ID              INT IDENTITY(1,1) PRIMARY KEY
CLA_DESCRIPCION     VARCHAR(100) NOT NULL
CLA_ES_EDITABLE     CHAR(1) DEFAULT 'S' NOT NULL
CLA_ESTADO          CHAR(1) DEFAULT 'A' NOT NULL
CLA_FECHA_CREACION  DATETIME NOT NULL
```

### Relación con parámetros

La tabla `TBL_CLASE` se relaciona con `TBL_PARAMETROS` de la siguiente forma:

```sql
TBL_PARAMETROS.PARAM_CLASE_ID -> TBL_CLASE.CLA_ID
```

Una clase puede tener muchos parámetros asociados.

---

## 3. Procedures creados

Para la entidad `Clase`, se crearon los siguientes procedimientos almacenados:

```text
UspListaClases
UspConsultaClase
UspCreaClase
UspActualizaClase
UspEliminaClase
```

---

# 4. Consideraciones generales

* No se realiza eliminación física de registros.
* La eliminación se maneja mediante borrado lógico usando el campo `CLA_ESTADO`.
* Los estados válidos son:

  * `A`: Activo.
  * `I`: Inactivo.
* El campo `CLA_ES_EDITABLE` define si una clase puede ser modificada desde operaciones generales.
* Los valores válidos para `CLA_ES_EDITABLE` son:

  * `S`: Sí es editable.
  * `N`: No es editable.
* El procedimiento de creación exige los campos obligatorios para registrar una clase nueva.
* El procedimiento de actualización permite edición parcial.
* Si un campo no se envía en la actualización, conserva su valor actual.
* Los procedures de escritura usan `TRY/CATCH`.
* Los procedures de escritura usan transacciones para garantizar atomicidad.
* Se valida que no existan clases duplicadas por descripción.
* Una clase no editable no puede ser modificada ni eliminada lógicamente.
* Una clase con parámetros activos asociados no puede ser inactivada.

---

# 5. Detalle de procedures

---

## 5.1. UspListaClases

### Objetivo

Listar las clases registradas en el sistema, permitiendo aplicar filtros opcionales por estado, editabilidad o descripción.

### Nombre

```sql
dbo.UspListaClases
```

### Parámetros de entrada

| Parámetro          |           Tipo | Obligatorio | Descripción                                                       |
| ------------------ | -------------: | ----------: | ----------------------------------------------------------------- |
| `@CLA_ESTADO`      |      `CHAR(1)` |          No | Permite filtrar por estado: `A` o `I`.                            |
| `@CLA_ES_EDITABLE` |      `CHAR(1)` |          No | Permite filtrar por editabilidad: `S` o `N`.                      |
| `@CLA_DESCRIPCION` | `VARCHAR(100)` |          No | Permite buscar clases por coincidencia parcial en la descripción. |

### Resultado

Retorna un listado de clases junto con la cantidad de parámetros asociados.

### Campos retornados

| Campo                | Descripción                                  |
| -------------------- | -------------------------------------------- |
| `CLA_ID`             | Identificador de la clase.                   |
| `CLA_DESCRIPCION`    | Descripción de la clase.                     |
| `CLA_ES_EDITABLE`    | Indica si la clase es editable.              |
| `CLA_ESTADO`         | Estado de la clase.                          |
| `CLA_FECHA_CREACION` | Fecha de creación de la clase.               |
| `TOTAL_PARAMETROS`   | Cantidad de parámetros asociados a la clase. |

### Ejemplos de uso

Listar todas las clases:

```sql
EXEC dbo.UspListaClases;
```

Listar solo clases activas:

```sql
EXEC dbo.UspListaClases
    @CLA_ESTADO = 'A';
```

Listar solo clases editables:

```sql
EXEC dbo.UspListaClases
    @CLA_ES_EDITABLE = 'S';
```

Buscar clases por descripción:

```sql
EXEC dbo.UspListaClases
    @CLA_DESCRIPCION = 'Documento';
```

---

## 5.2. UspConsultaClase

### Objetivo

Consultar la información de una clase específica mediante su identificador.

### Nombre

```sql
dbo.UspConsultaClase
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                            |
| --------- | ----: | ----------: | -------------------------------------- |
| `@CLA_ID` | `INT` |          Sí | Identificador de la clase a consultar. |

### Resultado

Retorna la información completa de la clase consultada, incluyendo la cantidad de parámetros asociados.

### Campos retornados

| Campo                | Descripción                                  |
| -------------------- | -------------------------------------------- |
| `CLA_ID`             | Identificador de la clase.                   |
| `CLA_DESCRIPCION`    | Descripción de la clase.                     |
| `CLA_ES_EDITABLE`    | Indica si la clase es editable.              |
| `CLA_ESTADO`         | Estado de la clase.                          |
| `CLA_FECHA_CREACION` | Fecha de creación de la clase.               |
| `TOTAL_PARAMETROS`   | Cantidad de parámetros asociados a la clase. |

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaClase
    @CLA_ID = 1;
```

---

## 5.3. UspCreaClase

### Objetivo

Crear una nueva clase parametrizable.

Este procedimiento se utiliza únicamente para inserción. No actualiza registros existentes.

### Nombre

```sql
dbo.UspCreaClase
```

### Parámetros de entrada

| Parámetro          |           Tipo | Obligatorio | Descripción                                       |
| ------------------ | -------------: | ----------: | ------------------------------------------------- |
| `@CLA_DESCRIPCION` | `VARCHAR(100)` |          Sí | Descripción de la clase.                          |
| `@CLA_ES_EDITABLE` |      `CHAR(1)` |          No | Indica si la clase es editable. Por defecto: `S`. |
| `@CLA_ESTADO`      |      `CHAR(1)` |          No | Estado de la clase. Por defecto: `A`.             |

### Validaciones realizadas

El procedimiento valida que:

1. La descripción de la clase no esté vacía.
2. El campo `CLA_ES_EDITABLE` sea `S` o `N`.
3. El estado sea `A` o `I`.
4. No exista otra clase con la misma descripción.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `CLA_ID`  | Identificador de la clase creada.                         |

### Ejemplo de creación

```sql
EXEC dbo.UspCreaClase
    @CLA_DESCRIPCION = 'Medio de Pago',
    @CLA_ES_EDITABLE = 'S',
    @CLA_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                    | CLA_ID
--------|----------------------------|-------
1       | Clase creada correctamente. | 1
```

### Ejemplo de respuesta con error

```text
Exitoso | Mensaje                                   | CLA_ID
--------|-------------------------------------------|-------
0       | Ya existe una clase con la misma descripción. | NULL
```

---

## 5.4. UspActualizaClase

### Objetivo

Actualizar una clase existente.

Este procedimiento permite edición parcial. Si un campo no se envía, conserva el valor actual almacenado en la base de datos.

### Nombre

```sql
dbo.UspActualizaClase
```

### Parámetros de entrada

| Parámetro          |           Tipo | Obligatorio | Descripción                             |
| ------------------ | -------------: | ----------: | --------------------------------------- |
| `@CLA_ID`          |          `INT` |          Sí | Identificador de la clase a actualizar. |
| `@CLA_DESCRIPCION` | `VARCHAR(100)` |          No | Nueva descripción de la clase.          |
| `@CLA_ES_EDITABLE` |      `CHAR(1)` |          No | Nuevo valor de editabilidad: `S` o `N`. |
| `@CLA_ESTADO`      |      `CHAR(1)` |          No | Nuevo estado de la clase: `A` o `I`.    |

### Validaciones realizadas

El procedimiento valida que:

1. El identificador de la clase sea obligatorio.
2. La clase exista.
3. La clase sea editable.
4. Si se envía `CLA_ES_EDITABLE`, su valor sea `S` o `N`.
5. Si se envía `CLA_ESTADO`, su valor sea `A` o `I`.
6. Si se envía una nueva descripción, no exista otra clase con la misma descripción.

### Comportamiento de actualización parcial

Si un parámetro llega como `NULL`, no se modifica el valor actual.

Ejemplo: si solo se envía `@CLA_DESCRIPCION`, únicamente se actualiza la descripción.

```sql
EXEC dbo.UspActualizaClase
    @CLA_ID = 1,
    @CLA_DESCRIPCION = 'Tipos de Documento';
```

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `CLA_ID`  | Identificador de la clase actualizada.                    |

### Ejemplo de actualización parcial

```sql
EXEC dbo.UspActualizaClase
    @CLA_ID = 1,
    @CLA_DESCRIPCION = 'Tipos de Documento';
```

### Ejemplo de actualización de varios campos

```sql
EXEC dbo.UspActualizaClase
    @CLA_ID = 1,
    @CLA_DESCRIPCION = 'Tipo Documento',
    @CLA_ES_EDITABLE = 'S',
    @CLA_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                         | CLA_ID
--------|---------------------------------|-------
1       | Clase actualizada correctamente. | 1
```

### Ejemplo de respuesta con error

```text
Exitoso | Mensaje                                      | CLA_ID
--------|----------------------------------------------|-------
0       | La clase no es editable y no puede ser modificada. | 1
```

---

## 5.5. UspEliminaClase

### Objetivo

Inactivar una clase mediante borrado lógico.

Este procedimiento no elimina físicamente el registro de la tabla. En su lugar, actualiza el campo `CLA_ESTADO` a `I`.

### Nombre

```sql
dbo.UspEliminaClase
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                            |
| --------- | ----: | ----------: | -------------------------------------- |
| `@CLA_ID` | `INT` |          Sí | Identificador de la clase a inactivar. |

### Validaciones realizadas

El procedimiento valida que:

1. La clase exista.
2. La clase sea editable.
3. La clase no tenga parámetros activos asociados.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `CLA_ID`  | Identificador de la clase inactivada.                     |

### Ejemplo de uso

```sql
EXEC dbo.UspEliminaClase
    @CLA_ID = 1;
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                         | CLA_ID
--------|---------------------------------|-------
1       | Clase inactivada correctamente. | 1
```

### Ejemplo de respuesta con error

```text
Exitoso | Mensaje                                                                  | CLA_ID
--------|--------------------------------------------------------------------------|-------
0       | No se puede inactivar la clase porque tiene parámetros activos asociados. | 1
```

---

# 6. Flujo recomendado de prueba

Para validar el correcto funcionamiento de los procedimientos, se recomienda ejecutar el siguiente flujo.

## 6.1. Crear una clase

```sql
EXEC dbo.UspCreaClase
    @CLA_DESCRIPCION = 'Categoría Producto',
    @CLA_ES_EDITABLE = 'S',
    @CLA_ESTADO = 'A';
```

## 6.2. Listar clases

```sql
EXEC dbo.UspListaClases;
```

## 6.3. Consultar una clase específica

```sql
EXEC dbo.UspConsultaClase
    @CLA_ID = 1;
```

## 6.4. Actualizar parcialmente una clase

```sql
EXEC dbo.UspActualizaClase
    @CLA_ID = 1,
    @CLA_DESCRIPCION = 'Categorías de Producto';
```

## 6.5. Actualizar varios campos de una clase

```sql
EXEC dbo.UspActualizaClase
    @CLA_ID = 1,
    @CLA_DESCRIPCION = 'Categoría Producto',
    @CLA_ES_EDITABLE = 'S',
    @CLA_ESTADO = 'A';
```

## 6.6. Inactivar una clase

```sql
EXEC dbo.UspEliminaClase
    @CLA_ID = 1;
```

> Nota: si la clase tiene parámetros activos asociados, el procedimiento no permitirá inactivarla. Primero se deben inactivar los parámetros asociados mediante `UspEliminaParametro`.

---

# 7. Reglas de negocio aplicadas

## 7.1. Clases parametrizables

Una clase representa un grupo de parámetros relacionados.

Ejemplo:

```text
Clase: Tipo Documento
Parámetros:
- Cédula de ciudadanía
- NIT
- Pasaporte
```

```text
Clase: Medio de Pago
Parámetros:
- Efectivo
- Tarjeta débito
- Transferencia
```

```text
Clase: Tipo Movimiento Inventario
Parámetros:
- Entrada por compra
- Salida por venta
- Ajuste positivo
- Ajuste negativo
```

---

## 7.2. Editabilidad

El campo `CLA_ES_EDITABLE` define si una clase puede ser modificada desde operaciones generales del sistema.

Cuando `CLA_ES_EDITABLE = 'N'`, la clase se considera protegida y no puede ser modificada ni eliminada lógicamente mediante los procedures estándar.

Esto permite proteger clases críticas del sistema, como:

```text
Tipo Movimiento Inventario
Estado Documento
Medio de Pago
```

Dependiendo de las reglas del negocio, estas clases pueden ser administradas solo por perfiles técnicos o scripts controlados.

---

## 7.3. Borrado lógico

Las clases no se eliminan físicamente de la base de datos.

Cuando se ejecuta `UspEliminaClase`, el registro queda con estado `I`.

Esto permite conservar trazabilidad y evitar afectaciones a registros históricos relacionados.

---

## 7.4. Restricción por parámetros activos

Una clase no puede ser inactivada si tiene parámetros activos asociados.

Ejemplo:

```text
Clase: Tipo Documento
Parámetros activos:
- Cédula de ciudadanía
- NIT
```

En este caso, la clase no puede inactivarse hasta que sus parámetros asociados sean inactivados previamente.

---

## 7.5. Prevención de duplicados

No se permite crear dos clases con la misma descripción.

Ejemplo no permitido:

```text
Clase: Medio de Pago
Clase: Medio de Pago
```

---

## 7.6. Separación entre creación y actualización

La creación y la actualización se manejan con procedures separados:

```text
UspCreaClase
UspActualizaClase
```

Esto evita ambigüedad entre insertar un nuevo registro y modificar uno existente.

`UspCreaClase` exige los datos necesarios para crear una clase nueva.

`UspActualizaClase` permite modificar parcialmente una clase existente, conservando los valores actuales de los campos que no se envían.

---

# 8. Manejo de errores

Los procedures de escritura utilizan `TRY/CATCH`.

Si ocurre un error:

1. Se revierte la transacción con `ROLLBACK`.
2. Se retorna una respuesta con `Exitoso = 0`.
3. Se muestra el mensaje del error en el campo `Mensaje`.

Ejemplo:

```text
Exitoso = 0
Mensaje = Ya existe una clase con la misma descripción.
```

---

# 9. Consideraciones para Backend

Desde el Backend, estos procedures pueden ser consumidos desde la capa de repositorio usando Dapper.

## 9.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales si se requieren
Controller -> Expone endpoint REST
```

## 9.2. Endpoints sugeridos

| Método HTTP | Endpoint sugerido  | Procedure           |
| ----------- | ------------------ | ------------------- |
| `GET`       | `/api/clases`      | `UspListaClases`    |
| `GET`       | `/api/clases/{id}` | `UspConsultaClase`  |
| `POST`      | `/api/clases`      | `UspCreaClase`      |
| `PATCH`     | `/api/clases/{id}` | `UspActualizaClase` |
| `DELETE`    | `/api/clases/{id}` | `UspEliminaClase`   |

> Si el Backend decide trabajar con actualización completa mediante `PUT`, debe enviar todos los campos. Sin embargo, este procedure está diseñado para actualización parcial, por eso encaja mejor con `PATCH`.

---

# 10. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` por separado en DBeaver si la herramienta presenta problemas con separadores de lote.
* Evitar el uso de `GO` en DBeaver si genera errores de sintaxis.
* Mantener los nombres de procedures bajo la convención definida.
* Mantener el borrado lógico como estándar del sistema.
* No consumir directamente la tabla `TBL_CLASE` desde el Backend si ya existen procedures definidos para su operación.
* La clase es la entidad padre de la parametrización; por eso se recomienda crear primero las clases y luego los parámetros asociados.
* `UspCreaClase` y `UspActualizaClase` reemplazan el comportamiento anterior de `UspAlmacenaClase`.
