namespace DrugsApp.Infrastructure.ExternalServices.Email;

using System;

/// <summary>
/// Email provider configuration for both SMTP and Azure Communication Services.
/// </summary>
public sealed class EmailSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// Provider name for SMTP (Papercut/local).
    /// </summary>
    public const string SmtpProvider = "Smtp";

    /// <summary>
    /// Provider name for Azure Communication Services Email.
    /// </summary>
    public const string AzureCommunicationServicesProvider = "AzureCommunicationServices";

    /// <summary>
    /// <see cref="SmtpProvider"/> for Papercut/local SMTP, or
    /// <see cref="AzureCommunicationServicesProvider"/> for Azure Communication Services Email.
    /// </summary>
    public string Provider { get; init; } = SmtpProvider;

    /// <summary>
    /// Sender email address.
    /// </summary>
    public string SenderEmail { get; init; } = string.Empty;

    /// <summary>
    /// Sender display name.
    /// </summary>
    public string SenderName { get; init; } = string.Empty;

    /// <summary>
    /// SMTP host name.
    /// </summary>
    public string SmtpHost { get; init; } = string.Empty;

    /// <summary>
    /// SMTP port number.
    /// </summary>
    public int SmtpPort { get; init; } = 2525;

    /// <summary>
    /// Optional SMTP username.
    /// </summary>
    public string? SmtpUsername { get; init; }

    /// <summary>
    /// Optional SMTP password.
    /// </summary>
    public string? SmtpPassword { get; init; }

    /// <summary>
    /// Azure Communication Services connection string (production email).
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Base URL for client-side confirmation links.
    /// </summary>
    public string ClientBaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// True when Azure Communication Services is configured as the provider.
    /// </summary>
    public bool UsesAzureCommunicationServices =>
        string.Equals(Provider, AzureCommunicationServicesProvider, StringComparison.OrdinalIgnoreCase);
}

