namespace Budgexa.Application.Invoices.Queries.GenerateInvoiceWithAi;

using System.Net;
using System.Text.Json;
using Budgexa.Application.Common.Constants;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Common.Services;
using Budgexa.Application.PublicBudgets.Services;
using Budgexa.Domain.Exceptions;
using MediatR;

public sealed class GenerateInvoiceWithAiQueryHandler(
    IAiService aiService,
    ICurrentUserService currentUserService,
    ICustomerMatchingService customerMatchingService,
    IItemMatchingService itemMatchingService,
    IDateParsingService dateParsingService)
    : IRequestHandler<GenerateInvoiceWithAiQuery, PrivateInvoiceAiResponseDto>
{
    public async Task<PrivateInvoiceAiResponseDto> Handle(
        GenerateInvoiceWithAiQuery request,
        CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;

        // 1. Call AI service with PrivateInvoicePrompt
        var aiResult = await aiService.GenerateJsonAsync(
            AiPrompts.PrivateInvoicePrompt,
            request.Request.UserRequest,
            cancellationToken);

        // 2. Deserialize AI response to AiPrivateInvoiceDto
        AiPrivateInvoiceDto aiData;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            aiData = JsonSerializer.Deserialize<AiPrivateInvoiceDto>(aiResult.JsonResponse, options)
                ?? throw new AppException(
                    HttpStatusCode.InternalServerError,
                    "AI:InvalidResponse",
                    "AI returned invalid or null JSON.");
        }
        catch (JsonException ex)
        {
            throw new AppException(
                HttpStatusCode.InternalServerError,
                "AI:DeserializationFailed",
                $"Failed to deserialize AI response: {ex.Message}. Raw JSON: {aiResult.JsonResponse}");
        }

        // 3. Match customer by name/tax ID
        var customerId = await customerMatchingService.FindCustomerIdAsync(
            companyId,
            aiData.CustomerName,
            aiData.CustomerTaxId,
            cancellationToken);

        // 4. Parse dates
        var issueDate = dateParsingService.ParseDate(aiData.IssueDate);
        var dueDate = dateParsingService.ParseDate(aiData.DueDate);

        // 5. Match items
        var matchedItems = await itemMatchingService.MatchItemsAsync(
            companyId,
            aiData.Items ?? [],
            cancellationToken);

        // 6. Build response
        return new PrivateInvoiceAiResponseDto(
            CustomerId: customerId,
            Series: aiData.Series,
            Number: aiData.Number,
            IssueDate: issueDate,
            DueDate: dueDate,
            Currency: aiData.Currency,
            Notes: aiData.Notes,
            Items: matchedItems
        );
    }
}
