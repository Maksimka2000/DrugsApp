using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using DrugsApp.Domain.AuthFeature;

namespace DrugsApp.Features.AccountFeature.ChangeAccountPassword;

[ApiController]
[Route("api/account/change-password")]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Account")]
public sealed class ChangeAccountPasswordController(
    IMediator _mediator,
    IValidator<ChangeAccountPasswordRequest> _validator) : ControllerBase
{
    public const string RouteName = "ChangeAccountPassword";

    /// <summary>
    /// Changes password for the current user.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeAccountPasswordRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        ChangeAccountPasswordCommand command = request.MapToCommand(userId);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
