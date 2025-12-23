using FluentValidation;

namespace DrugsApp.Features.TestFeature.CreateTest;

/// <summary>
/// Validator for <see cref="CreateTestRequest"/>.
/// </summary>
public sealed class CreateTestRequestValidator : AbstractValidator<CreateTestRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTestRequestValidator"/> class.
    /// </summary>
    public CreateTestRequestValidator()
    {
        RuleFor(request => request.Attribute1)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(request => request.Attribute2)
            .GreaterThanOrEqualTo(0)
            .When(request => request.Attribute2.HasValue);

        RuleFor(request => request.Attribute3)
            .ExclusiveBetween(2, 10)
            .When(request => request.Attribute3.HasValue);
    }
}

