namespace Budgexa.Application.Customers.Commands.CreateCustomer;

using Budgexa.Application.Customers.DTOs;
using MediatR;

public sealed record CreateCustomerCommand(CustomerCreateDto Dto) : IRequest<CustomerDto>;
