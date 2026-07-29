namespace Budgexa.Application.Tests.Budgets.Queries.GenerateBudgetWithAi;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.PublicBudgets.DTOs;
using Budgexa.Application.PublicBudgets.Queries.GeneratePublicBudgetWithAi;
using Budgexa.Application.PublicBudgets.Services;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;

public class GenerateBudgetWithAiQueryHandlerTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid ActiveStatusId = StatusIds.New;

    [Fact]
    public async Task Handle_MatchesItemsByFuzzyScore()
    {
        // Arrange
        var aiService = Substitute.For<IAiService>();
        aiService
            .GenerateBudgetJsonAsync("4 ventanas de aluminio y 2 puertas de madera", Arg.Any<CancellationToken>())
            .Returns(new BudgetItemsAiResult(
                "4 ventanas de aluminio y 2 puertas de madera",
                new List<BudgetItem>
                {
                    new("Ventana De Aluminio", 4),
                    new("Puerta De Madera", 2),
                },
                "qwen2.5:7b"));

        var items = new List<Item>
        {
            Item.Create(CompanyId, ActiveStatusId, null, "Ventana Corredera De Aluminio Blanca", null, ItemType.Product, UnitMeasure.Quantity, "ud", 150m, 21m, "EUR", Guid.NewGuid()),
            Item.Create(CompanyId, ActiveStatusId, null, "Puerta de Interior de Madera Block Roble", null, ItemType.Product, UnitMeasure.Quantity, "ud", 200m, 21m, "EUR", Guid.NewGuid()),
            Item.Create(CompanyId, ActiveStatusId, null, "Sofá Marrón Grande", null, ItemType.Product, UnitMeasure.Quantity, "ud", 500m, 21m, "EUR", Guid.NewGuid()),
        };

        var company = Company.Create("Test Company", null, DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid(), null, null, CompanyId);

        var db = CreateMockDbContext(company, items);

        var sut = new GeneratePublicBudgetWithAiQueryHandler(aiService, db);

        // Act
        var response = await sut.Handle(
            new GeneratePublicBudgetWithAiQuery(new PublicBudgetAiRequestDto(CompanyId, "4 ventanas de aluminio y 2 puertas de madera")),
            CancellationToken.None);

        // Assert
        response.OriginalRequest.Should().Be("4 ventanas de aluminio y 2 puertas de madera");
        response.Model.Should().Be("qwen2.5:7b");
        response.Items.Should().HaveCount(2);
        response.Items[0].ProductName.Should().Be("Ventana Corredera De Aluminio Blanca");
        response.Items[0].Quantity.Should().Be(4);
        response.Items[1].ProductName.Should().Be("Puerta de Interior de Madera Block Roble");
        response.Items[1].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoItemsMatchAboveThreshold()
    {
        var aiService = Substitute.For<IAiService>();
        aiService
            .GenerateBudgetJsonAsync("un coche deportivo", Arg.Any<CancellationToken>())
            .Returns(new BudgetItemsAiResult(
                "un coche deportivo",
                new List<BudgetItem> { new("Coche Deportivo", 1) },
                "qwen2.5:7b"));

        var items = new List<Item>
        {
            Item.Create(CompanyId, ActiveStatusId, null, "Ventana Corredera De Aluminio", null, ItemType.Product, UnitMeasure.Quantity, "ud", 150m, 21m, "EUR", Guid.NewGuid()),
            Item.Create(CompanyId, ActiveStatusId, null, "Puerta de Madera", null, ItemType.Product, UnitMeasure.Quantity, "ud", 200m, 21m, "EUR", Guid.NewGuid()),
        };

        var company = Company.Create("Test Company", null, DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid(), null, null, CompanyId);
        var db = CreateMockDbContext(company, items);

        var sut = new GeneratePublicBudgetWithAiQueryHandler(aiService, db);

        var response = await sut.Handle(
            new GeneratePublicBudgetWithAiQuery(new PublicBudgetAiRequestDto(CompanyId, "un coche deportivo")),
            CancellationToken.None);

        response.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PicksBestMatch_WhenMultipleItemsScoreAboveThreshold()
    {
        var aiService = Substitute.For<IAiService>();
        aiService
            .GenerateBudgetJsonAsync("1 sofa marron", Arg.Any<CancellationToken>())
            .Returns(new BudgetItemsAiResult(
                "1 sofa marron",
                new List<BudgetItem> { new("Sofa Marron", 1) },
                "qwen2.5:7b"));

        var items = new List<Item>
        {
            Item.Create(CompanyId, ActiveStatusId, null, "Sofá Marrón Grande", null, ItemType.Product, UnitMeasure.Quantity, "ud", 500m, 21m, "EUR", Guid.NewGuid()),
            Item.Create(CompanyId, ActiveStatusId, null, "Sofá Marrón Terciopelo Luxury Edition", null, ItemType.Product, UnitMeasure.Quantity, "ud", 900m, 21m, "EUR", Guid.NewGuid()),
            Item.Create(CompanyId, ActiveStatusId, null, "Sofá Amarillo", null, ItemType.Product, UnitMeasure.Quantity, "ud", 450m, 21m, "EUR", Guid.NewGuid()),
        };

        var company = Company.Create("Test Company", null, DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid(), null, null, CompanyId);
        var db = CreateMockDbContext(company, items);

        var sut = new GeneratePublicBudgetWithAiQueryHandler(aiService, db);

        var response = await sut.Handle(
            new GeneratePublicBudgetWithAiQuery(new PublicBudgetAiRequestDto(CompanyId, "1 sofa marron")),
            CancellationToken.None);

        response.Items.Should().HaveCount(1);
        // Should pick "Sofá Marrón Grande" (shorter name = tie-breaker)
        response.Items[0].ProductName.Should().Be("Sofá Marrón Grande");
        response.Items[0].Quantity.Should().Be(1);
    }

    private static IApplicationDbContext CreateMockDbContext(Company company, List<Item> items)
    {
        var db = Substitute.For<IApplicationDbContext>();

        // Mock Companies DbSet
        var companyList = new List<Company> { company }.AsQueryable();
        var mockCompaniesDbSet = Substitute.For<DbSet<Company>, IQueryable<Company>, IAsyncEnumerable<Company>>();
        ((IQueryable<Company>)mockCompaniesDbSet).Provider.Returns(new TestAsyncQueryProvider<Company>(companyList.Provider));
        ((IQueryable<Company>)mockCompaniesDbSet).Expression.Returns(companyList.Expression);
        ((IQueryable<Company>)mockCompaniesDbSet).ElementType.Returns(companyList.ElementType);
        ((IQueryable<Company>)mockCompaniesDbSet).GetEnumerator().Returns(companyList.GetEnumerator());
        ((IAsyncEnumerable<Company>)mockCompaniesDbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(new TestAsyncEnumerator<Company>(companyList.GetEnumerator()));
        db.Companies.Returns(mockCompaniesDbSet);

        // Mock Items DbSet
        var itemList = items.AsQueryable();
        var mockItemsDbSet = Substitute.For<DbSet<Item>, IQueryable<Item>, IAsyncEnumerable<Item>>();
        ((IQueryable<Item>)mockItemsDbSet).Provider.Returns(new TestAsyncQueryProvider<Item>(itemList.Provider));
        ((IQueryable<Item>)mockItemsDbSet).Expression.Returns(itemList.Expression);
        ((IQueryable<Item>)mockItemsDbSet).ElementType.Returns(itemList.ElementType);
        ((IQueryable<Item>)mockItemsDbSet).GetEnumerator().Returns(itemList.GetEnumerator());
        ((IAsyncEnumerable<Item>)mockItemsDbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(new TestAsyncEnumerator<Item>(itemList.GetEnumerator()));
        db.Items.Returns(mockItemsDbSet);

        return db;
    }
}

internal sealed class TestAsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<T>(expression, inner);
    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TElement>(expression, inner);
    public object? Execute(System.Linq.Expressions.Expression expression) => inner.Execute(expression);
    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => inner.Execute<TResult>(expression);
    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executeMethod = typeof(IQueryProvider).GetMethods()
            .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition)
            .MakeGenericMethod(resultType);

        var result = executeMethod.Invoke(inner, [expression]);
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [result])!;
    }
}

internal sealed class TestAsyncEnumerable<T>(System.Linq.Expressions.Expression expression, IQueryProvider provider)
    : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
{
    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(provider);
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());
}
