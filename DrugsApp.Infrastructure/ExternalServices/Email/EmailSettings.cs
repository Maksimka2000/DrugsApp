namespace DrugsApp.Infrastructure.ExternalServices.Email;

using System;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public const string SmtpProvider = "Smtp";

    public const string AzureCommunicationServicesProvider = "AzureCommunicationServices";

    /// <summary>
    /// <see cref="SmtpProvider"/> for Papercut/local SMTP, or
    /// <see cref="AzureCommunicationServicesProvider"/> for Azure Communication Services Email.
    /// </summary>
    public string Provider { get; init; } = SmtpProvider;

    public string SenderEmail { get; init; } = string.Empty;

    public string SenderName { get; init; } = string.Empty;

    public string SmtpHost { get; init; } = string.Empty;

    public int SmtpPort { get; init; } = 2525;

    public string? SmtpUsername { get; init; }

    public string? SmtpPassword { get; init; }

    /// <summary>
    /// Azure Communication Services connection string (production email).
    /// </summary>
    public string? ConnectionString { get; init; }

    public string ClientBaseUrl { get; init; } = string.Empty;

    public bool UsesAzureCommunicationServices =>
        string.Equals(Provider, AzureCommunicationServicesProvider, StringComparison.OrdinalIgnoreCase);
}

