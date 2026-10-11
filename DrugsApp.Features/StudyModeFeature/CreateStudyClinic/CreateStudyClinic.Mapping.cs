using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyClinic;

/// <summary>
/// Pure mapping helpers of CreateStudyClinic (no I/O, no business rules).
/// </summary>
public static class CreateStudyClinicMapping
{
    /// <summary>
    /// Maps the HTTP request and route values to the MediatR command.
    /// </summary>
    public static CreateStudyClinicCommand MapToCommand(this CreateStudyClinicRequest request, Guid createdByUserId) => new(
        CreatedByUserId: createdByUserId,
        Request: request);

    /// <summary>
    /// Maps the command to a new aggregate through its validating factory.
    /// </summary>
    public static ErrorOr<StudyClinic> MapToEntity(this CreateStudyClinicCommand command) => StudyClinic.Create(
        name: command.Request.Name,
        scenarioDescription: command.Request.ScenarioDescription,
        createdByUserId: command.CreatedByUserId,
        visibility: StudyModeMappings.MapToVisibility(command.Request.Visibility) ?? StudyClinicVisibility.Private);
}
