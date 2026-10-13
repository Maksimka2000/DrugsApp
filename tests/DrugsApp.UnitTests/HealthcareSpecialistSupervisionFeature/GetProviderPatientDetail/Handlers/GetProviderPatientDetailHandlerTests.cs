using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ApplicationServices;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using DrugsApp.Features.HealthConditionFeature.Shared;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularFoodFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
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

    private static GetProviderPatientDetailHandler CreateHandler(
        Guid providerUserId,
        Guid patientUserId,
        SupervisionAccessLevel accessLevel,
        Mock<IPatientMedicalProfileReadService> medicalProfileReadService)
    {
        HealthcareProviderProfile providerProfile = HealthcareProviderProfile
            .Create(providerUserId, professionCode: null, otherProfessionLabel: null, workplaceLabel: null, shortDescription: null)
            .Value;
        SupervisionConnection connection = SupervisionConnection
            .CreateOrReactivate(patientUserId, providerUserId, accessLevel, DateTime.UtcNow)
            .Value;
        UserProfile patientProfile = UserProfile
            .Create(patientUserId, "anna@example.com", "Anna", "Kowalska", new DateOnly(1992, 3, 14), ProfileGender.female)
            .Value;

        var providerProfileRepository = new Mock<IHealthcareProviderProfileRepository>();
        providerProfileRepository
            .Setup(r => r.GetTrackedByUserIdAsync(providerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providerProfile);

        var connectionRepository = new Mock<ISupervisionConnectionRepository>();
        connectionRepository
            .Setup(r => r.GetActiveForProviderAndPatientAsync(providerUserId, patientUserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        var userProfileRepository = new Mock<IUserProfileRepository>();
        userProfileRepository
            .Setup(r => r.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patientProfile);

        return new GetProviderPatientDetailHandler(
            providerProfileRepository.Object,
            connectionRepository.Object,
            userProfileRepository.Object,
            medicalProfileReadService.Object);
    }

    [Fact]
    public async Task Handle_WhenRestrictedView_ReturnsProfileOnlyWithoutMedicalData()
    {
        var providerUserId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var medicalProfileReadService = new Mock<IPatientMedicalProfileReadService>();
        GetProviderPatientDetailHandler handler = CreateHandler(
            providerUserId, patientUserId, SupervisionAccessLevel.RestrictedView, medicalProfileReadService);

        ErrorOr<ProviderPatientDetailResponse> result = await handler.Handle(
            new GetProviderPatientDetailQuery(providerUserId, patientUserId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.PatientProfile.Should().NotBeNull();
        result.Value.PatientProfile!.FirstName.Should().Be("Anna");
        result.Value.DrugAllergies.Should().BeNull();
        result.Value.RegularDrugHistory.Should().BeNull();
        result.Value.MealTimes.Should().BeNull();
        medicalProfileReadService.Verify(
            s => s.LoadAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(SupervisionAccessLevel.ViewOnly)]
    [InlineData(SupervisionAccessLevel.ViewAndModify)]
    public async Task Handle_WhenViewAccess_ReturnsMemberShapedMedicalDataWithHistoryAndMealTimes(
        SupervisionAccessLevel accessLevel)
    {
        var providerUserId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var medicalProfileReadService = new Mock<IPatientMedicalProfileReadService>();
        medicalProfileReadService
            .Setup(s => s.LoadAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProviderPatientMedicalDetailData(
                ReproductiveHealth: new ProviderReproductiveHealthDetailDto(false, null, null, null, null, null),
                AllergySummary: null,
                HealthConditions: Array.Empty<UserHealthConditionDto>(),
                DrugProblems: Array.Empty<DrugProblemListItemDto>(),
                DrugAllergies: Array.Empty<ClientDrugAllergyDto>(),
                SupplementAllergies: Array.Empty<ClientSupplementAllergyDto>(),
                FoodAllergies: Array.Empty<ClientFoodAllergyDto>(),
                OtherAllergies: Array.Empty<ClientOtherAllergyDto>(),
                RegularDrugs: Array.Empty<RegularDrugListItemDto>(),
                RegularFoods: Array.Empty<RegularFoodListItemDto>(),
                RegularSupplements: Array.Empty<RegularSupplementListItemDto>(),
                RegularDrugHistory: new ProviderIntakeHistoryDto<RegularDrugHistoryItemDto>(
                    Array.Empty<RegularDrugHistoryItemDto>(), HasMore: true),
                RegularFoodHistory: new ProviderIntakeHistoryDto<RegularFoodHistoryListItemDto>(
                    Array.Empty<RegularFoodHistoryListItemDto>(), HasMore: false),
                RegularSupplementHistory: new ProviderIntakeHistoryDto<RegularSupplementHistoryItemDto>(
                    Array.Empty<RegularSupplementHistoryItemDto>(), HasMore: false),
                MealTimes: Array.Empty<MealTimeListItemDto>()));
        GetProviderPatientDetailHandler handler = CreateHandler(
            providerUserId, patientUserId, accessLevel, medicalProfileReadService);

        ErrorOr<ProviderPatientDetailResponse> result = await handler.Handle(
            new GetProviderPatientDetailQuery(providerUserId, patientUserId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.DrugAllergies.Should().NotBeNull();
        result.Value.RegularDrugHistory!.HasMore.Should().BeTrue();
        result.Value.MealTimes.Should().NotBeNull();
        result.Value.PatientUserId.Should().Be(patientUserId);
    }
}
