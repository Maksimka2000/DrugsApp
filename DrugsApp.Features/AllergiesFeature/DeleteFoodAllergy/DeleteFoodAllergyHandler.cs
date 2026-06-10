using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.DeleteFoodAllergy;

internal sealed class DeleteFoodAllergyHandler(IClientFoodAllergyRepository _repository)
    : IRequestHandler<DeleteFoodAllergyCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteFoodAllergyCommand command, CancellationToken cancellationToken)
    {
        ClientFoodAllergy? allergy = await _repository.GetByIdForUserAsync(command.UserId, command.Id, cancellationToken);
        if (allergy is null)
            return Error.NotFound(description: "Food allergy not found.", metadata: new() { ["resourceId"] = command.Id.ToString() });
        _repository.Remove(allergy);
        return Result.Success;
    }
}
