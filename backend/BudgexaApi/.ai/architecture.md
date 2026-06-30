# Backend Architecture - Budgexa

## Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│         Budgexa.API                     │
│    (Presentation Layer)                 │
│  - Endpoints                            │
│  - Middleware                           │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Budgexa.Application                │
│    (Application Layer)                  │
│  - Commands / Queries (CQRS)            │
│  - Handlers (MediatR)                   │
│  - DTOs / Mapping                       │
│  - Validation                           │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         Budgexa.Domain                  │
│      (Domain Layer)                     │
│  - Entities                             │
│  - Value Objects                        │
│  - Domain Events                        │
│  - Interfaces                           │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│     Budgexa.Infrastructure              │
│    (Infrastructure Layer)               │
│  - DbContext (EF Core)                  │
│  - External Services                    │
│  - Migrations                           │
└─────────────────────────────────────────┘
```

## Dependency Flow
- **API** depends on **Application**
- **Application** depends on **Domain**
- **Infrastructure** depends on **Domain** and **Application**
- **Domain** has no dependencies (core)

## Request Flow
```
HTTP Request
  ↓
Endpoint
  ↓
MediatR (Send Command/Query)
  ↓
Handler
  ↓
IApplicationDbContext (EF Core)
  ↓
Database
```

## Authentication Flow
1. Client sends credentials to `/api/auth/login`
2. API validates and generates JWT
3. Client stores token
4. Client sends token in `Authorization: Bearer <token>`
5. Middleware validates token
6. Request proceeds if valid

## Database
- **Provider**: PostgreSQL / SQL Server
- **ORM**: Entity Framework Core
- **Migrations**: Code-first
- **Data Access**: Handlers depend on `IApplicationDbContext` and query/mutate EF Core directly. No Repository or Unit of Work abstractions are used.
  - **Queries**: project directly to DTOs via `Select()` with `AsNoTracking()`.
  - **Commands**: load aggregates with the DbContext, mutate them through domain methods, then call `SaveChangesAsync` explicitly in the handler.

## Key Technologies
- **MediatR** - CQRS and request handling
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **Serilog** - Structured logging
- **JWT** - Authentication

## Testing Architecture
Each production project has a mirrored test project under `tests/`:

```
backend/BudgexaApi/
├── src/
│   ├── Budgexa.API/
│   ├── Budgexa.Application/
│   ├── Budgexa.Domain/
│   └── Budgexa.Infrastructure/
└── tests/
    ├── Budgexa.Domain.Tests/         # Pure entity / constant / exception tests (no I/O)
    ├── Budgexa.Application.Tests/    # Handlers, validators, MediatR behaviors, helpers
    ├── Budgexa.Infrastructure.Tests/ # JWT, password hashing, current-user, settings
    └── Budgexa.API.Tests/            # Middleware (GlobalExceptionHandler, etc.)
```

### Stack
- **xUnit** test framework
- **FluentAssertions** for readable assertions
- **NSubstitute** for mocking interfaces (`IPasswordHasher`, `IJwtTokenGenerator`, `ICurrentUserService`, `IAiService`, etc.)
- **EF Core InMemory** provider for handler tests that touch `IApplicationDbContext`
- **FluentValidation** for validator tests (no separate `TestHelper` package on .NET 10)

### Conventions
- Application handler tests use `TestDbContextFactory.Create()` + `TestDataSeeder` (in `tests/Budgexa.Application.Tests/TestHelpers/`) to spin up isolated InMemory databases per test.
- Grid query tests (anything reaching `GridQueryExtensions.ToGridResponseAsync`) must run against an EF Core context, not a plain `IQueryable`, because the helper internally calls `ToListAsync`.
- Types that NSubstitute proxies (e.g. nested test request records used with `IValidator<T>`) must be `public` — the strong-named `FluentValidation` assembly rejects internal proxy targets.
- JWT tests parse tokens with `JwtSecurityTokenHandler.ReadJwtToken(...)` instead of comparing opaque strings.
- `CurrentUserService` tests build an explicit `ClaimsPrincipal` + `DefaultHttpContext` to verify claim/header precedence.

### Running tests
```bash
# Full suite (from backend/BudgexaApi)
dotnet test

# Single layer
dotnet test tests/Budgexa.Application.Tests/Budgexa.Application.Tests.csproj
```
