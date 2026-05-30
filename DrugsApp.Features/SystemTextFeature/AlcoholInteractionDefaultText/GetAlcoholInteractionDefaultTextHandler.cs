using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SystemTextFeature.Entities;
using DrugsApp.Domain.SystemTextFeature.Repositories;
using DrugsApp.Features.SystemTextFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SystemTextFeature.AlcoholInteractionDefaultText;

/// <summary>
/// Handler for retrieving the default alcohol interaction text.
/// </summary>
internal sealed class GetAlcoholInteractionDefaultTextHandler(
    ISystemTextRepository _systemTextRepository)
    : IRequestHandler<GetAlcoholInteractionDefaultTextQuery, ErrorOr<AlcoholInteractionDefaultTextDto>>
{
    private const string AlcoholInteractionDefaultTextKey = "activeSubstance.alcohol.defaultText";

    public async Task<ErrorOr<AlcoholInteractionDefaultTextDto>> Handle(
        GetAlcoholInteractionDefaultTextQuery query,
        CancellationToken cancellationToken)
    {
        SystemText? systemText = await _systemTextRepository.GetByKeyAsync(
            AlcoholInteractionDefaultTextKey,
            cancellationToken);

        if (systemText is null)
        {
            return Error.NotFound(
                code: "SystemTextNotFound",
                description: "System text not found");
        }

        return systemText.MapToDto();
    }
}
