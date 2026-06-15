namespace DrugsApp.Features.PatientSupervisionFeature.Shared;

/// <summary>
/// Apply payload snapshot for provider-proposed custom regular supplements (free-text name).
/// </summary>
public sealed record ProposedCustomRegularSupplementApplyPayload(
    string CustomSupplementName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);
