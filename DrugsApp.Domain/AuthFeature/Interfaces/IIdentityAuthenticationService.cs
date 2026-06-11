using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

public interface IIdentityAuthenticationService
{
    Task<ErrorOr<AuthRegistrationResult>> RegisterAsync(
        string email,
        string password,
        UserProfilePersonalData personalData,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> AssignHealthcareProviderRoleAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> RemoveHealthcareProviderRoleAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<ErrorOr<AuthTokensResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<ErrorOr<AuthTokensResult>> LoginAdminAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<ErrorOr<AuthTokensResult>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<ErrorOr<RequestLoginEmailChangeResult>> RequestEmailChangeAsync(
        Guid userId,
        string newEmail,
        string currentPassword,
        CancellationToken cancellationToken);

    Task<ErrorOr<ConfirmLoginEmailChangeResult>> ConfirmEmailChangeAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> DeleteAccountAsync(
        Guid userId,
        string currentPassword,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> TeardownClientAccountAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken);

    Task<ErrorOr<AuthRegistrationResult?>> ResendConfirmationEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken);
}

public sealed record AuthRegistrationResult(
    Guid UserId,
    string Email);

public sealed record AuthTokensResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds);

public sealed record UserProfilePersonalData(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    ProfileGender Gender);

public sealed record RequestLoginEmailChangeResult(
    string Email,
    string PendingEmail,
    bool VerificationRequired);

public sealed record ConfirmLoginEmailChangeResult(
    string Email,
    bool EmailChangeConfirmed);
