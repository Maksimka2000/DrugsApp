using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientProposal;

public sealed record GetPatientProposalRequest(Guid ProposalId);

public sealed record GetPatientProposalQuery(Guid PatientUserId, Guid ProposalId)
    : IRequest<ErrorOr<PatientProposalDetailResponse>>;

internal sealed class GetPatientProposalHandler(IMedicalChangeProposalRepository proposalRepository)
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

        var sections = detail.Sections
            .Select(section => new PatientProposalDetailSectionDto(
                section.SectionCode,
                section.Items.Select(item => new PatientProposalDetailSectionItemDto(
                    item.CategoryCode,
                    item.Source,
                    item.RelatedCatalogId,
                    item.CustomValue,
                    item.DisplayName,
                    item.SecondaryText,
                    item.StartDate,
                    item.EndDate,
                    item.Note,
                    item.IntakeTimes)).ToList()))
            .ToList();

        return new PatientProposalDetailResponse(
            ProposalId: detail.ProposalId,
            ProposalState: detail.ProposalState,
            ProposalSummary: detail.ProposalSummary,
            SubmittedAtUtc: detail.SubmittedAtUtc,
            HealthcareProfessionalSummary: "Healthcare professional", // placeholder summary
            Sections: sections);
    }
}
