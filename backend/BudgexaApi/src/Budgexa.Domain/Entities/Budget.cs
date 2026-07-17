namespace Budgexa.Domain.Entities;

using Budgexa.Domain.Common;

public sealed class Budget : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid StatusId { get; private set; }

    public string Number { get; private set; } = default!;
    public DateOnly IssueDate { get; private set; }
    public DateOnly? ValidUntil { get; private set; }

    public string Currency { get; private set; } = "EUR";
    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }

    public string? Notes { get; private set; }
    public string? TermsAndConditions { get; private set; }

    public Company Company { get; private set; } = default!;
    public Customer Customer { get; private set; } = default!;
    public Status Status { get; private set; } = default!;

    private readonly List<BudgetLine> _lines = new();
    public IReadOnlyCollection<BudgetLine> Lines => _lines.AsReadOnly();

    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    private Budget() { }

    private Budget(
        Guid id,
        Guid companyId,
        Guid customerId,
        Guid statusId,
        string number,
        DateOnly issueDate,
        DateOnly? validUntil,
        string currency,
        string? notes,
        string? termsAndConditions,
        Guid createdByUserId)
    {
        Id = id;
        CompanyId = companyId;
        CustomerId = customerId;
        StatusId = statusId;
        Number = number;
        IssueDate = issueDate;
        ValidUntil = validUntil;
        Currency = currency;
        Notes = notes;
        TermsAndConditions = termsAndConditions;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Budget Create(
        Guid companyId,
        Guid customerId,
        Guid statusId,
        string number,
        DateOnly issueDate,
        DateOnly? validUntil,
        string currency,
        string? notes,
        string? termsAndConditions,
        Guid createdByUserId,
        Guid? id = null)
    {
        Validate(number, currency, issueDate, validUntil);

        return new Budget(
            id ?? Guid.NewGuid(),
            companyId,
            customerId,
            statusId,
            number,
            issueDate,
            validUntil,
            currency,
            notes,
            termsAndConditions,
            createdByUserId);
    }

    public void UpdateHeader(
        Guid customerId,
        string number,
        DateOnly issueDate,
        DateOnly? validUntil,
        string currency,
        string? notes,
        string? termsAndConditions,
        Guid updatedByUserId)
    {
        Validate(number, currency, issueDate, validUntil);

        CustomerId = customerId;
        Number = number;
        IssueDate = issueDate;
        ValidUntil = validUntil;
        Currency = currency;
        Notes = notes;
        TermsAndConditions = termsAndConditions;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    public BudgetLine AddLine(
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate,
        Guid updatedByUserId,
        Guid? lineId = null)
    {
        var line = BudgetLine.Create(itemId, sortOrder, description, unit, quantity, unitPrice, discountPercentage, taxRate, lineId);
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
        Guid updatedByUserId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new InvalidOperationException($"Budget line '{lineId}' not found.");

        line.Update(itemId, sortOrder, description, unit, quantity, unitPrice, discountPercentage, taxRate);
        RecalculateTotals();
        Touch(updatedByUserId);
    }

    public void RemoveLine(Guid lineId, Guid updatedByUserId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new InvalidOperationException($"Budget line '{lineId}' not found.");

        _lines.Remove(line);
        RecalculateTotals();
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
        Total = Subtotal + TaxAmount;
    }

    private void Touch(Guid updatedByUserId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    private static void Validate(string number, string currency, DateOnly issueDate, DateOnly? validUntil)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Budget number cannot be empty.", nameof(number));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        if (validUntil.HasValue && validUntil.Value < issueDate)
            throw new ArgumentException("Valid-until date cannot be before issue date.", nameof(validUntil));
    }
}
