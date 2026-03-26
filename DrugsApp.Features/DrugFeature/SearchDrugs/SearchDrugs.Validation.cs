using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugFeature.SearchDrugs;

/// <summary>
/// Validator for SearchDrugs query parameters.
/// </summary>
public sealed class SearchDrugsRequestValidator : AbstractValidator<SearchDrugsRequest>
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

    private static readonly HashSet<string> AllowedTypeOfDrug = new(StringComparer.OrdinalIgnoreCase)
    {
        "standard",
        "ophthalmic",
        "special"
    };

    public SearchDrugsRequestValidator()
    {
        When(x => x.SearchQuery is not null, () =>
        {
            RuleFor(x => x.SearchQuery)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Search query must not be empty when provided.")
                .Must(value => value!.Trim().Length <= 500)
                .WithMessage("Search query must not exceed 500 characters.");
        });


        When(x => x.TypeOfDrugs is not null, () =>
        {
            RuleFor(x => x.TypeOfDrugs!)
                .Must(types => types.All(type => AllowedTypeOfDrug.Contains(type.Trim())))
                .WithMessage("type_of_drug must be one of: all, standard, ophthalmic, special.")
                .Must(types => types.Distinct().Count() == types.Count)
                .WithMessage("Duplicate type_of_drug values are not allowed.");
        });
    
        RuleFor(x => x.SortBy!)
            .Must(value => AllowedSortBy.Contains(value.Trim()))
            .WithMessage("sort_by must be one of: commercial_name, created_at, updated_at.");
                
        RuleFor(x => x.OrderBy!)
            .Must(value => AllowedOrderBy.Contains(value.Trim()))
            .WithMessage("order_by must be one of: ascending, descending.");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("skip must be greater than or equal to 0.");
        
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 30)
            .WithMessage("take must be between 1 and 30.");
        
    }
}

