using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.UpdateDrugAllergyNote;

/// <summary>
/// Handles drug allergy note updates.
/// </summary>
internal sealed class UpdateDrugAllergyNoteHandler(IClientDrugAllergyRepository _drugAllergyRepository)
    : IRequestHandler<UpdateDrugAllergyNoteCommand, ErrorOr<UpdatedDrugAllergyResponse>>
{
    public async Task<ErrorOr<UpdatedDrugAllergyResponse>> Handle(
        UpdateDrugAllergyNoteCommand command,
        CancellationToken cancellationToken)
    {
        ClientDrugAllergy? allergy = await _drugAllergyRepository.GetByIdForUserAsync(
            command.UserId,
            command.Id,
            cancellationToken);

        if (allergy is null)
        {
            return Error.NotFound(
                description: "Drug allergy not found.",
                metadata: new()
                {
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        string? noteToApply = command.Request.ClearNote == true
            ? null
            : command.Request.Note;

        ErrorOr<Success> updateResult = allergy.UpdateNote(noteToApply);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        return allergy.MapToDto().MapToResponse();
    }
}
