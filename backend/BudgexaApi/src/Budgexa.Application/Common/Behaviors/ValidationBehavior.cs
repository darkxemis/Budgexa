namespace Budgexa.Application.Common.Behaviors;

using System.Net;
using Budgexa.Domain.Exceptions;
using FluentValidation;
using MediatR;

internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = failures
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.First().ErrorMessage);

        if (errors.Count > 0)
        {
            throw new AppException(
                HttpStatusCode.BadRequest,
                ErrorTags.Validation.Failed,
                "One or more validation errors occurred.",
                errors);
        }

        return await next(cancellationToken);
    }
}
