# Carga de Imágenes de Productos

## 1. Objetivo

Este documento describe la implementación actual del flujo de carga de imágenes para productos dentro del backend del sistema POS / Sistema de Ventas.

El objetivo de esta funcionalidad es permitir que el backend reciba una imagen enviada desde el frontend, la cargue en Cloudinary y retorne una URL segura que posteriormente se almacena en el campo:

```sql
TBL_PRODUCTOS.PROD_IMAGEN
```

Este campo no almacena el archivo físico, ni binarios, ni base64. Solo almacena la URL final de la imagen.

---

## 2. Estado actual

La integración con Cloudinary fue implementada correctamente y el endpoint de carga ya responde de forma funcional.

Endpoint implementado:

```http
POST /api/uploads/product-images
```

El endpoint recibe una imagen mediante `multipart/form-data` usando el campo:

```text
file
```

Ejemplo de respuesta real obtenida:

```json
{
  "IsSuccess": true,
  "Data": {
    "Url": "https://res.cloudinary.com/<cloud-name>/image/upload/v1782319012/ventaspro/products/wu5ftpak8xm0apyshpnx.png",
    "PublicId": "ventaspro/products/wu5ftpak8xm0apyshpnx",
    "OriginalFileName": "Fondo.png",
    "SizeBytes": 359318,
    "ContentType": "image/png"
  },
  "Message": "Imagen subida correctamente."
}
```

---

## 3. Flujo funcional actual

El flujo implementado actualmente es el siguiente:

```text
Frontend selecciona una imagen
        ↓
Frontend envía la imagen al backend como multipart/form-data
        ↓
UploadController recibe el archivo como IFormFile
        ↓
FileUploadApplication procesa la solicitud
        ↓
IFileStorageService define el contrato de almacenamiento
        ↓
CloudinaryFileStorageService sube la imagen a Cloudinary
        ↓
Cloudinary retorna una URL segura
        ↓
Backend retorna Response<FileUploadResultDto>
        ↓
Frontend usa la URL en ProductCreateDto.Image o ProductUpdateDto.Image
        ↓
El backend guarda la URL en TBL_PRODUCTOS.PROD_IMAGEN
```

Flujo actual de capas:

```text
UploadController
    ↓
FileUploadApplication
    ↓
IFileStorageService
    ↓
CloudinaryFileStorageService
    ↓
Cloudinary
```

---

## 4. Endpoint

### 4.1. Ruta

```http
POST /api/uploads/product-images
```

### 4.2. Tipo de solicitud

La solicitud debe enviarse como:

```text
multipart/form-data
```

### 4.3. Campo requerido

```text
file
```

### 4.4. Ejemplo en Postman

Headers:

```http
Authorization: Bearer <ACCESS_TOKEN>
```

Body:

```text
form-data
key: file
type: File
value: imagen seleccionada
```

---

## 5. Respuesta del endpoint

La respuesta usa el formato estándar del sistema:

```csharp
Response<FileUploadResultDto>
```

Estructura esperada:

```json
{
  "IsSuccess": true,
  "Data": {
    "Url": "https://res.cloudinary.com/...",
    "PublicId": "ventaspro/products/...",
    "OriginalFileName": "image.jpg",
    "SizeBytes": 123456,
    "ContentType": "image/jpeg"
  },
  "Message": "Imagen subida correctamente."
}
```

### 5.1. Campos retornados

| Campo              | Descripción                                                                                                                       |
| ------------------ | --------------------------------------------------------------------------------------------------------------------------------- |
| `Url`              | URL segura de la imagen cargada en Cloudinary. Es el valor que debe guardarse en `PROD_IMAGEN`.                                   |
| `PublicId`         | Identificador público generado por Cloudinary para la imagen. Sirve para futuras operaciones como eliminar o reemplazar imágenes. |
| `OriginalFileName` | Nombre original del archivo enviado por el usuario.                                                                               |
| `SizeBytes`        | Tamaño del archivo en bytes.                                                                                                      |
| `ContentType`      | Tipo MIME del archivo enviado.                                                                                                    |

---

## 6. Relación con productos

El endpoint de upload no crea ni actualiza productos directamente.

La responsabilidad del endpoint es únicamente:

```text
Recibir imagen → subir imagen a Cloudinary → retornar URL
```

Luego, el frontend debe usar la URL retornada en alguno de los siguientes DTOs:

```csharp
ProductCreateDto.Image
ProductUpdateDto.Image
```

Estos campos finalmente se almacenan en:

```sql
TBL_PRODUCTOS.PROD_IMAGEN
```

Ejemplo:

```json
{
  "Name": "Producto de prueba",
  "Description": "Producto con imagen",
  "CategoryId": 1,
  "Image": "https://res.cloudinary.com/<cloud-name>/image/upload/v1782319012/ventaspro/products/wu5ftpak8xm0apyshpnx.png",
  "SaleValue": 45000,
  "VatPercentage": 19,
  "ProfitMarginPercentage": 30,
  "BusinessId": 1,
  "CreatedByUserId": 1,
  "Status": "A"
}
```

---

## 7. Validaciones implementadas

La carga de imágenes valida actualmente:

```text
- El archivo es obligatorio.
- El tamaño máximo permitido es de 3 MB.
- Solo se permiten imágenes con los siguientes tipos MIME:
  - image/jpeg
  - image/png
  - image/webp
```

Si alguna validación falla, la aplicación retorna una respuesta con:

```json
{
  "IsSuccess": false,
  "Data": null,
  "Message": "Mensaje de error correspondiente"
}
```

---

## 8. Seguridad

El endpoint requiere autenticación.

Actualmente se protege con el permiso:

```text
productos.crear
```

Esto permite que usuarios con permiso para crear productos puedan cargar imágenes de producto.

### 8.1. Pendiente de autorización

En una fase posterior, este endpoint debería permitir acceso si el usuario tiene alguno de estos permisos:

```text
productos.crear
productos.actualizar
```

Es decir, debería funcionar como una validación tipo:

```text
productos.crear OR productos.actualizar
```

Actualmente no se implementó esta lógica porque el sistema de autorización existente no soporta todavía validaciones OR de permisos de forma explícita.

No se recomienda agregar dos atributos `[RequirePermission]` esperando un comportamiento OR, ya que en ASP.NET Core varios requisitos de autorización normalmente pueden terminar exigiendo que se cumplan todos, no solo uno.

---

## 9. Configuración de Cloudinary

La integración usa la sección de configuración:

```json
{
  "Cloudinary": {
    "CloudName": "",
    "ApiKey": "",
    "ApiSecret": "",
    "ProductFolder": "ventaspro/products"
  }
}
```

### 9.1. ProductFolder

La propiedad:

```json
"ProductFolder": "ventaspro/products"
```

indica la carpeta lógica dentro de Cloudinary donde se almacenarán las imágenes de productos.

Ejemplo de `PublicId` generado:

```text
ventaspro/products/wu5ftpak8xm0apyshpnx
```

Esto permite mantener organizadas las imágenes dentro de Cloudinary por dominio funcional.

---

## 10. Manejo de secretos

Las credenciales reales de Cloudinary no deben quedar versionadas en el repositorio.

No se deben hardcodear valores reales en:

```text
appsettings.json
appsettings.Development.json
```

Para entorno local se recomienda usar:

```text
User Secrets
```

Ejemplo:

```bash
dotnet user-secrets init --project .\UtpPos.WebAPI\UtpPos.WebAPI.csproj

dotnet user-secrets set "Cloudinary:CloudName" "TU_CLOUD_NAME" --project .\UtpPos.WebAPI\UtpPos.WebAPI.csproj
dotnet user-secrets set "Cloudinary:ApiKey" "TU_API_KEY" --project .\UtpPos.WebAPI\UtpPos.WebAPI.csproj
dotnet user-secrets set "Cloudinary:ApiSecret" "TU_API_SECRET" --project .\UtpPos.WebAPI\UtpPos.WebAPI.csproj
dotnet user-secrets set "Cloudinary:ProductFolder" "ventaspro/products" --project .\UtpPos.WebAPI\UtpPos.WebAPI.csproj
```

Para producción se recomienda usar variables de entorno o secretos del proveedor de despliegue.

Ejemplo de variables de entorno:

```text
Cloudinary__CloudName
Cloudinary__ApiKey
Cloudinary__ApiSecret
Cloudinary__ProductFolder
```

---

## 11. Paquetes y referencias

### 11.1. CloudinaryDotNet

El paquete `CloudinaryDotNet` debe permanecer únicamente en el proyecto de infraestructura donde se implementa el proveedor concreto:

```text
UtpPos.Framework.InfraStructure.Repository
```

Esto evita que las capas superiores dependan directamente del SDK de Cloudinary.

### 11.2. Microsoft.AspNetCore.App

Para este MVP se usa `IFormFile` directamente en el flujo de carga.

Por esa razón, los proyectos de clase que usan `IFormFile` requieren la referencia:

```xml
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

Esta referencia fue agregada a los proyectos donde fue necesario para compilar el flujo actual.

---

## 12. Deuda técnica actual

La implementación actual fue realizada como una solución MVP y pragmática.

Actualmente `IFormFile` se usa más allá del Controller, llegando a capas internas como Application e Infrastructure.

Esto permite avanzar rápido y mantener un flujo fácil de entender, pero introduce una deuda técnica:

```text
Las capas internas quedan acopladas a tipos propios de ASP.NET Core.
```

En una arquitectura más estricta, `IFormFile` debería existir únicamente en la capa WebAPI, porque representa un detalle del transporte HTTP.

### 12.1. Riesgo

El principal riesgo es que interfaces internas como `IFileUploadApplication` o `IFileStorageService` dependan directamente de ASP.NET Core.

Esto reduce la independencia de las capas y dificulta reutilizar la lógica de carga desde otros orígenes no HTTP, como:

```text
- Procesos de consola
- Jobs en background
- Cargas desde integraciones externas
- Pruebas unitarias más aisladas
```

### 12.2. Decisión actual

Para esta fase del proyecto, esta deuda técnica se considera aceptable porque:

```text
- El objetivo actual es validar funcionalmente la carga de imágenes.
- El backend ya expone la funcionalidad por HTTP.
- El flujo es más simple de entender e implementar.
- No se requiere todavía soportar múltiples proveedores de almacenamiento.
- La funcionalidad ya responde correctamente con una URL real de Cloudinary.
```

### 12.3. Mejora futura

En una fase posterior se recomienda reemplazar el uso de `IFormFile` en capas internas por un modelo propio del sistema, por ejemplo:

```text
FileUploadRequest
```

Ese modelo podría contener:

```text
- Nombre del archivo
- Tipo de contenido
- Tamaño
- Contenido del archivo
```

De esta forma, solo el Controller conocería `IFormFile`, y Application/Infrastructure trabajarían con un contrato propio del sistema.

---

## 13. Pendientes y mejoras futuras

### 13.1. Autorización OR

Implementar soporte para permisos alternativos:

```text
productos.crear OR productos.actualizar
```

Esto permitiría que el endpoint de imágenes pueda usarse tanto en creación como en edición de productos.

### 13.2. Validación por firma real del archivo

Actualmente se valida el tipo MIME reportado por el archivo.

Como mejora futura, se recomienda validar también la firma real del archivo, conocida como magic bytes, para evitar archivos maliciosos con extensiones o content types falsificados.

### 13.3. Validación de configuración al iniciar

Actualmente la configuración de Cloudinary se usa desde `appsettings`, `user-secrets` o variables de entorno.

Como mejora futura, se recomienda validar al inicio de la aplicación que existan los valores requeridos:

```text
CloudName
ApiKey
ApiSecret
ProductFolder
```

### 13.4. Manejo de eliminación/reemplazo de imágenes

El endpoint ya retorna `PublicId`, pero actualmente el producto solo almacena la URL en `PROD_IMAGEN`.

Como mejora futura se podría agregar una columna adicional:

```sql
PROD_IMAGEN_PUBLIC_ID VARCHAR(255) NULL
```

Esto facilitaría eliminar o reemplazar imágenes antiguas en Cloudinary cuando un producto cambie su imagen.

### 13.5. Abstracción de proveedor de almacenamiento

Cloudinary es el proveedor actual del MVP.

A futuro se podría desacoplar el almacenamiento para permitir otros proveedores como:

```text
- Amazon S3
- Azure Blob Storage
- Supabase Storage
- MinIO
```

La idea sería mantener estable el contrato de la aplicación y cambiar únicamente la implementación concreta del proveedor.

### 13.6. Integración frontend

Pendiente conectar el frontend para:

```text
- Seleccionar imagen desde el formulario de producto.
- Previsualizar la imagen antes de guardar.
- Enviar la imagen al endpoint de upload.
- Recibir la URL de Cloudinary.
- Asignar la URL a ProductCreateDto.Image o ProductUpdateDto.Image.
- Guardar el producto.
- Renderizar la imagen en ProductCard.
```

---

## 14. Prueba manual recomendada

### 14.1. Probar upload

Endpoint:

```http
POST http://localhost:5211/api/uploads/product-images
```

Headers:

```http
Authorization: Bearer <ACCESS_TOKEN>
```

Body:

```text
form-data
key: file
type: File
```

Resultado esperado:

```json
{
  "IsSuccess": true,
  "Data": {
    "Url": "https://res.cloudinary.com/...",
    "PublicId": "ventaspro/products/...",
    "OriginalFileName": "image.png",
    "SizeBytes": 359318,
    "ContentType": "image/png"
  },
  "Message": "Imagen subida correctamente."
}
```

### 14.2. Probar producto con imagen

Una vez obtenida la URL, crear o actualizar un producto enviando la URL en el campo:

```json
{
  "Image": "https://res.cloudinary.com/..."
}
```

Luego validar que el listado de productos retorne esa URL en el campo `Image` y que el frontend pueda renderizarla correctamente.

---

## 15. Resumen

La carga de imágenes de productos quedó implementada de forma funcional usando Cloudinary.

La solución actual cumple con el objetivo MVP:

```text
Recibir imagen → cargar en Cloudinary → retornar URL → guardar URL en producto
```

La implementación es válida para esta fase del proyecto, aunque deja como deuda técnica el uso de `IFormFile` en capas internas.

En futuras fases se recomienda mejorar la autorización, validar la firma real del archivo, manejar eliminación/reemplazo de imágenes y desacoplar el contrato de carga de archivos de ASP.NET Core.
