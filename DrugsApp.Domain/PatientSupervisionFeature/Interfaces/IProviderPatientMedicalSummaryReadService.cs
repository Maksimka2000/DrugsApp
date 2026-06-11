using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;

namespace DrugsApp.Domain.PatientSupervisionFeature.Interfaces;

/// <summary>
/// Loads read-only medical summaries for provider patient detail views.
/// </summary>
public interface IProviderPatientMedicalSummaryReadService
{
    Task<ProviderPatientMedicalSummaries> LoadSummariesAsync(
        Guid patientUserId,
        CancellationToken cancellationToken);
}
