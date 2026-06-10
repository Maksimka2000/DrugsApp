using System;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblemType;

/// <summary>
/// Mapping helpers for updating drug problem types.
/// </summary>
public static class UpdateDrugProblemTypeMapping
{
    public static UpdateDrugProblemTypeCommand MapToCommand(this UpdateDrugProblemTypeRequest request, Guid id) =>
        new(id, request);
}
