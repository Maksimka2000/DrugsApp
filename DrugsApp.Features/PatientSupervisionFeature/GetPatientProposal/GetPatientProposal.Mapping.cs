using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientProposal;

internal static class GetPatientProposalMapping
{
    /// <summary>
    /// Maps a proposal draft item from repository DTO to API DTO.
    /// </summary>
    /// <param name="item">Draft item from repository.</param>
    /// <returns>API-facing draft item DTO.</returns>
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
