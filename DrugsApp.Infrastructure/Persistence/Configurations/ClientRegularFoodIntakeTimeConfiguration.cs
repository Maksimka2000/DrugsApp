using DrugsApp.Domain.RegularFoodFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientRegularFoodIntakeTimeConfiguration : BaseEntityConfiguration<ClientRegularFoodIntakeTime>
{
    public override void Configure(EntityTypeBuilder<ClientRegularFoodIntakeTime> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_regular_food_intake_times");

        builder.Property(x => x.RegularFoodId)
            .IsRequired();

        builder.Property(x => x.IntakeTime)
            .IsRequired();

        builder.HasIndex(x => new { x.RegularFoodId, x.IntakeTime })
            .HasDatabaseName("UX_client_regular_food_intake_times_regular_food_id_intake_time")
            .IsUnique();
    }
}
