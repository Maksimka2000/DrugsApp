using DrugsApp.Domain.PatientSupervisionFeature.Entities;

using DrugsApp.Domain.PatientSupervisionFeature.Enums;

using DrugsApp.Domain.PatientSupervisionFeature.Repositories;

using DrugsApp.Features.PatientSupervisionFeature.Shared;

using ErrorOr;

using MediatR;



namespace DrugsApp.Features.PatientSupervisionFeature.GetSupervisionRequest;

/// <summary>
/// Request payload for fetching a supervision request detail.
/// </summary>
/// <param name="RequestId">Supervision request identifier.</param>
public sealed record GetSupervisionRequestRequest(Guid RequestId);

/// <summary>
/// Query to fetch a supervision request detail for a patient.
/// </summary>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="RequestId">Supervision request identifier.</param>
public sealed record GetSupervisionRequestQuery(Guid PatientUserId, Guid RequestId)
    : IRequest<ErrorOr<SupervisionRequestDetailDto>>;



internal sealed class GetSupervisionRequestHandler(
    ISupervisionRequestRepository supervisionRequestRepository,
    ISupervisionReadRepository supervisionReadRepository)
    : IRequestHandler<GetSupervisionRequestQuery, ErrorOr<SupervisionRequestDetailDto>>
{
    /// <summary>
    /// Retrieves supervision request detail for the given patient.
    /// </summary>
    /// <param name="query">Supervision request query.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Supervision request detail or errors.</returns>
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



        var healthcareProfessional = new SupervisionRequestHealthcareProfessionalDto(
            HealthcareProfessionalUserId: request.HealthcareProfessionalUserId,
            ProfilePhotoUrl: MemberProfilePhotoUrls.Build(
                request.HealthcareProfessionalUserId,
                providerDetail?.ProfilePhotoStorageKey),
            FirstName: providerDetail?.FirstName ?? string.Empty,
            LastName: providerDetail?.LastName ?? string.Empty,
            ProfessionLabel: providerDetail?.ProfessionLabel,
            WorkplaceLabel: providerDetail?.WorkplaceLabel,
            ShortDescription: providerDetail?.ShortDescription);



        return new SupervisionRequestDetailDto(

            RequestId: request.Id,

            RequestState: isExpired ? SupervisionRequestState.Expired.ToString() : request.RequestState.ToString(),

            RequestedAtUtc: request.RequestedAtUtc,

            HealthcareProfessional: healthcareProfessional);

    }

}


