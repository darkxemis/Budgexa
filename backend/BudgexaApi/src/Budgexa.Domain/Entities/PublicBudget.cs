namespace Budgexa.Domain.Entities;

using Budgexa.Domain.Common;

public sealed class PublicBudget : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string CustomerFirstName { get; private set; } = default!;
    public string CustomerLastName { get; private set; } = default!;
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
        string customerFirstName,
        string customerLastName)
    {
        Id = id;
        CompanyId = companyId;
        LanguageId = languageId;
        CustomerFirstName = customerFirstName;
        CustomerLastName = customerLastName;
        CreatedAt = DateTime.UtcNow;
    }

    public static PublicBudget Create(
        Guid companyId,
        Guid languageId,
        string customerFirstName,
        string customerLastName,
        Guid? id = null)
    {
        Validate(customerFirstName, customerLastName);

        return new PublicBudget(
            id ?? Guid.NewGuid(),
            companyId,
            languageId,
            customerFirstName.Trim(),
            customerLastName.Trim());
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

    private static void Validate(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Customer first name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Customer last name cannot be empty.", nameof(lastName));
    }
}
