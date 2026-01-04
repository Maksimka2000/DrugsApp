using System;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;

namespace DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;

internal static class UpdateDrugFormLinksFromTempDrugReviewMapping
{
    public static UpdateDrugFormLinksFromTempDrugReviewCommand MapToCommand(
        this UpdateDrugFormLinksFromTempDrugReviewRequest request,
        Guid tempDrugId) => new(
        TempDrugId: tempDrugId,
        Request: request);

    public static UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceDraft MapToDraft(this UpdateDrugFormLinksFromTempDrugReviewCommand command)
        => new(
            TempDrugId: command.TempDrugId,
            Note: command.Request.Note,
            CanonicalDrugApplicationServiceDraft: new UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft(
                DrugId: command.Request.DrugId,
                DrugFormId: command.Request.DrugFormId,
                CommercialName: command.Request.CommercialName,
                TypeOfProduct: command.Request.TypeOfProduct,
                ActiveSubstanceIds: command.Request.ActiveSubstanceIds,
                ProductIdentifier: command.Request.ProductIdentifier,
                AtcCode: command.Request.AtcCode,
                Dosage: command.Request.Dosage,
                Leaflet: command.Request.Leaflet,
                EducationalMaterials: command.Request.EducationalMaterials,
                RouteOfAdministrationIds: command.Request.RouteOfAdministrationIds,
                PharmaceuticalFormIds: command.Request.PharmaceuticalFormIds));

    public static UpdatedDrugFormLinksFromTempDrugReviewResponse MapToResponse(this UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult tempDrugApplicationServiceResult)
        => new(
            TempDrugId: tempDrugApplicationServiceResult.TempDrugId,
            NewStatus: tempDrugApplicationServiceResult.NewStatus,
            LinkedDrugFormId: tempDrugApplicationServiceResult.LinkedDrugFormId,
            Note: tempDrugApplicationServiceResult.Note,
            DrugForm: tempDrugApplicationServiceResult.DrugForm);
}

