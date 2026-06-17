using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentAssertions;

namespace DrugsApp.UnitTests.HealthcareSpecialistSupervisionFeature.SubmitProviderProposal.ApplicationServices;

public sealed class MedicalChangeProposalJsonTests
{
    [Fact]
    public void ValidateAddOnlyPayload_WhenUpdateKeyPresent_ReturnsValidationError()
    {
        const string payload = """{"update": {"drug_form_id": "00000000-0000-0000-0000-000000000001"}}""";

        ErrorOr<Success> result = MedicalChangeProposalJson.ValidateAddOnlyPayload(payload, "apply_payload");

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("apply_payload must describe an add-only operation.");
    }
}
