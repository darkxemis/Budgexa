namespace Budgexa.Application.Invoices.Queries.GetInvoiceById;

using Budgexa.Application.Invoices.DTOs;
using MediatR;

public sealed record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto>;
