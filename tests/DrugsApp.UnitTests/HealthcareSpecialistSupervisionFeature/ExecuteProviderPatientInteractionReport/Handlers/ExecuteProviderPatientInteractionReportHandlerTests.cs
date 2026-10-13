using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionReport;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionReport.Handlers;

public sealed class ExecuteProviderPatientInteractionReportHandlerTests
{
    private static readonly ProviderPatientInteractionReportRequest Request = new(
        ShowAllDrugsInteractions: true,
        ShowAllFoodAndSupplementInteractions: true,
        ShowOralGuidance: false);

    private readonly Guid _providerUserId = Guid.NewGuid();
    private readonly Guid _patientUserId = Guid.NewGuid();
    private readonly Mock<IHealthcareProviderProfileRepository> _providerProfileRepository = new();
    private readonly Mock<ISupervisionConnectionRepository> _connectionRepository = new();
    private readonly Mock<IDrugInteractionToolReportApplicationService> _reportService = new();

    private ExecuteProviderPatientInteractionReportHandler CreateHandler() => new(
        _providerProfileRepository.Object,
        _connectionRepository.Object,
        _reportService.Object);

    private void SetupProviderProfile(bool exists)
    {
        HealthcareProviderProfile? profile = exists
            ? HealthcareProviderProfile
                .Create(_providerUserId, professionCode: null, otherProfessionLabel: null, workplaceLabel: null, shortDescription: null)
                .Value
            : null;

        _providerProfileRepository
            .Setup(r => r.GetTrackedByUserIdAsync(_providerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
    }

    private void SetupConnection(SupervisionAccessLevel? accessLevel)
    {
        SupervisionConnection? connection = accessLevel is null
            ? null
            : SupervisionConnection
                .CreateOrReactivate(_patientUserId, _providerUserId, accessLevel.Value, DateTime.UtcNow)
                .Value;

        _connectionRepository
            .Setup(r => r.GetActiveForProviderAndPatientAsync(
                _providerUserId,
                _patientUserId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);
    }

    private Task<ErrorOr<ProviderPatientInteractionReportResponse>> Run() =>
        CreateHandler().Handle(
            new ExecuteProviderPatientInteractionReportCommand(_providerUserId, _patientUserId, Request),
            CancellationToken.None);

    private void VerifyReportNeverComputed() =>
        _reportService.Verify(
            s => s.ComputeRegisteredReportAsync(
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<ReportAudience>(),
                It.IsAny<bool>()),
            Times.Never);

    [Fact]
    public async Task Handle_WhenCallerHasNoProviderProfile_ReturnsForbidden()
    {
        SetupProviderProfile(exists: false);

        ErrorOr<ProviderPatientInteractionReportResponse> result = await Run();

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        VerifyReportNeverComputed();
    }

    [Fact]
    public async Task Handle_WhenNoActiveConnection_ReturnsNotFound()
    {
        SetupProviderProfile(exists: true);
        SetupConnection(accessLevel: null);

        ErrorOr<ProviderPatientInteractionReportResponse> result = await Run();

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        VerifyReportNeverComputed();
    }

    [Theory]
    [InlineData(SupervisionAccessLevel.BlockAccess)]
    [InlineData(SupervisionAccessLevel.RestrictedView)]
    public async Task Handle_WhenAccessLevelDoesNotAllowReading_ReturnsForbidden(SupervisionAccessLevel accessLevel)
    {
        SetupProviderProfile(exists: true);
        SetupConnection(accessLevel);

        ErrorOr<ProviderPatientInteractionReportResponse> result = await Run();

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        VerifyReportNeverComputed();
    }

    [Theory]
    [InlineData(SupervisionAccessLevel.ViewOnly)]
    [InlineData(SupervisionAccessLevel.ViewAndModify)]
    public async Task Handle_WhenAccessLevelAllowsReading_ComputesReportForPatientNotProvider(
        SupervisionAccessLevel accessLevel)
    {
        SetupProviderProfile(exists: true);
        SetupConnection(accessLevel);

        _reportService
            .Setup(s => s.ComputeRegisteredReportAsync(
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<ReportAudience>(),
                It.IsAny<bool>()))
            .ReturnsAsync(Error.Conflict(description: "report stub"));

        ErrorOr<ProviderPatientInteractionReportResponse> result = await Run();

        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("report stub");
        _reportService.Verify(
            s => s.ComputeRegisteredReportAsync(
                _patientUserId,
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 0),
                true,
                true,
                false,
                true,
                It.IsAny<CancellationToken>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                ReportAudience.Provider,
                true),
            Times.Once);
        _reportService.Verify(
            s => s.ComputeRegisteredReportAsync(
                _providerUserId,
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<ReportAudience>(),
                It.IsAny<bool>()),
            Times.Never);
    }
}
