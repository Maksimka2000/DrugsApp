using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.SupplementFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.RegularSupplementsFeature.Entities;

/// <summary>
/// One catalog supplement member inside a user-owned own supplement compound.
/// </summary>
public sealed class ClientOwnSupplementItem : BaseEntity
{
    // Attributes

    /// <summary>
    /// Parent own supplement identifier.
    /// </summary>
    public Guid OwnSupplementId { get; private set; }

    /// <summary>
    /// Catalog supplement identifier referenced by this member row.
    /// </summary>
    public Guid SupplementId { get; private set; }

    /// <summary>
    /// Zero-based display order unique within the parent compound.
    /// </summary>
    public int SortOrder { get; private set; }

    // Relationships

    /// <summary>
    /// Catalog supplement navigation used for read projections.
    /// </summary>
    public Supplement Supplement { get; private set; } = null!;

    /// <summary>
    /// Parent own supplement navigation.
    /// </summary>
    public ClientOwnSupplement OwnSupplement { get; private set; } = null!;

    private ClientOwnSupplementItem() : base()
    {
    }

    private ClientOwnSupplementItem(Guid ownSupplementId, Guid supplementId, int sortOrder) : this()
    {
        OwnSupplementId = ownSupplementId;
        SupplementId = supplementId;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a validated member row for an own supplement compound.
    /// </summary>
    /// <param name="supplementId">Catalog supplement identifier.</param>
    /// <param name="sortOrder">Zero-based display order within the parent.</param>
    /// <returns>Validated child row or validation errors.</returns>
    public static ErrorOr<ClientOwnSupplementItem> Create(Guid supplementId, int sortOrder)
    {
        if (supplementId == Guid.Empty)
        {
            return Error.Validation(
                description: "Supplement identifier is required.",
                metadata: new()
                {
                    ["field"] = "supplementId",
                });
        }

        if (sortOrder < 0)
        {
            return Error.Validation(
                description: "Sort order must be zero or greater.",
                metadata: new()
                {
                    ["field"] = "sortOrder",
                });
        }

        return new ClientOwnSupplementItem(Guid.Empty, supplementId, sortOrder);
    }

    internal void AssignOwnSupplementId(Guid ownSupplementId)
    {
        OwnSupplementId = ownSupplementId;
    }
}
