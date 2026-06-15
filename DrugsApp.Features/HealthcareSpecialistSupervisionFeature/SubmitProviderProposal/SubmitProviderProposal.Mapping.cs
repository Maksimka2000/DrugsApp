using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.SubmitProviderProposal;

internal static class SubmitProviderProposalMapping
{
    public static SubmitProviderProposalCommand MapToCommand(
        this SubmitProviderProposalRequest request,
        Guid providerUserId,
        Guid patientId) =>
        new(
            ProviderUserId: providerUserId,
            PatientId: patientId,
            DraftItems: request.DraftItems);

    public static ProposalDraftItemInput MapToInput(this ProposalDraftItemDto item) =>
        new(
            CategoryCode: item.CategoryCode,
            Source: item.Source,
            RelatedCatalogId: item.RelatedCatalogId,
            CustomValue: item.CustomValue,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            Note: item.Note,
            IntakeTimes: item.IntakeTimes);
}
