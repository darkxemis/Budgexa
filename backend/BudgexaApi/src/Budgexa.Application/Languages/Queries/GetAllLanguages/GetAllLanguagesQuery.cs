namespace Budgexa.Application.Languages.Queries.GetAllLanguages;

using Budgexa.Application.Languages.DTOs;
using MediatR;

public sealed record GetAllLanguagesQuery : IRequest<List<LanguageDto>>;