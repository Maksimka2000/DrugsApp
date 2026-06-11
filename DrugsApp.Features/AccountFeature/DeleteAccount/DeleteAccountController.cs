using System;
using System.Security.Claims;
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

namespace DrugsApp.Features.AccountFeature.DeleteAccount;

[ApiController]
[Route("api/account/delete")]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Account")]
public sealed class DeleteAccountController(
    IMediator _mediator,
    IValidator<DeleteAccountRequest> _validator) : ControllerBase
{
    public const string RouteName = "DeleteAccount";

    /// <summary>
    /// Deletes the current user account after password verification and confirmation phrase.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteAccountRequest request,
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

        DeleteAccountCommand command = request.MapToCommand(userId);

        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
