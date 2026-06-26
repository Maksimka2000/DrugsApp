using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.UpdateFood;


/// <summary>
/// Handles updating foods with uniqueness and alias replacement.
/// </summary>
public sealed class UpdateFoodHandler(IFoodRepository _foodRepository) : IRequestHandler<UpdateFoodCommand, ErrorOr<UpdatedFoodResponse>>
{

    /// <summary>
    /// Updates a food with uniqueness and alias replacement.
    /// </summary>
    /// <param name="command">Update command with food ID and request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated food response.</returns>
    public async Task<ErrorOr<UpdatedFoodResponse>> Handle(
        UpdateFoodCommand command,
        CancellationToken cancellationToken)
    {
        Food? food = await _foodRepository.GetByIdAsync(command.Id, cancellationToken);
        if (food is null)
        {
            return Error.NotFound(
                description: "Food was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "Food",
                    ["resourceId"] = command.Id.ToString()
                });
        }

        // update the name
        if (!string.IsNullOrWhiteSpace(command.Request.Name))
        {
            string normalizedName = NormalizeName(command.Request.Name);

            if (!food.Name.Equals(normalizedName, StringComparison.Ordinal))
            {
                Food? existing = await _foodRepository.GetByNameAsync(
                    normalizedName,
                    cancellationToken);
                if (existing is not null)
                {
                    return Error.Conflict(
                        description: "Food with the same name already exists.",
                        metadata: new Dictionary<string, object>
                        {
                            ["existingResourceId"] = existing.Id.ToString()
                        });
                }

                food.UpdateName(normalizedName);
            }
        }


        // update the aliases
        if (command.Request.Aliases is not null)
        {
            food.ReplaceAliases(command.Request.Aliases);
        }


        return food.MapToUpdatedResponse();
    }

    private static string NormalizeName(string value)
    {
        string trimmed = value.Trim();
        string collapsed = string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return collapsed;
    }
}
