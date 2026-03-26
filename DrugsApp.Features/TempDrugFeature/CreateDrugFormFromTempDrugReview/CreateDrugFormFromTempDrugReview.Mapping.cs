using System;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;

namespace DrugsApp.Features.TempDrugFeature.CreateDrugFormFromTempDrugReview;

internal static class CreateDrugFormFromTempDrugReviewMapping
{
    /// <summary>
    /// Maps the request to the command.
    /// </summary>
    public static CreateDrugFormFromTempDrugReviewCommand MapToCommand(
        this CreateDrugFormFromTempDrugReviewRequest request,
        Guid tempDrugId) => new(
        TempDrugId: tempDrugId,
        Request: request);

    /// <summary>
    /// Maps the command to the draft.
    /// </summary>
    public static CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceDraft MapToDraft(this CreateDrugFormFromTempDrugReviewCommand command)
        => new(
            TempDrugId: command.TempDrugId,
            Note: command.Request.Note,
            CanonicalDraft: new CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft(
                DrugId: command.Request.DrugId,
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

    /// <summary>
    /// Maps the result to the response.
    /// </summary>
    public static CreatedDrugFormFromTempDrugReviewResponse MapToResponse(this CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult tempDrugApplicationServiceResult)
        => new(
            TempDrugId: tempDrugApplicationServiceResult.TempDrugId,
            NewStatus: tempDrugApplicationServiceResult.NewStatus,
            LinkedDrugFormId: tempDrugApplicationServiceResult.LinkedDrugFormId,
            Note: tempDrugApplicationServiceResult.Note,
            DrugForm: tempDrugApplicationServiceResult.DrugForm);
}

