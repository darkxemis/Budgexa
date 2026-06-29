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
