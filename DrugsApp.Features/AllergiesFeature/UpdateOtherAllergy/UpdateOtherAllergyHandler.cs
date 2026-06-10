using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.UpdateOtherAllergy;

internal sealed class UpdateOtherAllergyHandler(IClientOtherAllergyRepository _repository)
    : IRequestHandler<UpdateOtherAllergyCommand, ErrorOr<UpdatedOtherAllergyResponse>>
{
    public async Task<ErrorOr<UpdatedOtherAllergyResponse>> Handle(
        UpdateOtherAllergyCommand command,
        CancellationToken cancellationToken)
    {
        ClientOtherAllergy? allergy = await _repository.GetByIdForUserAsync(command.UserId, command.Id, cancellationToken);
        if (allergy is null)
            return Error.NotFound(description: "Other allergy not found.", metadata: new() { ["resourceId"] = command.Id.ToString() });

        if (!string.IsNullOrWhiteSpace(command.Request.AllergyName))
        {
            string normalized = AllergySharedMappings.NormalizeForLookup(command.Request.AllergyName);
            bool exists = await _repository.ExistsNameNormalizedAsync(
                command.UserId, normalized, command.Id, cancellationToken);
            if (exists)
            {
                return Error.Conflict(
                    description: "Other allergy with this name already exists.",
                    metadata: new() { ["field"] = "allergyName", ["conflictingName"] = command.Request.AllergyName });
            }

            ErrorOr<Success> nameResult = allergy.UpdateName(command.Request.AllergyName);
            if (nameResult.IsError) return nameResult.Errors;
        }

        if (command.Request.ClearNote == true || command.Request.Note is not null)
        {
            string? noteToApply = command.Request.ClearNote == true ? null : command.Request.Note;
            ErrorOr<Success> noteResult = allergy.UpdateNote(noteToApply);
            if (noteResult.IsError) return noteResult.Errors;
        }

        return allergy.MapToDto().MapToResponse();
    }
}
