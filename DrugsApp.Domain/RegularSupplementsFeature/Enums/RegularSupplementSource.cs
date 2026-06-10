namespace DrugsApp.Domain.RegularSupplementsFeature.Enums;

/// <summary>
/// Discriminator for catalog versus user-owned compound supplement targets. [description]
/// </summary>
public enum RegularSupplementSource
{
    /// <summary>
    /// Schedule references a shared catalog supplement row.
    /// </summary>
    Database,

    /// <summary>
    /// Schedule references a user-private own supplement compound.
    /// </summary>
    Own
}
