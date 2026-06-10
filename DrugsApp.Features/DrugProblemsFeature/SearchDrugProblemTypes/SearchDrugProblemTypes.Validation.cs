using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblemTypes;

/// <summary>
/// Validator for searching drug problem types.
/// </summary>
public sealed class SearchDrugProblemTypesRequestValidator : AbstractValidator<SearchDrugProblemTypesRequest>
{
    public SearchDrugProblemTypesRequestValidator()
    {
        When(request => !string.IsNullOrWhiteSpace(request.ProblemTypeNameQuery), () =>
        {
            RuleFor(request => request.ProblemTypeNameQuery!)
                .NotEmpty()
                .MaximumLength(200);
        });

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
