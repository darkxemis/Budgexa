namespace Budgexa.Application.Languages.DTOs;

public sealed record LanguageDto(
    Guid Id,
    string Code,
    string Name);