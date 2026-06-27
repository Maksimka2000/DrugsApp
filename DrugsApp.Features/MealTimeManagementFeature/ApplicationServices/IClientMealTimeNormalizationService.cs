namespace DrugsApp.Features.MealTimeManagementFeature.ApplicationServices;

/// <summary>
/// Normalizes meal names for duplicate checks and persistence.
/// </summary>
public interface IClientMealTimeNormalizationService
{
    /// <summary>
    /// Trims and upper-invariant normalizes a meal name.
    /// </summary>
    /// <param name="mealName">Meal name to normalize.</param>
    /// <returns>Normalized meal name.</returns>
    string NormalizeMealName(string mealName);
}
