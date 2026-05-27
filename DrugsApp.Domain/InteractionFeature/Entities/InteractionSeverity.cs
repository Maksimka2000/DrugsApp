using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.InteractionFeature.Entities;

/// <summary>
/// Lookup entity describing interaction severity scale.
/// </summary>
public sealed class InteractionSeverity : BaseEntity
{
    private InteractionSeverity() : base()
    {
    }

    /// <summary>
    /// Normalized severity scale number.
    /// </summary>
    public short SeverityNumber { get; private set; }

    /// <summary>
    /// Severity label.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional severity description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Creates a new severity entry.
    /// </summary>
    public static InteractionSeverity Create(
        short severityNumber,
        string name,
        string? description)
    {
        return new InteractionSeverity
        {
            SeverityNumber = severityNumber,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description!.Trim()
        };
    }
}
