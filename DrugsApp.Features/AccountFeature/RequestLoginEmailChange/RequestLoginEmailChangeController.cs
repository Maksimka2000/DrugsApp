using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.AccountFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using DrugsApp.Domain.AuthFeature;

namespace DrugsApp.Features.AccountFeature.RequestLoginEmailChange;

[ApiController]
[Route("api/account/change-email")]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Account")]
public sealed class RequestLoginEmailChangeController(
    IMediator _mediator,
    IValidator<RequestLoginEmailChangeRequest> _validator) : ControllerBase
{
    public const string RouteName = "RequestLoginEmailChange";

    /// <summary>
    /// Requests a login email change for the current user (verification required).
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(RequestLoginEmailChangeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RequestLoginEmailChangeResponse>> RequestChange(
        [FromBody] RequestLoginEmailChangeRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        RequestLoginEmailChangeCommand command = request.MapToCommand(userId);

        ErrorOr<RequestLoginEmailChangeResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
