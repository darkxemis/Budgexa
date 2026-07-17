namespace Budgexa.Application.Invoices.Commands.RegisterInvoicePayment;

using Budgexa.Application.Invoices.DTOs;
using MediatR;

public sealed record RegisterInvoicePaymentCommand(Guid Id, RegisterInvoicePaymentDto Dto) : IRequest<InvoiceDto>;
