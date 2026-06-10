using DrugsApp.Domain.HealthConditionFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class HealthConditionConfiguration : BaseEntityConfiguration<HealthCondition>
{
    public override void Configure(EntityTypeBuilder<HealthCondition> builder)
    {
        base.Configure(builder);

        builder.ToTable("HealthConditions");

        builder.Property(x => x.HealthConditionName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedHealthConditionName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.NormalizedHealthConditionName)
            .IsUnique();

        builder.HasIndex(
                x => x.NormalizedHealthConditionName,
                "IX_HealthConditions_NormalizedHealthConditionName_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.HasMany(x => x.UserHealthConditions)
            .WithOne(x => x.HealthCondition)
            .HasForeignKey(x => x.HealthConditionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
