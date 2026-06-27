namespace DrugsApp.Features.AccountFeature.Shared;

/// <summary>
/// Response payload returned after initiating a login email change.
/// </summary>
public sealed record RequestLoginEmailChangeResponse
{
    /// <summary>
    /// Current confirmed email for the user.
    /// </summary>
    public string Email { get; init; }

    /// <summary>
    /// Pending email awaiting confirmation.
    /// </summary>
    public string PendingEmail { get; init; }

    /// <summary>
    /// Indicates whether additional verification is required.
    /// </summary>
    public bool VerificationRequired { get; init; }

    /// <summary>
    /// Creates a login email change response.
    /// </summary>
    /// <param name="Email">Current confirmed email.</param>
    /// <param name="PendingEmail">Pending email awaiting confirmation.</param>
    /// <param name="VerificationRequired">True when verification is still required.</param>
    public RequestLoginEmailChangeResponse(
        string Email,
        string PendingEmail,
        bool VerificationRequired)
    {
        this.Email = Email;
        this.PendingEmail = PendingEmail;
        this.VerificationRequired = VerificationRequired;
    }
}

/// <summary>
/// Response payload returned after confirming a login email change.
/// </summary>
public sealed record ConfirmLoginEmailChangeResponse
{
    /// <summary>
    /// Final confirmed login email.
    /// </summary>
    public string Email { get; init; }

    /// <summary>
    /// Indicates whether the pending email change succeeded.
    /// </summary>
    public bool EmailChangeConfirmed { get; init; }

    /// <summary>
    /// Creates an email change confirmation response.
    /// </summary>
    /// <param name="Email">Final confirmed login email.</param>
    /// <param name="EmailChangeConfirmed">True when the pending change is confirmed.</param>
    public ConfirmLoginEmailChangeResponse(string Email, bool EmailChangeConfirmed)
    {
        this.Email = Email;
        this.EmailChangeConfirmed = EmailChangeConfirmed;
    }
}
