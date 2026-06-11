using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.SubmitProviderProposal;

internal static class SubmitProviderProposalMapping
{
    public static SubmitProviderProposalCommand MapToCommand(
        this SubmitProviderProposalRequest request,
        Guid providerUserId,
        Guid connectionId)
    {
        return new SubmitProviderProposalCommand(
            ProviderUserId: providerUserId,
            ConnectionId: connectionId,
            DraftItems: request.DraftItems);
    }
}
