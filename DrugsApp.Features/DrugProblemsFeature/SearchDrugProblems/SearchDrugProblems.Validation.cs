using FluentValidation;

namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblems;

/// <summary>
/// Validator for searching drug problems.
/// </summary>
public sealed class SearchDrugProblemsRequestValidator : AbstractValidator<SearchDrugProblemsRequest>
{
    public SearchDrugProblemsRequestValidator()
    {
        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
