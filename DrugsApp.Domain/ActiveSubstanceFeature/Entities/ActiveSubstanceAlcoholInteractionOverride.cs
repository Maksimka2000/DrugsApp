using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.ActiveSubstanceFeature.Entities;

/// <summary>
/// Override text describing alcohol interaction guidance for a specific active substance.
/// </summary>
public sealed class ActiveSubstanceAlcoholInteractionOverride : BaseEntity
{
    /// <summary>
    /// Foreign key referencing the owning <see cref="ActiveSubstance"/>.
    /// </summary>
    public Guid ActiveSubstanceId { get; private set; }

    /// <summary>
    /// Navigation to the owning active substance.
    /// </summary>
    public ActiveSubstance ActiveSubstance { get; private set; } = null!;

    /// <summary>
    /// Alcohol-specific override text shown in interaction UI.
    /// </summary>
    public string AlcoholSpecialText { get; private set; } = string.Empty;

    private ActiveSubstanceAlcoholInteractionOverride() : base()
    {
    }

    /// <summary>
    /// Creates a new alcohol interaction override entry.
    /// </summary>
    public static ActiveSubstanceAlcoholInteractionOverride Create(
        Guid activeSubstanceId,
        string alcoholSpecialText)
    {
        string normalizedText = NormalizeRequiredText(alcoholSpecialText);

        if (string.IsNullOrEmpty(normalizedText))
        {
            return new ActiveSubstanceAlcoholInteractionOverride
            {
                ActiveSubstanceId = activeSubstanceId,
                AlcoholSpecialText = string.Empty
            };
        }

        return new ActiveSubstanceAlcoholInteractionOverride
        {
            ActiveSubstanceId = activeSubstanceId,
            AlcoholSpecialText = normalizedText
        };
    }

    /// <summary>
    /// Updates the override text.
    /// </summary>
    /// <param name="alcoholSpecialText">Override text.</param>
    public void UpdateAlcoholSpecialText(string alcoholSpecialText)
    {
        string normalizedText = NormalizeRequiredText(alcoholSpecialText);

        if (string.IsNullOrEmpty(normalizedText))
        {
            return;
        }

        if (AlcoholSpecialText == normalizedText)
        {
            return;
        }

        AlcoholSpecialText = normalizedText;
    }

    private static string NormalizeRequiredText(string value)
    {
        string trimmed = value.Trim();
        return string.IsNullOrEmpty(trimmed) ? string.Empty : trimmed;
    }
}

