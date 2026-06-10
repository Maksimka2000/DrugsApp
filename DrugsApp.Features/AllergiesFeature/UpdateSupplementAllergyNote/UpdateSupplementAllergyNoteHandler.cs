using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.UpdateSupplementAllergyNote;

internal sealed class UpdateSupplementAllergyNoteHandler(IClientSupplementAllergyRepository _repository)
    : IRequestHandler<UpdateSupplementAllergyNoteCommand, ErrorOr<UpdatedSupplementAllergyResponse>>
{
    public async Task<ErrorOr<UpdatedSupplementAllergyResponse>> Handle(
        UpdateSupplementAllergyNoteCommand command,
        CancellationToken cancellationToken)
    {
        ClientSupplementAllergy? allergy = await _repository.GetByIdForUserAsync(command.UserId, command.Id, cancellationToken);
        if (allergy is null)
            return Error.NotFound(description: "Supplement allergy not found.", metadata: new() { ["resourceId"] = command.Id.ToString() });

        string? noteToApply = command.Request.ClearNote == true ? null : command.Request.Note;
        ErrorOr<Success> updateResult = allergy.UpdateNote(noteToApply);
        if (updateResult.IsError) return updateResult.Errors;

        return allergy.MapToDto().MapToResponse();
    }
}
