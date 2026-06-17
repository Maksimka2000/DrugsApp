using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using ErrorOr;
using FluentAssertions;

namespace DrugsApp.UnitTests.PatientSupervisionFeature.Domain;

public sealed class SupervisionRequestTests
{
    [Fact]
    public void Approve_WhenNotPending_ReturnsConflict()
    {
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        DateTime requestedAt = DateTime.UtcNow.AddDays(-1);

        SupervisionRequest request = SupervisionRequest
            .Create(patientId, providerId, requestedAt)
            .Value;

        request.Approve(requestedAt.AddHours(1), patientId).IsError.Should().BeFalse();

        ErrorOr<Success> secondApprove = request.Approve(requestedAt.AddHours(2), patientId);

        secondApprove.IsError.Should().BeTrue();
        secondApprove.FirstError.Type.Should().Be(ErrorType.Conflict);
        secondApprove.FirstError.Description.Should().Be("Request already resolved.");
    }

    [Fact]
    public void Approve_WhenExpired_ReturnsValidationError()
    {
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        DateTime requestedAt = DateTime.UtcNow.AddDays(-10);

        SupervisionRequest request = SupervisionRequest
            .Create(patientId, providerId, requestedAt)
            .Value;

        ErrorOr<Success> result = request.Approve(DateTime.UtcNow, patientId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("Request expired.");
    }
}
