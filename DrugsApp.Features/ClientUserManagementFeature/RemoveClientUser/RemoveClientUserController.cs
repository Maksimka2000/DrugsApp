using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ClientUserManagementFeature.RemoveClientUser;

[ApiController]
[Route("api/client-users")]
[Tags("Client Users")]
public sealed class RemoveClientUserController(IMediator mediator) : ControllerBase
{
    public const string RouteName = "RemoveClientUser";

    /// <summary>
    /// Removes a client user account.
    /// </summary>
    /// <param name="clientUserId">Identifier of the client user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    [HttpDelete("{client_user_id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        [FromRoute(Name = "client_user_id")] Guid clientUserId,
        CancellationToken cancellationToken)
    {
        if (clientUserId == Guid.Empty)
        {
            return ValidationProblem("client_user_id must not be empty.");
        }

        RemoveClientUserCommand command = new(clientUserId);
        ErrorOr<Success> result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
