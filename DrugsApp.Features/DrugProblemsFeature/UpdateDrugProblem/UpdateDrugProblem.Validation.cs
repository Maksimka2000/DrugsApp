using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblem;

/// <summary>
/// Validator for updating a drug problem.
/// </summary>
public sealed class UpdateDrugProblemRequestValidator : AbstractValidator<UpdateDrugProblemRequest>
{
    public UpdateDrugProblemRequestValidator()
    {
        When(request => request.DrugId is not null, () =>
        {
            RuleFor(request => request.DrugId!.Value)
                .NotEqual(Guid.Empty);
        });

        When(request => !string.IsNullOrWhiteSpace(request.CustomDrugName), () =>
        {
            RuleFor(request => request.CustomDrugName!)
                .NotEmpty()
                .MaximumLength(300);
        });

        When(request => !string.IsNullOrWhiteSpace(request.ProblemNote), () =>
        {
            RuleFor(request => request.ProblemNote!)
                .NotEmpty()
                .MaximumLength(2000);
        });

        RuleFor(request => request)
            .Must(request =>
                request.DrugId is not null ||
                !string.IsNullOrWhiteSpace(request.CustomDrugName) ||
                !string.IsNullOrWhiteSpace(request.ProblemNote))
            .WithMessage("At least one field must be provided.");
    }
}
