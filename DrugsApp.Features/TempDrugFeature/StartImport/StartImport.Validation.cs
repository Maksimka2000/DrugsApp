using DrugsApp.Features.TempDrugFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.TempDrugFeature.StartImport;

/// <summary>
/// Validator for start import request payload.
/// </summary>
public sealed class StartImportRequestValidator : AbstractValidator<StartImportRequest>
{
    public StartImportRequestValidator()
    {
        RuleFor(request => request.MappingPlan)
            .NotNull()
            .Must(mappingPlan => mappingPlan.Count > 0)
            .WithMessage("MappingPlan must contain at least one mapping.");

        RuleForEach(request => request.MappingPlan.Keys)
            .Must(fieldName => TempDrugFields.All.Contains(fieldName))
            .WithMessage("MappingPlan contains unsupported TempDrug field.");

        RuleForEach(request => request.MappingPlan.Values)
            .NotEmpty()
            .WithMessage("Each mapping value must be a non-empty source column name.");

        When(
            request => request.Notes is not null,
            () => RuleFor(request => request.Notes)
                .MaximumLength(2000)
                .WithMessage("Notes must not exceed 2000 characters."));
    }
}

