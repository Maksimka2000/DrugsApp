using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientHealthcareProfessional;

/// <summary>
/// Request to load a healthcare professional visible to a patient.
/// </summary>
/// <param name="HealthcareProfessionalUserId">Healthcare professional user identifier.</param>
public sealed record GetPatientHealthcareProfessionalRequest(Guid HealthcareProfessionalUserId);

/// <summary>
/// Query to load a patient-visible healthcare professional.
/// </summary>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="HealthcareProfessionalUserId">Healthcare professional user identifier.</param>
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
            detail.ProfilePhotoStorageKey is not null,
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
