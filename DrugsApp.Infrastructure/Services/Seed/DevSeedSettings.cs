namespace DrugsApp.Infrastructure.Services.Seed;

/// <summary>
/// Configuration for idempotent development patient profile seeding.
/// </summary>
public sealed class DevSeedSettings
{
    /// <summary>
    /// Configuration section path for dev seed settings.
    /// </summary>
    public const string SectionName = "DevSeed";

    /// <summary>
    /// When false, seeding is skipped entirely.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Unique key stored in <c>dev_seed_applications</c> to prevent re-seeding.
    /// Bump when seed data shape changes and you need a fresh apply on existing databases.
    /// </summary>
    public string SeedKey { get; init; } = "dev_data_v4";

    /// <summary>
    /// Seed patient email.
    /// </summary>
    public string PatientEmail { get; init; } = "seed.client1@drugsapp.local";

    /// <summary>
    /// Seed patient password.
    /// </summary>
    public string PatientPassword { get; init; } = "SeedPatient123!";

    /// <summary>
    /// Seed patient first name.
    /// </summary>
    public string PatientFirstName { get; init; } = "Seed";

    /// <summary>
    /// Seed patient last name.
    /// </summary>
    public string PatientLastName { get; init; } = "Patient";

    /// <summary>
    /// Seed patient birth date (ISO).
    /// </summary>
    public string DateOfBirth { get; init; } = "1990-05-15";

    /// <summary>
    /// Seed patient gender.
    /// </summary>
    public string Gender { get; init; } = "female";
}
