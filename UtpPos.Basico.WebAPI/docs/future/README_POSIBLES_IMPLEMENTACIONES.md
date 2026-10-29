# README - Roadmap Técnico Futuro

## 1. Objetivo

Este documento resume funcionalidades técnicas y decisiones arquitectónicas que se planean implementar más adelante en el backend del Sistema de Ventas.

La idea es dejar registradas decisiones futuras importantes para no perder el rumbo del proyecto, evitar soluciones improvisadas y mantener coherencia con la arquitectura por capas del sistema.

El proyecto trabaja actualmente con:

```text
ASP.NET Core Web API
SQL Server
Dapper
Stored Procedures
Arquitectura en capas
Blazor / MudBlazor en frontend
```

---

## 2. Redis

### Objetivo

Redis puede implementarse más adelante para mejorar rendimiento, seguridad y control de sesiones.

No es obligatorio en la primera versión, pero sería útil cuando el sistema crezca y aumente la cantidad de usuarios, permisos y peticiones concurrentes.

---

### 2.1. Caché de permisos

Guardar temporalmente los permisos de un usuario para no consultar la base de datos en cada request protegido.

Ejemplo de llave:

```text
user:15:permissions
```

Contenido esperado:

```text
usuarios.listar
usuarios.crear
ventas.registrar
productos.consultar
```

Beneficio:

```text
Menos consultas a SQL Server
Mayor velocidad en endpoints protegidos
Mejor rendimiento general
```

---

### 2.2. Revocación rápida de tokens

Redis puede usarse para guardar tokens revocados o sesiones cerradas.

Ejemplo:

```text
jwt:blacklist:{tokenId}
```

Beneficio:

```text
Cerrar sesión de forma más rápida
Invalidar tokens antes de que expiren
Mejor control de seguridad
```

---

### 2.3. Rate limiting

Redis también puede servir para controlar intentos excesivos de login o consumo abusivo de endpoints.

Ejemplo:

```text
login_attempts:usuario@correo.com
```

Beneficio:

```text
Reducir ataques de fuerza bruta
Proteger endpoints críticos
Controlar abuso del sistema
```

---

## 3. Migraciones

### Objetivo

Implementar un sistema formal de migraciones para controlar cambios de base de datos.

Actualmente el proyecto trabaja con scripts SQL manuales, pero a futuro conviene tener una herramienta que registre qué scripts ya fueron ejecutados.

Esto ayudaría a manejar de forma ordenada cambios como:

```text
Creación de tablas
Alteración de columnas
Creación o modificación de Stored Procedures
Creación de vistas
Seeds iniciales
Fixes de datos
Índices
Restricciones
```

---

## 4. Opciones para migraciones

### 4.1. Migrador propio

Crear una herramienta propia similar a Yii2 migrations.

Podría manejar archivos con métodos conceptuales:

```text
Up
Down
SafeUp
SafeDown
```

También podría registrar migraciones ejecutadas en una tabla:

```sql
TBL_MIGRACIONES
```

Esta opción da control total, pero requiere más desarrollo y mantenimiento.

---

### 4.2. DbUp

DbUp es una opción sencilla para proyectos .NET que usan SQL scripts.

Ventajas:

```text
Permite ejecutar scripts SQL ordenados
Registra cuáles ya se ejecutaron
Funciona bien con Dapper y Stored Procedures
No obliga a usar Entity Framework
Encaja bien con el enfoque actual del proyecto
```

---

### 4.3. FluentMigrator

FluentMigrator permite crear migraciones en C#.

Ventajas:

```text
Migraciones versionadas
Soporte para Up y Down
Mayor estructura para cambios de tablas
```

Desventaja:

```text
Puede ser más pesado que DbUp para un proyecto basado principalmente en scripts SQL y Stored Procedures.
```

---

## 5. Estructura futura sugerida para scripts

```text
database/
├── migrations/
│   ├── 202606180001_create_rbac_tables.sql
│   ├── 202606180002_create_rbac_views.sql
│   ├── 202606180003_create_rbac_procedures.sql
│   └── 202606180004_seed_rbac_base.sql
├── procedures/
├── views/
├── seeds/
└── fixes/
```

---

## 6. Deuda técnica: Cloudinary e IFormFile

### Contexto

Se implementó una integración inicial con Cloudinary para permitir la carga de imágenes de productos.

El endpoint backend actual permite subir imágenes y retornar información como:

```text
Url
PublicId
OriginalFileName
SizeBytes
ContentType
```

El campo de producto debe guardar únicamente la URL o ruta de la imagen, no el archivo físico.

Ejemplo:

```text
TBL_PRODUCTOS.PROD_IMAGEN
```

---

### Deuda técnica actual

Para implementar rápidamente la subida de imágenes, se utilizó `IFormFile`.

`IFormFile` pertenece a ASP.NET Core y obligó a agregar referencias de framework relacionadas con ASP.NET Core en proyectos internos de la solución.

Esto permitió compilar y avanzar, pero genera una deuda técnica porque algunas capas internas quedan acopladas a tipos propios de la capa web.

La deuda técnica principal es:

```text
Application e Infrastructure conocen IFormFile.
IFormFile pertenece a Microsoft.AspNetCore.Http.
La abstracción de archivo no debería depender directamente de ASP.NET Core.
```

---

### Problema arquitectónico

La arquitectura ideal debería evitar que capas internas dependan de tipos web.

La capa WebAPI sí puede recibir:

```csharp
IFormFile
```

Pero luego debería transformar ese archivo en una abstracción propia del sistema, por ejemplo:

```csharp
FileUploadRequest
```

o:

```csharp
UploadFileInput
```

De esa manera, Application e Infrastructure trabajarían con un contrato propio y no con una interfaz de ASP.NET Core.

---

### Implementación futura recomendada

Crear una abstracción interna similar a:

```csharp
public sealed class FileUploadRequest
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public Stream Content { get; set; } = Stream.Null;
}
```

Flujo ideal futuro:

```text
UploadController
    ↓ recibe IFormFile
    ↓ convierte a FileUploadRequest
FileUploadApplication
    ↓ trabaja con FileUploadRequest
IFileStorageService
    ↓ guarda archivo
CloudinaryFileStorageService
    ↓ usa Cloudinary
```

---

### Beneficios del ajuste

```text
Menor acoplamiento con ASP.NET Core
Application no depende de tipos web
Infrastructure puede cambiar Cloudinary por otro proveedor
Mejor testabilidad
Mejor separación de responsabilidades
```

---

### Estado actual

```text
Implementado:
- Endpoint de subida de imágenes de producto.
- Integración con Cloudinary.
- Respuesta con URL, PublicId y metadata del archivo.
- Guardado de URL en el campo de imagen del producto.

Pendiente:
- Reemplazar IFormFile en Application/Infrastructure por una abstracción propia.
- Mantener IFormFile solo en la capa WebAPI.
- Revisar referencias Microsoft.AspNetCore.App agregadas en proyectos internos.
- Documentar mejor la estrategia de almacenamiento de archivos.
```

---

## 7. Gestión de secretos

### Objetivo

Evitar que claves sensibles queden guardadas en archivos versionados.

La integración con Cloudinary requiere valores como:

```text
CloudName
ApiKey
ApiSecret
ProductFolder
```

Estos valores no deberían quedar en `appsettings.json` ni en archivos que se suban al repositorio.

---

### Implementación futura recomendada

Usar variables de entorno o `dotnet user-secrets` en desarrollo.

Ejemplo:

```bash
dotnet user-secrets set "Cloudinary:CloudName" "TU_CLOUD_NAME"
dotnet user-secrets set "Cloudinary:ApiKey" "TU_API_KEY"
dotnet user-secrets set "Cloudinary:ApiSecret" "TU_API_SECRET"
dotnet user-secrets set "Cloudinary:ProductFolder" "ventaspro/products"
```

Para ambientes productivos, usar variables de entorno, secretos del proveedor cloud o un vault.

---

## 8. Inventario y Kardex

### Objetivo

Implementar el módulo completo de inventario y Kardex.

Este módulo será fundamental para controlar existencias, movimientos, costos y trazabilidad de productos.

---

### Regla principal

El inventario no debe modificarse directamente desde cualquier módulo.

Los cambios de stock deben quedar trazados mediante movimientos de Kardex.

Ejemplos de movimientos:

```text
Compra
Venta
Ajuste de entrada
Ajuste de salida
Inventario inicial
Devolución de cliente
Devolución a proveedor
Anulación de compra
Anulación de venta
```

---

### Kardex no debe ser CRUD tradicional

Kardex debe manejarse como histórico.

No debería existir un update/delete tradicional de movimientos de Kardex.

Si hay un error, se debe registrar un movimiento compensatorio o de ajuste.

Regla:

```text
No se borra historia.
Se compensa con nuevos movimientos.
```

---

### Pendiente

```text
Implementar registro formal de movimientos.
Calcular saldos anteriores y actuales.
Actualizar inventario desde Kardex.
Registrar costo ponderado anterior y actual.
Manejar inventario inicial.
Manejar ajustes.
Manejar devoluciones.
Evitar modificaciones directas de stock sin trazabilidad.
```

---

## 9. Precio de venta, costo ponderado y margen

### Contexto

El precio de venta no debería definirse manualmente en la creación básica del producto.

El producto puede tener un margen de utilidad configurado, pero el precio de venta real o sugerido depende del costo del inventario.

---

### Regla esperada

El precio de venta debe calcularse a partir de:

```text
Costo ponderado actual
Margen de utilidad
IVA si aplica
```

Ejemplo:

```text
Costo ponderado: 10.000
Margen: 30%

Precio sugerido = 10.000 * 1.30
Precio sugerido = 13.000
```

---

### Responsabilidad de cada módulo

```text
Productos:
- Datos base del producto.
- Categoría.
- Imagen.
- IVA.
- Margen.
- SKU / External ID.
- Estado.

Inventario/Kardex:
- Stock.
- Costo ponderado.
- Último costo.
- Movimientos.
- Entradas.
- Salidas.

Ventas:
- Precio usado en la venta.
- Descuentos.
- Promociones.
- Lista de precios si aplica.
```

---

### Pendiente

```text
Retirar o replantear el campo manual de valor de venta en la creación de productos.
Mostrar precio sugerido solo cuando exista inventario/costo.
Calcular precio desde costo ponderado + margen.
Evaluar listas de precios por cliente, negocio o sede.
Evaluar promociones y precios especiales.
```

---

## 10. Parametrización general

### Objetivo

Evitar crear endpoints y Stored Procedures específicos por cada lista parametrizable.

El sistema ya cuenta con:

```text
TBL_CLASE
TBL_PARAMETROS
```

Por eso las listas como categorías de producto, métodos de pago, tipos de documento o canales de venta deben consumirse desde un mecanismo genérico.

---

### Enfoque recomendado

Usar códigos técnicos estables:

```text
TBL_CLASE.CLA_CODIGO
TBL_PARAMETROS.PARAM_CODIGO
```

Ejemplos:

```text
CAT-PROD
MET-PAGO
TIP-DOC
CAN-VTA
```

Los parámetros deben usar el código de la clase como prefijo:

```text
CAT-PROD-ALI
CAT-PROD-BEB
CAT-PROD-TEC
MET-PAGO-EFE
MET-PAGO-TCR
```

---

### Endpoint genérico esperado

```http
GET /api/parametros/por-clase/{codigoClase}
```

Ejemplo:

```http
GET /api/parametros/por-clase/CAT-PROD
```

---

### Pendiente

```text
Actualizar SP de creación/actualización de clases para manejar CLA_CODIGO.
Actualizar SP de creación/actualización de parámetros para manejar PARAM_CODIGO.
Hacer obligatorios los códigos técnicos cuando la migración esté consolidada.
Evitar consultas por descripción visible.
Evitar hardcodear CLA_ID en backend o frontend.
```

---

## 11. RBAC, sesiones y seguridad

### Objetivo

Continuar fortaleciendo el módulo de autenticación, autorización y sesiones.

El sistema debe proteger endpoints usando permisos y roles.

---

### Mejoras futuras

```text
Gestión administrativa completa de roles.
Asignación de permisos a roles desde interfaz.
Asignación de roles a usuarios desde interfaz.
Gestión administrativa de sesiones.
Cierre de sesión por dispositivo.
Cierre de sesión global.
Revocación de refresh tokens.
Cambio de contraseña.
Recuperación de contraseña.
Política fuerte de contraseñas.
Bloqueo por intentos fallidos.
Auditoría de accesos.
```

---

### Redis como complemento

Redis puede complementar RBAC y sesiones para:

```text
Cachear permisos.
Revocar tokens rápidamente.
Controlar intentos de login.
Aplicar rate limiting.
```

---

## 12. Compras

### Objetivo

Implementar el módulo completo de compras para registrar entradas de productos, costos, proveedores e impacto en inventario.

---

### Relación con inventario

Las compras deben generar movimientos de Kardex y actualizar inventario.

Flujo esperado:

```text
Compra
    ↓
Detalle de compra
    ↓
Movimiento de Kardex
    ↓
Actualización de inventario
    ↓
Recalculo de costo ponderado
```

---

### Pendiente

```text
Registrar compra.
Registrar detalle de compra.
Validar proveedor.
Calcular IVA por producto.
Actualizar Kardex.
Actualizar inventario.
Recalcular costo ponderado.
Permitir anulación de compra con movimiento compensatorio.
```

---

## 13. Ventas

### Objetivo

Implementar el módulo completo de ventas para registrar salidas de productos, pagos, clientes y afectación de inventario.

---

### Relación con inventario

Toda venta debe generar salida de inventario mediante Kardex.

Flujo esperado:

```text
Venta
    ↓
Detalle de venta
    ↓
Movimiento de Kardex
    ↓
Actualización de inventario
```

---

### Pendiente

```text
Registrar venta.
Registrar detalle de venta.
Validar stock disponible.
Manejar formas de pago.
Manejar ventas a crédito.
Manejar anticipos.
Manejar saldos a favor.
Registrar anulaciones.
Registrar devoluciones.
Actualizar inventario por Kardex.
```

---

## 14. Listas de precios y promociones

### Objetivo

Separar el precio base o sugerido del producto de reglas comerciales específicas.

---

### Casos futuros

```text
Lista de precios por cliente.
Lista de precios por negocio.
Lista de precios por sede.
Promociones temporales.
Descuentos por cantidad.
Precios especiales.
```

Este módulo debe implementarse después de tener consolidado inventario, Kardex, compras y ventas.

---

## 15. Parametrización contable y tributaria

### Objetivo

Preparar el sistema para configuraciones contables y tributarias futuras.

---

### Pendiente

```text
Parametrización contable por operación.
Parametrización tributaria.
Manejo formal de IVA.
Manejo de retenciones si aplica.
Resoluciones de facturación.
Estados de resolución activa o vencida.
Trazabilidad de factura.
Documentos electrónicos.
Documento soporte.
Nómina electrónica si el alcance futuro lo requiere.
```

Estos temas no deben mezclarse todavía con el flujo inicial de productos/inventario/ventas.

---

## 16. Auditoría y trazabilidad

### Objetivo

Registrar acciones importantes realizadas por usuarios dentro del sistema.

---

### Casos recomendados

```text
Creación de productos.
Actualización de productos.
Cambios de estado.
Inicio de sesión.
Cierre de sesión.
Registro de compras.
Registro de ventas.
Ajustes de inventario.
Anulaciones.
Cambios de permisos.
Cambios de roles.
```

---

### Pendiente

```text
Definir tabla de auditoría.
Definir operaciones auditables.
Registrar usuario, fecha, operación, entidad afectada y valores relevantes.
Evitar auditoría excesiva en operaciones de consulta.
```

---

## 17. Estructura futura de documentación

### Objetivo

Mantener documentación técnica por módulo y por procedimiento importante.

---

### Estructura sugerida

```text
docs/
├── database/
│   ├── procedures/
│   ├── views/
│   ├── tables/
│   └── migrations/
├── auth/
├── rbac/
├── products/
├── inventory/
├── kardex/
├── purchases/
├── sales/
├── uploads/
└── roadmap/
```

---

## 18. Resumen de pendientes principales

```text
Redis para permisos, sesiones, revocación y rate limiting.
Migraciones formales con DbUp, FluentMigrator o migrador propio.
Refactor de Cloudinary para remover IFormFile de capas internas.
Gestión segura de secretos.
Módulo completo de inventario.
Módulo completo de Kardex.
Cálculo correcto de costo ponderado.
Precio de venta calculado desde costo + margen.
Módulo de compras.
Módulo de ventas.
Listas de precios y promociones.
Parametrización contable y tributaria.
Auditoría y trazabilidad.
Mejoras administrativas de RBAC y sesiones.
Documentación técnica por módulo.
```
