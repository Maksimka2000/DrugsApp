namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblemType;

/// <summary>
/// Mapping helpers for creating drug problem types.
/// </summary>
public static class CreateDrugProblemTypeMapping
{
    public static CreateDrugProblemTypeCommand MapToCommand(this CreateDrugProblemTypeRequest request) =>
        new(request);
}
