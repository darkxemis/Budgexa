namespace Budgexa.Domain.Entities;

using Budgexa.Domain.Common;
using Budgexa.Domain.Enums;

public sealed class Invoice : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid StatusId { get; private set; }
    public Guid? BudgetId { get; private set; }

    public string Number { get; private set; } = default!;
    public string Series { get; private set; } = default!;
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }

    public string Currency { get; private set; } = "EUR";
    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal WithholdingAmount { get; private set; }
    public decimal Total { get; private set; }
    public decimal AmountPaid { get; private set; }

    public PaymentMethod? PaymentMethod { get; private set; }
    public string? PaymentReference { get; private set; }
    public string? Notes { get; private set; }

    public Company Company { get; private set; } = default!;
    public Customer Customer { get; private set; } = default!;
    public Status Status { get; private set; } = default!;
    public Budget? Budget { get; private set; }

    private readonly List<InvoiceLine> _lines = new();
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public decimal AmountDue => Total - AmountPaid;
    public bool IsFullyPaid => AmountPaid >= Total;

    private Invoice() { }

    private Invoice(
        Guid id,
        Guid companyId,
        Guid customerId,
        Guid statusId,
        Guid? budgetId,
        string series,
        string number,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string? notes,
        Guid createdByUserId)
    {
        Id = id;
        CompanyId = companyId;
        CustomerId = customerId;
        StatusId = statusId;
        BudgetId = budgetId;
        Series = series;
        Number = number;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency;
        Notes = notes;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Invoice Create(
        Guid companyId,
        Guid customerId,
        Guid statusId,
        string series,
        string number,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string? notes,
        Guid createdByUserId,
        Guid? budgetId = null,
        Guid? id = null)
    {
        Validate(series, number, currency, issueDate, dueDate);

        return new Invoice(
            id ?? Guid.NewGuid(),
            companyId,
            customerId,
            statusId,
            budgetId,
            series,
            number,
            issueDate,
            dueDate,
            currency,
            notes,
            createdByUserId);
    }

    public void UpdateHeader(
        Guid customerId,
        string series,
        string number,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string? notes,
        Guid updatedByUserId)
    {
        Validate(series, number, currency, issueDate, dueDate);

        CustomerId = customerId;
        Series = series;
        Number = number;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency;
        Notes = notes;
        Touch(updatedByUserId);
    }

    public InvoiceLine AddLine(
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate,
        decimal withholdingRate,
        Guid updatedByUserId,
        Guid? lineId = null)
    {
        var line = InvoiceLine.Create(itemId, sortOrder, description, unit, quantity, unitPrice, discountPercentage, taxRate, withholdingRate, lineId);
        line.AttachTo(Id);
        _lines.Add(line);
        RecalculateTotals();
        Touch(updatedByUserId);
        return line;
    }

    public void UpdateLine(
        Guid lineId,
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate,
        decimal withholdingRate,
        Guid updatedByUserId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new InvalidOperationException($"Invoice line '{lineId}' not found.");

        line.Update(itemId, sortOrder, description, unit, quantity, unitPrice, discountPercentage, taxRate, withholdingRate);
        RecalculateTotals();
        Touch(updatedByUserId);
    }

    public void RemoveLine(Guid lineId, Guid updatedByUserId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new InvalidOperationException($"Invoice line '{lineId}' not found.");

        _lines.Remove(line);
        RecalculateTotals();
        Touch(updatedByUserId);
    }

    public void RegisterPayment(
        decimal amount,
        PaymentMethod method,
        string? reference,
        Guid updatedByUserId)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        if (AmountPaid + amount > Total)
            throw new InvalidOperationException("Payment exceeds outstanding invoice amount.");

        AmountPaid += amount;
        PaymentMethod = method;
        PaymentReference = reference;
        Touch(updatedByUserId);
    }

    public void ChangeStatus(Guid newStatusId, Guid updatedByUserId)
    {
        StatusId = newStatusId;
        Touch(updatedByUserId);
    }

    private void RecalculateTotals()
    {
        Subtotal = _lines.Sum(l => l.Subtotal);
        TaxAmount = _lines.Sum(l => l.TaxAmount);
        WithholdingAmount = _lines.Sum(l => l.WithholdingAmount);
        Total = Subtotal + TaxAmount - WithholdingAmount;
    }

    private void Touch(Guid updatedByUserId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    private static void Validate(string series, string number, string currency, DateOnly issueDate, DateOnly dueDate)
    {
        if (string.IsNullOrWhiteSpace(series))
            throw new ArgumentException("Invoice series cannot be empty.", nameof(series));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Invoice number cannot be empty.", nameof(number));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        if (dueDate < issueDate)
            throw new ArgumentException("Due date cannot be before issue date.", nameof(dueDate));
    }
}
