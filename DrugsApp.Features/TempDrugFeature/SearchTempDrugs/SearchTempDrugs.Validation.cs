using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.TempDrugFeature.Entities;
using FluentValidation;

namespace DrugsApp.Features.TempDrugFeature.SearchTempDrugs;

/// <summary>
/// Validator for SearchTempDrugs query parameters.
/// </summary>
public sealed class SearchTempDrugsRequestValidator : AbstractValidator<SearchTempDrugsRequest>
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "created_at",
        "updated_at",
        "commercial_name",
        "identifier"
    };

    private static readonly HashSet<string> AllowedOrderBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "ascending",
        "descending"
    };

    private static readonly HashSet<string> AllowedTypeOfProduct = new(StringComparer.OrdinalIgnoreCase)
    {
        "ludzki",
        "weterynaryjny"
    };
    
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending_review",
        "review_later",
        "reviewed_resolved",
        "reviewed_added"
    };

    public SearchTempDrugsRequestValidator()
    {
        When(x => x.CommercialName is not null, () => RuleFor(x => x.CommercialName)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("commercial_name must not be empty when provided.")
                .Must(value => value!.Trim().Length <= 300)
                .WithMessage("commercial_name must not exceed 300 characters."));

        When(x => x.Statuses is not null, () => RuleFor(x => x.Statuses!)
                .Must(statuses => statuses.All(status => AllowedStatuses.Contains(status.Trim())))
                .WithMessage("statuses contains invalid values. Satus must be one of: pending_review, review_later, reviewed_resolved, reviewed_added.")
                .Must(statuses => statuses.Distinct().Count() == statuses.Count)
                .WithMessage("Duplicate statuses values are not allowed."));

        When(x => x.TypeOfProducts is not null, () => RuleFor(x => x.TypeOfProducts!)
                .Must(types => types.All(type => AllowedTypeOfProduct.Contains(type.Trim())))
                .WithMessage("type_of_products must be one of: Ludzki, Weterynaryjny.")
                .Must(types => types
                    .Select(type => type.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count() == types.Count)
                .WithMessage("Duplicate type_of_products values are not allowed."));

        RuleFor(x => x.SortBy!)
            .Must(value => AllowedSortBy.Contains(value.Trim()))
            .WithMessage("sort_by must be one of: created_at, updated_at, commercial_name, identifier.");

        RuleFor(x => x.OrderBy!)
            .Must(value => AllowedOrderBy.Contains(value.Trim()))
            .WithMessage("order_by must be one of: ascending, descending.");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("skip must be greater than or equal to 0.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("take must be between 1 and 40.");
    }
}


