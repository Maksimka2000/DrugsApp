using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.GetSupervisionRequest;

public sealed record GetSupervisionRequestRequest(Guid RequestId);

public sealed record GetSupervisionRequestQuery(Guid PatientUserId, Guid RequestId)
    : IRequest<ErrorOr<SupervisionRequestDetailDto>>;

internal sealed class GetSupervisionRequestHandler(
    ISupervisionRequestRepository supervisionRequestRepository,
    ISupervisionReadRepository supervisionReadRepository)
    : IRequestHandler<GetSupervisionRequestQuery, ErrorOr<SupervisionRequestDetailDto>>
{
    public async Task<ErrorOr<SupervisionRequestDetailDto>> Handle(
        GetSupervisionRequestQuery query,
        CancellationToken cancellationToken)
    {
        SupervisionRequest? request = await supervisionRequestRepository.GetByIdForPatientAsync(
            query.PatientUserId,
            query.RequestId,
            track: false,
            cancellationToken);

        if (request is null)
        {
            return Error.NotFound(
                description: "Supervision request not found for patient.",
                metadata: new()
                {
                    ["resourceType"] = "SupervisionRequest",
                    ["resourceId"] = query.RequestId.ToString()
                });
        }

        bool isExpired = request.RequestState == SupervisionRequestState.Pending && DateTime.UtcNow >= request.ExpiresAtUtc;

        PatientHealthcareProfessionalDetail? providerDetail = await supervisionReadRepository.GetPatientHealthcareProfessionalDetailAsync(
            query.PatientUserId,
            request.HealthcareProfessionalUserId,
            cancellationToken);

        string healthcareProfessionalSummary = providerDetail is null
            ? "Healthcare professional"
            : $"{providerDetail.FirstName} {providerDetail.LastName}".Trim();

        var dto = new SupervisionRequestDetailDto(
            RequestId: request.Id,
            RequestState: isExpired ? SupervisionRequestState.Expired.ToString() : request.RequestState.ToString(),
            ExpiresAtUtc: request.ExpiresAtUtc,
            HealthcareProfessionalSummary: healthcareProfessionalSummary,
            AvailableAccessLevels: new List<string> { "ViewAndModify", "ViewOnly", "RestrictedView", "BlockAccess" },
            PrivacyWarningText: "Sharing grants provider access per selected level.");

        return dto;
    }
}
