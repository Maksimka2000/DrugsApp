using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionReport;

/// <summary>
/// Request payload for a provider-run safety report over a supervised patient's stored data.
/// </summary>
/// <param name="ShowAllDrugsInteractions">Whether drug-to-drug interactions are included.</param>
/// <param name="ShowAllFoodAndSupplementInteractions">Whether food and supplement interactions are included.</param>
/// <param name="ShowOralGuidance">Whether oral intake guidance is included.</param>
public sealed record ProviderPatientInteractionReportRequest(
    bool ShowAllDrugsInteractions,
    bool ShowAllFoodAndSupplementInteractions,
    bool ShowOralGuidance);

/// <summary>
/// Response envelope for the provider safety report.
/// </summary>
/// <param name="Report">Computed registered-style interaction report for the patient.</param>
public sealed record ProviderPatientInteractionReportResponse(
    RegisteredDrugInteractionToolReportReadModel Report);

/// <summary>
/// Command to compute the safety report of a supervised patient for a provider.
/// </summary>
/// <param name="ProviderUserId">Provider user identifier.</param>
/// <param name="PatientId">Patient user identifier.</param>
/// <param name="Request">Report request payload.</param>
public sealed record ExecuteProviderPatientInteractionReportCommand(
    Guid ProviderUserId,
    Guid PatientId,
    ProviderPatientInteractionReportRequest Request)
    : IRequest<ErrorOr<ProviderPatientInteractionReportResponse>>;

internal sealed class ExecuteProviderPatientInteractionReportHandler(
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    ISupervisionConnectionRepository supervisionConnectionRepository,
    IDrugInteractionToolReportApplicationService reportApplicationService)
    : IRequestHandler<ExecuteProviderPatientInteractionReportCommand, ErrorOr<ProviderPatientInteractionReportResponse>>
{
    public async Task<ErrorOr<ProviderPatientInteractionReportResponse>> Handle(
        ExecuteProviderPatientInteractionReportCommand command,
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

        bool canReadRecord = connection.AccessLevelCode is SupervisionAccessLevel.ViewOnly
            or SupervisionAccessLevel.ViewAndModify;
        if (!canReadRecord)
        {
            return Error.Forbidden(description: "Access level does not allow a safety report for this patient.");
        }

        ErrorOr<RegisteredDrugInteractionToolReportReadModel> reportResult =
            await reportApplicationService.ComputeRegisteredReportAsync(
                connection.PatientUserId,
                Array.Empty<Guid>(),
                includeUserData: true,
                command.Request.ShowAllDrugsInteractions,
                command.Request.ShowOralGuidance,
                command.Request.ShowAllFoodAndSupplementInteractions,
                cancellationToken);

        if (reportResult.IsError)
        {
            return reportResult.Errors;
        }

        return new ProviderPatientInteractionReportResponse(reportResult.Value);
    }
}
