namespace DrugsApp.Infrastructure.Services.Seed;

/// <summary>
/// Configuration for idempotent development patient profile seeding.
/// </summary>
public sealed class DevSeedSettings
{
    public const string SectionName = "DevSeed";

    /// <summary>
    /// When false, seeding is skipped entirely.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Unique key stored in <c>dev_seed_applications</c> to prevent re-seeding.
    /// Bump when seed data shape changes and you need a fresh apply on existing databases.
    /// </summary>
    public string SeedKey { get; init; } = "patient_profile_v2";

    public string PatientEmail { get; init; } = "seed.patient@drugsapp.local";

    public string PatientPassword { get; init; } = "SeedPatient123!";

    public string PatientFirstName { get; init; } = "Seed";

    public string PatientLastName { get; init; } = "Patient";

    public string DateOfBirth { get; init; } = "1990-05-15";

    public string Gender { get; init; } = "female";
}
