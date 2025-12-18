using System;
using DrugsApp.Domain.VetDrugFeature.Entities;
namespace DrugsApp.Features.VetDrugFeature.UpdateVetDrug;

/// <summary>
/// Mapping extensions for UpdateVetDrug use case.
/// </summary>
public static class UpdateVetDrugMapping
{
    /// <summary>
    /// Maps update request and route parameter to UpdateVetDrugCommand.
    /// </summary>
    public static UpdateVetDrugCommand MapToCommand(
        this UpdateVetDrugRequest request,
        Guid vetDrugId)
    {
        return new UpdateVetDrugCommand(
            vetDrugId, 
            request);
    }

    /// <summary>
    /// Maps VetDrug entity to UpdatedVetDrugResponse.
    /// </summary>
    public static UpdatedVetDrugResponse MapToUpdatedResponse(
        this VetDrug vetDrug)
    {
        return new UpdatedVetDrugResponse(
            vetDrug.Id, 
            vetDrug.CommercialName
        );
    }
}

