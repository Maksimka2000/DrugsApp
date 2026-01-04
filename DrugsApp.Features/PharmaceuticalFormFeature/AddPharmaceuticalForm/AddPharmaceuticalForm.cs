using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm;

/// <summary>
/// Handles adding a new pharmaceutical form with uniqueness checks.
/// </summary>
internal sealed class AddPharmaceuticalFormHandler(IPharmaceuticalFormRepository _pharmaceuticalFormRepository)
    : IRequestHandler<AddPharmaceuticalFormCommand, ErrorOr<CreatedPharmaceuticalFormResponse>>
{

    /// <summary>
    /// Creates a new pharmaceutical form if no duplicate exists.
    /// </summary>
    /// <param name="command">Create command containing the formulation name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created pharmaceutical form response.</returns>
    public async Task<ErrorOr<CreatedPharmaceuticalFormResponse>> Handle(
        AddPharmaceuticalFormCommand command,
        CancellationToken cancellationToken)
    {
        string normalizedName = command.Request.FormulationName.Trim();

        PharmaceuticalForm? existing = await _pharmaceuticalFormRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is not null)
        {
            return Error.Conflict(
                description: "A pharmaceutical form with the same name already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["conflictReason"] = "duplicateFormulationName",
                    ["resourceId"] = existing.Id.ToString()
                });
        }

        PharmaceuticalForm pharmaceuticalForm = command.MapToEntity();

        await _pharmaceuticalFormRepository.AddAsync(pharmaceuticalForm, cancellationToken);

        CreatedPharmaceuticalFormResponse response = pharmaceuticalForm.MapToResponse();

        return response;
    }
}


