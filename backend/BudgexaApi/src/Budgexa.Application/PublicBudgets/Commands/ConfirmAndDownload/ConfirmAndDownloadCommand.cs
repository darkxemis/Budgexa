namespace Budgexa.Application.PublicBudgets.Commands.ConfirmAndDownload;

using Budgexa.Application.PublicBudgets.DTOs;
using MediatR;

public sealed record ConfirmAndDownloadCommand(
    ConfirmPublicBudgetRequestDto Request
) : IRequest<ConfirmAndDownloadResult>;

public sealed record ConfirmAndDownloadResult(
    byte[] PdfBytes,
    string FileName);
