namespace DrugsApp.Domain.SupplementFeature.Entities;

/// <summary>
/// Defines the available fields for searching supplement groups in the system.
/// </summary>
/// <remarks>
/// This enumeration is used to specify the field by which the supplement group search is conducted,
/// allowing for more precise and targeted queries.
/// </remarks>
public enum SupplementGroupSearchField
{
    /// <summary>
    /// Represents the absence of a specific search field when querying supplement group data.
    /// </summary>
    /// <remarks>
    /// Use this field to indicate that no specific search criteria should be applied.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Represents the search field for querying supplement group data based on group names.
    /// </summary>
    /// <remarks>
    /// This field is used to filter or search supplement groups by their designated group names.
    /// </remarks>
    GroupName = 1,

    /// <summary>
    /// Represents the search field for querying supplement group data based on supplement names.
    /// </summary>
    /// <remarks>
    /// Use this field to filter and retrieve groups of supplements based on their associated names.
    /// </remarks>
    SuplementName = 2
}
