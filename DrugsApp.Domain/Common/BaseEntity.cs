
using System;
namespace DrugsApp.Domain.Common;

/// <summary>
/// Provides identity and audit timestamps for every aggregate or entity.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier generated at construction time.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// UTC timestamp recorded when the entity was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; protected set; }

    /// <summary>
    /// UTC timestamp recorded when the entity last changed.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; protected set; }

    /// <summary>
    /// Initializes identity and CreatedAt fields for derived entities.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
    }    
}
