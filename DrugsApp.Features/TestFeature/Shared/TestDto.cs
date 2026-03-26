using System;

namespace DrugsApp.Features.TestFeature.Shared;

/// <summary>
/// DTO representing a test aggregate.
/// </summary>
/// <param name="Id">Test identifier.</param>
/// <param name="Attribute1">First attribute value.</param>
/// <param name="Attribute2">Optional second attribute value.</param>
/// <param name="Attribute3">Optional third attribute value.</param>
/// <param name="Attribute4">Fourth attribute value.</param>
public sealed record TestDto(
    Guid Id,
    string Attribute1,
    int? Attribute2,
    decimal? Attribute3,
    bool Attribute4);

