using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupportFeature.RemoveFaqEntry;

/// <summary>
/// Request model for deleting a FAQ entry.
/// </summary>
public sealed record RemoveFaqEntryRequest(Guid Id);

/// <summary>
/// Command for deleting a FAQ entry.
/// </summary>
public sealed record RemoveFaqEntryCommand(Guid Id) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Controller for deleting FAQ entries on the admin support surface.
/// </summary>
[ApiController]
[Tags("Support")]
[Route("api/support/admin/faqs")]
public sealed class RemoveFaqEntryController(
    IMediator mediator,
    IValidator<RemoveFaqEntryRequest> validator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IValidator<RemoveFaqEntryRequest> _validator = validator;

    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "RemoveFaqEntry";

    /// <summary>
    /// Deletes an existing FAQ entry.
    /// </summary>
    /// <param name="id">FAQ entry identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">FAQ entry deleted successfully.</response>
    /// <response code="404">FAQ entry not found.</response>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        RemoveFaqEntryRequest request = new(id);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        RemoveFaqEntryCommand command = request.MapToCommand();
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
