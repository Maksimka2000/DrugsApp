using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblemType;

/// <summary>
/// Handles updates to drug problem types.
/// </summary>
internal sealed class UpdateDrugProblemTypeHandler(IDrugProblemTypeRepository _repository)
    : IRequestHandler<UpdateDrugProblemTypeCommand, ErrorOr<DrugProblemTypeDto>>
{
    public async Task<ErrorOr<DrugProblemTypeDto>> Handle(
        UpdateDrugProblemTypeCommand command,
        CancellationToken cancellationToken)
    {
        DrugProblemType? entity = await _repository.GetByIdAsync(
            command.Id,
            track: true,
            cancellationToken);

        if (entity is null)
        {
            return Error.NotFound(
                description: "Drug problem type not found.",
                metadata: new()
                {
                    ["resourceType"] = "DrugProblemType",
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        if (!string.IsNullOrWhiteSpace(command.Request.ProblemTypeName))
        {
            string normalizedName = NormalizeForLookup(command.Request.ProblemTypeName);
            DrugProblemType? existing = await _repository.GetByNameAsync(normalizedName, cancellationToken);
            if (existing is not null && existing.Id != entity.Id)
            {
                return Error.Conflict(
                    description: "Drug problem type already exists.",
                    metadata: new()
                    {
                        ["field"] = "problemTypeName",
                        ["conflictingResourceId"] = existing.Id.ToString(),
                    });
            }

            ErrorOr<Success> updateResult = entity.UpdateName(normalizedName);
            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }
        }

        return entity.MapToDto();
    }

    private static string NormalizeForLookup(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
