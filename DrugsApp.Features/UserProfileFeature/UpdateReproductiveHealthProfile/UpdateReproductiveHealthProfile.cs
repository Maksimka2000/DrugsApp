using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.UpdateReproductiveHealthProfile;

public sealed record UpdateReproductiveHealthProfileRequest(
    bool CurrentlyPregnant,
    bool PlanningToGetPregnant,
    bool Menopause,
    ContraceptionMethodCode? ContraceptionMethodCode);

public sealed record UpdateReproductiveHealthProfileCommand(
    Guid UserId,
    UpdateReproductiveHealthProfileRequest Request) : IRequest<ErrorOr<ReproductiveHealthProfileResponse>>;

internal sealed class UpdateReproductiveHealthProfileHandler(
    IReproductiveHealthProfileApplicationService _service)
    : IRequestHandler<UpdateReproductiveHealthProfileCommand, ErrorOr<ReproductiveHealthProfileResponse>>
{
    public async Task<ErrorOr<ReproductiveHealthProfileResponse>> Handle(UpdateReproductiveHealthProfileCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<ReproductiveHealthProfileUpsertResult> result = await _service.UpsertAsync(
            command.UserId,
            new ReproductiveHealthProfileUpsertRequest(
                command.Request.CurrentlyPregnant,
                command.Request.PlanningToGetPregnant,
                command.Request.Menopause,
                command.Request.ContraceptionMethodCode),
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return ReproductiveHealthProfileResponse.From(result.Value.Profile);
    }
}
