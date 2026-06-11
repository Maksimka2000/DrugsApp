using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.SupplementFeature.Shared;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.UpdateSupplement;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplement;



/// <summary>
/// Handles partial updates for supplements.
/// </summary>
public sealed class UpdateSupplementHandler(ISupplementRepository _supplementRepository) : IRequestHandler<UpdateSupplementCommand, ErrorOr<UpdatedSupplementResponse>>
{
    /// <summary>
    /// Handles the update command for a supplement, processing partial updates
    /// such as name, aliases, or other properties.
    /// </summary>
    /// <param name="command">The command containing the supplement ID and update details.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an <c>ErrorOr</c> object, which includes either an updated supplement response
    /// or an error if the operation fails.
    /// </returns>
    public async Task<ErrorOr<UpdatedSupplementResponse>> Handle(
        UpdateSupplementCommand command,
        CancellationToken cancellationToken)
    {
        
        // load the supplement from the repository
        Supplement? supplement = await _supplementRepository.GetByIdAsync(command.SupplementId, cancellationToken);
        if (supplement is null)
        {
            return Error.NotFound(
                description: "Supplement not found.",
                metadata: new()
                {
                    ["resourceType"] = "Supplement",
                    ["resourceId"] = command.SupplementId.ToString()
                });
        }

        // update the name if provided
        if (!string.IsNullOrWhiteSpace(command.Request.Name))
        {
            supplement.UpdateName(command.Request.Name);
        }

        // update the aliases if provided
        if (command.Request.Aliases is not null)
        {
            supplement.UpdateAliases(command.Request.Aliases);
        }

        // update the manufacturer if provided
        return supplement.MapToResponse();
    }
}
