using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.DeleteSupplement;



/// <summary>
/// Handles deletion of supplements with reference guards.
/// </summary>
public sealed class DeleteSupplementHandler(
    ISupplementRepository _supplementRepository,
    IInteractionRepository _interactionRepository)
    : IRequestHandler<DeleteSupplementCommand, ErrorOr<DeleteSupplementResponse>>
{
    /// <summary>
    /// Handles the deletion process for a supplement, ensuring proper validation and repository interaction.
    /// </summary>
    /// <param name="command">The command containing the details of the supplement to be deleted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// An ErrorOr result containing either the deletion response or an error if the supplement was not found.
    /// </returns>
    public async Task<ErrorOr<DeleteSupplementResponse>> Handle(
        DeleteSupplementCommand command,
        CancellationToken cancellationToken)
    {
        Supplement? supplement = await _supplementRepository.GetByIdAsync(command.SupplementId, cancellationToken);
        if (supplement is null)
        {
            return Error.NotFound(
                description: "Supplement not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "Supplement",
                    ["resourceId"] = command.SupplementId.ToString()
                });
        }

        List<SupplementGroup> supplementGroups =
            await _supplementRepository.ListGroupsBySupplementIdAsync(supplement.Id, cancellationToken);

        if (supplementGroups.Count > 0)
        {
            return Error.Conflict(
                description: "Supplement cannot be removed because it is a member of supplement groups.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = supplement.Id.ToString(),
                    ["groupIds"] = supplementGroups.Select(group => group.Id.ToString()).ToList(),
                    ["groupNames"] = supplementGroups.Select(group => group.Name).ToList()
                });
        }

        List<Interaction> interactions =
            await _interactionRepository.ListBySupplementIdAsync(supplement.Id, cancellationToken);

        if (interactions.Count > 0)
        {
            return Error.Conflict(
                description: "Supplement cannot be removed because it participates in interactions.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = supplement.Id.ToString(),
                    ["interactionIds"] = interactions.Select(interaction => interaction.Id.ToString()).ToList()
                });
        }

        _supplementRepository.Remove(supplement);

        return supplement.MapToDeletedResponse();
    }
}
