# UTP POS WebAPI Backend

## Negocios module architecture

The `Negocios` module is implemented through the existing layered architecture:

```text
Controller -> Application Interface -> Application Main -> Domain Interface
-> Domain Core -> Infrastructure Interface -> Infrastructure Repository
-> IConnectionFactory -> SQL Server stored procedures
```

Controllers depend only on application interfaces. Application classes map DTOs and return `Response<T>`. Domain classes apply light validations and call repository interfaces. Repository classes use Dapper with `Microsoft.Data.SqlClient` and execute stored procedures only.

Current module files:

- DTOs: `UtpPos.Framework.Application.DTO/Business`
- Application contract: `UtpPos.Framework.Application.Interface/Business/IBusinessApplication.cs`
- Application implementation: `UtpPos.Framework.Application.Main/Business/BusinessApplication.cs`
- Domain contract: `UtpPos.Framework.Domain.Interface/Business/IBusinessDomain.cs`
- Domain implementation: `UtpPos.Framework.Domain.Core/Business/BusinessDomain.cs`
- Repository contract: `UtpPos.Framework.InfraStructure.Interface/Business/IBusinessRepository.cs`
- Dapper repository: `UtpPos.Framework.InfraStructure.Repository/Business/BusinessRepository.cs`
- REST controller: `UtpPos.WebAPI/Controllers/BusinessController.cs`
- Mapping: `UtpPos.Framework.Transversal.Mapper/MappingProfile.cs`

To replicate this structure for `Clases`, `Parametros`, `Terceros`, `Usuarios`, `Productos`, `Inventario`, `Kardex`, `Compras` and `Ventas`:

1. Create the domain entity with clean English property names.
2. Create DTOs for list filters, reads, creates and partial updates.
3. Add application, domain and repository interfaces.
4. Implement application mapping and `Response<T>` wrapping.
5. Implement domain validations that do not duplicate stored procedure responsibilities.
6. Implement the repository with Dapper, `commandType: CommandType.StoredProcedure`, and private database result models for stored procedure column names.
7. Add AutoMapper mappings and DI registrations in `Program.cs`.
8. Add a REST controller that calls only the application interface.

Database credentials must be supplied through `ConnectionStrings:DefaultConnection`; do not hardcode real credentials in code.

## Testing

Testing documentation, commands, environment variables, current coverage and pending work are documented in `Docs/testing.md`.

## Dashboard

Dashboard API contract, permissions and stored procedure mapping are documented in `Docs/dashboard.md`.
