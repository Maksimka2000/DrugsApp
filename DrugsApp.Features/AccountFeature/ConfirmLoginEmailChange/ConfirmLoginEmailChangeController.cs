using System;
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

namespace DrugsApp.Features.AccountFeature.ConfirmLoginEmailChange;

[ApiController]
[Route("api/account/confirm-email-change")]
[AllowAnonymous]
[Tags("Account")]
public sealed class ConfirmLoginEmailChangeController(
    IMediator _mediator,
    IValidator<ConfirmLoginEmailChangeRequest> _validator) : ControllerBase
{
    public const string RouteName = "ConfirmLoginEmailChange";

    /// <summary>
    /// Confirms a pending login email change using the verification token.
    /// </summary>
    [HttpPost("{userId:guid?}", Name = RouteName)]
    [ProducesResponseType(typeof(ConfirmLoginEmailChangeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfirmLoginEmailChangeResponse>> Confirm(
        [FromRoute] Guid? userId,
        [FromBody] ConfirmLoginEmailChangeRequest request,
        CancellationToken cancellationToken)
    {
        Guid resolvedUserId = userId ?? request.UserId;

        ConfirmLoginEmailChangeRequest effectiveRequest = request with { UserId = resolvedUserId };

        ValidationResult? validationResult = await _validator.ValidateAsync(effectiveRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ConfirmLoginEmailChangeCommand command = effectiveRequest.MapToCommand();

        ErrorOr<ConfirmLoginEmailChangeResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ConfirmLoginEmailChangeResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
