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
6. **Unit tested** - Every new handler, validator, behavior, middleware or domain rule must ship with xUnit tests under `tests/`. The full `dotnet test` run must stay green before closing a change.

## Project Structure
```
src/
├── Budgexa.API/               # Presentation layer
│   ├── Endpoints/             # API endpoint definitions
│   ├── Middleware/            # HTTP middleware
│
├── Budgexa.Application/       # Application layer
│   ├── [Feature]/
│   │   ├── Commands/          # Write operations
│   │   └── Queries/           # Read operations
│
├── Budgexa.Domain/            # Domain layer
│   ├── Entities/              # Domain entities
│   ├── Constants/             # Well-known IDs and constants (e.g. StatusIds)
│   ├── Enums/                 # Domain enums
│   └── Exceptions/            # Domain exceptions
│
└── Budgexa.Infrastructure/    # Infrastructure layer
    ├── Persistence/           # EF Core DbContext, configurations, migrations
    └── Services/              # External services (JWT, password hashing, current user, etc.)

tests/
├── Budgexa.Domain.Tests/          # Entity / constant / exception tests (pure, no I/O)
├── Budgexa.Application.Tests/     # Handler / validator / behavior / helper tests
│   └── TestHelpers/               # TestDbContextFactory, TestDataSeeder
├── Budgexa.Infrastructure.Tests/  # JWT, BCrypt, CurrentUserService, settings providers
└── Budgexa.API.Tests/             # Middleware (e.g. GlobalExceptionHandler)
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

## Testing Rules
- **Framework**: xUnit + FluentAssertions + NSubstitute. EF Core InMemory for handler tests.
- **Location**: tests live under `tests/<Project>.Tests/` and mirror the `src/` folder layout (e.g. `src/Budgexa.Application/Users/Commands/CreateUser/CreateUserCommandHandler.cs` -> `tests/Budgexa.Application.Tests/Users/Commands/CreateUser/CreateUserCommandHandlerTests.cs`).
- **Naming**: test class = `<TypeUnderTest>Tests`; test method = `<Method_or_Scenario>_<ExpectedOutcome>`.
- **Isolation**: never share state between tests. Use `TestDbContextFactory.Create()` and `TestDataSeeder` (in `Budgexa.Application.Tests/TestHelpers/`) for handler tests; each call returns a fresh InMemory database keyed by `Guid.NewGuid()`.
- **Validators**: use FluentValidation's built-in `TestValidate(...)` / `ShouldHaveValidationErrorFor(...)`. There is no separate `FluentValidation.TestHelper` package on .NET 10 — the helpers ship inside the main `FluentValidation` package.
- **Mocking**: prefer real implementations for domain types; mock only ports/interfaces (`IPasswordHasher`, `IJwtTokenGenerator`, `IJwtSettingsProvider`, `ILoginLockoutSettingsProvider`, `ICurrentUserService`, `IAiService`).
- **NSubstitute + strong-named assemblies**: any nested type used as a generic argument for a substituted interface (e.g. `IValidator<TRequest>`) must be `public`, otherwise Castle DynamicProxy cannot generate the proxy.
- **Grid queries**: tests that exercise `GridQueryExtensions.ToGridResponseAsync` must run against an EF Core context (InMemory is fine). A plain `IQueryable` does not implement `IAsyncEnumerable<T>` and will throw.
- **JWT**: parse tokens with `JwtSecurityTokenHandler.ReadJwtToken(...)` and assert against claims, not raw strings.
- **Middleware (`IExceptionHandler`)**: invoke `TryHandleAsync(...)` directly with a `DefaultHttpContext`, a `MemoryStream`-backed response body, and a substituted `IHostEnvironment` to switch Development vs Production behavior.
- **Definition of done**: `dotnet test` from `backend/BudgexaApi/` must finish with zero failures before merging.
