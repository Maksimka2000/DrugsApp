using System.Reflection;
using System.Text;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Persistence.Identity;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DrugsApp.Infrastructure.Services.Seed;

/// <summary>
/// Applies <c>Seed/seed.sql</c> once per <see cref="DevSeedSettings.SeedKey"/> after ensuring dev accounts exist.
/// </summary>
public sealed class PatientDevDataSeeder(
    DrugsAppDbContext dbContext,
    NpgsqlDataSource dataSource,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IUserProfileRepository userProfileRepository,
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    IOptions<DevSeedSettings> devSeedOptions,
    ILogger<PatientDevDataSeeder> logger)
{
    private const string MetadataTableSql = """
        CREATE TABLE IF NOT EXISTS dev_seed_applications (
            seed_key varchar(128) PRIMARY KEY,
            user_id uuid NOT NULL,
            applied_at_utc timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc')
        );
        """;

    private readonly DevSeedSettings _settings = devSeedOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            logger.LogDebug("Patient dev seed skipped because DevSeed:Enabled is false.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.SeedKey))
        {
            logger.LogWarning("Patient dev seed skipped because DevSeed:SeedKey is empty.");
            return;
        }

        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(MetadataTableSql, cancellationToken);

            string seedKey = _settings.SeedKey.Trim();
            await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);

            if (await IsSeedAlreadyAppliedAsync(connection, seedKey, cancellationToken))
            {
                logger.LogInformation(
                    "Patient dev seed '{SeedKey}' already applied; skipping.",
                    seedKey);
                return;
            }

            await EnsureSeedUsersAsync(cancellationToken);

            string sql = await LoadSeedSqlAsync(cancellationToken);
            sql = sql.Replace("{{SEED_USER_ID}}", DevSeedUserDefinitions.Client1Id.ToString(), StringComparison.Ordinal);

            await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await using (NpgsqlCommand command = new(sql, connection, transaction))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await using NpgsqlCommand metadataCommand = new(
                    """
                    INSERT INTO dev_seed_applications (seed_key, user_id, applied_at_utc)
                    VALUES (@seedKey, @userId, NOW() AT TIME ZONE 'utc')
                    """,
                    connection,
                    transaction);
                metadataCommand.Parameters.AddWithValue("seedKey", seedKey);
                metadataCommand.Parameters.AddWithValue("userId", DevSeedUserDefinitions.Client1Id);
                await metadataCommand.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            logger.LogInformation(
                "Dev seed '{SeedKey}' applied for {UserCount} accounts (primary client {ClientEmail}).",
                seedKey,
                DevSeedUserDefinitions.All.Count,
                DevSeedUserDefinitions.All[0].Email);
        }
        catch (NpgsqlException ex) when (IsDatabaseUnavailable(ex))
        {
            logger.LogWarning(
                ex,
                "Patient dev seed skipped because the database is unavailable.");
        }
    }

    private async Task EnsureSeedUsersAsync(CancellationToken cancellationToken)
    {
        foreach (DevSeedUserDefinitions.SeedUserDefinition definition in DevSeedUserDefinitions.All)
        {
            await EnsureSeedUserAsync(definition, cancellationToken);
        }

        await EnsureLegacySettingsPatientAliasAsync(cancellationToken);
    }

    private async Task EnsureLegacySettingsPatientAliasAsync(CancellationToken cancellationToken)
    {
        string legacyEmail = _settings.PatientEmail.Trim();
        DevSeedUserDefinitions.SeedUserDefinition client1 = DevSeedUserDefinitions.All[0];

        if (string.Equals(legacyEmail, client1.Email, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ApplicationUser? legacyUser = await userManager.FindByEmailAsync(legacyEmail);
        if (legacyUser is not null && legacyUser.Id != client1.Id)
        {
            logger.LogInformation(
                "Legacy DevSeed patient email {LegacyEmail} exists with a different id; keeping seed.client1 as primary.",
                legacyEmail);
            return;
        }

        if (legacyUser is null)
        {
            ApplicationUser aliasUser = new()
            {
                Id = Guid.NewGuid(),
                Email = legacyEmail,
                UserName = legacyEmail,
                EmailConfirmed = true,
                FullName = $"{_settings.PatientFirstName.Trim()} {_settings.PatientLastName.Trim()}",
                CreatedAtUtc = DateTime.UtcNow
            };

            IdentityResult createResult = await userManager.CreateAsync(aliasUser, _settings.PatientPassword);
            EnsureIdentitySucceeded(createResult, $"Failed to create legacy alias user '{legacyEmail}'.");
            await dbContext.SaveChangesAsync(cancellationToken);
            legacyUser = aliasUser;
        }

        await EnsureRoleAsync(legacyUser.Id, AuthRoles.Client, cancellationToken);

        if (await userProfileRepository.GetByUserIdAsync(legacyUser.Id, cancellationToken) is null)
        {
            if (!DateOnly.TryParse(_settings.DateOfBirth, out DateOnly dateOfBirth))
            {
                throw new InvalidOperationException($"DevSeed:DateOfBirth '{_settings.DateOfBirth}' is not a valid date.");
            }

            if (!Enum.TryParse(_settings.Gender, ignoreCase: true, out ProfileGender gender))
            {
                throw new InvalidOperationException($"DevSeed:Gender '{_settings.Gender}' is not a valid profile gender.");
            }

            ErrorOr<UserProfile> profileResult = UserProfile.Create(
                id: legacyUser.Id,
                email: legacyEmail,
                firstName: _settings.PatientFirstName.Trim(),
                lastName: _settings.PatientLastName.Trim(),
                dateOfBirth: dateOfBirth,
                gender: gender);

            if (profileResult.IsError)
            {
                string detail = string.Join("; ", profileResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to create legacy alias profile. {detail}");
            }

            await userProfileRepository.AddAsync(profileResult.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureSeedUserAsync(
        DevSeedUserDefinitions.SeedUserDefinition definition,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(definition.Id.ToString());
        if (user is null)
        {
            user = await userManager.FindByEmailAsync(definition.Email);
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = definition.Id,
                Email = definition.Email,
                UserName = definition.Email,
                EmailConfirmed = true,
                FullName = $"{definition.FirstName} {definition.LastName}",
                CreatedAtUtc = DateTime.UtcNow
            };

            IdentityResult createResult = await userManager.CreateAsync(user, definition.Password);
            EnsureIdentitySucceeded(createResult, $"Failed to create seed user '{definition.Email}'.");
            await dbContext.SaveChangesAsync(cancellationToken);
            user = (await userManager.FindByIdAsync(definition.Id.ToString()))!;
        }
        else
        {
            bool changed = false;
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                changed = true;
            }

            string fullName = $"{definition.FirstName} {definition.LastName}";
            if (!string.Equals(user.FullName, fullName, StringComparison.Ordinal))
            {
                user.FullName = fullName;
                changed = true;
            }

            if (changed)
            {
                IdentityResult updateResult = await userManager.UpdateAsync(user);
                EnsureIdentitySucceeded(updateResult, $"Failed to update seed user '{definition.Email}'.");
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        foreach (string roleName in definition.Roles)
        {
            await EnsureRoleAsync(user.Id, roleName, cancellationToken);
        }

        UserProfile? profile = await userProfileRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (profile is null)
        {
            ErrorOr<UserProfile> profileResult = UserProfile.Create(
                id: user.Id,
                email: definition.Email,
                firstName: definition.FirstName,
                lastName: definition.LastName,
                dateOfBirth: definition.DateOfBirth,
                gender: definition.Gender);

            if (profileResult.IsError)
            {
                string detail = string.Join("; ", profileResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException(
                    $"Failed to create profile for '{definition.Email}'. {detail}");
            }

            profileResult.Value.MarkOnboardingCompleted(DateTime.UtcNow);
            await userProfileRepository.AddAsync(profileResult.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (definition.ProviderProfession is null)
        {
            return;
        }

        HealthcareProviderProfile? providerProfile =
            await healthcareProviderProfileRepository.GetTrackedByUserIdAsync(user.Id, cancellationToken);

        if (providerProfile is not null)
        {
            return;
        }

        ErrorOr<HealthcareProviderProfile> providerResult = HealthcareProviderProfile.Create(
            userId: user.Id,
            professionCode: definition.ProviderProfession,
            otherProfessionLabel: null,
            workplaceLabel: definition.ProviderWorkplace,
            shortDescription: definition.ProviderDescription);

        if (providerResult.IsError)
        {
            string detail = string.Join("; ", providerResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException(
                $"Failed to create healthcare provider profile for '{definition.Email}'. {detail}");
        }

        providerResult.Value.MarkOnboardingCompleted(DateTime.UtcNow);
        await healthcareProviderProfileRepository.AddAsync(providerResult.Value, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        ApplicationRole? role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            throw new InvalidOperationException(
                $"Role '{roleName}' is missing. Run identity authorization seeding first.");
        }

        bool hasRole = await dbContext.UserRoles.AnyAsync(
            userRole => userRole.UserId == userId && userRole.RoleId == role.Id,
            cancellationToken);

        if (!hasRole)
        {
            dbContext.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = role.Id
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<bool> IsSeedAlreadyAppliedAsync(
        NpgsqlConnection connection,
        string seedKey,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = new(
            "SELECT 1 FROM dev_seed_applications WHERE seed_key = @seedKey LIMIT 1",
            connection);
        command.Parameters.AddWithValue("seedKey", seedKey);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    private static async Task<string> LoadSeedSqlAsync(CancellationToken cancellationToken)
    {
        Assembly assembly = typeof(PatientDevDataSeeder).Assembly;
        await using Stream? stream = assembly.GetManifestResourceStream("DrugsApp.Infrastructure.Seed.seed.sql");
        if (stream is null)
        {
            throw new InvalidOperationException("Embedded resource DrugsApp.Infrastructure.Seed.seed.sql was not found.");
        }

        using StreamReader reader = new(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static bool IsDatabaseUnavailable(NpgsqlException exception) =>
        exception.SqlState is "57P03" or "3D000" or "28P01" or "08006" or "08001" or "08004";

    private static void EnsureIdentitySucceeded(IdentityResult identityResult, string message)
    {
        if (identityResult.Succeeded)
        {
            return;
        }

        string detail = string.Join("; ", identityResult.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {detail}");
    }
}
