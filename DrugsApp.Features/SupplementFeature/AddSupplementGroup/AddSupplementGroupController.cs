using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.SupplementFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupplementFeature.AddSupplementGroup;

/// <summary>
/// Request model for creating a supplement group.
/// </summary>
public sealed record AddSupplementGroupRequest(
    string Name,
    IReadOnlyCollection<Guid> SupplementIds);

/// <summary>
/// Command for adding a supplement group.
/// </summary>
public sealed record AddSupplementGroupCommand(
    AddSupplementGroupRequest Request) : IRequest<ErrorOr<CreatedSupplementGroupResponse>>;

/// <summary>
/// Response after creating a supplement group.
/// </summary>
public sealed record CreatedSupplementGroupResponse(
    SupplementGroupDto SupplementGroup);

    

/// <summary>
/// Controller for creating supplement groups.
/// </summary>
[ApiController]
[Tags("SupplementGroups")]
[Route("api/supplement-groups")]
public sealed class AddSupplementGroupController(
    IMediator mediator,
    IValidator<AddSupplementGroupRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating supplement groups.
    /// </summary>
    public const string RouteName = "AddSupplementGroup";

    /// <summary>
    /// Creates a new supplement group.
    /// </summary>
    /// <param name="request">Create group payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns created supplement group.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="409">Group name already exists.</response>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreatedSupplementGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedSupplementGroupResponse>> Create(
        [FromBody] AddSupplementGroupRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddSupplementGroupCommand command = request.MapToCommand();
        ErrorOr<CreatedSupplementGroupResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: UpdateSupplementGroup.UpdateSupplementGroupController.RouteName,
                routeValues: new { id = response.SupplementGroup.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
