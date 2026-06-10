using FluentValidation;

namespace DrugsApp.Features.RegularDrugsFeature.ListRegularDrugs;

/// <summary>
/// Validates list regular drugs query parameters.
/// </summary>
public sealed class ListRegularDrugsRequestValidator : AbstractValidator<ListRegularDrugsRequest>
{
    public ListRegularDrugsRequestValidator()
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
