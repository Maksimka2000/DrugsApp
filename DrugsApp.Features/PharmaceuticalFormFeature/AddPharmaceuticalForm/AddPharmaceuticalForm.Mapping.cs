using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;

namespace DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm;

/// <summary>
/// Mapping extensions for AddPharmaceuticalForm use case.
/// </summary>
public static class AddPharmaceuticalFormMapping
{
    /// <summary>
    /// Maps add request to AddPharmaceuticalFormCommand.
    /// </summary>
    public static AddPharmaceuticalFormCommand MapToCommand(this AddPharmaceuticalFormRequest request) => new(
        Request: request);

    /// <summary>
    /// Maps AddPharmaceuticalFormCommand to PharmaceuticalForm entity.
    /// </summary>
    public static PharmaceuticalForm MapToEntity(this AddPharmaceuticalFormCommand command) => PharmaceuticalForm.Create(
        formulationName: command.Request.FormulationName);

    /// <summary>
    /// Maps PharmaceuticalForm entity to CreatedPharmaceuticalFormResponse.
    /// </summary>
    public static CreatedPharmaceuticalFormResponse MapToResponse(this PharmaceuticalForm pharmaceuticalForm) => new(
        Id: pharmaceuticalForm.Id,
        FormulationName: pharmaceuticalForm.FormulationName);
}


