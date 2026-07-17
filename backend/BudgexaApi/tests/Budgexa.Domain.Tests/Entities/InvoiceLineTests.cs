namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class InvoiceLineTests
{
    private static Invoice InvoiceWithLine(
        decimal quantity,
        decimal unitPrice,
        decimal discount,
        decimal taxRate,
        decimal withholdingRate,
        out InvoiceLine line)
    {
        var invoice = Invoice.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "A", "INV-1",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "EUR", null, Guid.NewGuid());

        line = invoice.AddLine(null, 1, "Service", "unit", quantity, unitPrice, discount, taxRate, withholdingRate, Guid.NewGuid());
        return invoice;
    }

    [Fact]
    public void Create_WithValidArguments_CalculatesAllAmounts()
    {
        InvoiceWithLine(quantity: 2m, unitPrice: 50m, discount: 0m, taxRate: 21m, withholdingRate: 15m, out var line);

        line.Subtotal.Should().Be(100m);
        line.TaxAmount.Should().Be(21m);
        line.WithholdingAmount.Should().Be(15m);
        line.Total.Should().Be(106m);
    }

    [Fact]
    public void Create_WithDiscount_AppliesDiscountBeforeTaxAndWithholding()
    {
        InvoiceWithLine(quantity: 1m, unitPrice: 100m, discount: 10m, taxRate: 21m, withholdingRate: 15m, out var line);

        line.Subtotal.Should().Be(90m);
        line.TaxAmount.Should().Be(18.90m);
        line.WithholdingAmount.Should().Be(13.50m);
        line.Total.Should().Be(95.40m);
    }

    [Fact]
    public void Create_WithoutWithholding_KeepsWithholdingAmountZero()
    {
        InvoiceWithLine(quantity: 1m, unitPrice: 100m, discount: 0m, taxRate: 21m, withholdingRate: 0m, out var line);

        line.WithholdingAmount.Should().Be(0m);
        line.Total.Should().Be(121m);
    }

    [Fact]
    public void Update_RecalculatesAllAmounts()
    {
        var invoice = InvoiceWithLine(1m, 100m, 0m, 21m, 0m, out var line);

        invoice.UpdateLine(line.Id, null, 1, "X", "unit", 4m, 25m, 0m, 21m, 15m, Guid.NewGuid());

        line.Subtotal.Should().Be(100m);
        line.TaxAmount.Should().Be(21m);
        line.WithholdingAmount.Should().Be(15m);
        line.Total.Should().Be(106m);
    }

    [Fact]
    public void Create_AttachesToInvoice()
    {
        var invoice = InvoiceWithLine(1m, 10m, 0m, 0m, 0m, out var line);

        line.InvoiceId.Should().Be(invoice.Id);
    }

    [Theory]
    [InlineData("", "unit", 1, 1, 0, 0, 0)]
    [InlineData("desc", "", 1, 1, 0, 0, 0)]
    [InlineData("desc", "unit", 0, 1, 0, 0, 0)]
    [InlineData("desc", "unit", -1, 1, 0, 0, 0)]
    [InlineData("desc", "unit", 1, -1, 0, 0, 0)]
    [InlineData("desc", "unit", 1, 1, -1, 0, 0)]
    [InlineData("desc", "unit", 1, 1, 101, 0, 0)]
    [InlineData("desc", "unit", 1, 1, 0, -1, 0)]
    [InlineData("desc", "unit", 1, 1, 0, 101, 0)]
    [InlineData("desc", "unit", 1, 1, 0, 0, -1)]
    [InlineData("desc", "unit", 1, 1, 0, 0, 101)]
    public void AddLine_WithInvalidArguments_ThrowsArgumentException(
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discount,
        decimal taxRate,
        decimal withholdingRate)
    {
        var invoice = Invoice.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "A", "INV-1",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "EUR", null, Guid.NewGuid());

        var act = () => invoice.AddLine(null, 1, description, unit, quantity, unitPrice, discount, taxRate, withholdingRate, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }
}
