using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientRegularFoodConfiguration : BaseEntityConfiguration<ClientRegularFood>
{
    public override void Configure(EntityTypeBuilder<ClientRegularFood> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_regular_foods");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.FoodId)
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
            .HasOne(x => x.Food)
            .WithMany()
            .HasForeignKey(x => x.FoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.IntakeTimes)
            .WithOne(x => x.RegularFood)
            .HasForeignKey(x => x.RegularFoodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_client_regular_foods_user_id_status");

        builder.HasIndex(x => new { x.UserId, x.FoodId })
            .HasDatabaseName("UX_client_regular_foods_user_id_food_id_live")
            .IsUnique()
            .HasFilter("\"Status\" IN ('Scheduled', 'Active')");

        builder.HasIndex(x => new { x.UserId, x.Status, x.EndDate })
            .HasDatabaseName("IX_client_regular_foods_user_id_status_end_date")
            .HasFilter("\"Status\" IN ('Scheduled', 'Active')");

        builder.HasIndex(x => new { x.UserId, x.ArchivedAtUtc })
            .HasDatabaseName("IX_client_regular_foods_user_id_archived_at_utc")
            .HasFilter("\"Status\" = 'Expired'");
    }
}
