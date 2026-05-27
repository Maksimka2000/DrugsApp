using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.FoodFeature.Shared;
using DrugsApp.Features.InteractionFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.UpdateFoodGroup;

/// <summary>
/// Handles updating food groups with uniqueness and membership validation.
/// </summary>
public sealed class UpdateFoodGroupHandler(
    IFoodGroupRepository _foodGroupRepository,
    IFoodRepository _foodRepository,
    IInteractionExpandedSyncApplicationService _interactionExpandedSyncApplicationService)
    : IRequestHandler<UpdateFoodGroupCommand, ErrorOr<UpdatedFoodGroupResponse>>
{

    /// <summary>
    /// Updates a food group with uniqueness and membership validation.
    /// </summary>
    /// <param name="command">Update command with food group ID and request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated food group response.</returns>
    public async Task<ErrorOr<UpdatedFoodGroupResponse>> Handle(
        UpdateFoodGroupCommand command,
        CancellationToken cancellationToken)
    {
        FoodGroup? foodGroup = await _foodGroupRepository.GetByIdAsync(command.Id, cancellationToken);
        if (foodGroup is null)
        {
            return Error.NotFound(
                description: "Food group was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "FoodGroup",
                    ["resourceId"] = command.Id.ToString()
                });
        }

        // update the name
        if (!string.IsNullOrWhiteSpace(command.Request.Name))
        {
            
            string normalizedName = NormalizeName(command.Request.Name);

            if (foodGroup.Name.Equals(normalizedName, StringComparison.Ordinal))
            {
                return Error.Conflict(
                    description: "Food group name already exists.",
                    metadata: new Dictionary<string, object>
                    {
                        ["existingResourceId"] = foodGroup.Id.ToString()
                    });
            }

            foodGroup.UpdateName(normalizedName);
        }


        var existingMemberIds = foodGroup.Foods
            .Select(food => food.Id)
            .ToHashSet();

        if (command.Request.Members is not null && command.Request.Members.Count > 0){
            // update the members
            List<Food> foods = await _foodRepository.GetByIdsAsync(
                command.Request.Members,
                cancellationToken);
            if (foods.Count != command.Request.Members.Count)
            {
                IEnumerable<Guid> missingIds = command.Request.Members.Except(foods.Select(food => food.Id));
                return Error.Validation(
                    description: "One or more foods were not found.",
                    metadata: new Dictionary<string, object>
                    {
                        ["missingResourceIds"] = string.Join(",", missingIds)
                    });
            }

            var requestedMemberIds = command.Request.Members.ToHashSet();

            var addedMembers = requestedMemberIds.Except(existingMemberIds).ToList();
            var removedMembers = existingMemberIds.Except(requestedMemberIds).ToList();

            foodGroup.ReplaceMembers(foods);

            if (addedMembers.Count > 0 || removedMembers.Count > 0)
            {
                ErrorOr<Success> syncResult = await _interactionExpandedSyncApplicationService
                    .SyncAfterGroupMembershipChangedAsync(
                        new SyncInteractionExpandedAfterGroupMembershipChangedRequest(
                            InteractionType.FoodGroup,
                            foodGroup.Id,
                            addedMembers,
                            removedMembers),
                        cancellationToken);

                if (syncResult.IsError)
                {
                    return syncResult.Errors;
                }
            }
        }

        return foodGroup.MapToUpdatedResponse();
    }

    private static string NormalizeName(string value)
    {
        string trimmed = value.Trim();
        string collapsed = string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return collapsed;
    }
}
