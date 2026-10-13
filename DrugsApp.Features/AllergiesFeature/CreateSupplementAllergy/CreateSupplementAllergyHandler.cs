using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.Common.Limits;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.CreateSupplementAllergy;

internal sealed class CreateSupplementAllergyHandler(
    IClientSupplementAllergyRepository _allergyRepository,
    ISupplementRepository _supplementRepository)
    : IRequestHandler<CreateSupplementAllergyCommand, ErrorOr<CreatedSupplementAllergyResponse>>
{
    public async Task<ErrorOr<CreatedSupplementAllergyResponse>> Handle(
        CreateSupplementAllergyCommand command,
        CancellationToken cancellationToken)
    {
        int existingCount = await _allergyRepository.CountAsync(command.UserId, cancellationToken);
        ErrorOr<Success> limitCheck = UserRecordLimits.EnsureBelowLimit(
            existingCount,
            UserRecordLimits.MAX_PROFILE_RECORDS_PER_TYPE,
            "SupplementAllergy");
        if (limitCheck.IsError)
        {
            return limitCheck.Errors;
        }

        AllergyRecordSource? source = AllergySharedMappings.MapSourceFromToken(command.Request.Source);
        if (source is null)
        {
            return Error.Validation(description: "Invalid source value.", metadata: new() { ["field"] = "source" });
        }

        return source.Value switch
        {
            AllergyRecordSource.Database => await CreateDatabaseAsync(command, cancellationToken),
            AllergyRecordSource.Custom => await CreateCustomAsync(command, cancellationToken),
            _ => Error.Validation(description: "Invalid source value."),
        };
    }

    private async Task<ErrorOr<CreatedSupplementAllergyResponse>> CreateDatabaseAsync(
        CreateSupplementAllergyCommand command,
        CancellationToken cancellationToken)
    {
        Guid supplementId = command.Request.SupplementId!.Value;
        Supplement? supplement = await _supplementRepository.GetByIdAsync(supplementId, cancellationToken);
        if (supplement is null)
        {
            return Error.NotFound(
                description: "Supplement not found.",
                metadata: new() { ["resourceType"] = "Supplement", ["resourceId"] = supplementId.ToString() });
        }

        if (await _allergyRepository.ExistsBySupplementIdAsync(command.UserId, supplementId, cancellationToken))
        {
            return Error.Conflict(
                description: "Supplement allergy already exists for this supplement.",
                metadata: new() { ["conflictingResourceId"] = supplementId.ToString(), ["field"] = "supplementId" });
        }

        string normalizedDisplayName = AllergySharedMappings.NormalizeForLookup(supplement.Name);
        if (await _allergyRepository.ExistsCustomNameNormalizedAsync(command.UserId, normalizedDisplayName, cancellationToken))
        {
            return Error.Conflict(
                description: "A custom supplement allergy with the same name already exists.",
                metadata: new() { ["field"] = "customSupplementName", ["conflictingName"] = supplement.Name });
        }

        ErrorOr<ClientSupplementAllergy> created = ClientSupplementAllergy.CreateDatabase(
            command.UserId, supplementId, command.Request.Note);
        if (created.IsError) return created.Errors;

        await _allergyRepository.AddAsync(created.Value, cancellationToken);
        return created.Value.MapToDto().MapToResponse();
    }

    private async Task<ErrorOr<CreatedSupplementAllergyResponse>> CreateCustomAsync(
        CreateSupplementAllergyCommand command,
        CancellationToken cancellationToken)
    {
        string customName = command.Request.CustomSupplementName!;
        string normalized = AllergySharedMappings.NormalizeForLookup(customName);

        if (await _allergyRepository.ExistsCustomNameNormalizedAsync(command.UserId, normalized, cancellationToken))
        {
            return Error.Conflict(
                description: "Custom supplement allergy with this name already exists.",
                metadata: new() { ["field"] = "customSupplementName", ["conflictingName"] = customName });
        }

        if (await _allergyRepository.ExistsDatabaseDisplayNameNormalizedAsync(command.UserId, normalized, cancellationToken))
        {
            return Error.Conflict(
                description: "A catalog-backed supplement allergy with the same name already exists.",
                metadata: new() { ["field"] = "customSupplementName", ["conflictingName"] = customName });
        }

        ErrorOr<ClientSupplementAllergy> created = ClientSupplementAllergy.CreateCustom(
            command.UserId, customName, command.Request.Note);
        if (created.IsError) return created.Errors;

        await _allergyRepository.AddAsync(created.Value, cancellationToken);
        return created.Value.MapToDto().MapToResponse();
    }
}
