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
4. **CQRS** - Command Query Responsibility Segregation (MediatR)
5. **Repository Pattern** - Data access abstraction

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
├── Interfaces/        # Repository interfaces
└── Exceptions/        # Domain exceptions

Budgexa.Infrastructure/ # Infrastructure layer
├── Persistence/       # EF Core DbContext
├── Repositories/      # Repository implementations
└── Services/          # External services
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
