using System;
using DrugsApp.Features;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.TestFeature.Shared;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TestFeature.RetrieveTest;

/// <summary>
/// Controller for retrieving tests.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RetrieveTestController"/> class.
/// </remarks>
/// <param name="mediator">MediatR mediator.</param>
[ApiController]
[Tags("Test")]
[Route("api/tests")]
public sealed class RetrieveTestController(
    IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "GetTestById";

    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a test by id.
    /// </summary>
    /// <param name="testId">Test id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the requested test.</response>
    /// <response code="404">Test not found.</response>
    [HttpGet("{testId:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(TestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDto>> GetById(
        [FromRoute] Guid testId,
        CancellationToken cancellationToken)
    {
    
        GetTestByIdQuery query = RetrieveTestMapping.MapToQuery(testId);
        ErrorOr<TestDto> response = await _mediator.Send(query, cancellationToken);


        if (response.IsError)
        {
            return response.Errors.MapToProblem(HttpContext);
        }

        return Ok(response.Value);
    }
}

/// <summary>
/// Query that retrieves a test by id.
/// </summary>
/// <param name="TestId">Test id.</param>
public sealed record GetTestByIdQuery(Guid TestId) : IRequest<ErrorOr<TestDto>>;

