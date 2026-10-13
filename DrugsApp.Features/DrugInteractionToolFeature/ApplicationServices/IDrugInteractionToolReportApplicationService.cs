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
        CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? selectedSupplementIds = null,
        IReadOnlyCollection<Guid>? selectedFoodIds = null,
        bool showPossibleInteractions = true);

    /// <summary>
    /// Computes the authenticated client tool report (CASE ONE) or the provider report over a supervised patient.
    /// </summary>
    /// <remarks>
    /// <paramref name="selectedSupplementIds"/> and <paramref name="selectedFoodIds"/> are catalog items selected for
    /// the analysis (for a provider: items selected for prescription); they are analysed like the selected drugs and
    /// are never treated as taken regularly.
    /// </remarks>
    Task<ErrorOr<RegisteredDrugInteractionToolReportReadModel>> ComputeRegisteredReportAsync(
        Guid userId,
        IReadOnlyCollection<Guid> manualDrugFormIds,
        bool includeUserData,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? selectedSupplementIds = null,
        IReadOnlyCollection<Guid>? selectedFoodIds = null,
        ReportAudience audience = ReportAudience.Registered,
        bool showPossibleInteractions = true);
}
