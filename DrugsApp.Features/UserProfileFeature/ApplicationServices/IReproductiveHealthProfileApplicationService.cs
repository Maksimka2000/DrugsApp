using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.ApplicationServices;

/// <summary>
/// Application service contract for reproductive health profile upsert workflows.
/// </summary>
public interface IReproductiveHealthProfileApplicationService
{
    Task<ErrorOr<ReproductiveHealthProfileUpsertResult>> UpsertAsync(
        Guid userId,
        ReproductiveHealthProfileUpsertRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Request payload for reproductive health profile upsert operations.
/// </summary>
/// <param name="CurrentlyPregnant">Whether the user is currently pregnant.</param>
/// <param name="PlanningToGetPregnant">Whether the user plans pregnancy.</param>
/// <param name="PregnancyPlanningUnknown">Whether pregnancy planning status is explicitly unknown.</param>
/// <param name="Menopause">Whether the user declares menopause.</param>
/// <param name="ContraceptionMethodCode">Optional contraception method code.</param>
public sealed record ReproductiveHealthProfileUpsertRequest(
    bool CurrentlyPregnant,
    bool PlanningToGetPregnant,
    bool PregnancyPlanningUnknown,
    bool Menopause,
    ContraceptionMethodCode? ContraceptionMethodCode);

/// <summary>
/// Result of a reproductive health profile upsert.
/// </summary>
/// <param name="Profile">The resulting profile aggregate.</param>
/// <param name="Created">Indicates whether the profile was newly created.</param>
public sealed record ReproductiveHealthProfileUpsertResult(
    ClientReproductiveHealthProfile Profile,
    bool Created);
