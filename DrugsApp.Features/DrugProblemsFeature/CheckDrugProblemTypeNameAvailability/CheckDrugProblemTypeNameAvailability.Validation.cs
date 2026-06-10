using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemTypeNameAvailability;

/// <summary>
/// Validator for drug problem type name availability.
/// </summary>
public sealed class CheckDrugProblemTypeNameAvailabilityRequestValidator : AbstractValidator<CheckDrugProblemTypeNameAvailabilityRequest>
{
    public CheckDrugProblemTypeNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.ProblemTypeName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(200);

        When(request => request.ExcludeId is not null, () =>
        {
            RuleFor(request => request.ExcludeId!.Value)
                .NotEqual(Guid.Empty);
        });
    }
}
