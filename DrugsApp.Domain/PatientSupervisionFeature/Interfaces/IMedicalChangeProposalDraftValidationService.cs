using System.Threading.Tasks;
using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Interfaces;

/// <summary>
/// Validates provider draft items before persistence.
/// </summary>
public interface IMedicalChangeProposalDraftValidationService
{
    Task<ErrorOr<Success>> ValidateAsync(
        Guid patientUserId,
        IReadOnlyCollection<ProposalDraftItem> draftItems,
        CancellationToken cancellationToken);
}
