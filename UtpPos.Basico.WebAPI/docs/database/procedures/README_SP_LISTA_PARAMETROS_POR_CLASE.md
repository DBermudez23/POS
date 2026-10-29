# Stored Procedure: UspListaParametrosPorClase

## 1. Objetivo

El procedimiento almacenado `dbo.UspListaParametrosPorClase` permite consultar parámetros asociados a una clase específica dentro del modelo de parametrización general del sistema.

Este SP nace para evitar crear endpoints y procedimientos específicos por cada lista parametrizable, como:

```text
Categorías de producto
Métodos de pago
Tipos de documento
Canales de venta
Motivos de ajuste de inventario
Tipos de tercero
Unidades de medida
```

En lugar de crear un SP por cada caso, se utiliza un único procedimiento genérico que recibe el código técnico de la clase y retorna sus parámetros activos.

---

## 2. Tablas involucradas

El procedimiento trabaja sobre las siguientes tablas:

```text
TBL_CLASE
TBL_PARAMETROS
```

### 2.1. TBL_CLASE

Representa una agrupación lógica de parámetros.

Ejemplos:

```text
CATEGORÍAS DE PRODUCTO
MÉTODOS DE PAGO
TIPOS DE DOCUMENTO
CANALES DE VENTA
```

Campos relevantes:

| Campo             | Descripción                                   |
| ----------------- | --------------------------------------------- |
| `CLA_ID`          | Identificador interno de la clase.            |
| `CLA_CODIGO`      | Código técnico estable de la clase.           |
| `CLA_DESCRIPCION` | Nombre visible de la clase.                   |
| `CLA_ESTADO`      | Estado de la clase. `A` activo, `I` inactivo. |

### 2.2. TBL_PARAMETROS

Representa los valores parametrizables asociados a una clase.

Ejemplos para la clase `CAT-PROD`:

```text
ALIMENTOS
BEBIDAS
ASEO
TECNOLOGÍA
ROPA
```

Campos relevantes:

| Campo               | Descripción                                                |
| ------------------- | ---------------------------------------------------------- |
| `PARAM_ID`          | Identificador interno del parámetro.                       |
| `CLA_ID`            | Identificador de la clase a la que pertenece el parámetro. |
| `PARAM_CODIGO`      | Código técnico estable del parámetro.                      |
| `PARAM_DESCRIPCION` | Nombre visible del parámetro.                              |
| `PARAM_ESTADO`      | Estado del parámetro. `A` activo, `I` inactivo.            |

---

## 3. Códigos técnicos

Se agregó un modelo de códigos técnicos para desacoplar la lógica del sistema de las descripciones visibles.

La descripción puede cambiar sin romper backend, frontend ni integraciones.

Ejemplo:

| Clase                  | `CLA_CODIGO` |
| ---------------------- | ------------ |
| CATEGORÍAS DE PRODUCTO | `CAT-PROD`   |
| MÉTODOS DE PAGO        | `MET-PAGO`   |
| TIPOS DE DOCUMENTO     | `TIP-DOC`    |
| CANALES DE VENTA       | `CAN-VTA`    |

Los parámetros usan como prefijo el código de su clase.

Ejemplo para categorías de producto:

| Parámetro  | `PARAM_CODIGO` |
| ---------- | -------------- |
| ALIMENTOS  | `CAT-PROD-ALI` |
| BEBIDAS    | `CAT-PROD-BEB` |
| ASEO       | `CAT-PROD-ASE` |
| PAPELERÍA  | `CAT-PROD-PAP` |
| TECNOLOGÍA | `CAT-PROD-TEC` |
| ROPA       | `CAT-PROD-ROP` |
| SERVICIOS  | `CAT-PROD-SER` |
| OTROS      | `CAT-PROD-OTR` |

---

## 4. Procedimiento almacenado

Nombre:

```sql
dbo.UspListaParametrosPorClase
```

Responsabilidad:

```text
Listar parámetros asociados a una clase parametrizable usando el código técnico de la clase.
```

---

## 5. Parámetros de entrada

| Parámetro       | Tipo          | Requerido | Descripción                                           |
| --------------- | ------------- | --------- | ----------------------------------------------------- |
| `@CLA_CODIGO`   | `VARCHAR(50)` | Sí        | Código técnico de la clase que se desea consultar.    |
| `@PARAM_ESTADO` | `CHAR(1)`     | No        | Estado de los parámetros a retornar. Por defecto `A`. |

Ejemplo:

```sql
EXEC dbo.UspListaParametrosPorClase
    @CLA_CODIGO = 'CAT-PROD';
```

Ejemplo incluyendo estado:

```sql
EXEC dbo.UspListaParametrosPorClase
    @CLA_CODIGO = 'CAT-PROD',
    @PARAM_ESTADO = 'A';
```

---

## 6. Resultado esperado

El SP debe retornar los parámetros de la clase solicitada.

Campos recomendados para consumo desde backend:

| Campo              | Descripción                                                           |
| ------------------ | --------------------------------------------------------------------- |
| `Id`               | Identificador del parámetro. Corresponde a `PARAM_ID`.                |
| `Code`             | Código técnico del parámetro. Corresponde a `PARAM_CODIGO`.           |
| `Description`      | Descripción visible del parámetro. Corresponde a `PARAM_DESCRIPCION`. |
| `ClassId`          | Identificador de la clase. Corresponde a `CLA_ID`.                    |
| `ClassCode`        | Código técnico de la clase. Corresponde a `CLA_CODIGO`.               |
| `ClassDescription` | Descripción visible de la clase. Corresponde a `CLA_DESCRIPCION`.     |
| `Status`           | Estado del parámetro. Corresponde a `PARAM_ESTADO`.                   |

Ejemplo de resultado para `CAT-PROD`:

| Id | Code           | Description |
| -- | -------------- | ----------- |
| 38 | `CAT-PROD-ALI` | ALIMENTOS   |
| 39 | `CAT-PROD-BEB` | BEBIDAS     |
| 40 | `CAT-PROD-ASE` | ASEO        |
| 41 | `CAT-PROD-PAP` | PAPELERÍA   |
| 42 | `CAT-PROD-TEC` | TECNOLOGÍA  |
| 43 | `CAT-PROD-ROP` | ROPA        |
| 44 | `CAT-PROD-SER` | SERVICIOS   |
| 45 | `CAT-PROD-OTR` | OTROS       |

---

## 7. Uso desde backend

El backend debe consumir este SP desde el repositorio del módulo de parámetros.

Flujo esperado:

```text
ParametrosController
    ↓
ParametroApplication
    ↓
ParametroRepository
    ↓
Dapper
    ↓
dbo.UspListaParametrosPorClase
    ↓
SQL Server
```

El controlador no debe ejecutar SQL directamente.

La capa Application debe retornar la respuesta usando el patrón estándar:

```csharp
Response<T>
```

---

## 8. Endpoint sugerido

El endpoint recomendado para consumir este SP es:

```http
GET /api/parametros/por-clase/{codigoClase}
```

Ejemplo:

```http
GET /api/parametros/por-clase/CAT-PROD
```

Respuesta esperada:

```json
{
  "IsSuccess": true,
  "Data": [
    {
      "Id": 38,
      "Code": "CAT-PROD-ALI",
      "Description": "ALIMENTOS"
    },
    {
      "Id": 42,
      "Code": "CAT-PROD-TEC",
      "Description": "TECNOLOGÍA"
    }
  ],
  "Message": "Parámetros listados correctamente."
}
```

---

## 9. Caso de uso: categorías de producto

El módulo de productos debe consumir las categorías desde el endpoint genérico de parámetros.

El frontend no debe pedir manualmente el ID de la categoría al usuario.

El campo actual de categoría en el formulario de producto debe consumir:

```http
GET /api/parametros/por-clase/CAT-PROD
```

Y mostrar un selector con las descripciones:

```text
ALIMENTOS
BEBIDAS
ASEO
TECNOLOGÍA
ROPA
```

Internamente, el valor seleccionado debe seguir siendo el `PARAM_ID`, ya que:

```sql
TBL_PRODUCTOS.PROD_CATEGORIA_ID
```

referencia:

```sql
TBL_PARAMETROS.PARAM_ID
```

---

## 10. Ventajas del enfoque genérico

Este diseño evita crear un endpoint y un SP por cada lista parametrizable.

En lugar de esto:

```http
GET /api/parametros/categorias-producto
GET /api/parametros/metodos-pago
GET /api/parametros/tipos-documento
GET /api/parametros/canales-venta
```

Se utiliza un único endpoint:

```http
GET /api/parametros/por-clase/{codigoClase}
```

Ejemplos:

```http
GET /api/parametros/por-clase/CAT-PROD
GET /api/parametros/por-clase/MET-PAGO
GET /api/parametros/por-clase/TIP-DOC
GET /api/parametros/por-clase/CAN-VTA
```

Esto permite:

```text
- Reutilizar lógica.
- Reducir código duplicado.
- Evitar endpoints innecesarios.
- Mantener estable el contrato del frontend.
- Permitir cambios de descripción sin romper el sistema.
```

---

## 11. Reglas importantes

```text
- No consultar parámetros por descripción visible desde el frontend.
- No hardcodear CLA_ID en backend ni frontend.
- Usar siempre CLA_CODIGO para consultar una clase parametrizable.
- Usar PARAM_ID como valor seleccionado cuando el parámetro se guarda en otras tablas.
- Usar PARAM_CODIGO para lógica técnica cuando se necesite identificar un parámetro específico.
- La descripción es solo para visualización.
```

---

## 12. Ejemplos de consumo

### 12.1. Categorías de producto

```sql
EXEC dbo.UspListaParametrosPorClase
    @CLA_CODIGO = 'CAT-PROD';
```

### 12.2. Métodos de pago

```sql
EXEC dbo.UspListaParametrosPorClase
    @CLA_CODIGO = 'MET-PAGO';
```

### 12.3. Tipos de documento

```sql
EXEC dbo.UspListaParametrosPorClase
    @CLA_CODIGO = 'TIP-DOC';
```

### 12.4. Canales de venta

```sql
EXEC dbo.UspListaParametrosPorClase
    @CLA_CODIGO = 'CAN-VTA';
```

---

## 13. Validaciones recomendadas

El SP debe validar que se envíe un código de clase.

Si `@CLA_CODIGO` llega vacío o nulo, debe retornar un resultado controlado o no retornar registros.

También se recomienda que el backend maneje los casos donde:

```text
- No existe la clase solicitada.
- La clase está inactiva.
- No existen parámetros activos para la clase.
```

---

## 14. Pendientes

```text
- Ajustar los SP de creación/actualización de clases para manejar CLA_CODIGO.
- Ajustar los SP de creación/actualización de parámetros para manejar PARAM_CODIGO.
- Evaluar si PARAM_CODIGO debe ser obligatorio en una fase posterior.
- Implementar endpoint GET /api/parametros/por-clase/{codigoClase}.
- Conectar el formulario de productos al endpoint usando CAT-PROD.
- Reemplazar el campo numérico de categoría por un MudSelect.
```

---

## 15. Resumen

`dbo.UspListaParametrosPorClase` centraliza la consulta de parámetros por clase técnica.

El sistema deja de depender de descripciones visibles como `CATEGORÍAS DE PRODUCTO` y pasa a depender de códigos técnicos estables como `CAT-PROD`.

Este enfoque mejora la mantenibilidad del módulo de parámetros y permite que otros módulos, como productos, ventas, terceros o inventario, consuman listas parametrizables sin crear endpoints específicos por cada caso.
