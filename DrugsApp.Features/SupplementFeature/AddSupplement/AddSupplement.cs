using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.SupplementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.AddSupplement;



/// <summary>
/// Handles creating supplements with uniqueness checks.
/// </summary>
public sealed class AddSupplementHandler(ISupplementRepository _supplementRepository) : IRequestHandler<AddSupplementCommand, ErrorOr<CreatedSupplementResponse>>
{

    public async Task<ErrorOr<CreatedSupplementResponse>> Handle(
        AddSupplementCommand command,
        CancellationToken cancellationToken)
    {

        string normalizedName = command.Request.Name.Trim();

        Supplement? existing = await _supplementRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is not null)
        {
            return Error.Conflict(
                description: "Supplement name already exists.",
                metadata: new()
                {
                    ["existingResourceId"] = existing.Id.ToString()
                });
        }

        Supplement created = command.Request.MapToEntity();

        await _supplementRepository.AddAsync(created, cancellationToken);
        
        return created.MapToResponse();
    }
}
