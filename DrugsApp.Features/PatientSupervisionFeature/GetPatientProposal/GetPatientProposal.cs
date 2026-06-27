using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientProposal;

/// <summary>
/// Request payload for retrieving a patient proposal.
/// </summary>
/// <param name="ProposalId">Proposal identifier.</param>
public sealed record GetPatientProposalRequest(Guid ProposalId);

/// <summary>
/// Query for retrieving a patient proposal detail.
/// </summary>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="ProposalId">Proposal identifier.</param>
public sealed record GetPatientProposalQuery(Guid PatientUserId, Guid ProposalId)
    : IRequest<ErrorOr<PatientProposalDetailResponse>>;

internal sealed class GetPatientProposalHandler(
    IMedicalChangeProposalRepository proposalRepository,
    ISupervisionReadRepository supervisionReadRepository)
    : IRequestHandler<GetPatientProposalQuery, ErrorOr<PatientProposalDetailResponse>>
{
    public async Task<ErrorOr<PatientProposalDetailResponse>> Handle(
        GetPatientProposalQuery query,
        CancellationToken cancellationToken)
    {
        MedicalChangeProposalDetail? detail = await proposalRepository.GetDetailForPatientAsync(
            query.PatientUserId,
            query.ProposalId,
            cancellationToken);

        if (detail is null)
        {
            return Error.NotFound(
                description: "Proposal not found for patient.",
                metadata: new()
                {
                    ["resourceType"] = "MedicalChangeProposal",
                    ["resourceId"] = query.ProposalId.ToString()
                });
        }

        PatientHealthcareProfessionalDetail? providerDetail =
            await supervisionReadRepository.GetPatientHealthcareProfessionalDetailAsync(
                query.PatientUserId,
                detail.HealthcareProfessionalUserId,
                cancellationToken);

        var healthcareProfessional = new SupervisionRequestHealthcareProfessionalDto(
            HealthcareProfessionalUserId: detail.HealthcareProfessionalUserId,
            ProfilePhotoUrl: MemberProfilePhotoUrls.Build(
                detail.HealthcareProfessionalUserId,
                providerDetail?.ProfilePhotoStorageKey),
            FirstName: providerDetail?.FirstName ?? string.Empty,
            LastName: providerDetail?.LastName ?? string.Empty,
            ProfessionLabel: providerDetail?.ProfessionLabel,
            WorkplaceLabel: providerDetail?.WorkplaceLabel,
            ShortDescription: providerDetail?.ShortDescription);

        return new PatientProposalDetailResponse(
            ProposalId: detail.ProposalId,
            ProposalState: detail.ProposalState,
            ProposalSummary: detail.ProposalSummary,
            SubmittedAtUtc: detail.SubmittedAtUtc,
            HealthcareProfessional: healthcareProfessional,
            DraftItems: detail.DraftItems.Select(item => item.MapToDto()).ToList());
    }
}
