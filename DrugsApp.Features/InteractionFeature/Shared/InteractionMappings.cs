using DrugsApp.Domain.InteractionFeature.Entities;

namespace DrugsApp.Features.InteractionFeature.Shared;

/// <summary>
/// A static class that provides mapping functionalities for converting interaction-related entities
/// to their corresponding Data Transfer Objects (DTOs).
/// </summary>
public static class InteractionMappings
{
    /// <summary>
    /// Maps an <see cref="InteractionSeverity"/> entity to an <see cref="InteractionSeverityDto"/>
    /// that encapsulates severity details such as identifier, severity number, name, and description.
    /// </summary>
    /// <param name="severity">The <see cref="InteractionSeverity"/> entity to map.</param>
    /// <returns>An instance of <see cref="InteractionSeverityDto"/> containing the mapped severity details.</returns>
    public static InteractionSeverityDto MapToDto(this InteractionSeverity severity)
    {
        return new InteractionSeverityDto(
            Id: severity.Id,
            SeverityNumber: severity.SeverityNumber,
            Name: severity.Name,
            Description: severity.Description);
    }

    /// <summary>
    /// Maps an <see cref="Interaction"/> entity to an <see cref="InteractionCardDto"/> which is used
    /// to represent interaction details including severity, text, and associated entities.
    /// </summary>
    /// <param name="interaction">The interaction entity to map.</param>
    /// <param name="left">The left interaction query found DTO representing one side of the interaction.</param>
    /// <param name="right">The right interaction query found DTO representing the other side of the interaction.</param>
    /// <param name="severity">The severity DTO containing details about the interaction's severity.</param>
    /// <returns>A new instance of <see cref="InteractionCardDto"/> containing the mapped interaction details.</returns>
    public static InteractionCardDto MapToCardDto(
        this Interaction interaction,
        InteractionQueryFoundDto left,
        InteractionQueryFoundDto right,
        InteractionSeverityDto severity)
    {
        return new InteractionCardDto(
            InteractionId: interaction.Id,
            Left: left,
            Right: right,
            Severity: severity,
            DefaultText: interaction.DefaultText,
            StomachText: interaction.StomachText,
            CreatedAtUtc: interaction.CreatedAtUtc,
            UpdatedAtUtc: interaction.UpdatedAtUtc);
    }
}
