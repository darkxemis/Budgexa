namespace Budgexa.Application.Invoices.DTOs;

using Budgexa.Domain.Enums;

public sealed record InvoiceDto(
    Guid Id,
    string Series,
    string Number,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    decimal Subtotal,
    decimal TaxAmount,
    decimal WithholdingAmount,
    decimal Total,
    decimal AmountPaid,
    decimal AmountDue,
    bool IsFullyPaid,
    PaymentMethod? PaymentMethod,
    string? PaymentReference,
    string? Notes,
    Guid CompanyId,
    string CompanyName,
    Guid CustomerId,
    string CustomerName,
    Guid? BudgetId,
    string? BudgetNumber,
    Guid StatusId,
    string StatusName,
    List<InvoiceLineDto> Lines,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record InvoiceCreateDto(
    Guid CustomerId,
    Guid? BudgetId,
    string Series,
    string Number,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    string? Notes,
    List<InvoiceLineUpsertDto> Lines);

public sealed record InvoiceUpdateDto(
    Guid CustomerId,
    string Series,
    string Number,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    string? Notes,
    List<InvoiceLineUpsertDto> Lines);

public sealed record InvoiceGridDto(
    Guid Id,
    string Series,
    string Number,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    decimal Total,
    decimal AmountPaid,
    decimal AmountDue,
    Guid CustomerId,
    string CustomerName,
    Guid StatusId,
    string StatusName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record ChangeInvoiceStatusDto(Guid StatusId);

public sealed record RegisterInvoicePaymentDto(
    decimal Amount,
    PaymentMethod Method,
    string? Reference);
