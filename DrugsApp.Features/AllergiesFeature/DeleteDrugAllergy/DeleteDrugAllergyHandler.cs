using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.DeleteDrugAllergy;

/// <summary>
/// Handles deletion of drug allergies.
/// </summary>
internal sealed class DeleteDrugAllergyHandler(IClientDrugAllergyRepository _drugAllergyRepository)
    : IRequestHandler<DeleteDrugAllergyCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteDrugAllergyCommand command,
        CancellationToken cancellationToken)
    {
        ClientDrugAllergy? allergy = await _drugAllergyRepository.GetByIdForUserAsync(
            command.UserId,
            command.Id,
            cancellationToken);

        if (allergy is null)
        {
            return Error.NotFound(
                description: "Drug allergy not found.",
                metadata: new()
                {
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        _drugAllergyRepository.Remove(allergy);

        return Result.Success;
    }
}
