namespace Budgexa.Application.Invoices.Commands.ChangeInvoiceStatus;

using Budgexa.Application.Invoices.DTOs;
using MediatR;

public sealed record ChangeInvoiceStatusCommand(Guid Id, ChangeInvoiceStatusDto Dto) : IRequest<InvoiceDto>;
