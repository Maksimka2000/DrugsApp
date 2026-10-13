using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionPrecheck;

public sealed class ProviderPatientInteractionPrecheckRequestValidator
    : AbstractValidator<ProviderPatientInteractionPrecheckRequest>
{
    public ProviderPatientInteractionPrecheckRequestValidator()
    {
        When(
            request => request.ManualDrugFormIds is { Count: > 0 },
            () =>
            {
                RuleFor(request => request.ManualDrugFormIds!)
                    .Must(ids => ids.Count <= 20)
                    .WithMessage("ManualDrugFormIds must not contain more than 20 items.")
                    .Must(ids => ids.All(id => id != Guid.Empty))
                    .WithMessage("ManualDrugFormIds must not contain empty Guid values.")
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("ManualDrugFormIds must be unique.");
            });

        When(
            request => request.AdditionalFoodIds is { Count: > 0 },
            () =>
            {
                RuleFor(request => request.AdditionalFoodIds!)
                    .Must(ids => ids.Count <= 20)
                    .WithMessage("AdditionalFoodIds must not contain more than 20 items.")
                    .Must(ids => ids.All(id => id != Guid.Empty))
                    .WithMessage("AdditionalFoodIds must not contain empty Guid values.")
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("AdditionalFoodIds must be unique.");
            });

        When(
            request => request.AdditionalSupplementIds is { Count: > 0 },
            () =>
            {
                RuleFor(request => request.AdditionalSupplementIds!)
                    .Must(ids => ids.Count <= 20)
                    .WithMessage("AdditionalSupplementIds must not contain more than 20 items.")
                    .Must(ids => ids.All(id => id != Guid.Empty))
                    .WithMessage("AdditionalSupplementIds must not contain empty Guid values.")
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("AdditionalSupplementIds must be unique.");
            });
    }
}
