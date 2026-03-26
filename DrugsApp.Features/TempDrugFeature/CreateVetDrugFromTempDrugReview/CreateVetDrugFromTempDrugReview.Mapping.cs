using System;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;

namespace DrugsApp.Features.TempDrugFeature.CreateVetDrugFromTempDrugReview;

internal static class CreateVetDrugFromTempDrugReviewMapping
{
    public static CreateVetDrugFromTempDrugReviewCommand MapToCommand(
        this CreateVetDrugFromTempDrugReviewRequest request,
        Guid tempDrugId) => new(
        TempDrugId: tempDrugId,
        Request: request);

    public static CreateVetDrugFromTempDrugReviewDraft MapToDraft(this CreateVetDrugFromTempDrugReviewCommand command)
        => new(
            TempDrugId: command.TempDrugId,
            Note: command.Request.Note,
            CanonicalDraft: new CreateVetDrugFromReviewVetDrugApplicationServiceDraft(
                CommercialName: command.Request.CommercialName,
                TypeOfProduct: command.Request.TypeOfProduct,
                ProductIdentifier: command.Request.ProductIdentifier));

    public static CreatedVetDrugFromTempDrugReviewResponse MapToResponse(this CreateVetDrugFromTempDrugReviewResult result)
        => new(
            TempDrugId: result.TempDrugId,
            NewStatus: result.NewStatus,
            LinkedVetDrugId: result.LinkedVetDrugId,
            Note: result.Note,
            VetDrug: result.VetDrug);
}

