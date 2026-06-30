namespace Budgexa.Application.Tests.Common.Behaviors;

using System.Net;
using Budgexa.Application.Common.Behaviors;
using Budgexa.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

public class ValidationBehaviorTests
{
    public sealed record TestRequest(string Name) : IRequest<string>;

    public sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            this.RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
        }
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());
        RequestHandlerDelegate<string> next = (_) => Task.FromResult("ok");

        var result = await behavior.Handle(new TestRequest("anything"), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ValidatorsPass_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { new TestRequestValidator() });
        var called = false;
        RequestHandlerDelegate<string> next = (_) =>
        {
            called = true;
            return Task.FromResult("ok");
        };

        var result = await behavior.Handle(new TestRequest("Alice"), next, CancellationToken.None);

        called.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsAppExceptionWithFailedTag()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { new TestRequestValidator() });
        RequestHandlerDelegate<string> next = (_) => Task.FromResult("ok");

        var act = () => behavior.Handle(new TestRequest(string.Empty), next, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ex.Which.Tag.Should().Be(ErrorTags.Validation.Failed);
        ex.Which.Metadata.Should().NotBeNull();
        ex.Which.Metadata!.Should().ContainKey(nameof(TestRequest.Name));
    }

    [Fact]
    public async Task Handle_MultipleValidators_AggregatesErrorsByProperty()
    {
        var first = Substitute.For<IValidator<TestRequest>>();
        first
            .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "first failure"),
            }));

        var second = Substitute.For<IValidator<TestRequest>>();
        second
            .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "second failure"),
                new ValidationFailure("Other", "other failure"),
            }));

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { first, second });
        RequestHandlerDelegate<string> next = (_) => Task.FromResult("ok");

        var act = () => behavior.Handle(new TestRequest("ok"), next, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Metadata.Should().NotBeNull();
        ex.Which.Metadata!.Should().HaveCount(2);
        ex.Which.Metadata!["Name"].Should().Be("first failure");
        ex.Which.Metadata!["Other"].Should().Be("other failure");
    }
}
