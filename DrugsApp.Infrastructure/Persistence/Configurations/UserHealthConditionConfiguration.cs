using DrugsApp.Domain.HealthConditionFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class UserHealthConditionConfiguration : BaseEntityConfiguration<UserHealthCondition>
{
    public override void Configure(EntityTypeBuilder<UserHealthCondition> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserHealthConditions");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.HealthConditionId)
            .IsRequired(false);

        builder.Property(x => x.CustomHealthConditionName)
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedCustomHealthConditionName)
            .HasMaxLength(256);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(2000);

        builder.HasOne(x => x.HealthCondition)
            .WithMany(x => x.UserHealthConditions)
            .HasForeignKey(x => x.HealthConditionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(
                x => new { x.UserId, x.HealthConditionId },
                "IX_UserHealthConditions_UserId_HealthConditionId_Database")
            .HasFilter("\"Source\" = 'Database'")
            .IsUnique();

        builder.HasIndex(
                x => new { x.UserId, x.NormalizedCustomHealthConditionName },
                "IX_UserHealthConditions_UserId_NormalizedCustomHealthConditionName_Custom")
            .HasFilter("\"Source\" = 'Custom'")
            .IsUnique();

        builder.HasIndex(x => x.NormalizedCustomHealthConditionName)
            .HasDatabaseName("IX_UserHealthConditions_NormalizedCustomHealthConditionName_trgm")
            .HasFilter("\"Source\" = 'Custom'")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");
    }
}
