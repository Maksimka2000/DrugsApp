namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblemType;

/// <summary>
/// Mapping helpers for creating drug problem types.
/// </summary>
public static class CreateDrugProblemTypeMapping
{
    /// <summary>
    /// Creates a command to add a new drug problem type.
    /// </summary>
    /// <param name="request">Request payload describing the new type.</param>
    /// <returns>Command carrying the creation payload.</returns>
    public static CreateDrugProblemTypeCommand MapToCommand(this CreateDrugProblemTypeRequest request) =>
        new(request);
}
