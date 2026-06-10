using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientRegularSupplementConfiguration : BaseEntityConfiguration<ClientRegularSupplement>
{
    public override void Configure(EntityTypeBuilder<ClientRegularSupplement> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_regular_supplements");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.HistoryReason)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired(false);

        builder
            .HasOne(x => x.Supplement)
            .WithMany()
            .HasForeignKey(x => x.SupplementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.OwnSupplement)
            .WithMany()
            .HasForeignKey(x => x.OwnSupplementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.IntakeTimes)
            .WithOne(x => x.RegularSupplement)
            .HasForeignKey(x => x.RegularSupplementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_client_regular_supplements_user_id_status");

        builder.HasIndex(x => new { x.UserId, x.SupplementId })
            .HasDatabaseName("UX_client_regular_supplements_user_id_supplement_id_live")
            .IsUnique()
            .HasFilter(
                $"\"Status\" IN ('{RegularSupplementStatus.Scheduled}', '{RegularSupplementStatus.Active}') " +
                $"AND \"Source\" = '{RegularSupplementSource.Database}'");

        builder.HasIndex(x => new { x.UserId, x.OwnSupplementId })
            .HasDatabaseName("UX_client_regular_supplements_user_id_own_supplement_id_live")
            .IsUnique()
            .HasFilter(
                $"\"Status\" IN ('{RegularSupplementStatus.Scheduled}', '{RegularSupplementStatus.Active}') " +
                $"AND \"Source\" = '{RegularSupplementSource.Own}'");

        builder.HasIndex(x => new { x.UserId, x.ArchivedAtUtc })
            .HasDatabaseName("IX_client_regular_supplements_user_id_archived_at_utc")
            .HasFilter($"\"Status\" = '{RegularSupplementStatus.Expired}'");

        builder.HasIndex(x => new { x.Status, x.EndDate })
            .HasDatabaseName("IX_client_regular_supplements_status_end_date")
            .HasFilter(
                $"\"Status\" IN ('{RegularSupplementStatus.Scheduled}', '{RegularSupplementStatus.Active}') " +
                "AND \"EndDate\" IS NOT NULL");
    }
}
