using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.DeleteDrugProblemType;

/// <summary>
/// Handles deletion of drug problem types.
/// </summary>
internal sealed class DeleteDrugProblemTypeHandler(IDrugProblemTypeRepository _repository)
    : IRequestHandler<DeleteDrugProblemTypeCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteDrugProblemTypeCommand command,
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

        _repository.Remove(entity);

        return Result.Success;
    }
}
