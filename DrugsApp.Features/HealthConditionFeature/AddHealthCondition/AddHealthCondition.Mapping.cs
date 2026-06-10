namespace DrugsApp.Features.HealthConditionFeature.AddHealthCondition;

/// <summary>
/// Mapping helpers for add health condition use case.
/// </summary>
public static class AddHealthConditionMapping
{
    public static AddHealthConditionCommand MapToCommand(this AddHealthConditionRequest request) =>
        new(request);
}
