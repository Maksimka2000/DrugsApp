using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;

internal sealed class CreateOtherAllergyHandler(IClientOtherAllergyRepository _repository)
    : IRequestHandler<CreateOtherAllergyCommand, ErrorOr<CreatedOtherAllergyResponse>>
{
    public async Task<ErrorOr<CreatedOtherAllergyResponse>> Handle(
        CreateOtherAllergyCommand command,
        CancellationToken cancellationToken)
    {
        string normalized = AllergySharedMappings.NormalizeForLookup(command.Request.AllergyName);
        bool exists = await _repository.ExistsNameNormalizedAsync(
            command.UserId, normalized, excludeId: null, cancellationToken);
        if (exists)
        {
            return Error.Conflict(
                description: "Other allergy with this name already exists.",
                metadata: new() { ["field"] = "allergyName", ["conflictingName"] = command.Request.AllergyName });
        }

        ErrorOr<ClientOtherAllergy> created = ClientOtherAllergy.Create(
            command.UserId, command.Request.AllergyName, command.Request.Note);
        if (created.IsError) return created.Errors;

        await _repository.AddAsync(created.Value, cancellationToken);
        return created.Value.MapToDto().MapToResponse();
    }
}
