using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.CheckUserHealthConditionCustomNameAvailability;

/// <summary>
/// Handles custom user health condition name availability checks.
/// </summary>
public sealed class CheckUserHealthConditionCustomNameAvailabilityHandler(
    IUserHealthConditionRepository _userHealthConditionRepository)
    : IRequestHandler<CheckUserHealthConditionCustomNameAvailabilityQuery, ErrorOr<CheckUserHealthConditionCustomNameAvailabilityResponse>>
{
    public async Task<ErrorOr<CheckUserHealthConditionCustomNameAvailabilityResponse>> Handle(
        CheckUserHealthConditionCustomNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        bool exists = await _userHealthConditionRepository.ExistsDuplicateForUserAsync(
            query.UserId,
            query.NormalizedCustomHealthConditionName,
            cancellationToken);

        return new CheckUserHealthConditionCustomNameAvailabilityResponse(
            IsAvailable: !exists);
    }
}
