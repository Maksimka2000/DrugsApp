using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CreateProviderRequest;

internal static class CreateProviderRequestMapping
{
    public static CreateProviderRequestCommand MapToCommand(this CreateProviderRequestRequest request, Guid providerUserId) =>
        new(
            ProviderUserId: providerUserId,
            PatientEmail: request.PatientEmail);

    public static CreateProviderRequestResponseDto MapToResponse(this SupervisionRequest request)
    {
        return new CreateProviderRequestResponseDto(
            RequestId: request.Id,
            RequestState: request.RequestState.ToString(),
            ExpiresAtUtc: request.ExpiresAtUtc,
            RequestedAtUtc: request.RequestedAtUtc);
    }
}
