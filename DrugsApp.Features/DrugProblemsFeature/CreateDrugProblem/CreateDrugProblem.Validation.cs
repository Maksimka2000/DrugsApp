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

        When(request => request.DrugFormId is not null, () =>
        {
            RuleFor(request => request.DrugFormId!.Value)
                .NotEqual(System.Guid.Empty);
        });

        When(request => !string.IsNullOrWhiteSpace(request.CustomDrugName), () =>
        {
            RuleFor(request => request.CustomDrugName!)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(request => request.CustomActiveSubstances!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("customActiveSubstances must be provided.")
                .MaximumLength(300);

            RuleFor(request => request.CustomFormulationName!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("customFormulationName must be provided.")
                .MaximumLength(300);

            RuleFor(request => request.CustomDosage!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("customDosage must be provided.")
                .MaximumLength(200);
        });

        RuleFor(request => request)
            .Must(request =>
                request.DrugFormId is not null && string.IsNullOrWhiteSpace(request.CustomDrugName) ||
                request.DrugFormId is null && !string.IsNullOrWhiteSpace(request.CustomDrugName))
            .WithMessage("Exactly one of drugFormId or customDrugName must be provided.");
    }
}
