using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.DeleteDrugProblem;

/// <summary>
/// Handles deletion of drug problems.
/// </summary>
internal sealed class DeleteDrugProblemHandler(IDrugProblemRepository _drugProblemRepository)
    : IRequestHandler<DeleteDrugProblemCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteDrugProblemCommand command,
        CancellationToken cancellationToken)
    {
        DrugProblem? entity = await _drugProblemRepository.GetByIdForUserAsync(
            command.UserId,
            command.Id,
            track: true,
            cancellationToken);

        if (entity is null)
        {
            return Error.NotFound(
                description: "Drug problem not found.",
                metadata: new()
                {
                    ["resourceType"] = "DrugProblem",
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        _drugProblemRepository.Remove(entity);

        return Result.Success;
    }
}
