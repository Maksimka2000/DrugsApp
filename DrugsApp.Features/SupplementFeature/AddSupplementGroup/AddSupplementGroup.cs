using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.SupplementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.AddSupplementGroup;



/// <summary>
/// Handles creation of supplement groups with uniqueness and membership checks.
/// </summary>
public sealed class AddSupplementGroupHandler(ISupplementGroupRepository _supplementGroupRepository, ISupplementRepository _supplementRepository) : IRequestHandler<AddSupplementGroupCommand, ErrorOr<CreatedSupplementGroupResponse>>
{
    /// <summary>
    /// Handles the request for adding a new supplement group. This includes validating the group name for uniqueness,
    /// ensuring that all supplements exist, creating the supplement group entity, and persisting it to the repository.
    /// </summary>
    /// <param name="command">
    /// Represents the command that contains the data needed to create a new supplement group, including group name
    /// and the list of supplement IDs.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to signal the async operation to stop prematurely.
    /// </param>
    /// <returns>
    /// Returns an <see cref="ErrorOr{T}"/> object that encapsulates the created supplement group as a response if successful,
    /// or an error instance with details if the operation fails.
    /// </returns>
    public async Task<ErrorOr<CreatedSupplementGroupResponse>> Handle(
        AddSupplementGroupCommand command,
        CancellationToken cancellationToken)
    {
        AddSupplementGroupRequest request = command.Request;

        // check if the name is already taken
        string normalizedName = request.Name.Trim();
        SupplementGroup? existingByName = await _supplementGroupRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existingByName is not null)
        {
            return Error.Conflict(
                description: "Supplement group name already exists.",
                metadata: new()
                {
                    ["existingResourceId"] = existingByName.Id.ToString()
                });
        }

        // load the supplements
        List<Supplement> supplementMembers = await _supplementRepository.GetByIdsAsync(
            request.SupplementIds,
            cancellationToken);

        var missingIds = request.SupplementIds
            .Except(supplementMembers.Select(m => m.Id))
            .ToList();
        if (missingIds.Count > 0)
        {
            return Error.Validation(
                description: "Some supplements were not found.",
                metadata: new()
                {
                    ["missingIds"] = string.Join(",", missingIds)
                });
        }

        // create the supplement group
        SupplementGroup createdSupplementGroup = command.Request.MapToEntity();

        // set the members
        createdSupplementGroup.UpdateSupplements(supplementMembers);

        await _supplementGroupRepository.AddAsync(
            createdSupplementGroup,
            cancellationToken);

        return createdSupplementGroup.MapToResponse();
    }
}
