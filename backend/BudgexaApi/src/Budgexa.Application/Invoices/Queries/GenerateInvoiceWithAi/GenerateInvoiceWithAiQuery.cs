namespace Budgexa.Application.Invoices.Queries.GenerateInvoiceWithAi;

using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GenerateInvoiceWithAiQuery(
    PrivateInvoiceAiRequestDto Request
) : IRequest<PrivateInvoiceAiResponseDto>;
