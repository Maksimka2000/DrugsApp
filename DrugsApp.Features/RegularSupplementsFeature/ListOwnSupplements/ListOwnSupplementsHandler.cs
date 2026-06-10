using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.ListOwnSupplements;

/// <summary>
/// Handles listing own supplements for the current user.
/// </summary>
internal sealed class ListOwnSupplementsHandler(
    IClientOwnSupplementRepository _clientOwnSupplementRepository)
    : IRequestHandler<ListOwnSupplementsQuery, ErrorOr<ListOwnSupplementsResponse>>
{
    public async Task<ErrorOr<ListOwnSupplementsResponse>> Handle(
        ListOwnSupplementsQuery query,
        CancellationToken cancellationToken)
    {
        List<Domain.RegularSupplementsFeature.Entities.ClientOwnSupplement> items =
            await _clientOwnSupplementRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return items.MapToResponse();
    }
}
