using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientHealthcareProfessional;

public sealed record GetPatientHealthcareProfessionalRequest(Guid HealthcareProfessionalUserId);

public sealed record GetPatientHealthcareProfessionalQuery(Guid PatientUserId, Guid HealthcareProfessionalUserId)
    : IRequest<ErrorOr<PatientHealthcareProfessionalDetailDto>>;

internal sealed class GetPatientHealthcareProfessionalHandler(ISupervisionReadRepository supervisionReadRepository)
    : IRequestHandler<GetPatientHealthcareProfessionalQuery, ErrorOr<PatientHealthcareProfessionalDetailDto>>
{
    public async Task<ErrorOr<PatientHealthcareProfessionalDetailDto>> Handle(
        GetPatientHealthcareProfessionalQuery query,
        CancellationToken cancellationToken)
    {
        PatientHealthcareProfessionalDetail? detail = await supervisionReadRepository.GetPatientHealthcareProfessionalDetailAsync(
            query.PatientUserId,
            query.HealthcareProfessionalUserId,
            cancellationToken);

        if (detail is null)
        {
            return Error.NotFound(
                description: "Healthcare professional not visible to patient.",
                metadata: new()
                {
                    ["resourceType"] = "HealthcareProfessional",
                    ["resourceId"] = query.HealthcareProfessionalUserId.ToString()
                });
        }

        return new PatientHealthcareProfessionalDetailDto(
            detail.HealthcareProfessionalUserId,
            detail.HasProfilePhoto,
            detail.FirstName,
            detail.LastName,
            detail.ProfessionLabel,
            detail.WorkplaceLabel,
            detail.ShortDescription,
            detail.RelationshipState,
            detail.AccessLevelCode,
            detail.HasPendingProposal,
            detail.PendingProposalCount,
            detail.ActiveRequestId,
            detail.ActiveConnectionId);
    }
}
