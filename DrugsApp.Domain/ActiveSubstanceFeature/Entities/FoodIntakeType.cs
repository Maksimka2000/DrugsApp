namespace DrugsApp.Domain.ActiveSubstanceFeature.Entities;

/// <summary>
/// Describes how oral medication intake should relate to food consumption.
/// </summary>
public enum FoodIntakeType
{
    /// <summary>
    /// When the drug is used in sublingual oral administration way 
    /// </summary>
    Sublingual = 0,
    /// <summary>
    /// Drug is chewed 
    /// </summary>
    Chewed = 1,
    /// <summary>
    /// Drug must go on empty stomach
    /// </summary>
    EmptyStomach = 2,
    /// <summary>
    /// List of products
    /// </summary>
    EmptyStomachAvoidCertainProducts = 3,
    /// <summary>
    /// Administrate wiht meal
    /// </summary>
    WithMeal = 4,
    /// <summary>
    /// Administrate wiht meal but avoid list of products
    /// </summary>
    WithMealAvoidCertainProducts = 5,
    /// <summary>
    /// Administrate after the meal
    /// </summary>
    AfterMeal = 6
}


