using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;

/// <summary>
/// Validator for creating a drug problem.
/// </summary>
public sealed class CreateDrugProblemRequestValidator : AbstractValidator<CreateDrugProblemRequest>
{
    public CreateDrugProblemRequestValidator()
    {
        RuleFor(request => request.ProblemNote)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("problemNote must be provided.")
            .MaximumLength(2000);

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

        RuleFor(request => request)
            .Must(request =>
                request.DrugId is not null && string.IsNullOrWhiteSpace(request.CustomDrugName) ||
                request.DrugId is null && !string.IsNullOrWhiteSpace(request.CustomDrugName))
            .WithMessage("Exactly one of drugId or customDrugName must be provided.");
    }
}
