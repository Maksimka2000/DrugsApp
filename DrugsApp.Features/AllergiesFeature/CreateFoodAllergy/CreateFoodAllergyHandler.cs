using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.Common.Limits;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;

internal sealed class CreateFoodAllergyHandler(
    IClientFoodAllergyRepository _allergyRepository,
    IFoodRepository _foodRepository)
    : IRequestHandler<CreateFoodAllergyCommand, ErrorOr<CreatedFoodAllergyResponse>>
{
    public async Task<ErrorOr<CreatedFoodAllergyResponse>> Handle(
        CreateFoodAllergyCommand command,
        CancellationToken cancellationToken)
    {
        int existingCount = await _allergyRepository.CountAsync(command.UserId, cancellationToken);
        ErrorOr<Success> limitCheck = UserRecordLimits.EnsureBelowLimit(
            existingCount,
            UserRecordLimits.MAX_PROFILE_RECORDS_PER_TYPE,
            "FoodAllergy");
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

    private async Task<ErrorOr<CreatedFoodAllergyResponse>> CreateDatabaseAsync(
        CreateFoodAllergyCommand command,
        CancellationToken cancellationToken)
    {
        Guid foodId = command.Request.FoodId!.Value;
        Food? food = await _foodRepository.GetByIdAsync(foodId, cancellationToken);
        if (food is null)
        {
            return Error.NotFound(
                description: "Food not found.",
                metadata: new() { ["resourceType"] = "Food", ["resourceId"] = foodId.ToString() });
        }

        if (await _allergyRepository.ExistsByFoodIdAsync(command.UserId, foodId, cancellationToken))
        {
            return Error.Conflict(
                description: "Food allergy already exists for this food.",
                metadata: new() { ["conflictingResourceId"] = foodId.ToString(), ["field"] = "foodId" });
        }

        string normalizedDisplayName = AllergySharedMappings.NormalizeForLookup(food.Name);
        if (await _allergyRepository.ExistsCustomNameNormalizedAsync(command.UserId, normalizedDisplayName, cancellationToken))
        {
            return Error.Conflict(
                description: "A custom food allergy with the same name already exists.",
                metadata: new() { ["field"] = "customFoodName", ["conflictingName"] = food.Name });
        }

        ErrorOr<ClientFoodAllergy> created = ClientFoodAllergy.CreateDatabase(
            command.UserId, foodId, command.Request.Note);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _allergyRepository.AddAsync(created.Value, cancellationToken);
        return created.Value.MapToDto().MapToResponse();
    }

    private async Task<ErrorOr<CreatedFoodAllergyResponse>> CreateCustomAsync(
        CreateFoodAllergyCommand command,
        CancellationToken cancellationToken)
    {
        string customName = command.Request.CustomFoodName!;
        string normalized = AllergySharedMappings.NormalizeForLookup(customName);

        if (await _allergyRepository.ExistsCustomNameNormalizedAsync(command.UserId, normalized, cancellationToken))
        {
            return Error.Conflict(
                description: "Custom food allergy with this name already exists.",
                metadata: new() { ["field"] = "customFoodName", ["conflictingName"] = customName });
        }

        if (await _allergyRepository.ExistsDatabaseDisplayNameNormalizedAsync(command.UserId, normalized, cancellationToken))
        {
            return Error.Conflict(
                description: "A catalog-backed food allergy with the same name already exists.",
                metadata: new() { ["field"] = "customFoodName", ["conflictingName"] = customName });
        }

        ErrorOr<ClientFoodAllergy> created = ClientFoodAllergy.CreateCustom(
            command.UserId, customName, command.Request.Note);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _allergyRepository.AddAsync(created.Value, cancellationToken);
        return created.Value.MapToDto().MapToResponse();
    }
}
