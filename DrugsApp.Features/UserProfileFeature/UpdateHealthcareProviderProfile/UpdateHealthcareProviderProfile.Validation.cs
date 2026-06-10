using DrugsApp.Domain.UserProfileFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.UserProfileFeature.UpdateHealthcareProviderProfile;

public sealed class UpdateHealthcareProviderProfileRequestValidator : AbstractValidator<UpdateHealthcareProviderProfileRequest>
{
    public UpdateHealthcareProviderProfileRequestValidator()
    {
        RuleFor(r => r.ProfessionCode)
            .Must(code => code is null || Enum.IsDefined(typeof(HealthcareProviderProfession), code.Value))
            .WithMessage("Invalid profession code.");

        When(
            r => r.ProfessionCode == HealthcareProviderProfession.other,
            () =>
            {
                RuleFor(r => r.OtherProfessionLabel)
                    .NotEmpty()
                    .MaximumLength(100);
            });

        When(
            r => !string.IsNullOrWhiteSpace(r.WorkplaceLabel),
            () =>
            {
                RuleFor(r => r.WorkplaceLabel)
                    .MaximumLength(150);
            });

        When(
            r => !string.IsNullOrWhiteSpace(r.ShortDescription),
            () =>
            {
                RuleFor(r => r.ShortDescription)
                    .MaximumLength(500);
            });
    }
}
