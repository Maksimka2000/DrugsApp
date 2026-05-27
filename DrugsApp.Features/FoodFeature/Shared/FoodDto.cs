using System;
using System.Collections.Generic;

namespace DrugsApp.Features.FoodFeature.Shared;

/// <summary>
/// DTO representing a food.
/// </summary>
public sealed record FoodDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> Aliases,
    string Type);
