using DrugsApp.Domain.PatientSupervisionFeature;
using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.SubmitProviderProposal;

public sealed class SubmitProviderProposalRequestValidator : AbstractValidator<SubmitProviderProposalRequest>
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "regular_drug",
        "regular_food",
        "regular_supplement",
    };

    private const int MaxIntakeTimesPerItem = 12;

    public SubmitProviderProposalRequestValidator()
    {
        RuleFor(x => x.DraftItems)
            .NotNull()
            .Must(items => items.Count > 0)
            .WithMessage("At least one draft item is required.");

        RuleForEach(x => x.DraftItems)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.CategoryCode)
                    .NotEmpty()
                    .Must(code => AllowedCategories.Contains(code))
                    .WithMessage("category_code must be one of: regular_drug, regular_supplement, regular_food.");

                item.RuleFor(i => i.Note)
                    .MaximumLength(1000);

                item.RuleFor(i => i.RelatedCatalogId)
                    .NotEmpty()
                    .When(i => RequiresRelatedCatalogId(i.CategoryCode, i.Source));

                item.RuleFor(i => i.EndDate)
                    .GreaterThanOrEqualTo(i => i.StartDate!.Value)
                    .When(i => i.StartDate.HasValue && i.EndDate.HasValue)
                    .WithMessage("end_date must be on or after start_date.");

                item.When(
                    i => i.IntakeTimes is { Count: > 0 },
                    () =>
                    {
                        item.RuleFor(i => i.IntakeTimes!)
                            .Must(times => times.Count <= MaxIntakeTimesPerItem)
                            .WithMessage($"intake_times must not contain more than {MaxIntakeTimesPerItem} items.")
                            .Must(times => times.Distinct().Count() == times.Count)
                            .WithMessage("intake_times must be unique.");
                    });

                item.RuleFor(i => i.Source)
                    .NotEmpty()
                    .When(i => i.CategoryCode.Equals("regular_supplement", StringComparison.OrdinalIgnoreCase));

                item.RuleFor(i => i.CustomValue)
                    .NotEmpty()
                    .When(i => i.CategoryCode.Equals("regular_supplement", StringComparison.OrdinalIgnoreCase)
                               && string.Equals(i.Source, "custom", StringComparison.OrdinalIgnoreCase));
            });
    }

    private static bool RequiresRelatedCatalogId(string categoryCode, string? source) =>
        categoryCode.Equals("regular_drug", StringComparison.OrdinalIgnoreCase)
        || categoryCode.Equals("regular_food", StringComparison.OrdinalIgnoreCase)
        || categoryCode.Equals("regular_supplement", StringComparison.OrdinalIgnoreCase)
        && string.Equals(source, "database", StringComparison.OrdinalIgnoreCase);
}
