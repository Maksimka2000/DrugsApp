using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.DeleteSupplementGroup;



/// <summary>
/// Handles deletion of supplement groups with reference guard placeholder.
/// </summary>
public sealed class DeleteSupplementGroupHandler(ISupplementGroupRepository _supplementGroupRepository) : IRequestHandler<DeleteSupplementGroupCommand, ErrorOr<DeleteSupplementGroupResponse>>
{

    public async Task<ErrorOr<DeleteSupplementGroupResponse>> Handle(
        DeleteSupplementGroupCommand command,
        CancellationToken cancellationToken)
    {
        SupplementGroup? group = await _supplementGroupRepository.GetByIdAsync(
            command.SupplementGroupId,
            cancellationToken);

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


        _supplementGroupRepository.Remove(group);

        return group.MapToDeletedResponse();
    }
}
