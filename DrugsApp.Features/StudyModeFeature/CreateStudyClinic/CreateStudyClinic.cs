using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyClinic;

/// <summary>
/// Handles study clinic creation: name uniqueness check, aggregate creation and staged insert.
/// </summary>
internal sealed class CreateStudyClinicHandler(IStudyClinicRepository _studyClinicRepository)
    : IRequestHandler<CreateStudyClinicCommand, ErrorOr<StudyClinicDto>>
{
    /// <summary>
    /// Creates the clinic when its normalized name is not taken yet.
    /// </summary>
    /// <param name="command">Creation command with the caller id and body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created clinic, a name conflict, or validation errors.</returns>
    public async Task<ErrorOr<StudyClinicDto>> Handle(
        CreateStudyClinicCommand command,
        CancellationToken cancellationToken)
    {
        // check if name available
        string normalizedName = StudyClinic.NormalizeName(command.Request.Name);
        bool isNameTaken = await _studyClinicRepository.ExistsByNormalizedNameAsync(normalizedName, cancellationToken);
        if (isNameTaken)
        {
            return Error.Conflict(
                code: "StudyClinic.NameTaken",
                description: "A study clinic with this name already exists.",
                metadata: new()
                {
                    ["field"] = "name"
                });
        }

        // create
        ErrorOr<StudyClinic> clinicResult = command.MapToEntity();
        if (clinicResult.IsError)
        {
            return clinicResult.Errors;
        }

        // add to repository (no SaveChanges call—TransactionBehavior handles it)
        await _studyClinicRepository.AddAsync(clinicResult.Value, cancellationToken);

        return clinicResult.Value.MapToDto(command.CreatedByUserId);
    }
}
