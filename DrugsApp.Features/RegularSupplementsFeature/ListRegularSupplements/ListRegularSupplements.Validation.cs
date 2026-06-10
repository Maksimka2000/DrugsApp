using FluentValidation;

namespace DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplements;

/// <summary>
/// Validates list regular supplements query parameters.
/// </summary>
public sealed class ListRegularSupplementsRequestValidator : AbstractValidator<ListRegularSupplementsRequest>
{
    public ListRegularSupplementsRequestValidator()
    {
        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);

        When(
            request => !string.IsNullOrWhiteSpace(request.NameQuery),
            () =>
            {
                RuleFor(request => request.NameQuery!)
                    .MaximumLength(200);
            });
    }
}
