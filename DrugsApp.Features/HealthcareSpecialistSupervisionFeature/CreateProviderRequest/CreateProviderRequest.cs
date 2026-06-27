using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CreateProviderRequest;

/// <summary>
/// Request payload for creating a provider supervision request to a patient.
/// </summary>
/// <param name="PatientEmail">Email of the patient to invite.</param>
public sealed record CreateProviderRequestRequest(string PatientEmail);

/// <summary>
/// Command to create a provider supervision request.
/// </summary>
/// <param name="ProviderUserId">Provider user identifier.</param>
/// <param name="PatientEmail">Target patient email.</param>
public sealed record CreateProviderRequestCommand(Guid ProviderUserId, string PatientEmail)
    : IRequest<ErrorOr<CreateProviderRequestResponseDto>>;

internal sealed class CreateProviderRequestHandler(
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    IUserProfileRepository userProfileRepository,
    IUserRoleReadService userRoleReadService,
    ISupervisionRequestRepository supervisionRequestRepository,
    ISupervisionConnectionRepository supervisionConnectionRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<CreateProviderRequestCommand, ErrorOr<CreateProviderRequestResponseDto>>
{
    public async Task<ErrorOr<CreateProviderRequestResponseDto>> Handle(
        CreateProviderRequestCommand command,
        CancellationToken cancellationToken)
    {
        HealthcareProviderProfile? providerProfile = await healthcareProviderProfileRepository.GetTrackedByUserIdAsync(
            command.ProviderUserId,
            cancellationToken);

        if (providerProfile is null)
        {
            return Error.Forbidden(description: "Provider profile required.");
        }

        string normalizedEmail = command.PatientEmail.Trim().ToLowerInvariant();

        UserProfile? patientProfile = await userProfileRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (patientProfile is null)
        {
            return Error.NotFound(description: "Patient email not found.");
        }

        bool isClient = await userRoleReadService.IsInRoleAsync(patientProfile.Id, AuthRoles.Client, cancellationToken);
        if (!isClient)
        {
            return Error.Forbidden(description: "Target user is not a client.");
        }

        if (patientProfile.Id == command.ProviderUserId)
        {
            return Error.Validation(description: "Cannot request supervision with self.");
        }

        DateTime nowUtc = DateTime.UtcNow;

        bool hasPending = await supervisionRequestRepository.ExistsPendingForPairAsync(patientProfile.Id, command.ProviderUserId, cancellationToken);
        bool hasActive = await supervisionConnectionRepository.ExistsActiveForPairAsync(patientProfile.Id, command.ProviderUserId, cancellationToken);
        if (hasPending || hasActive)
        {
            return Error.Conflict(description: "Pending request or active connection already exists.");
        }

        ErrorOr<SupervisionRequest> createResult = SupervisionRequest.Create(patientProfile.Id, command.ProviderUserId, nowUtc);
        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        SupervisionRequest request = createResult.Value;
        await supervisionRequestRepository.AddAsync(request, cancellationToken);

        UserProfile? providerUserProfile = await userProfileRepository.GetByUserIdAsync(command.ProviderUserId, cancellationToken);
        string providerSummary = providerUserProfile is null
            ? "A healthcare provider"
            : $"{providerUserProfile.FirstName} {providerUserProfile.LastName}".Trim();

        ErrorOr<Success> projectionResult = await notificationProjectionService.ProjectAsync(
            SupervisionNotificationMappings.BuildRequestCreatedDraft(
                patientProfile.Id,
                request.Id,
                providerSummary),
            cancellationToken);
        if (projectionResult.IsError)
        {
            return projectionResult.Errors;
        }

        return request.MapToResponse();
    }
}
