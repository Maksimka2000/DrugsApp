using FluentValidation;

namespace DrugsApp.Features.InteractionFeature.SearchInteractions;

public sealed class SearchInteractionsRequestValidator : AbstractValidator<SearchInteractionsRequest>
{
    private static readonly string[] AllowedKinds = { "substance", "food", "supplement" };
    private static readonly string[] AllowedSortBy = { "severity", "created_at", "updated_at" };
    private static readonly string[] AllowedOrderBy = { "ascending", "descending" };

    public SearchInteractionsRequestValidator()
    {
        RuleFor(request => request.InteractionKind)
            .NotEmpty()
            .Must(kind => AllowedKinds.Contains(kind.ToLowerInvariant()))
            .WithMessage("interaction_kind must be one of: substance, food, supplement.");

        RuleFor(request => request.SortBy)
            .NotEmpty()
            .Must(sort => AllowedSortBy.Contains(sort.ToLowerInvariant()))
            .WithMessage("sort_by must be one of: severity, created_at, updated_at.");

        RuleFor(request => request.OrderBy)
            .NotEmpty()
            .Must(order => AllowedOrderBy.Contains(order.ToLowerInvariant()))
            .WithMessage("order_by must be one of: ascending, descending.");

        RuleFor(request => request.LeftQuery)
            .MaximumLength(256);

        RuleFor(request => request.RightQuery)
            .MaximumLength(256);

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
