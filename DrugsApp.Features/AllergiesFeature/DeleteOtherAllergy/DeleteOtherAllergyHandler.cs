using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.DeleteOtherAllergy;

internal sealed class DeleteOtherAllergyHandler(IClientOtherAllergyRepository _repository)
    : IRequestHandler<DeleteOtherAllergyCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteOtherAllergyCommand command, CancellationToken cancellationToken)
    {
        ClientOtherAllergy? allergy = await _repository.GetByIdForUserAsync(command.UserId, command.Id, cancellationToken);
        if (allergy is null)
            return Error.NotFound(description: "Other allergy not found.", metadata: new() { ["resourceId"] = command.Id.ToString() });
        _repository.Remove(allergy);
        return Result.Success;
    }
}
