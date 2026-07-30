namespace Budgexa.Application.Budgets.Commands.DownloadBudgetPdf;

using MediatR;

public sealed record DownloadBudgetPdfCommand(Guid BudgetId) : IRequest<DownloadBudgetPdfResult>;

public sealed record DownloadBudgetPdfResult(byte[] PdfBytes, string FileName);
