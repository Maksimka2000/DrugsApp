using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;

/// <summary>
/// Validates create regular drug request payloads.
/// </summary>
public sealed class CreateRegularDrugRequestValidator : AbstractValidator<CreateRegularDrugRequest>
{
    public CreateRegularDrugRequestValidator()
    {
        RuleFor(request => request.DrugId)
            .NotEmpty();

        RuleFor(request => request.DrugFormId)
            .NotEmpty();

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
