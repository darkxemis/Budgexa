namespace Budgexa.Application.Invoices.Commands.UpdateInvoice;

using Budgexa.Application.Invoices.DTOs;
using MediatR;

public sealed record UpdateInvoiceCommand(Guid Id, InvoiceUpdateDto Dto) : IRequest<InvoiceDto>;
