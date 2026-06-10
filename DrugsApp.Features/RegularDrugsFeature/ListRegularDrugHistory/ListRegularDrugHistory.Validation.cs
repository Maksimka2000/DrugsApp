using FluentValidation;

namespace DrugsApp.Features.RegularDrugsFeature.ListRegularDrugHistory;

/// <summary>
/// Validates list regular drug history query parameters.
/// </summary>
public sealed class ListRegularDrugHistoryRequestValidator : AbstractValidator<ListRegularDrugHistoryRequest>
{
    public ListRegularDrugHistoryRequestValidator()
    {
        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);

        When(
            request => !string.IsNullOrWhiteSpace(request.DrugNameQuery),
            () =>
            {
                RuleFor(request => request.DrugNameQuery!)
                    .MaximumLength(200);
            });
    }
}
