using System;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;

namespace DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;

internal static class CreateDrugFromTempDrugReviewMapping
{
    public static CreateDrugFromTempDrugReviewCommand MapToCommand(
        this CreateDrugFromTempDrugReviewRequest request,
        Guid tempDrugId) => new(
        TempDrugId: tempDrugId,
        Request: request);
    
    public static CreatedDrugFromTempDrugReviewResponse MapToResponse(this CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult tempDrugApplicationServiceResult)
        => new(
            TempDrugId: tempDrugApplicationServiceResult.TempDrugId,
            NewStatus: tempDrugApplicationServiceResult.NewStatus,
            LinkedDrugFormId: tempDrugApplicationServiceResult.LinkedDrugFormId,
            Note: tempDrugApplicationServiceResult.Note,
            Drug: tempDrugApplicationServiceResult.Drug);

    
    
    /// <summary>
    /// Maps to the application service dto. Nesting the canonical drug application service draft.
    /// </summary>
    /// <param name="command"> The command to map to the draft. </param>
    /// <returns> The draft. </returns>
    public static CreateDrugFromTempDrugReviewTempDrugApplicationServiceDraft MapToDraft(this CreateDrugFromTempDrugReviewCommand command)
        => new(
            TempDrugId: command.TempDrugId,
            Note: command.Request.Note?.Trim() ?? null,
            CanonicalDrugApplicationServiceDraft: new CreateDrugFromReviewDrugApplicationServiceDraft(
                CommercialName: command.Request.CommercialName.Trim(),
                TypeOfProduct: command.Request.TypeOfProduct.Trim() ,
                TypeOfDrug: command.Request.TypeOfDrug,
                ActiveSubstanceIds: command.Request.ActiveSubstanceIds,
                ProductIdentifier: command.Request.ProductIdentifier.Trim(),
                AtcCode: command.Request.AtcCode?.Trim() ?? null,
                Dosage: command.Request.Dosage?.Trim() ?? null,
                Leaflet: command.Request.Leaflet?.Trim() ?? null,
                EducationalMaterials: command.Request.EducationalMaterials?.Trim() ?? null,
                RouteOfAdministrationIds: command.Request.RouteOfAdministrationIds,
                PharmaceuticalFormIds: command.Request.PharmaceuticalFormIds));
}