namespace DrugsApp.Features.MealTimeManagementFeature.ApplicationServices;

/// <summary>
/// Normalizes meal names for duplicate checks and persistence.
/// </summary>
public interface IClientMealTimeNormalizationService
{
    string NormalizeMealName(string mealName);
}
