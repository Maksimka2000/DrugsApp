using System;
using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblem;

/// <summary>
/// Validator for updating a drug problem.
/// </summary>
public sealed class UpdateDrugProblemRequestValidator : AbstractValidator<UpdateDrugProblemRequest>
{
    public UpdateDrugProblemRequestValidator()
    {
        When(request => request.DrugFormId is not null, () =>
        {
            RuleFor(request => request.DrugFormId!.Value)
                .NotEqual(Guid.Empty);
        });

        When(request => HasAnyCustomDrugField(request), () =>
        {
            RuleFor(request => request.CustomDrugName!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(request => request.CustomActiveSubstances!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(request => request.CustomFormulationName!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(request => request.CustomDosage!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(200);
        });

        When(request => !string.IsNullOrWhiteSpace(request.ProblemNote), () =>
        {
            RuleFor(request => request.ProblemNote!)
                .NotEmpty()
                .MaximumLength(2000);
        });

        RuleFor(request => request)
            .Must(request =>
                request.DrugFormId is not null ||
                HasAnyCustomDrugField(request) ||
                !string.IsNullOrWhiteSpace(request.ProblemNote))
            .WithMessage("At least one field must be provided.");
    }

    private static bool HasAnyCustomDrugField(UpdateDrugProblemRequest request) =>
        !string.IsNullOrWhiteSpace(request.CustomDrugName) ||
        !string.IsNullOrWhiteSpace(request.CustomActiveSubstances) ||
        !string.IsNullOrWhiteSpace(request.CustomFormulationName) ||
        !string.IsNullOrWhiteSpace(request.CustomDosage);
}
