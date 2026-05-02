using System;
using System.Collections.Generic;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;

/// <summary>
/// DTO describing an active substance group with its members.
/// </summary>
public sealed record ActiveSubstanceGroupDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<ActiveSubstanceDto> Members);
