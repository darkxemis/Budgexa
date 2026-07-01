namespace Budgexa.Application.Invoices.Commands.DeleteInvoice;

using MediatR;

public sealed record DeleteInvoiceCommand(Guid Id) : IRequest;
