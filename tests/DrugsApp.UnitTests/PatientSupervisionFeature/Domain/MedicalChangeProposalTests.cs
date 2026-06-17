using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using ErrorOr;
using FluentAssertions;

namespace DrugsApp.UnitTests.PatientSupervisionFeature.Domain;

public sealed class MedicalChangeProposalTests
{
    [Fact]
    public void Create_WhenAccessLevelIsViewOnly_ReturnsValidationError()
    {
        ErrorOr<MedicalChangeProposal> result = MedicalChangeProposal.Create(
            connectionId: Guid.NewGuid(),
            patientUserId: Guid.NewGuid(),
            healthcareProfessionalUserId: Guid.NewGuid(),
            accessLevelSnapshot: SupervisionAccessLevel.ViewOnly,
            proposalSummary: "Add regular drug",
            submittedAtUtc: DateTime.UtcNow,
            items: []);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("Access level must allow modify.");
    }
}
