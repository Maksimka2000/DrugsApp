using DrugsApp.Features.ClientUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ClientUserManagementFeature.SuspendClientUser;

[ApiController]
[Route("api/client-users")]
[Tags("Client Users")]
public sealed class SuspendClientUserController(
    IMediator mediator,
    IValidator<SuspendClientUserRouteRequest> validator) : ControllerBase
{
    public const string RouteName = "SuspendClientUser";

    /// <summary>
    /// Suspends or unsuspends a client user.
    /// </summary>
    /// <param name="clientUserId">Identifier of the client user.</param>
    /// <param name="body">Body containing the target suspension state.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    [HttpPut("{client_user_id:guid}/suspension", Name = RouteName)]
    public async Task<ActionResult<ClientUserSuspensionResponse>> Suspend(
        [FromRoute(Name = "client_user_id")] Guid clientUserId,
        [FromBody] SuspendClientUserRequest body,
        CancellationToken cancellationToken)
    {
        SuspendClientUserRouteRequest request = new(clientUserId, body.IsSuspended);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SuspendClientUserCommand command = new(clientUserId, body.IsSuspended);
        ErrorOr<ClientUserSuspensionResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ClientUserSuspensionResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
