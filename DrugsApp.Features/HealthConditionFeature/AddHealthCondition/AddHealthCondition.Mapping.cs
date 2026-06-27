namespace DrugsApp.Features.HealthConditionFeature.AddHealthCondition;

/// <summary>
/// Mapping helpers for add health condition use case.
/// </summary>
public static class AddHealthConditionMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Add health condition request payload.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static AddHealthConditionCommand MapToCommand(this AddHealthConditionRequest request) =>
        new(request);
}
