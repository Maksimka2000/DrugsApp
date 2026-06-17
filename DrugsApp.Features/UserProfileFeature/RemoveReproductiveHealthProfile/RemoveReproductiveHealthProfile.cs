using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.RemoveReproductiveHealthProfile;

/// <summary>
/// 
/// </summary>
/// <param name="UserId"></param>
public sealed record RemoveReproductiveHealthProfileCommand(Guid UserId) : IRequest<ErrorOr<Success>>;

internal sealed class RemoveReproductiveHealthProfileHandler(
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository)
    : IRequestHandler<RemoveReproductiveHealthProfileCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RemoveReproductiveHealthProfileCommand command, CancellationToken cancellationToken)
    {
        await _clientReproductiveHealthProfileRepository.RemoveByUserIdAsync(command.UserId, cancellationToken);
        return Result.Success;
    }
}
