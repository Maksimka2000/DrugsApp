using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

public interface IIdentityAuthenticationService
{
    Task<ErrorOr<AuthRegistrationResult>> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<ErrorOr<AuthTokensResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<ErrorOr<AuthTokensResult>> RefreshAsync(
        string refreshToken,
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
