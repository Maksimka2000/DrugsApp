using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemCustomNameAvailability;

/// <summary>
/// Validator for custom drug problem name availability request.
/// </summary>
public sealed class CheckDrugProblemCustomNameAvailabilityRequestValidator : AbstractValidator<CheckDrugProblemCustomNameAvailabilityRequest>
{
    public CheckDrugProblemCustomNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.CustomDrugName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("custom_drug_name must be provided.")
            .MaximumLength(300);

        When(request => request.ExcludeId is not null, () =>
        {
            RuleFor(request => request.ExcludeId!.Value)
                .NotEqual(Guid.Empty);
        });
    }
}
