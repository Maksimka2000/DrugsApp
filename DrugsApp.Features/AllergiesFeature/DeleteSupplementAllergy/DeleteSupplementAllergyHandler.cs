using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.DeleteSupplementAllergy;

internal sealed class DeleteSupplementAllergyHandler(IClientSupplementAllergyRepository _repository)
    : IRequestHandler<DeleteSupplementAllergyCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteSupplementAllergyCommand command, CancellationToken cancellationToken)
    {
        ClientSupplementAllergy? allergy = await _repository.GetByIdForUserAsync(command.UserId, command.Id, cancellationToken);
        if (allergy is null)
            return Error.NotFound(description: "Supplement allergy not found.", metadata: new() { ["resourceId"] = command.Id.ToString() });
        _repository.Remove(allergy);
        return Result.Success;
    }
}
