using System.Collections.Concurrent;
using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.IntegrationTests.Common;

public enum CapturedEmailKind
{
    RegistrationConfirmation,
    PasswordReset,
    EmailChange
}

public sealed record CapturedEmailMessage(
    Guid UserId,
    string Email,
    string Token,
    CapturedEmailKind Kind);

public sealed class CapturingEmailVerificationSender : IEmailVerificationSender
{
    private readonly ConcurrentQueue<CapturedEmailMessage> _messages = new();

    public IReadOnlyCollection<CapturedEmailMessage> Messages => _messages.ToArray();

    public Task SendConfirmationAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        _messages.Enqueue(new CapturedEmailMessage(userId, email, confirmationToken, CapturedEmailKind.RegistrationConfirmation));
        return Task.CompletedTask;
    }

    public Task SendEmailChangeConfirmationAsync(
        Guid userId,
        string newEmail,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        _messages.Enqueue(new CapturedEmailMessage(userId, newEmail, confirmationToken, CapturedEmailKind.EmailChange));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        _messages.Enqueue(new CapturedEmailMessage(userId, email, confirmationToken, CapturedEmailKind.PasswordReset));
        return Task.CompletedTask;
    }

    public string? GetLatestToken(Guid userId, CapturedEmailKind kind) =>
        _messages
            .Where(message => message.UserId == userId && message.Kind == kind)
            .LastOrDefault()
            ?.Token;

    public void Clear() => _messages.Clear();
}
