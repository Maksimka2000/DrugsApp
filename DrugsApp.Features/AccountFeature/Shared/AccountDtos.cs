namespace DrugsApp.Features.AccountFeature.Shared;

public sealed record RequestLoginEmailChangeResponse(
    string Email,
    string PendingEmail,
    bool VerificationRequired);

public sealed record ConfirmLoginEmailChangeResponse(
    string Email,
    bool EmailChangeConfirmed);
