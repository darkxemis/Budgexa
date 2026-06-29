# Backend AI Rules - Budgexa

## Tech Stack
- **.NET 10** - Minimal APIs
- **C# 13**
- **Entity Framework Core** - Database ORM
- **Clean Architecture** - Domain-centric design

## Core Principles
1. **Clean Architecture** - Separate concerns across layers
2. **SOLID** - Follow SOLID principles
3. **Minimal APIs** - Use endpoint groups
4. **CQRS / Vertical Slices** - Command Query Responsibility Segregation (MediatR), one handler per slice
5. **Direct DbContext access** - Handlers depend on `IApplicationDbContext` (no Repository / Unit of Work). Queries project to DTOs with `Select()` + `AsNoTracking()`; commands load aggregates, mutate via domain methods, and call `SaveChangesAsync` explicitly.

## Project Structure
```
Budgexa.API/           # Presentation layer
├── Endpoints/         # API endpoint definitions
├── Middleware/        # HTTP middleware

Budgexa.Application/   # Application layer
├── [Feature]/
│   ├── Commands/      # Write operations
│   └── Queries/       # Read operations

Budgexa.Domain/        # Domain layer
├── Entities/          # Domain entities
├── Constants/         # Well-known IDs and constants (e.g. StatusIds)
├── Enums/             # Domain enums
└── Exceptions/        # Domain exceptions

Budgexa.Infrastructure/ # Infrastructure layer
├── Persistence/       # EF Core DbContext, configurations, migrations
└── Services/          # External services (JWT, password hashing, current user, etc.)
```

## Endpoint Pattern
```csharp
public static class FeatureEndpoints
{
    public static void MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feature")
            .WithTags("Feature");
            
        group.MapGet("/", GetAll)
            .RequireAuthorization();
    }
}
```

## CQRS Pattern
```csharp
// Command
public record CreateCommand(string Data) : IRequest<Result>;

// Handler
public class CreateHandler : IRequestHandler<CreateCommand, Result>
{
    public async Task<Result> Handle(CreateCommand request, CancellationToken ct)
    {
        // Implementation
    }
}
```

## Key Patterns
- **Dependency Injection** - Use built-in DI
- **Result Pattern** - Return `Result<T>` for operations
- **Validation** - FluentValidation
- **Logging** - Serilog
- **Authentication** - JWT Bearer tokens
