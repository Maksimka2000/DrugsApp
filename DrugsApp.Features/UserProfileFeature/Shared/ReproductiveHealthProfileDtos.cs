using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.UserProfileFeature.Shared;

/// <summary>
/// 
/// </summary>
/// <param name="CurrentlyPregnant"></param>
/// <param name="PlanningToGetPregnant"></param>
/// <param name="PregnancyPlanningUnknown"></param>
/// <param name="Menopause"></param>
/// <param name="ContraceptionMethodCode"></param>
/// <param name="UpdatedAtUtc"></param>
public sealed record ReproductiveHealthProfileResponse(
    bool CurrentlyPregnant,
    bool PlanningToGetPregnant,
    bool PregnancyPlanningUnknown,
    bool Menopause,
    ContraceptionMethodCode? ContraceptionMethodCode,
    DateTime UpdatedAtUtc)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
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
