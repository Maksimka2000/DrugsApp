namespace DrugsApp.Infrastructure.ExternalServices.Email;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public string SenderEmail { get; init; } = string.Empty;

    public string SenderName { get; init; } = string.Empty;

    public string SmtpHost { get; init; } = string.Empty;

    public int SmtpPort { get; init; } = 2525;

    public string? SmtpUsername { get; init; }

    public string? SmtpPassword { get; init; }

    public string ClientBaseUrl { get; init; } = string.Empty;
}

