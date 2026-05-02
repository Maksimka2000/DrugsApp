using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.SupplementFeature.Shared;
using DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;



/// <summary>
/// Handles updating supplement group name and membership.
/// </summary>
public sealed class UpdateSupplementGroupHandler(ISupplementGroupRepository _supplementGroupRepository, ISupplementRepository _supplementRepository) : IRequestHandler<UpdateSupplementGroupCommand, ErrorOr<UpdatedSupplementGroupResponse>>
{

    public async Task<ErrorOr<UpdatedSupplementGroupResponse>> Handle(
        UpdateSupplementGroupCommand command,
        CancellationToken cancellationToken)
    {

        // load the supplement group from the repository
        SupplementGroup? group = await _supplementGroupRepository.GetByIdAsync(
            id: command.SupplementGroupId,
            cancellationToken: cancellationToken);
        if (group is null)
        {
            return Error.NotFound(
                description: "Supplement group not found.",
                metadata: new()
                {
                    ["resourceType"] = "SupplementGroup",
                    ["resourceId"] = command.SupplementGroupId.ToString()
                });
        }

        // update the name if provided
        if (!string.IsNullOrWhiteSpace(command.Request.Name))
        {
            group.UpdateName(command.Request.Name);
        }

        // update the supplements if provided
        if (command.Request.SupplementIds is not null && command.Request.SupplementIds.Count > 0)
        {
            // convert the request supplement ids to a list
            var requestSupplementIds = command.Request.SupplementIds.ToList();

            // load the supplements from the repository
            List<Supplement> toAdd = await _supplementRepository.GetByIdsAsync(ids: requestSupplementIds, cancellationToken: cancellationToken);

            // check if the supplements were found
            if (toAdd.Count != requestSupplementIds.Count)
            {
                var missingIds = requestSupplementIds.Except(toAdd.Select(x => x.Id)).ToList();
                
                    return Error.Validation(
                        description: "Some supplements were not found.",
                        metadata: new()
                        {
                            ["missingIds"] = string.Join(",", missingIds)
                        });
                
            }

            group.UpdateSupplements(toAdd);
        }

        return group.MapToResponse();
    }
}
