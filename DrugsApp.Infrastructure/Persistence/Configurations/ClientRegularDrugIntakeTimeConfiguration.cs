using DrugsApp.Domain.RegularDrugsFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientRegularDrugIntakeTimeConfiguration : BaseEntityConfiguration<ClientRegularDrugIntakeTime>
{
    public override void Configure(EntityTypeBuilder<ClientRegularDrugIntakeTime> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_regular_drug_intake_times");

        builder.Property(x => x.RegularDrugId)
            .IsRequired();

        builder.Property(x => x.IntakeTime)
            .IsRequired();

        builder.HasIndex(x => new { x.RegularDrugId, x.IntakeTime })
            .HasDatabaseName("UX_client_regular_drug_intake_times_regular_drug_id_intake_time")
            .IsUnique();
    }
}
