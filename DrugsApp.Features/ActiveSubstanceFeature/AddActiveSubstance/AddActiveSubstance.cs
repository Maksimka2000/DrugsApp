using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;

/// <summary>
/// Handles adding a new active substance with uniqueness checks.
/// </summary>
internal sealed class AddActiveSubstanceHandler(IActiveSubstanceRepository _activeSubstanceRepository)
    : IRequestHandler<AddActiveSubstanceCommand, ErrorOr<CreatedActiveSubstanceResponse>>
{

    /// <summary>
    /// Creates a new active substance if no duplicate exists.
    /// </summary>
    /// <param name="command">Create command containing the active substance payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created active substance response.</returns>
    public async Task<ErrorOr<CreatedActiveSubstanceResponse>> Handle(
        AddActiveSubstanceCommand command,
        CancellationToken cancellationToken)
    {
        string normalizedName = command.Request.ActiveSubstanceName.Trim();

        ActiveSubstance? existing = await _activeSubstanceRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is not null)
        {
            return Error.Conflict(
                description: "An active substance with the same name already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["conflictReason"] = "duplicateActiveSubstanceName",
                    ["resourceId"] = existing.Id.ToString()
                });
        }

        ActiveSubstance activeSubstance = command.MapToEntity();

        await _activeSubstanceRepository.AddAsync(activeSubstance, cancellationToken);

        CreatedActiveSubstanceResponse response = activeSubstance.MapToResponse();

        return response;
    }
}


