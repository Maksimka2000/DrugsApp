using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.Common.Limits;
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
        int existingCount = await _repository.CountAsync(command.UserId, cancellationToken);
        ErrorOr<Success> limitCheck = UserRecordLimits.EnsureBelowLimit(
            existingCount,
            UserRecordLimits.MAX_PROFILE_RECORDS_PER_TYPE,
            "OtherAllergy");
        if (limitCheck.IsError)
        {
            return limitCheck.Errors;
        }

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
