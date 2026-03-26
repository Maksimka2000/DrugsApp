using System;

namespace DrugsApp.Features.DrugFeature.Shared;

/// <summary>
/// Data transfer object representing an id-name lookup entry.
/// </summary>
public sealed record LookupDto(Guid Id, string Name);

