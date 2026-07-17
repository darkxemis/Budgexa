namespace Budgexa.Application.Invoices.Commands.CreateInvoice;

using Budgexa.Application.Invoices.DTOs;
using MediatR;

public sealed record CreateInvoiceCommand(InvoiceCreateDto Dto) : IRequest<InvoiceDto>;
