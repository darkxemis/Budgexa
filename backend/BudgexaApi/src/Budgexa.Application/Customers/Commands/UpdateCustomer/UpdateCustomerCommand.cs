namespace Budgexa.Application.Customers.Commands.UpdateCustomer;

using Budgexa.Application.Customers.DTOs;
using MediatR;

public sealed record UpdateCustomerCommand(Guid Id, CustomerUpdateDto Dto) : IRequest<CustomerDto>;
