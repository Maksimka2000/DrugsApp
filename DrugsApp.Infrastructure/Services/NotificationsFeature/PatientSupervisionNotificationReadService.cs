using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using ErrorOr;

namespace DrugsApp.Infrastructure.Services.NotificationsFeature;

/// <summary>
/// Placeholder adapter until PatientSupervisionFeature read contracts are implemented.
/// </summary>
public sealed class PatientSupervisionNotificationReadService
    : IPatientSupervisionNotificationReadService
{
    public Task<ErrorOr<SupervisionRequestNotificationDetail>> GetRequestDetailAsync(
        Guid requestId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<ErrorOr<SupervisionRequestNotificationDetail>>(Error.NotFound(
            code: "SupervisionRequest.DependencyNotImplemented",
            description: "Patient supervision request detail hydration is not available yet.",
            metadata: new()
            {
                ["resourceType"] = "SupervisionRequest",
                ["resourceId"] = requestId.ToString(),
                ["dependency"] = "PatientSupervisionFeature"
            }));
    }

    public Task<ErrorOr<MedicalChangeProposalNotificationDetail>> GetProposalDetailAsync(
        Guid proposalId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<ErrorOr<MedicalChangeProposalNotificationDetail>>(Error.NotFound(
            code: "MedicalChangeProposal.DependencyNotImplemented",
            description: "Patient supervision proposal detail hydration is not available yet.",
            metadata: new()
            {
                ["resourceType"] = "MedicalChangeProposal",
                ["resourceId"] = proposalId.ToString(),
                ["dependency"] = "PatientSupervisionFeature"
            }));
    }
}
