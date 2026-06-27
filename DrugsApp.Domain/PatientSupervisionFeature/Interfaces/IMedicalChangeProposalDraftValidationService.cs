using System.Threading.Tasks;
using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Interfaces;

/// <summary>
/// Validates provider draft items before persistence.
/// </summary>
public interface IMedicalChangeProposalDraftValidationService
{
    /// <summary>
    /// Validates draft items for a patient before persisting a proposal.
    /// </summary>
    /// <param name="patientUserId">Patient owner for whom the draft is being created.</param>
    /// <param name="draftItems">Collection of draft items to validate.</param>
    /// <param name="cancellationToken">Propagation token to cancel validation.</param>
    /// <returns>Success when valid; an error when validation fails.</returns>
    Task<ErrorOr<Success>> ValidateAsync(
        Guid patientUserId,
        IReadOnlyCollection<ProposalDraftItem> draftItems,
        CancellationToken cancellationToken);
}
