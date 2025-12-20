using System;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;

namespace DrugsApp.Features.PharmaceuticalFormFeature.UpdatePharmaceuticalForm;

/// <summary>
/// Mapping extensions for UpdatePharmaceuticalForm use case.
/// </summary>
public static class UpdatePharmaceuticalFormMapping
{
    /// <summary>
    /// Maps update request and route parameter to UpdatePharmaceuticalFormCommand.
    /// </summary>
    public static UpdatePharmaceuticalFormCommand MapToCommand(
        this UpdatePharmaceuticalFormRequest request,
        Guid pharmaceuticalFormId) => new(
        PharmaceuticalFormId: pharmaceuticalFormId,
        Request: request);

    /// <summary>
    /// Maps PharmaceuticalForm entity to UpdatedPharmaceuticalFormResponse.
    /// </summary>
    public static UpdatedPharmaceuticalFormResponse MapToUpdatedResponse(this PharmaceuticalForm pharmaceuticalForm) => new(
        Id: pharmaceuticalForm.Id,
        FormulationName: pharmaceuticalForm.FormulationName);
}


