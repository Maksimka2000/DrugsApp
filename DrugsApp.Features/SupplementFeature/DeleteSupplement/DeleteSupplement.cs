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
