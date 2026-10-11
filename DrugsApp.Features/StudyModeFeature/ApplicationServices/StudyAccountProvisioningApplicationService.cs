using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.DomainServices;
using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using ErrorOr;

namespace DrugsApp.Features.StudyModeFeature.ApplicationServices;

/// <summary>
/// Orchestrates study account provisioning: clinic lookup and cap check, credential generation, Identity user
/// creation with login retry, completed user and provider profiles, password protection and clinic membership.
/// </summary>
internal sealed class StudyAccountProvisioningApplicationService(
    IStudyClinicRepository _studyClinicRepository,
    IStudyCredentialGenerator _studyCredentialGenerator,
    IStudyCredentialProtector _studyCredentialProtector,
    IIdentityUserManagementService _identityUserManagementService,
    IUserProfileApplicationService _userProfileApplicationService,
    IHealthcareProviderProfileApplicationService _healthcareProviderProfileApplicationService)
    : IStudyAccountProvisioningApplicationService
{
    private const int MAX_LOGIN_ATTEMPTS = 4;
    private const string EMAIL_ALREADY_EXISTS_CODE = "Auth.EmailAlreadyExists";

    /// <summary>
    /// Creates the Identity user, completed profiles and clinic membership of a new study account.
    /// </summary>
    /// <param name="clinicId">
    /// Target clinic identifier.
    /// </param>
    /// <param name="draft">
    /// Member kind, personal data, optional provider data and initial position.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Generated credentials and user identifier, or errors.
    /// </returns>
    public async Task<ErrorOr<StudyAccountProvisioningResult>> ProvisionAsync(
        Guid clinicId,
        StudyAccountProvisioningDraft draft,
        CancellationToken cancellationToken)
    {
        // locks the clinic row for the rest of the transaction so concurrent creations cannot exceed the cap
        StudyClinic? clinic = await _studyClinicRepository.GetByIdWithLockAsync(clinicId, cancellationToken);
        if (clinic is null)
        {
            return StudyModeErrors.ClinicNotFound(clinicId);
        }

        // fail fast on the cap before touching Identity (re-checked by AddMember)
        ErrorOr<Success> capacityResult = clinic.EnsureCapacityFor(draft.MemberKind);
        if (capacityResult.IsError)
        {
            return capacityResult.Errors;
        }

        bool isDoctor = draft.MemberKind == StudyMemberKind.Doctor;
        if (isDoctor && draft.ProviderData is null)
        {
            return Error.Validation(
                code: "StudyMode.ProviderDataRequired",
                description: "Study doctors require provider profile data.",
                metadata: new()
                {
                    ["field"] = "professionCode"
                });
        }

        string roleName = isDoctor ? AuthRoles.StudyDoctor : AuthRoles.StudyPatient;
        string password = _studyCredentialGenerator.GeneratePassword();

        ErrorOr<(Guid UserId, string LoginEmail)> accountResult = await CreateIdentityAccountAsync(
            draft,
            password,
            roleName,
            cancellationToken);
        if (accountResult.IsError)
        {
            return accountResult.Errors;
        }

        (Guid userId, string loginEmail) = accountResult.Value;

        ErrorOr<Success> profileResult = await _userProfileApplicationService.CreateCompletedProfileAsync(
            userId,
            loginEmail,
            draft.PersonalData,
            cancellationToken);
        if (profileResult.IsError)
        {
            return profileResult.Errors;
        }

        if (isDoctor)
        {
            ErrorOr<Success> providerResult = await _healthcareProviderProfileApplicationService.CreateCompletedAsync(
                userId,
                new HealthcareProviderProfileUpsertRequest(
                    draft.ProviderData!.ProfessionCode,
                    draft.ProviderData.OtherProfessionLabel,
                    draft.ProviderData.WorkplaceLabel,
                    draft.ProviderData.ShortDescription,
                    OnboardingCompletedAtUtc: null),
                cancellationToken);
            if (providerResult.IsError)
            {
                return providerResult.Errors;
            }
        }

        string protectedPassword = _studyCredentialProtector.Protect(password);

        ErrorOr<StudyClinicMember> memberResult = clinic.AddMember(
            userId,
            draft.MemberKind,
            loginEmail,
            protectedPassword,
            draft.PositionX,
            draft.PositionY);
        if (memberResult.IsError)
        {
            return memberResult.Errors;
        }

        return new StudyAccountProvisioningResult(userId, loginEmail, password);
    }

    private async Task<ErrorOr<(Guid UserId, string LoginEmail)>> CreateIdentityAccountAsync(
        StudyAccountProvisioningDraft draft,
        string password,
        string roleName,
        CancellationToken cancellationToken)
    {
        List<Error> lastErrors = [];
        for (int attempt = 1; attempt <= MAX_LOGIN_ATTEMPTS; attempt++)
        {
            string loginEmail = _studyCredentialGenerator.GenerateLoginEmail(
                draft.MemberKind,
                draft.PersonalData.FirstName,
                draft.PersonalData.LastName);

            ErrorOr<Guid> createResult = await _identityUserManagementService.CreateStudyAccountAsync(
                loginEmail,
                password,
                roleName,
                cancellationToken);
            if (!createResult.IsError)
            {
                return (createResult.Value, loginEmail);
            }

            bool isLoginCollision = createResult.FirstError.Code == EMAIL_ALREADY_EXISTS_CODE;
            if (!isLoginCollision)
            {
                return createResult.Errors;
            }

            lastErrors = createResult.Errors;
        }

        return lastErrors;
    }
}
