namespace DrugsApp.Features.MealTimeManagementFeature.ApplicationServices;

/// <summary>
/// Trims and upper-invariant normalizes meal names for duplicate checks.
/// </summary>
public sealed class ClientMealTimeNormalizationService : IClientMealTimeNormalizationService
{
    /// <inheritdoc />
    public string NormalizeMealName(string mealName) =>
        mealName.Trim().ToUpperInvariant();
}
