using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Infrastructure.Services.Seed;

/// <summary>
/// Fixed dev seed users referenced by <c>seed.sql</c> via stable UUIDs.
/// </summary>
internal static class DevSeedUserDefinitions
{
    internal sealed record SeedUserDefinition(
        Guid Id,
        string Email,
        string Password,
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        ProfileGender Gender,
        IReadOnlyList<string> Roles,
        HealthcareProviderProfession? ProviderProfession,
        string? ProviderWorkplace,
        string? ProviderDescription);

    internal static readonly Guid Client1Id = Guid.Parse("b1010000-0000-4000-8000-000000000001");
    internal static readonly Guid Client2Id = Guid.Parse("b1010000-0000-4000-8000-000000000002");
    internal static readonly Guid Client3Id = Guid.Parse("b1010000-0000-4000-8000-000000000003");
    internal static readonly Guid Client4Id = Guid.Parse("b1010000-0000-4000-8000-000000000004");
    internal static readonly Guid Client5Id = Guid.Parse("b1010000-0000-4000-8000-000000000005");

    internal static readonly Guid Provider1Id = Guid.Parse("b2010000-0000-4000-8000-000000000001");
    internal static readonly Guid Provider2Id = Guid.Parse("b2010000-0000-4000-8000-000000000002");
    internal static readonly Guid Provider3Id = Guid.Parse("b2010000-0000-4000-8000-000000000003");

    internal static IReadOnlyList<SeedUserDefinition> All { get; } =
    [
        new(
            Client1Id,
            "seed.client1@drugsapp.local",
            "Client1Seed123!",
            "Anna",
            "Kowalska",
            new DateOnly(1992, 3, 14),
            ProfileGender.female,
            [AuthRoles.Client],
            null,
            null,
            null),
        new(
            Client2Id,
            "seed.client2@drugsapp.local",
            "Client2Seed123!",
            "Piotr",
            "Nowak",
            new DateOnly(1988, 7, 22),
            ProfileGender.male,
            [AuthRoles.Client],
            null,
            null,
            null),
        new(
            Client3Id,
            "seed.client3@drugsapp.local",
            "Client3Seed123!",
            "Maria",
            "Wiśniewska",
            new DateOnly(1995, 11, 5),
            ProfileGender.female,
            [AuthRoles.Client],
            null,
            null,
            null),
        new(
            Client4Id,
            "seed.client4@drugsapp.local",
            "Client4Seed123!",
            "Tomasz",
            "Lewandowski",
            new DateOnly(1981, 1, 30),
            ProfileGender.male,
            [AuthRoles.Client],
            null,
            null,
            null),
        new(
            Client5Id,
            "seed.client5@drugsapp.local",
            "Client5Seed123!",
            "Katarzyna",
            "Zielińska",
            new DateOnly(1999, 9, 18),
            ProfileGender.female,
            [AuthRoles.Client],
            null,
            null,
            null),
        new(
            Provider1Id,
            "seed.provider1@drugsapp.local",
            "Provider1Seed123!",
            "Jan",
            "Mazur",
            new DateOnly(1975, 6, 12),
            ProfileGender.male,
            [AuthRoles.Client, AuthRoles.HealthcareProvider],
            HealthcareProviderProfession.doctor,
            "City Medical Center",
            "General practitioner focused on chronic care."),
        new(
            Provider2Id,
            "seed.provider2@drugsapp.local",
            "Provider2Seed123!",
            "Ewa",
            "Kamińska",
            new DateOnly(1983, 4, 8),
            ProfileGender.female,
            [AuthRoles.Client, AuthRoles.HealthcareProvider],
            HealthcareProviderProfession.pharmacist,
            "Central Pharmacy",
            "Clinical pharmacist specializing in drug interactions."),
        new(
            Provider3Id,
            "seed.provider3@drugsapp.local",
            "Provider3Seed123!",
            "Michał",
            "Wójcik",
            new DateOnly(1979, 12, 2),
            ProfileGender.male,
            [AuthRoles.Client, AuthRoles.HealthcareProvider],
            HealthcareProviderProfession.nurse,
            "Regional Clinic",
            "Care coordinator for supervised patients.")
    ];
}
