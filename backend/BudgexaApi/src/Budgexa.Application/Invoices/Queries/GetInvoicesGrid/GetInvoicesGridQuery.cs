namespace Budgexa.Application.Invoices.Queries.GetInvoicesGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Invoices.DTOs;
using MediatR;

public sealed record GetInvoicesGridQuery(GridRequestDto Request) : IRequest<GridResponseDto<InvoiceGridDto>>;
