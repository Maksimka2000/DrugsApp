using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;

/// <summary>
/// Shared orchestration contract for public and registered drug interaction tool reports [description].
/// </summary>
public interface IDrugInteractionToolReportApplicationService
{
    /// <summary>
    /// Computes the anonymous public tool report (CASE TWO).
    /// </summary>
    Task<ErrorOr<PublicDrugInteractionToolReportReadModel>> ComputePublicReportAsync(
        IReadOnlyCollection<Guid> manualDrugFormIds,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken);

    /// <summary>
    /// Computes the authenticated client tool report (CASE ONE).
    /// </summary>
    Task<ErrorOr<RegisteredDrugInteractionToolReportReadModel>> ComputeRegisteredReportAsync(
        Guid userId,
        IReadOnlyCollection<Guid> manualDrugFormIds,
        bool includeUserData,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken);
}
