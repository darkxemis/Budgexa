namespace Budgexa.Application.Customers.Queries.GetAllCustomers;

using Budgexa.Application.Customers.DTOs;
using MediatR;

public sealed record GetAllCustomersQuery() : IRequest<IEnumerable<CustomerDto>>;
