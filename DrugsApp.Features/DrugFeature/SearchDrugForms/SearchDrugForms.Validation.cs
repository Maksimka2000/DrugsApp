using System;
using System.Collections.Generic;
using FluentValidation;

namespace DrugsApp.Features.DrugFeature.SearchDrugForms;

/// <summary>
/// Validator for SearchDrugForms query parameters.
/// </summary>
public sealed class SearchDrugFormsRequestValidator : AbstractValidator<SearchDrugFormsRequest>
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "commercial_name",
        "created_at",
        "updated_at"
    };

    private static readonly HashSet<string> AllowedOrderBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "ascending",
        "descending"
    };

    public SearchDrugFormsRequestValidator()
    {
        When(request => request.SearchQuery is not null, () =>
        {
            RuleFor(request => request.SearchQuery)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Search query must not be empty when provided.")
                .Must(value => value!.Trim().Length <= 500)
                .WithMessage("Search query must not exceed 500 characters.");
        });

        RuleFor(request => request.SortBy)
            .Must(value => AllowedSortBy.Contains(value.Trim()))
            .WithMessage("sort_by must be one of: commercial_name, created_at, updated_at.");

        RuleFor(request => request.OrderBy)
            .Must(value => AllowedOrderBy.Contains(value.Trim()))
            .WithMessage("order_by must be one of: ascending, descending.");

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 30)
            .WithMessage("take must be between 1 and 30.");
    }
}
