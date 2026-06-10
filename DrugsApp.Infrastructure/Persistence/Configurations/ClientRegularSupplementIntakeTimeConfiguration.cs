using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientRegularSupplementIntakeTimeConfiguration : BaseEntityConfiguration<ClientRegularSupplementIntakeTime>
{
    public override void Configure(EntityTypeBuilder<ClientRegularSupplementIntakeTime> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_regular_supplement_intake_times");

        builder.Property(x => x.RegularSupplementId)
            .IsRequired();

        builder.Property(x => x.IntakeTime)
            .IsRequired();

        builder.HasIndex(x => new { x.RegularSupplementId, x.IntakeTime })
            .HasDatabaseName("UX_client_regular_supplement_intake_times_regular_supplement_id_intake_time")
            .IsUnique();
    }
}
