using DrugsApp.Features.ClientUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ClientUserManagementFeature.ListClientUsers;

[ApiController]
[Route("api/client-users")]
[Tags("Client Users")]
public sealed class ListClientUsersController(
    IMediator mediator,
    IValidator<ListClientUsersRequest> validator) : ControllerBase
{
    public const string RouteName = "ListClientUsers";

    /// <summary>
    /// Lists client users with paging and optional search.
    /// </summary>
    /// <param name="request">Query payload including paging and search.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    [HttpGet(Name = RouteName)]
    public async Task<ActionResult<ListClientUsersResponse>> List(
        [FromQuery] ListClientUsersRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListClientUsersQuery query = new(request.Skip, request.Take, request.Search);
        ErrorOr<ListClientUsersResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListClientUsersResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
