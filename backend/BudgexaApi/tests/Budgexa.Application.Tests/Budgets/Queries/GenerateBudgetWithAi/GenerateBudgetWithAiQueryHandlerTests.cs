namespace Budgexa.Application.Tests.Budgets.Queries.GenerateBudgetWithAi;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Queries.GenerateBudgetWithAi;
using Budgexa.Application.Budgets.Services;
using NSubstitute;

public class GenerateBudgetWithAiQueryHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToAiServiceAndMapsResult()
    {
        var aiService = Substitute.For<IAiService>();
        aiService
            .GenerateBudgetJsonAsync("Birthday party", Arg.Any<CancellationToken>())
            .Returns(new BudgetItemsAiResult(
                "Birthday party",
                new List<BudgetItem>
                {
                    new("Balloon", 10, "Colored"),
                    new("Cake", 1),
                },
                "gpt-test"));

        var sut = new GenerateBudgetWithAiQueryHandler(aiService);

        var response = await sut.Handle(
            new GenerateBudgetWithAiQuery(new GenerateBudgetWithAiRequestDto("Birthday party")),
            CancellationToken.None);

        response.OriginalRequest.Should().Be("Birthday party");
        response.Model.Should().Be("gpt-test");
        response.Items.Should().HaveCount(2);
        response.Items[0].ProductName.Should().Be("Balloon");
        response.Items[0].Quantity.Should().Be(10);
        response.Items[1].ProductName.Should().Be("Cake");
    }
}
