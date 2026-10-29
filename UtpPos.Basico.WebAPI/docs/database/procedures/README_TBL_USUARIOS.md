# README - Procedures de Usuarios

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para la gestión de la tabla `TBL_USUARIOS` dentro de la base de datos del Sistema de Ventas.

La tabla `TBL_USUARIOS` permite registrar los usuarios internos que operan el sistema. Estos usuarios pueden participar en procesos administrativos, creación de terceros, productos, compras, ventas, movimientos de inventario y auditoría operativa.

Los procedimientos almacenados definidos permiten listar, consultar, crear, actualizar e inactivar lógicamente registros de usuarios.

---

## 2. Tabla relacionada

### Tabla principal

```sql
TBL_USUARIOS
```

### Estructura base

```sql
USR_ID                 INT IDENTITY(1,1) PRIMARY KEY
USR_NEG_ID             INT NULL
USR_TIPO_DOC_ID        INT NOT NULL
USR_NUMERO_DOCUMENTO   VARCHAR(20) NOT NULL
USR_NOMBRE             VARCHAR(100) NOT NULL
USR_CORREO             VARCHAR(80) NOT NULL
USR_PASSWORD           VARCHAR(255) NOT NULL
USR_TELEFONO           VARCHAR(20) NULL
USR_ESTADO             CHAR(1) DEFAULT 'A' NOT NULL
USR_FECHA_CREACION     DATETIME NOT NULL
```

---

## 3. Relaciones principales

La tabla `TBL_USUARIOS` se relaciona con las siguientes tablas:

```sql
TBL_USUARIOS.USR_NEG_ID -> TBL_NEGOCIO.NEG_ID
TBL_USUARIOS.USR_TIPO_DOC_ID -> TBL_PARAMETROS.PARAM_ID
```

Estas relaciones permiten asociar cada usuario a un negocio y a un tipo de documento parametrizado.

---

## 4. Procedures creados

Para la entidad `Usuarios`, se crearon los siguientes procedimientos almacenados:

```text
UspListaUsuarios
UspConsultaUsuario
UspCreaUsuario
UspActualizaUsuario
UspEliminaUsuario
```

---

# 5. Consideraciones generales

* No se realiza eliminación física de registros.
* La eliminación se maneja mediante borrado lógico usando el campo `USR_ESTADO`.
* Los estados válidos son:

  * `A`: Activo.
  * `I`: Inactivo.
* El procedimiento de creación exige todos los campos obligatorios para registrar un nuevo usuario.
* El procedimiento de actualización permite edición parcial.
* Si un campo no se envía en la actualización, conserva su valor actual.
* El campo `USR_NEG_ID` permite valores `NULL`, por lo que un usuario puede no estar asociado directamente a un negocio.
* El campo `USR_TELEFONO` permite valores `NULL`.
* Los procedures de escritura usan `TRY/CATCH`.
* Los procedures de escritura usan transacciones para garantizar atomicidad.
* Se valida que el negocio exista y esté activo cuando se envía `USR_NEG_ID`.
* Se valida que el tipo de documento exista y esté activo.
* Se evita duplicar usuarios con el mismo correo.
* Se evita duplicar usuarios con el mismo tipo y número de documento dentro del mismo negocio.
* La contraseña no se procesa dentro de la base de datos. La base de datos solo almacena el valor recibido en `USR_PASSWORD`.

---

# 6. Manejo de contraseña

El campo `USR_PASSWORD` debe almacenar una contraseña previamente procesada por el Backend.

La base de datos no debe recibir ni almacenar contraseñas en texto plano.

Flujo esperado:

```text
Contraseña ingresada por el usuario
        ↓
Backend aplica hash seguro
        ↓
Base de datos guarda el hash en USR_PASSWORD
```

Ejemplo conceptual:

```text
Texto plano:       MiPassword123
Hash almacenado:   $2a$12$hash_generado_por_backend...
```

La lógica de hashing, validación de contraseña, comparación de contraseña y recuperación de acceso pertenece al Backend, no a los procedimientos almacenados.

---

# 7. Detalle de procedures

---

## 7.1. UspListaUsuarios

### Objetivo

Listar los usuarios registrados en el sistema, permitiendo aplicar filtros opcionales por negocio, tipo de documento, estado, número de documento, nombre o correo.

### Nombre

```sql
dbo.UspListaUsuarios
```

### Parámetros de entrada

| Parámetro               |           Tipo | Obligatorio | Descripción                                                               |
| ----------------------- | -------------: | ----------: | ------------------------------------------------------------------------- |
| `@USR_NEG_ID`           |          `INT` |          No | Permite filtrar usuarios por negocio.                                     |
| `@USR_TIPO_DOC_ID`      |          `INT` |          No | Permite filtrar usuarios por tipo de documento.                           |
| `@USR_ESTADO`           |      `CHAR(1)` |          No | Permite filtrar por estado: `A` o `I`.                                    |
| `@USR_NUMERO_DOCUMENTO` |  `VARCHAR(20)` |          No | Permite buscar usuarios por coincidencia parcial del número de documento. |
| `@USR_NOMBRE`           | `VARCHAR(100)` |          No | Permite buscar usuarios por coincidencia parcial del nombre.              |
| `@USR_CORREO`           |  `VARCHAR(80)` |          No | Permite buscar usuarios por coincidencia parcial del correo.              |

### Resultado

Retorna un listado de usuarios junto con información descriptiva del negocio y del tipo de documento.

### Campos retornados

| Campo                  | Descripción                          |
| ---------------------- | ------------------------------------ |
| `USR_ID`               | Identificador del usuario.           |
| `USR_NEG_ID`           | Identificador del negocio asociado.  |
| `NEG_NOMBRE`           | Nombre del negocio asociado.         |
| `USR_TIPO_DOC_ID`      | Identificador del tipo de documento. |
| `TIPO_DOCUMENTO`       | Descripción del tipo de documento.   |
| `USR_NUMERO_DOCUMENTO` | Número de documento del usuario.     |
| `USR_NOMBRE`           | Nombre del usuario.                  |
| `USR_CORREO`           | Correo del usuario.                  |
| `USR_TELEFONO`         | Teléfono del usuario.                |
| `USR_ESTADO`           | Estado del usuario.                  |
| `USR_FECHA_CREACION`   | Fecha de creación del usuario.       |

> Por seguridad, este procedure no retorna el campo `USR_PASSWORD`.

### Ejemplos de uso

Listar todos los usuarios:

```sql
EXEC dbo.UspListaUsuarios;
```

Listar usuarios activos:

```sql
EXEC dbo.UspListaUsuarios
    @USR_ESTADO = 'A';
```

Buscar por correo:

```sql
EXEC dbo.UspListaUsuarios
    @USR_CORREO = 'admin';
```

Buscar por nombre:

```sql
EXEC dbo.UspListaUsuarios
    @USR_NOMBRE = 'Daniel';
```

Buscar usuarios de un negocio específico:

```sql
EXEC dbo.UspListaUsuarios
    @USR_NEG_ID = 1;
```

---

## 7.2. UspConsultaUsuario

### Objetivo

Consultar la información de un usuario específico mediante su identificador.

### Nombre

```sql
dbo.UspConsultaUsuario
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                            |
| --------- | ----: | ----------: | -------------------------------------- |
| `@USR_ID` | `INT` |          Sí | Identificador del usuario a consultar. |

### Resultado

Retorna la información completa del usuario consultado, incluyendo información descriptiva del negocio y del tipo de documento.

### Campos retornados

| Campo                  | Descripción                          |
| ---------------------- | ------------------------------------ |
| `USR_ID`               | Identificador del usuario.           |
| `USR_NEG_ID`           | Identificador del negocio asociado.  |
| `NEG_NOMBRE`           | Nombre del negocio asociado.         |
| `USR_TIPO_DOC_ID`      | Identificador del tipo de documento. |
| `TIPO_DOCUMENTO`       | Descripción del tipo de documento.   |
| `USR_NUMERO_DOCUMENTO` | Número de documento del usuario.     |
| `USR_NOMBRE`           | Nombre del usuario.                  |
| `USR_CORREO`           | Correo del usuario.                  |
| `USR_TELEFONO`         | Teléfono del usuario.                |
| `USR_ESTADO`           | Estado del usuario.                  |
| `USR_FECHA_CREACION`   | Fecha de creación del usuario.       |

> Por seguridad, este procedure no retorna el campo `USR_PASSWORD`.

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaUsuario
    @USR_ID = 1;
```

---

## 7.3. UspCreaUsuario

### Objetivo

Crear un nuevo usuario interno del sistema.

Este procedimiento se utiliza únicamente para inserción. No actualiza registros existentes.

### Nombre

```sql
dbo.UspCreaUsuario
```

### Parámetros de entrada

| Parámetro               |           Tipo | Obligatorio | Descripción                                    |
| ----------------------- | -------------: | ----------: | ---------------------------------------------- |
| `@USR_NEG_ID`           |          `INT` |          No | Identificador del negocio asociado.            |
| `@USR_TIPO_DOC_ID`      |          `INT` |          Sí | Identificador del tipo de documento.           |
| `@USR_NUMERO_DOCUMENTO` |  `VARCHAR(20)` |          Sí | Número de documento del usuario.               |
| `@USR_NOMBRE`           | `VARCHAR(100)` |          Sí | Nombre del usuario.                            |
| `@USR_CORREO`           |  `VARCHAR(80)` |          Sí | Correo del usuario.                            |
| `@USR_PASSWORD`         | `VARCHAR(255)` |          Sí | Hash de la contraseña generado por el Backend. |
| `@USR_TELEFONO`         |  `VARCHAR(20)` |          No | Teléfono del usuario.                          |
| `@USR_ESTADO`           |      `CHAR(1)` |          No | Estado del usuario. Por defecto: `A`.          |

### Validaciones realizadas

El procedimiento valida que:

1. El tipo de documento sea obligatorio.
2. El número de documento sea obligatorio.
3. El nombre del usuario sea obligatorio.
4. El correo del usuario sea obligatorio.
5. La contraseña procesada sea obligatoria.
6. El estado sea válido: `A` o `I`.
7. Si se envía negocio, el negocio exista y esté activo.
8. El tipo de documento exista y esté activo.
9. No exista otro usuario con el mismo correo.
10. No exista otro usuario con el mismo negocio, tipo de documento y número de documento.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `USR_ID`  | Identificador del usuario creado.                         |

### Ejemplo de creación

```sql
EXEC dbo.UspCreaUsuario
    @USR_NEG_ID = 1,
    @USR_TIPO_DOC_ID = 1,
    @USR_NUMERO_DOCUMENTO = '1000000001',
    @USR_NOMBRE = 'Usuario Demo',
    @USR_CORREO = 'usuario.demo@test.com',
    @USR_PASSWORD = '$2a$12$hash_de_prueba_generado_en_backend',
    @USR_TELEFONO = '3001234567',
    @USR_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                       | USR_ID
--------|-------------------------------|-------
1       | Usuario creado correctamente. | 1
```

### Ejemplo de respuesta con error

```text
Exitoso | Mensaje                                      | USR_ID
--------|----------------------------------------------|-------
0       | Ya existe un usuario registrado con el mismo correo. | NULL
```

---

## 7.4. UspActualizaUsuario

### Objetivo

Actualizar un usuario existente.

Este procedimiento permite edición parcial. Si un campo no se envía, conserva el valor actual almacenado en la base de datos.

### Nombre

```sql
dbo.UspActualizaUsuario
```

### Parámetros de entrada

| Parámetro               |           Tipo | Obligatorio | Descripción                                                          |
| ----------------------- | -------------: | ----------: | -------------------------------------------------------------------- |
| `@USR_ID`               |          `INT` |          Sí | Identificador del usuario a actualizar.                              |
| `@USR_NEG_ID`           |          `INT` |          No | Nuevo negocio asociado al usuario.                                   |
| `@USR_TIPO_DOC_ID`      |          `INT` |          No | Nuevo tipo de documento.                                             |
| `@USR_NUMERO_DOCUMENTO` |  `VARCHAR(20)` |          No | Nuevo número de documento.                                           |
| `@USR_NOMBRE`           | `VARCHAR(100)` |          No | Nuevo nombre del usuario.                                            |
| `@USR_CORREO`           |  `VARCHAR(80)` |          No | Nuevo correo del usuario.                                            |
| `@USR_PASSWORD`         | `VARCHAR(255)` |          No | Nuevo hash de contraseña generado por el Backend.                    |
| `@USR_TELEFONO`         |  `VARCHAR(20)` |          No | Nuevo teléfono del usuario.                                          |
| `@USR_ESTADO`           |      `CHAR(1)` |          No | Nuevo estado del usuario: `A` o `I`.                                 |
| `@LIMPIAR_NEGOCIO`      |          `BIT` |          No | Si es `1`, limpia el negocio y lo deja en `NULL`. Por defecto: `0`.  |
| `@LIMPIAR_TELEFONO`     |          `BIT` |          No | Si es `1`, limpia el teléfono y lo deja en `NULL`. Por defecto: `0`. |

### Validaciones realizadas

El procedimiento valida que:

1. El identificador del usuario sea obligatorio.
2. El usuario exista.
3. Si se envía un negocio, este exista y esté activo.
4. Si se envía un tipo de documento, este exista y esté activo.
5. Si se envía un estado, sea válido: `A` o `I`.
6. No exista otro usuario con el mismo correo final.
7. No exista otro usuario con la misma combinación final de negocio, tipo de documento y número de documento.

### Comportamiento de actualización parcial

Si un parámetro llega como `NULL`, no se modifica el valor actual.

Ejemplo: si solo se envía `@USR_NOMBRE`, únicamente se actualiza el nombre.

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @USR_NOMBRE = 'Usuario Demo Actualizado';
```

### Actualización de contraseña

La actualización de contraseña debe enviar un nuevo hash generado por el Backend.

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @USR_PASSWORD = '$2a$12$nuevo_hash_generado_en_backend';
```

El procedure no valida la contraseña en texto plano ni compara contraseñas anteriores. Solo reemplaza el valor almacenado en `USR_PASSWORD`.

### Limpieza de negocio

Como `NULL` significa "no actualizar este campo", se usa el parámetro `@LIMPIAR_NEGOCIO` para indicar explícitamente que se desea dejar el negocio en `NULL`.

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @LIMPIAR_NEGOCIO = 1;
```

### Limpieza de teléfono

Como `NULL` significa "no actualizar este campo", se usa el parámetro `@LIMPIAR_TELEFONO` para indicar explícitamente que se desea borrar el teléfono.

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @LIMPIAR_TELEFONO = 1;
```

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `USR_ID`  | Identificador del usuario actualizado.                    |

### Ejemplo de actualización parcial

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @USR_NOMBRE = 'Usuario Demo Actualizado';
```

### Ejemplo de actualización de varios campos

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @USR_NOMBRE = 'Administrador Principal',
    @USR_CORREO = 'admin@test.com',
    @USR_TELEFONO = '3111234567',
    @USR_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                            | USR_ID
--------|------------------------------------|-------
1       | Usuario actualizado correctamente. | 1
```

### Ejemplo de respuesta con error

```text
Exitoso | Mensaje                                      | USR_ID
--------|----------------------------------------------|-------
0       | Ya existe un usuario registrado con el mismo correo. | 1
```

---

## 7.5. UspEliminaUsuario

### Objetivo

Inactivar un usuario mediante borrado lógico.

Este procedimiento no elimina físicamente el registro de la tabla. En su lugar, actualiza el campo `USR_ESTADO` a `I`.

### Nombre

```sql
dbo.UspEliminaUsuario
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                            |
| --------- | ----: | ----------: | -------------------------------------- |
| `@USR_ID` | `INT` |          Sí | Identificador del usuario a inactivar. |

### Validaciones realizadas

El procedimiento valida que el usuario exista antes de inactivarlo.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `USR_ID`  | Identificador del usuario inactivado.                     |

### Ejemplo de uso

```sql
EXEC dbo.UspEliminaUsuario
    @USR_ID = 1;
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                          | USR_ID
--------|----------------------------------|-------
1       | Usuario inactivado correctamente. | 1
```

---

# 8. Flujo recomendado de prueba

Para validar el correcto funcionamiento de los procedimientos, se recomienda ejecutar el siguiente flujo.

## 8.1. Crear un usuario

```sql
EXEC dbo.UspCreaUsuario
    @USR_NEG_ID = 1,
    @USR_TIPO_DOC_ID = 1,
    @USR_NUMERO_DOCUMENTO = '1000000001',
    @USR_NOMBRE = 'Usuario Demo',
    @USR_CORREO = 'usuario.demo@test.com',
    @USR_PASSWORD = '$2a$12$hash_de_prueba_generado_en_backend',
    @USR_TELEFONO = '3001234567',
    @USR_ESTADO = 'A';
```

## 8.2. Listar usuarios

```sql
EXEC dbo.UspListaUsuarios;
```

## 8.3. Consultar un usuario específico

```sql
EXEC dbo.UspConsultaUsuario
    @USR_ID = 1;
```

## 8.4. Actualizar parcialmente un usuario

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @USR_NOMBRE = 'Usuario Demo Actualizado';
```

## 8.5. Actualizar contraseña

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @USR_PASSWORD = '$2a$12$nuevo_hash_generado_en_backend';
```

## 8.6. Limpiar teléfono

```sql
EXEC dbo.UspActualizaUsuario
    @USR_ID = 1,
    @LIMPIAR_TELEFONO = 1;
```

## 8.7. Inactivar usuario

```sql
EXEC dbo.UspEliminaUsuario
    @USR_ID = 1;
```

## 8.8. Listar solo usuarios activos

```sql
EXEC dbo.UspListaUsuarios
    @USR_ESTADO = 'A';
```

---

# 9. Reglas de negocio aplicadas

## 9.1. Usuario por negocio

Un usuario puede estar asociado a un negocio mediante el campo `USR_NEG_ID`.

Como `USR_NEG_ID` permite valores `NULL`, el sistema también puede manejar usuarios globales o administrativos sin negocio asignado.

---

## 9.2. Identificación única por negocio

No se permite crear dos usuarios con el mismo negocio, tipo de documento y número de documento.

Ejemplo no permitido:

```text
Negocio: 1
Tipo Documento: Cédula de ciudadanía
Número Documento: 1000000001

Negocio: 1
Tipo Documento: Cédula de ciudadanía
Número Documento: 1000000001
```

Si `USR_NEG_ID` es `NULL`, la validación trata ese valor como un mismo grupo sin negocio asignado.

---

## 9.3. Correo único

No se permite crear dos usuarios con el mismo correo.

El correo se normaliza a minúsculas antes de almacenarse o compararse.

Ejemplo no permitido:

```text
admin@test.com
ADMIN@test.com
```

Ambos se consideran el mismo correo.

---

## 9.4. Parametrización

El campo `USR_TIPO_DOC_ID` apunta a `TBL_PARAMETROS`.

Esto permite administrar tipos de documento como:

```text
Tipo Documento:
- Cédula de ciudadanía
- NIT
- Pasaporte
- Cédula de extranjería
```

---

## 9.5. Contraseña hasheada

El sistema debe almacenar en `USR_PASSWORD` únicamente el hash de la contraseña.

La validación y generación del hash debe realizarse en el Backend.

El procedure no debe encargarse de:

```text
- Hashear la contraseña.
- Comparar contraseñas.
- Validar complejidad de contraseña.
- Recuperar contraseña.
- Enviar correos de recuperación.
```

---

## 9.6. Borrado lógico

Los usuarios no se eliminan físicamente de la base de datos.

Cuando se ejecuta `UspEliminaUsuario`, el registro queda con estado `I`.

Esto permite conservar trazabilidad y evitar inconsistencias con registros históricos creados por ese usuario.

---

## 9.7. Separación entre creación y actualización

La creación y la actualización se manejan con procedures separados:

```text
UspCreaUsuario
UspActualizaUsuario
```

Esto evita ambigüedad entre insertar un nuevo registro y modificar uno existente.

`UspCreaUsuario` exige los datos necesarios para crear un usuario nuevo.

`UspActualizaUsuario` permite modificar parcialmente un usuario existente, conservando los valores actuales de los campos que no se envían.

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
Mensaje = Ya existe un usuario registrado con el mismo correo.
```

---

# 11. Consideraciones para Backend

Desde el Backend, estos procedures pueden ser consumidos desde la capa de repositorio usando Dapper.

## 11.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales si se requieren
Controller -> Expone endpoint REST
```

## 11.2. Endpoints sugeridos

| Método HTTP | Endpoint sugerido    | Procedure             |
| ----------- | -------------------- | --------------------- |
| `GET`       | `/api/usuarios`      | `UspListaUsuarios`    |
| `GET`       | `/api/usuarios/{id}` | `UspConsultaUsuario`  |
| `POST`      | `/api/usuarios`      | `UspCreaUsuario`      |
| `PATCH`     | `/api/usuarios/{id}` | `UspActualizaUsuario` |
| `DELETE`    | `/api/usuarios/{id}` | `UspEliminaUsuario`   |

> Si el Backend decide trabajar con actualización completa mediante `PUT`, debe enviar todos los campos. Sin embargo, este procedure está diseñado para actualización parcial, por eso encaja mejor con `PATCH`.

---

# 12. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` por separado en DBeaver si la herramienta presenta problemas con separadores de lote.
* Evitar el uso de `GO` en DBeaver si genera errores de sintaxis.
* Mantener los nombres de procedures bajo la convención definida.
* Mantener el borrado lógico como estándar del sistema.
* No consumir directamente la tabla `TBL_USUARIOS` desde el Backend si ya existen procedures definidos para su operación.
* No retornar `USR_PASSWORD` en procedures de listado o consulta.
* El campo `USR_PASSWORD` fue definido como `VARCHAR(255)` para permitir almacenar hashes de diferentes algoritmos.
* La seguridad de contraseña debe manejarse desde el Backend.
