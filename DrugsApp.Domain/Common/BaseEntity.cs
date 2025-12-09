
using System;
namespace DrugsApp.Domain.Common;

public abstract class BaseEntity
{
    // Generated automatically when entity is created. Unique identifier for the entity.
    public Guid Id { get; protected set; }
    // Set automatically in the constructor.  UTC timestamp when the entity was created.
    public DateTime CreatedAt { get; protected set; }
    // Updated automatically when entity changes. UTC timestamp when the entity was last modified.
    public DateTime? UpdatedAt { get; protected set; }
    // Forces use of factory methods in derived classes. Protected constructor to prevent direct instantiation.
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
    // Call this method in all entity mutation methods. Marks the entity as modified by updating the UpdatedAt timestamp.
    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
