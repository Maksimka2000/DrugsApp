using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientMealTimeConfiguration : BaseEntityConfiguration<ClientMealTime>
{
    public override void Configure(EntityTypeBuilder<ClientMealTime> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClientMealTimes");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MealName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.NormalizedMealName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.IntakeTime)
            .HasColumnType("time")
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_ClientMealTimes_UserId");

        builder.HasIndex(x => new { x.UserId, x.IntakeTime })
            .HasDatabaseName("IX_ClientMealTimes_UserId_IntakeTime")
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.NormalizedMealName })
            .HasDatabaseName("IX_ClientMealTimes_UserId_NormalizedMealName")
            .IsUnique();
    }
}
