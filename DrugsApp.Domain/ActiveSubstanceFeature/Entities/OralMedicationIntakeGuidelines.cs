using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.ActiveSubstanceFeature.Entities;

/// <summary>
/// Represents oral medication intake guidelines associated with an active substance.
/// </summary>
public sealed class OralMedicationIntakeGuidelines : BaseEntity
{
    private OralMedicationIntakeGuidelines() : base()
    {
    }

    /// <summary>
    /// Foreign key identifying the owning <see cref="ActiveSubstance"/>.
    /// </summary>
    public Guid ActiveSubstanceId { get; private set; }

    /// <summary>
    /// Navigation to the owning active substance.
    /// </summary>
    public ActiveSubstance ActiveSubstance { get; private set; } = null!;


    /// <summary>
    /// Food-intake relation type for oral medication.
    /// </summary>
    public FoodIntakeType FoodIntakeType { get; private set; }

    /// <summary>
    /// Wait time in minutes before meal (optional).
    /// </summary>
    public int? WaitBeforeMinutes { get; private set; }

    /// <summary>
    /// Wait time in minutes after meal (optional).
    /// </summary>
    public int? WaitAfterMinutes { get; private set; }

    /// <summary>
    /// Free-text meal notes (optional).
    /// </summary>
    public string? MealNotes { get; private set; }

    /// <summary>
    /// Foods to avoid when taking the medication (optional).
    /// </summary>
    public string? AvoidFoodDrug { get; private set; }

    /// <summary>
    /// Foods to avoid during the whole therapy (optional).
    /// </summary>
    public string? AvoidFoodTherapy { get; private set; }

    /// <summary>
    /// Free-text liquid notes (optional).
    /// </summary>
    public string? LiquidNotes { get; private set; }

    /// <summary>
    /// Whether liquid-specific instructions are required.
    /// </summary>
    public bool? LiquidInstructions { get; private set; }

    /// <summary>
    /// Whether intake is recommended in the morning.
    /// </summary>
    public bool? RecommendedMorning { get; private set; }

    /// <summary>
    /// Whether intake is recommended in the evening.
    /// </summary>
    public bool? RecommendedEvening { get; private set; }

    /// <summary>
    /// Creates a new guidelines entity for the specified active substance.
    /// </summary>
    internal static OralMedicationIntakeGuidelines Create(
        Guid activeSubstanceId,
        FoodIntakeType foodIntakeType,
        int? waitBeforeMinutes,
        int? waitAfterMinutes,
        string? mealNotes,
        string? avoidFoodDrug,
        string? avoidFoodTherapy,
        string? liquidNotes,
        bool? liquidInstructions,
        bool? recommendedMorning,
        bool? recommendedEvening) => new()
        {
            ActiveSubstanceId = activeSubstanceId,
            FoodIntakeType = foodIntakeType,
            WaitBeforeMinutes = waitBeforeMinutes,
            WaitAfterMinutes = waitAfterMinutes,
            MealNotes = NormalizeOptionalText(mealNotes),
            AvoidFoodDrug = NormalizeOptionalText(avoidFoodDrug),
            AvoidFoodTherapy = NormalizeOptionalText(avoidFoodTherapy),
            LiquidNotes = NormalizeOptionalText(liquidNotes),
            LiquidInstructions = liquidInstructions,
            RecommendedMorning = recommendedMorning,
            RecommendedEvening = recommendedEvening
        };

    /// <summary>
    /// Updates the food intake type.
    /// </summary>
    public void UpdateFoodIntakeType(FoodIntakeType foodIntakeType)
    {
        FoodIntakeType = foodIntakeType;
    }

    /// <summary>
    /// Sets wait time in minutes before meal.
    /// </summary>
    public void SetWaitBeforeMinutes(int? waitBeforeMinutes)
    {
        WaitBeforeMinutes = waitBeforeMinutes;
    }

    /// <summary>
    /// Sets wait time in minutes after meal.
    /// </summary>
    public void SetWaitAfterMinutes(int? waitAfterMinutes)
    {
        WaitAfterMinutes = waitAfterMinutes;
    }

    /// <summary>
    /// Sets free-text meal notes.
    /// </summary>
    public void SetMealNotes(string? mealNotes)
    {
        MealNotes = NormalizeOptionalText(mealNotes);
    }

    /// <summary>
    /// Sets foods to avoid when taking the medication.
    /// </summary>
    public void SetAvoidFoodDrug(string? avoidFoodDrug)
    {
        AvoidFoodDrug = NormalizeOptionalText(avoidFoodDrug);
    }

    /// <summary>
    /// Sets foods to avoid during the whole therapy.
    /// </summary>
    public void SetAvoidFoodTherapy(string? avoidFoodTherapy)
    {
        AvoidFoodTherapy = NormalizeOptionalText(avoidFoodTherapy);
    }

    /// <summary>
    /// Sets free-text liquid notes.
    /// </summary>
    public void SetLiquidNotes(string? liquidNotes)
    {
        LiquidNotes = NormalizeOptionalText(liquidNotes);
    }

    /// <summary>
    /// Sets whether liquid-specific instructions are required.
    /// </summary>
    public void SetLiquidInstructions(bool? liquidInstructions)
    {
        LiquidInstructions = liquidInstructions;
    }

    /// <summary>
    /// Sets whether intake is recommended in the morning.
    /// </summary>
    public void SetRecommendedMorning(bool? recommendedMorning)
    {
        RecommendedMorning = recommendedMorning;
    }

    /// <summary>
    /// Sets whether intake is recommended in the evening.
    /// </summary>
    public void SetRecommendedEvening(bool? recommendedEvening)
    {
        RecommendedEvening = recommendedEvening;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (value is null)
        {
            return null;
        }

        string trimmedValue = value.Trim();
        return trimmedValue.Length == 0 ? null : trimmedValue;
    }
}


