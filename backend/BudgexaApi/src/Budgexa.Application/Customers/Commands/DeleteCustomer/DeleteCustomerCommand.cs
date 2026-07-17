namespace Budgexa.Application.Customers.Commands.DeleteCustomer;

using MediatR;

public sealed record DeleteCustomerCommand(Guid Id) : IRequest;
