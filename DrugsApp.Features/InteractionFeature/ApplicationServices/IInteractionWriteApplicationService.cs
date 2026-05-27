using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.InteractionFeature.ApplicationServices;

/// <summary>
/// Write-side workflow for creating interactions.
/// </summary>
public interface IInteractionWriteApplicationService
{
    /// <summary>
    /// Creates a new interaction based on the provided input data.
    /// </summary>
    /// <param name="input">The input data required to create the interaction, including party details, atomic types, and other information.</param>
    /// <param name="cancellationToken">A token to observe for cancellation of the asynchronous operation.</param>
    /// <returns/>
    Task<ErrorOr<CreateInteractionWorkflowOutput>> CreateAsync(
        CreateInteractionWorkflowInput input,
        CancellationToken cancellationToken);
}


