using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblemType;

/// <summary>
/// Validator for updating drug problem types.
/// </summary>
public sealed class UpdateDrugProblemTypeRequestValidator : AbstractValidator<UpdateDrugProblemTypeRequest>
{
    public UpdateDrugProblemTypeRequestValidator()
    {
        When(request => !string.IsNullOrWhiteSpace(request.ProblemTypeName), () =>
        {
            RuleFor(request => request.ProblemTypeName!)
                .NotEmpty()
                .MaximumLength(200);
        });

        RuleFor(request => request)
            .Must(request => !string.IsNullOrWhiteSpace(request.ProblemTypeName))
            .WithMessage("At least one field must be provided.");
    }
}
