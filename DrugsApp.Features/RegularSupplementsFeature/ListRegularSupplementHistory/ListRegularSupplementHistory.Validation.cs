using FluentValidation;

namespace DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplementHistory;

/// <summary>
/// Validates list regular supplement history query parameters.
/// </summary>
public sealed class ListRegularSupplementHistoryRequestValidator : AbstractValidator<ListRegularSupplementHistoryRequest>
{
    public ListRegularSupplementHistoryRequestValidator()
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
