using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.UpdateFoodAllergyNote;

internal sealed class UpdateFoodAllergyNoteHandler(IClientFoodAllergyRepository _repository)
    : IRequestHandler<UpdateFoodAllergyNoteCommand, ErrorOr<UpdatedFoodAllergyResponse>>
{
    public async Task<ErrorOr<UpdatedFoodAllergyResponse>> Handle(
        UpdateFoodAllergyNoteCommand command,
        CancellationToken cancellationToken)
    {
        ClientFoodAllergy? allergy = await _repository.GetByIdForUserAsync(command.UserId, command.Id, cancellationToken);
        if (allergy is null)
            return Error.NotFound(description: "Food allergy not found.", metadata: new() { ["resourceId"] = command.Id.ToString() });

        string? noteToApply = command.Request.ClearNote == true ? null : command.Request.Note;
        ErrorOr<Success> updateResult = allergy.UpdateNote(noteToApply);
        if (updateResult.IsError) return updateResult.Errors;

        return allergy.MapToDto().MapToResponse();
    }
}
