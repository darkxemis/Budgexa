namespace Budgexa.Application.Customers.Queries.GetCustomerById;

using Budgexa.Application.Customers.DTOs;
using MediatR;

public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;
