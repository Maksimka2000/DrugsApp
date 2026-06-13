using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.UserProfileFeature.Shared;

public sealed record ReproductiveHealthProfileResponse(
    bool CurrentlyPregnant,
    bool PlanningToGetPregnant,
    bool PregnancyPlanningUnknown,
    bool Menopause,
    ContraceptionMethodCode? ContraceptionMethodCode,
    DateTime UpdatedAtUtc)
{
    public static ReproductiveHealthProfileResponse From(ClientReproductiveHealthProfile profile)
    {
        return new ReproductiveHealthProfileResponse(
            profile.CurrentlyPregnant,
            profile.PlanningToGetPregnant,
            profile.PregnancyPlanningUnknown,
            profile.Menopause,
            profile.ContraceptionMethodCode,
            profile.UpdatedAtUtc ?? profile.CreatedAtUtc);
    }
}
