using System;

namespace DrugsApp.Features.TestFeature.Shared;

public sealed record TestDto(
    Guid Id,
    string Attribute1,
    int? Attribute2,
    decimal? Attribute3,
    bool Attribute4);

