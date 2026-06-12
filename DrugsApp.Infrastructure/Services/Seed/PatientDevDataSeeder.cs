using System.Reflection;
using System.Text;
using DrugsApp.Domain.AuthFeature;
using ErrorOr;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DrugsApp.Infrastructure.Services.Seed;

/// <summary>
/// Applies <c>Seed/seed.sql</c> once per <see cref="DevSeedSettings.SeedKey"/> after ensuring a dev patient account exists.
/// </summary>
public sealed class PatientDevDataSeeder(
    DrugsAppDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IUserProfileRepository userProfileRepository,
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
            await using NpgsqlConnection connection = new(dbContext.Database.GetConnectionString()!);
            await connection.OpenAsync(cancellationToken);

            if (await IsSeedAlreadyAppliedAsync(connection, seedKey, cancellationToken))
            {
                logger.LogInformation(
                    "Patient dev seed '{SeedKey}' already applied; skipping.",
                    seedKey);
                return;
            }

            Guid userId = await EnsureSeedPatientUserAsync(cancellationToken);
            string sql = await LoadSeedSqlAsync(cancellationToken);
            sql = sql.Replace("{{SEED_USER_ID}}", userId.ToString(), StringComparison.Ordinal);

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
                metadataCommand.Parameters.AddWithValue("userId", userId);
                await metadataCommand.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            logger.LogInformation(
                "Patient dev seed '{SeedKey}' applied for user {Email} ({UserId}).",
                seedKey,
                _settings.PatientEmail,
                userId);
        }
        catch (NpgsqlException ex) when (IsDatabaseUnavailable(ex))
        {
            logger.LogWarning(
                ex,
                "Patient dev seed skipped because the database is unavailable.");
        }
    }

    private async Task<Guid> EnsureSeedPatientUserAsync(CancellationToken cancellationToken)
    {
        string email = _settings.PatientEmail.Trim();
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                FullName = $"{_settings.PatientFirstName.Trim()} {_settings.PatientLastName.Trim()}",
                CreatedAtUtc = DateTime.UtcNow
            };

            IdentityResult createResult = await userManager.CreateAsync(user, _settings.PatientPassword);
            EnsureIdentitySucceeded(createResult, "Failed to create seed patient user.");
            await dbContext.SaveChangesAsync(cancellationToken);
            user = (await userManager.FindByEmailAsync(email))!;
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            IdentityResult updateResult = await userManager.UpdateAsync(user);
            EnsureIdentitySucceeded(updateResult, "Failed to confirm seed patient email.");
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureClientRoleAsync(user.Id, cancellationToken);

        UserProfile? profile = await userProfileRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (profile is null)
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
                id: user.Id,
                email: email,
                firstName: _settings.PatientFirstName.Trim(),
                lastName: _settings.PatientLastName.Trim(),
                dateOfBirth: dateOfBirth,
                gender: gender);

            if (profileResult.IsError)
            {
                string detail = string.Join("; ", profileResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to create seed patient profile. {detail}");
            }

            await userProfileRepository.AddAsync(profileResult.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return user.Id;
    }

    private async Task EnsureClientRoleAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationRole? clientRole = await roleManager.FindByNameAsync(AuthRoles.Client);
        if (clientRole is null)
        {
            throw new InvalidOperationException(
                $"Role '{AuthRoles.Client}' is missing. Run identity authorization seeding first.");
        }

        bool hasRole = await dbContext.UserRoles.AnyAsync(
            userRole => userRole.UserId == userId && userRole.RoleId == clientRole.Id,
            cancellationToken);

        if (!hasRole)
        {
            dbContext.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = clientRole.Id
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
