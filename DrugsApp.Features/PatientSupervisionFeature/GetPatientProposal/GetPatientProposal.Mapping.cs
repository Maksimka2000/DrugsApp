using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientProposal;

internal static class GetPatientProposalMapping
{
    public static PatientProposalDraftItemDto MapToDto(this MedicalChangeProposalDraftItemDto item) =>
        new(
            CategoryCode: item.CategoryCode,
            Source: item.Source,
            RelatedCatalogId: item.RelatedCatalogId,
            CustomValue: item.CustomValue,
            DisplayName: item.DisplayName,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            Note: item.Note,
            IntakeTimes: item.IntakeTimes,
            ApplyPayload: item.ApplyPayload);
}
