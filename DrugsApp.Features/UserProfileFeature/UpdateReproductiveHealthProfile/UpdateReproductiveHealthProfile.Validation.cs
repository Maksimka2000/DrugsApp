using DrugsApp.Domain.UserProfileFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.UserProfileFeature.UpdateReproductiveHealthProfile;

public sealed class UpdateReproductiveHealthProfileRequestValidator : AbstractValidator<UpdateReproductiveHealthProfileRequest>
{
    public UpdateReproductiveHealthProfileRequestValidator()
    {
        RuleFor(r => r.ContraceptionMethodCode)
            .Must(code => code is null || Enum.IsDefined(typeof(ContraceptionMethodCode), code.Value))
            .WithMessage("Invalid contraception method code.");

        RuleFor(r => r.PregnancyPlanningUnknown)
            .Must((request, unknown) =>
                !unknown || !request.CurrentlyPregnant && !request.PlanningToGetPregnant)
            .WithMessage(
                "Pregnancy planning cannot be unknown when pregnancy status is declared.");
    }
}
