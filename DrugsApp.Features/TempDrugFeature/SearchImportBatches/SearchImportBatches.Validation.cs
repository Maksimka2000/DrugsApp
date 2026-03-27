using FluentValidation;

namespace DrugsApp.Features.TempDrugFeature.SearchImportBatches;

/// <summary>
/// Validator for import batches history query parameters.
/// </summary>
public sealed class SearchImportBatchesRequestValidator : AbstractValidator<SearchImportBatchesRequest>
{


    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "created_at",
    };

    private static readonly HashSet<string> AllowedOrderBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "ascending",
        "descending"   
    };
    public SearchImportBatchesRequestValidator()
    {
        RuleFor(request => request.SortBy!)
            .Must(sortBy => AllowedSortBy.Contains(sortBy.Trim()))
            .WithMessage("sort_by must be set to 'date'.");

        RuleFor(request => request.OrderBy!)
            .Must(orderBy => AllowedOrderBy.Contains(orderBy.Trim()))
            .WithMessage("order_by must be 'ascending' or 'descending'.");

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 30)
            .WithMessage("take must be between 1 and 30.");
    }
}

