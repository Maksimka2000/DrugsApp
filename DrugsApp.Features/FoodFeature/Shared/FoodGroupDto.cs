using System;
using System.Collections.Generic;

namespace DrugsApp.Features.FoodFeature.Shared;

/// <summary>
/// DTO representing a food group with its members.
/// </summary>
public sealed record FoodGroupDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<FoodDto> Foods,
    string Type);
