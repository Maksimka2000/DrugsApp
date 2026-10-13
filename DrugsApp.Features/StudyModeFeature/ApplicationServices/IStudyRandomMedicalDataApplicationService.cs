using DrugsApp.Domain.StudyModeFeature.Enums;
using ErrorOr;

namespace DrugsApp.Features.StudyModeFeature.ApplicationServices;

/// <summary>
/// Fills a study patient's medical section with random, realistic sample data by reusing the existing client
/// create commands, so every duplicate check, eligibility rule and per-user limit applies unchanged.
/// </summary>
public interface IStudyRandomMedicalDataApplicationService
{
    /// <summary>
    /// Adds up to <paramref name="count"/> random items to the given section of the study patient.
    /// </summary>
    /// <param name="patientUserId">
    /// Study patient user identifier (acting user of every created row).
    /// </param>
    /// <param name="section">
    /// Medical section to fill.
    /// </param>
    /// <param name="count">
    /// Requested number of items (1–10).
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Generation outcome, or the first unexpected error raised by a client create command.
    /// </returns>
    Task<ErrorOr<StudyRandomDataResult>> GenerateAsync(
        Guid patientUserId,
        StudyMedicalSection section,
        int count,
        CancellationToken cancellationToken);
}

/// <summary>
/// Outcome of a random data generation run.
/// </summary>
/// <param name="Section">Filled section.</param>
/// <param name="RequestedCount">Requested number of items.</param>
/// <param name="CreatedCount">Number of items actually created.</param>
/// <param name="LimitReached">Whether the per-user limit stopped the run.</param>
/// <param name="LimitErrors">The limit conflict returned by the client handler, when it stopped the run.</param>
public sealed record StudyRandomDataResult(
    StudyMedicalSection Section,
    int RequestedCount,
    int CreatedCount,
    bool LimitReached,
    List<Error>? LimitErrors);
