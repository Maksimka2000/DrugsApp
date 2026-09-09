using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionPrecheck;

/// <summary>
/// Request payload for provider interaction pre-check against a supervised patient.
/// </summary>
/// <param name="ManualDrugFormIds">Proposed medication form ids from the recommendation basket.</param>
/// <param name="AdditionalFoodIds">Proposed food catalog ids to include in taken-now analysis.</param>
/// <param name="AdditionalSupplementIds">Proposed supplement catalog ids to include in taken-now analysis.</param>
public sealed record ProviderPatientInteractionPrecheckRequest(
    IReadOnlyCollection<Guid>? ManualDrugFormIds,
    IReadOnlyCollection<Guid>? AdditionalFoodIds,
    IReadOnlyCollection<Guid>? AdditionalSupplementIds);

/// <summary>
/// Response envelope for provider interaction pre-check.
/// </summary>
/// <param name="Report">Computed registered-style interaction report for the patient.</param>
public sealed record ProviderPatientInteractionPrecheckResponse(
    RegisteredDrugInteractionToolReportReadModel Report);

/// <summary>
/// Command to run interaction pre-check for a supervised patient.
/// </summary>
/// <param name="ProviderUserId">Provider user identifier.</param>
/// <param name="PatientId">Patient user identifier.</param>
/// <param name="Request">Pre-check request payload.</param>
public sealed record ExecuteProviderPatientInteractionPrecheckCommand(
    Guid ProviderUserId,
    Guid PatientId,
    ProviderPatientInteractionPrecheckRequest Request)
    : IRequest<ErrorOr<ProviderPatientInteractionPrecheckResponse>>;

internal sealed class ExecuteProviderPatientInteractionPrecheckHandler(
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    ISupervisionConnectionRepository supervisionConnectionRepository,
    IDrugInteractionToolReportApplicationService reportApplicationService)
    : IRequestHandler<ExecuteProviderPatientInteractionPrecheckCommand, ErrorOr<ProviderPatientInteractionPrecheckResponse>>
{
    public async Task<ErrorOr<ProviderPatientInteractionPrecheckResponse>> Handle(
        ExecuteProviderPatientInteractionPrecheckCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<Success> profileGuard = await HealthcareProviderProfileGuard.EnsureExistsAsync(
            healthcareProviderProfileRepository,
            command.ProviderUserId,
            cancellationToken);
        if (profileGuard.IsError)
        {
            return profileGuard.Errors;
        }

        SupervisionConnection? connection = await supervisionConnectionRepository.GetActiveForProviderAndPatientAsync(
            command.ProviderUserId,
            command.PatientId,
            track: false,
            cancellationToken);

        if (connection is null)
        {
            return Error.NotFound(
                description: "Active supervision connection not found for patient.",
                metadata: new()
                {
                    ["resourceType"] = "SupervisionConnection",
                    ["resourceId"] = command.PatientId.ToString()
                });
        }

        if (connection.AccessLevelCode == SupervisionAccessLevel.BlockAccess)
        {
            return Error.Forbidden(description: "Access to this patient's record is blocked.");
        }

        IReadOnlyCollection<Guid> manualDrugFormIds = command.Request.ManualDrugFormIds ?? Array.Empty<Guid>();
        IReadOnlyCollection<Guid> additionalFoodIds = command.Request.AdditionalFoodIds ?? Array.Empty<Guid>();
        IReadOnlyCollection<Guid> additionalSupplementIds =
            command.Request.AdditionalSupplementIds ?? Array.Empty<Guid>();

        ErrorOr<RegisteredDrugInteractionToolReportReadModel> reportResult =
            await reportApplicationService.ComputeRegisteredReportAsync(
                connection.PatientUserId,
                manualDrugFormIds,
                includeUserData: true,
                showAllDrugsInteractions: true,
                showOralGuidance: true,
                showAllFoodAndSupplementInteractions: true,
                cancellationToken,
                additionalTakenNowFoodIds: additionalFoodIds,
                additionalTakenNowSupplementIds: additionalSupplementIds);

        if (reportResult.IsError)
        {
            return reportResult.Errors;
        }

        return new ProviderPatientInteractionPrecheckResponse(reportResult.Value);
    }
}
