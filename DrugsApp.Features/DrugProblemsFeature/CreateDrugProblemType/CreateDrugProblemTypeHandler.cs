using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblemType;

/// <summary>
/// Handles creation of drug problem types.
/// </summary>
internal sealed class CreateDrugProblemTypeHandler(IDrugProblemTypeRepository _repository)
    : IRequestHandler<CreateDrugProblemTypeCommand, ErrorOr<DrugProblemTypeDto>>
{
    public async Task<ErrorOr<DrugProblemTypeDto>> Handle(
        CreateDrugProblemTypeCommand command,
        CancellationToken cancellationToken)
    {
        string normalizedName = NormalizeForLookup(command.Request.ProblemTypeName);

        DrugProblemType? existing = await _repository.GetByNameAsync(normalizedName, cancellationToken);
        if (existing is not null)
        {
            return Error.Conflict(
                description: "Drug problem type already exists.",
                metadata: new()
                {
                    ["conflictingResourceId"] = existing.Id.ToString(),
                    ["field"] = "problemTypeName",
                });
        }

        ErrorOr<DrugProblemType> created = DrugProblemType.Create(normalizedName);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _repository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDto();
    }

    private static string NormalizeForLookup(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
