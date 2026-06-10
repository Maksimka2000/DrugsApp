using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblemType;

/// <summary>
/// Validator for creating drug problem types.
/// </summary>
public sealed class CreateDrugProblemTypeRequestValidator : AbstractValidator<CreateDrugProblemTypeRequest>
{
    public CreateDrugProblemTypeRequestValidator()
    {
        RuleFor(request => request.ProblemTypeName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(200);
    }
}
