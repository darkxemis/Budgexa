namespace Budgexa.Application.Invoices.Queries.GetAllInvoices;

using Budgexa.Application.Invoices.DTOs;
using MediatR;

public sealed record GetAllInvoicesQuery : IRequest<List<InvoiceGridDto>>;
