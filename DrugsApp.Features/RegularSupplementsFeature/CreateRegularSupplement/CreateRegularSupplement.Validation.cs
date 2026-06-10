using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;

/// <summary>
/// Validates create regular supplement request payloads.
/// </summary>
public sealed class CreateRegularSupplementRequestValidator : AbstractValidator<CreateRegularSupplementRequest>
{
    public CreateRegularSupplementRequestValidator()
    {
        RuleFor(request => request.Source)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(source => source.Equals("database", StringComparison.OrdinalIgnoreCase) ||
                            source.Equals("own", StringComparison.OrdinalIgnoreCase))
            .WithMessage("source must be database or own.");

        When(
            request => request.Source.Equals("database", StringComparison.OrdinalIgnoreCase),
            () =>
            {
                RuleFor(request => request.SupplementId)
                    .NotNull()
                    .NotEmpty();

                RuleFor(request => request.OwnSupplementId)
                    .Null();
            });

        When(
            request => request.Source.Equals("own", StringComparison.OrdinalIgnoreCase),
            () =>
            {
                RuleFor(request => request.OwnSupplementId)
                    .NotNull()
                    .NotEmpty();

                RuleFor(request => request.SupplementId)
                    .Null();
            });

        When(
            request => request.IntakeTimes is not null,
            () =>
            {
                RuleFor(request => request.IntakeTimes!)
                    .Must(intakeTimes => intakeTimes.Distinct().Count() == intakeTimes.Count)
                    .WithMessage("Intake times must be unique.");
            });
    }
}
