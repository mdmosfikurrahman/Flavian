# Flavian

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

A clean architecture foundation for building .NET 9.0 microservices. Production-grade infrastructure with zero business logic — clone, rename, and start building.

## Why it exists

Every .NET codebase I have been asked to rescue died the same way: business logic in controllers, one
enormous `DbContext`, no transaction boundaries, and migrations nobody dares run. None of it looks
like a problem in month one, and all of it is the problem by month nine.

The fix is not clever code. It is deciding the boundaries before the first feature arrives — and then
having something enforce them. That is all Flavian is: the skeleton, with the dependency direction
enforced by project references rather than by good intentions, and the parts you would otherwise
rewrite on every new service (DI wiring, repositories, validation, error shape, auth, migrations,
audit) already built and working.

It ships with **zero business logic** on purpose. There is nothing to delete before you start.

## Architecture

```
Flavian/
├── Flavian.Domain              → Entities, base classes, attributes
├── Flavian.Shared              → DTOs, exceptions, utilities, config models
├── Flavian.Persistence         → EF Core, repositories, UoW, migrations
├── Flavian.Infrastructure      → External service integrations
├── Flavian.Application         → Services, validators, mappers, GraphQL resolvers
├── Flavian.Configuration       → DI wiring, middleware, startup extensions
└── Flavian.WebAPI              → Entry point, controllers
```

### Dependency Flow

```
WebAPI → Configuration → Application → Persistence → Domain
                              ↓              ↓
                        Infrastructure     Shared
```

## What's Included

### Core Infrastructure
- **Convention-Based DI** — Services, repositories, and validators auto-registered by namespace convention. No manual binding required.
- **Generic Repository + Unit of Work** — Full CRUD with soft delete, pagination, and transaction management out of the box.
- **Base Entity System** — `BaseEntity` (with audit fields, soft delete, V7 GUIDs) and `BaseAuditEntity` for audit trail tracking.
- **EF Core Fluent Configurations** — Reusable `ConfigureBaseEntity()` and `ConfigureBaseAuditEntity()` extension methods.

### API Layer
- **API Versioning** — URL segment versioning (`v1.0`, `v2.0`) with Swagger docs per version.
- **Global Route Prefix** — All controllers automatically prefixed (default: `api`).
- **GraphQL** — HotChocolate integration with reflection-based resolver discovery.
- **Swagger** — Bearer token auth, enum support, multi-version documentation.

### Security
- **JWT Authentication** — Token validation with structured 401/403 error responses.
- **Rate Limiting** — IP-based fixed window rate limiter.
- **User Context** — Claims-based `IUserContext` service for accessing authenticated user info.

### Data & Migrations
- **YAML-Based SQL Migrations** — Lightweight migration system with SHA256 checksum integrity verification. No EF migrations needed.
- **Soft Delete** — Implemented at repository level via `WhereNotDeleted()` expression filter.

### Error Handling
- **Global Exception Handler** — Maps custom exceptions to proper HTTP status codes with consistent JSON responses.
- **Custom Exceptions** — `NotFoundException`, `ValidationException`, `AlreadyExistsException`, `InactiveResourceException`, `DeletedResourceException`, `FeatureNotImplementedException`.
- **Standard Response DTOs** — `StandardResponse`, `ServiceResponse<T>`, `PaginationResponse<T>` for consistent API responses.

### Validation
- **FluentValidation** — Auto-discovered validators registered by namespace convention.
- **Request Validator Helper** — Centralized validation with structured error output.

### Audit System
- **AuditDetailsBuilder** — Reflection-based audit trail that auto-generates create/update/delete details, respects `[AuditIgnore]` attribute, and masks sensitive fields.

### DevOps
- **Dockerfile** — Multi-stage build (SDK → Runtime).
- **docker-compose.yml** — Ready for staging deployment.

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- SQL Server

### Run Locally

```bash
# Clone
git clone https://github.com/mdmosfikurrahman/Flavian.git
cd Flavian

# Update connection string in appsettings.Development.json

# Run
dotnet run --project Flavian.WebAPI
```

Swagger UI: `http://localhost:5000/api/swagger`
GraphQL: `http://localhost:5000/api/v1/graphql`

### Docker

```bash
docker-compose up --build
```

API available at `http://localhost:8080`

## Demo Entity

A complete `Demo` entity is included as a reference implementation showing the full vertical slice:

| Layer | Files |
|---|---|
| Domain | `Demo.cs`, `DemoAudit.cs` |
| Persistence | `DemoRepository`, `DemoConfigurations`, `DemoAuditConfigurations` |
| Application | `DemoService`, `DemoMapper`, `DemoRequestValidator`, `DemoQuery`, `DemoMutation` |
| WebAPI | `DemoController` (full CRUD) |
| Migration | `001_create_demo_table.sql` |

## Adding a New Entity

1. **Domain** — Create entity class extending `BaseEntity`
2. **Persistence** — Add `IEntityTypeConfiguration`, repository interface + implementation, register in `IUnitOfWork`
3. **Application** — Create request/response DTOs, validator, mapper, service interface + implementation
4. **WebAPI** — Add controller
5. **Migration** — Add SQL file to `Database/Db/` and register in `Changelog.yaml`

Convention-based DI handles registration automatically — no manual wiring needed.

## Tech Stack

| Component | Technology |
|---|---|
| Runtime | .NET 9.0 |
| ORM | Entity Framework Core 9.0 |
| Database | SQL Server |
| API Docs | Swashbuckle (Swagger) |
| GraphQL | HotChocolate 15 |
| Validation | FluentValidation 12 |
| Auth | JWT Bearer |
| Migrations | Custom YAML + SQL |
| Container | Docker |

## Renaming it for your own service

Flavian is meant to be used under a different name. Three steps:

1. Rename the solution and the seven project folders — `Flavian.*` → `YourService.*`
2. Find-and-replace the `Flavian` root namespace across the solution
3. Delete the `Demo` vertical slice listed above once you have copied its shape for your first entity

The convention-based DI resolves by namespace, so nothing else needs rewiring.

## License

MIT — use it commercially, no attribution required.

## Who maintains this

Built by **Md. Mosfikur Rahman** — backend engineer in Dhaka, Bangladesh (GMT+6), working in .NET and
Java. Eight .NET 9 microservices in production, database per service, REST and gRPC; earlier, Spring
Boot at national scale and a GraphQL backend-for-frontend for Rakuten. Alongside the engineering, ten
peer-reviewed publications and 78 manuscripts reviewed for international journals.

- Portfolio — https://mdmosfikurrahman.github.io
- GitHub — https://github.com/mdmosfikurrahman
- LinkedIn — https://linkedin.com/in/mdmosfikurrahman

## Need this built for your project?

If you would rather have the service than the skeleton, I take a small number of freelance projects
at a time and build to exactly this standard:

- **.NET 9 microservice, clean architecture** — https://www.fiverr.com/s/2pKpNEX
- **Spring Boot REST API** — https://www.fiverr.com/s/YLRLA2a

Questions about the structure itself are welcome in an issue — that part is free, and the answer is
usually useful to someone else too.
