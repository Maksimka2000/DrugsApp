using System;
using DrugsApp.Features;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.TestFeature.RetrieveTest;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TestFeature.CreateTest;

/// <summary>
/// Controller for creating tests.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateTestController"/> class.
/// </remarks>
/// <param name="mediator">MediatR mediator.</param>
/// <param name="validator">Validator for the request.</param>
[ApiController]
[Tags("Test")]
[Route("api/tests")]
public sealed class CreateTestController(
    IMediator mediator,
    IValidator<CreateTestRequest> validator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IValidator<CreateTestRequest> _validator = validator;

    /// <summary>
    /// Creates a new test.
    /// </summary>
    /// <param name="request">Create request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the created test.</response>
    /// <response code="400">Invalid request payload.</response>
    [HttpPost]
    public async Task<ActionResult<CreatedTestResponse>> Create(
        [FromBody] CreateTestRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateTestCommand command = request.MapToCommand();
        ErrorOr<CreatedTestResponse> response = await _mediator.Send(command, cancellationToken);


        return response.Match(
               respnonse => CreatedAtRoute(
                    routeName: RetrieveTestController.RouteName,
                    routeValues: new { testId = response.Value.Id },
                    value: response.Value),
               errors => errors.MapToProblem(HttpContext));
            
            
    }
}

/// <summary>
/// Request payload for creating a test.
/// </summary>
public sealed record CreateTestRequest
{
    /// <summary>
    /// The first attribute value.
    /// </summary>
    public string Attribute1 { get; init; } = string.Empty;

    /// <summary>
    /// Optional second attribute value.
    /// </summary>
    public int? Attribute2 { get; init; }

    /// <summary>
    /// Optional third attribute value.
    /// </summary>
    public decimal? Attribute3 { get; init; }

    /// <summary>
    /// Optional fourth attribute value.
    /// </summary>
    public bool? Attribute4 { get; init; }
}

/// <summary>
/// Response returned after creating a test.
/// </summary>
/// <param name="Id">Test identifier.</param>
/// <param name="Attribute1">First attribute value.</param>
/// <param name="Attribute2">Optional second attribute value.</param>
/// <param name="Attribute3">Optional third attribute value.</param>
/// <param name="Attribute4">Fourth attribute value.</param>
public sealed record CreatedTestResponse(
    Guid Id,
    string Attribute1,
    int? Attribute2,
    decimal? Attribute3,
    bool Attribute4);

/// <summary>
/// Command that creates a test aggregate.
/// </summary>
/// <param name="Attribute1">First attribute value.</param>
/// <param name="Attribute2">Optional second attribute value.</param>
/// <param name="Attribute3">Optional third attribute value.</param>
/// <param name="Attribute4">Optional fourth attribute value.</param>
public sealed record CreateTestCommand(
    string Attribute1,
    int? Attribute2,
    decimal? Attribute3,
    bool? Attribute4) : IRequest<ErrorOr<CreatedTestResponse>>;

