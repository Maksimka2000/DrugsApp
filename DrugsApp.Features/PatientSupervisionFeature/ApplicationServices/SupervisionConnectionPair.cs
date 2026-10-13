namespace DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;

/// <summary>
/// Desired active supervision edge between a provider (or study doctor) and a patient.
/// </summary>
/// <param name="ProviderUserId">Provider or study doctor user identifier.</param>
/// <param name="PatientUserId">Patient user identifier.</param>
public sealed record SupervisionConnectionPair(
    Guid ProviderUserId,
    Guid PatientUserId);
