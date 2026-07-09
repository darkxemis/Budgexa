namespace Budgexa.Domain.Entities;

using Budgexa.Domain.Common;

public sealed class PublicBudget : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string BudgetNumber { get; private set; } = default!;
    public string CustomerFirstName { get; private set; } = default!;
    public string CustomerLastName { get; private set; } = default!;
    public string? CustomerPhone { get; private set; }
    public string? CustomerEmail { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }

    public Company Company { get; private set; } = default!;
    public Language Language { get; private set; } = default!;

    private readonly List<PublicBudgetLine> _lines = new();
    public IReadOnlyCollection<PublicBudgetLine> Lines => _lines.AsReadOnly();

    private PublicBudget() { }

    private PublicBudget(
        Guid id,
        Guid companyId,
        Guid languageId,
        string budgetNumber,
        string customerFirstName,
        string customerLastName,
        string? customerPhone,
        string? customerEmail)
    {
        Id = id;
        CompanyId = companyId;
        LanguageId = languageId;
        BudgetNumber = budgetNumber;
        CustomerFirstName = customerFirstName;
        CustomerLastName = customerLastName;
        CustomerPhone = customerPhone;
        CustomerEmail = customerEmail;
        CreatedAt = DateTime.UtcNow;
    }

    public static PublicBudget Create(
        Guid companyId,
        Guid languageId,
        string customerFirstName,
        string customerLastName,
        string? customerPhone = null,
        string? customerEmail = null,
        Guid? id = null)
    {
        Validate(customerFirstName, customerLastName);

        var budgetNumber = GenerateBudgetNumber();

        return new PublicBudget(
            id ?? Guid.NewGuid(),
            companyId,
            languageId,
            budgetNumber,
            customerFirstName.Trim(),
            customerLastName.Trim(),
            customerPhone?.Trim(),
            customerEmail?.Trim());
    }

    public void AddLines(IEnumerable<PublicBudgetLine> lines)
    {
        foreach (var line in lines)
        {
            line.AttachTo(Id);
            _lines.Add(line);
        }

        Recalculate();
    }

    private void Recalculate()
    {
        SubTotal = _lines.Sum(l => l.SubTotal);
        TaxTotal = _lines.Sum(l => l.TaxAmount);
        GrandTotal = SubTotal + TaxTotal;
    }

    private static string GenerateBudgetNumber()
    {
        var now = DateTime.UtcNow;
        var suffix = Random.Shared.Next(0, 0xFFFF).ToString("X4");
        return $"PRE-{now:yyyyMMdd}-{now:HHmmss}-{suffix}";
    }

    private static void Validate(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Customer first name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Customer last name cannot be empty.", nameof(lastName));
    }
}
