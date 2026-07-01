namespace Budgexa.Domain.Entities;

using Budgexa.Domain.Common;

public sealed class Customer : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid StatusId { get; private set; }

    public string LegalName { get; private set; } = default!;
    public string? TradeName { get; private set; }
    public string TaxId { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    public string? AddressLine { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Province { get; private set; }
    public string? Country { get; private set; }

    public string? Notes { get; private set; }

    public Company Company { get; private set; } = default!;
    public Status Status { get; private set; } = default!;

    public ICollection<Budget> Budgets { get; private set; } = new List<Budget>();
    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    private Customer() { }

    private Customer(
        Guid id,
        Guid companyId,
        Guid statusId,
        string legalName,
        string? tradeName,
        string taxId,
        string? email,
        string? phone,
        string? addressLine,
        string? city,
        string? postalCode,
        string? province,
        string? country,
        string? notes,
        Guid createdByUserId)
    {
        Id = id;
        CompanyId = companyId;
        StatusId = statusId;
        LegalName = legalName;
        TradeName = tradeName;
        TaxId = taxId;
        Email = email;
        Phone = phone;
        AddressLine = addressLine;
        City = city;
        PostalCode = postalCode;
        Province = province;
        Country = country;
        Notes = notes;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Customer Create(
        Guid companyId,
        Guid statusId,
        string legalName,
        string? tradeName,
        string taxId,
        string? email,
        string? phone,
        string? addressLine,
        string? city,
        string? postalCode,
        string? province,
        string? country,
        string? notes,
        Guid createdByUserId,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException("Customer legal name cannot be empty.", nameof(legalName));

        if (string.IsNullOrWhiteSpace(taxId))
            throw new ArgumentException("Customer tax id cannot be empty.", nameof(taxId));

        return new Customer(
            id ?? Guid.NewGuid(),
            companyId,
            statusId,
            legalName,
            tradeName,
            taxId,
            email,
            phone,
            addressLine,
            city,
            postalCode,
            province,
            country,
            notes,
            createdByUserId);
    }

    public void Update(
        string legalName,
        string? tradeName,
        string taxId,
        string? email,
        string? phone,
        string? addressLine,
        string? city,
        string? postalCode,
        string? province,
        string? country,
        string? notes,
        Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException("Customer legal name cannot be empty.", nameof(legalName));

        if (string.IsNullOrWhiteSpace(taxId))
            throw new ArgumentException("Customer tax id cannot be empty.", nameof(taxId));

        LegalName = legalName;
        TradeName = tradeName;
        TaxId = taxId;
        Email = email;
        Phone = phone;
        AddressLine = addressLine;
        City = city;
        PostalCode = postalCode;
        Province = province;
        Country = country;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    public void MarkAsDeleted(Guid deletedStatusId, Guid updatedByUserId)
    {
        StatusId = deletedStatusId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }
}
