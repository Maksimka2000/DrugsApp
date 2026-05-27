using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.DeleteSupplement;



/// <summary>
/// Handles deletion of supplements with reference guards.
/// </summary>
public sealed class DeleteSupplementHandler(ISupplementRepository _supplementRepository) : IRequestHandler<DeleteSupplementCommand, ErrorOr<DeleteSupplementResponse>>
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
                metadata: new()
                {
                    ["resourceType"] = "Supplement",
                    ["resourceId"] = command.SupplementId.ToString()
                });
        }

        _supplementRepository.Remove(supplement);

        return supplement.MapToDeletedResponse();
    }
}
