using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PharmaceuticalFormFeature.UpdatePharmaceuticalForm;

/// <summary>
/// Handles updating a pharmaceutical form's formulation name.
/// </summary>
internal sealed class UpdatePharmaceuticalFormHandler
    : IRequestHandler<UpdatePharmaceuticalFormCommand, ErrorOr<UpdatedPharmaceuticalFormResponse>>
{
    private readonly IPharmaceuticalFormRepository _pharmaceuticalFormRepository;

    public UpdatePharmaceuticalFormHandler(IPharmaceuticalFormRepository pharmaceuticalFormRepository)
    {
        _pharmaceuticalFormRepository = pharmaceuticalFormRepository;
    }

    /// <summary>
    /// Updates the pharmaceutical form's formulation name.
    /// </summary>
    /// <param name="command">Update command containing the form ID and new formulation name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated pharmaceutical form response with the new formulation name.</returns>
    public async Task<ErrorOr<UpdatedPharmaceuticalFormResponse>> Handle(
        UpdatePharmaceuticalFormCommand command,
        CancellationToken cancellationToken)
    {
        PharmaceuticalForm? pharmaceuticalForm = await _pharmaceuticalFormRepository.GetByIdAsync(
            command.PharmaceuticalFormId,
            cancellationToken);

        if (pharmaceuticalForm is null)
        {
            return Error.NotFound(
                description: "The requested pharmaceutical form was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "PharmaceuticalForm",
                    ["resourceId"] = command.PharmaceuticalFormId.ToString()
                });
        }

        if (!string.IsNullOrWhiteSpace(command.Request.FormulationName))
        {
            string normalizedName = command.Request.FormulationName.Trim();

            if (normalizedName != pharmaceuticalForm.FormulationName)
            {
                PharmaceuticalForm? duplicate = await _pharmaceuticalFormRepository.GetByNameAsync(
                    normalizedName,
                    cancellationToken);

                if (duplicate is not null && duplicate.Id != pharmaceuticalForm.Id)
                {
                    return Error.Conflict(
                        description: "A pharmaceutical form with the same name already exists.",
                        metadata: new Dictionary<string, object>
                        {
                            ["conflictReason"] = "duplicateFormulationName",
                            ["resourceId"] = duplicate.Id.ToString()
                        });
                }

                pharmaceuticalForm.UpdateFormulationName(normalizedName);
            }
        }

        return pharmaceuticalForm.MapToUpdatedResponse();
    }
}


