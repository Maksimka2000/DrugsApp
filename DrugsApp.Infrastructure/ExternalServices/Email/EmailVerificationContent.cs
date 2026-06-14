using System;

namespace DrugsApp.Infrastructure.ExternalServices.Email;

internal static class EmailVerificationContent
{
    public static (string Subject, string HtmlBody, string ConfirmUrl) BuildRegistrationConfirmation(
        EmailSettings settings,
        Guid userId,
        string confirmationToken)
    {
        string confirmUrl = BuildClientUrl(
            settings.ClientBaseUrl,
            $"/auth/confirm-email?userId={userId:D}&token={Uri.EscapeDataString(confirmationToken)}");

        string htmlBody =
            $"""
             <p>To verify your email address, click this link:</p>
             <p><a href="{confirmUrl}">Confirm email</a></p>
             """;

        return ("Confirm your email", htmlBody, confirmUrl);
    }

    public static (string Subject, string HtmlBody, string ConfirmUrl) BuildEmailChangeConfirmation(
        EmailSettings settings,
        Guid userId,
        string confirmationToken)
    {
        string confirmUrl = BuildClientUrl(
            settings.ClientBaseUrl,
            $"/account/confirm-email-change?userId={userId:D}&token={Uri.EscapeDataString(confirmationToken)}");

        string htmlBody =
            $"""
             <p>To confirm your new login email, click this link:</p>
             <p><a href="{confirmUrl}">Confirm email change</a></p>
             """;

        return ("Confirm your new email", htmlBody, confirmUrl);
    }

    public static (string Subject, string HtmlBody, string ResetUrl) BuildPasswordReset(
        EmailSettings settings,
        Guid userId,
        string resetToken)
    {
        string resetUrl = BuildClientUrl(
            settings.ClientBaseUrl,
            $"/auth/reset-password?userId={userId:D}&token={Uri.EscapeDataString(resetToken)}");

        string htmlBody =
            $"""
             <p>To reset your password, click this link:</p>
             <p><a href="{resetUrl}">Reset password</a></p>
             """;

        return ("Reset your password", htmlBody, resetUrl);
    }

    private static string BuildClientUrl(string clientBaseUrl, string pathAndQuery)
    {
        string baseUrl = clientBaseUrl.TrimEnd('/');
        return pathAndQuery.StartsWith('/') ? $"{baseUrl}{pathAndQuery}" : $"{baseUrl}/{pathAndQuery}";
    }
}
