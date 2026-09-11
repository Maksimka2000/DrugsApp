using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SystemTextFeature.Entities;
using DrugsApp.Domain.SystemTextFeature.Repositories;
using DrugsApp.Features.SystemTextFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SystemTextFeature.AlcoholInteractionDefaultText;

/// <summary>
/// Handler for updating the default alcohol interaction text.
/// </summary>
internal sealed class UpdateAlcoholInteractionDefaultTextHandler(
    ISystemTextRepository _systemTextRepository)
    : IRequestHandler<UpdateAlcoholInteractionDefaultTextCommand, ErrorOr<AlcoholInteractionDefaultTextDto>>
{
    private const string AlcoholInteractionDefaultTextKey = "activeSubstance.alcohol.defaultText";

    public async Task<ErrorOr<AlcoholInteractionDefaultTextDto>> Handle(
        UpdateAlcoholInteractionDefaultTextCommand command,
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

        ErrorOr<Success> updateResult = systemText.UpdateValue(command.DefaultText);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        await _systemTextRepository.InvalidateAsync(AlcoholInteractionDefaultTextKey, cancellationToken);

        return systemText.MapToDto();
    }
}
