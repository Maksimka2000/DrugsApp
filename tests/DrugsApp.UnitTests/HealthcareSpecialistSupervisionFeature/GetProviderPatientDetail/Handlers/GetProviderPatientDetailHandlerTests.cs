using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ApplicationServices;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail.Handlers;

public sealed class GetProviderPatientDetailHandlerTests
{
    [Fact]
    public async Task Handle_WhenBlockAccess_ReturnsBlockedProfileWithoutMedicalData()
    {
        var providerUserId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();

        HealthcareProviderProfile providerProfile = HealthcareProviderProfile
            .Create(providerUserId, professionCode: null, otherProfessionLabel: null, workplaceLabel: null, shortDescription: null)
            .Value;

        SupervisionConnection connection = SupervisionConnection
            .CreateOrReactivate(patientUserId, providerUserId, SupervisionAccessLevel.BlockAccess, DateTime.UtcNow)
            .Value;

        var providerProfileRepository = new Mock<IHealthcareProviderProfileRepository>();
        providerProfileRepository
            .Setup(r => r.GetTrackedByUserIdAsync(providerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providerProfile);

        var connectionRepository = new Mock<ISupervisionConnectionRepository>();
        connectionRepository
            .Setup(r => r.GetActiveForProviderAndPatientAsync(
                providerUserId,
                patientUserId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        var userProfileRepository = new Mock<IUserProfileRepository>();
        var medicalProfileReadService = new Mock<IPatientMedicalProfileReadService>();

        var handler = new GetProviderPatientDetailHandler(
            providerProfileRepository.Object,
            connectionRepository.Object,
            userProfileRepository.Object,
            medicalProfileReadService.Object);

        ErrorOr<ProviderPatientDetailResponse> result = await handler.Handle(
            new GetProviderPatientDetailQuery(providerUserId, patientUserId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.AccessBlockedMessage.Should().Be("Access is blocked by the patient.");
        result.Value.PatientProfile.Should().BeNull();
        result.Value.RegularDrugs.Should().BeNull();
        medicalProfileReadService.Verify(
            s => s.LoadAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
