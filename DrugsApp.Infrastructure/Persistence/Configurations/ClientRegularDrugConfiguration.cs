using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientRegularDrugConfiguration : BaseEntityConfiguration<ClientRegularDrug>
{
    public override void Configure(EntityTypeBuilder<ClientRegularDrug> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_regular_drugs");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.DrugId)
            .IsRequired();

        builder.Property(x => x.DrugFormId)
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
            .HasOne(x => x.Drug)
            .WithMany()
            .HasForeignKey(x => x.DrugId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.DrugForm)
            .WithMany()
            .HasForeignKey(x => x.DrugFormId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.IntakeTimes)
            .WithOne(x => x.RegularDrug)
            .HasForeignKey(x => x.RegularDrugId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_client_regular_drugs_user_id_status");

        builder.HasIndex(x => new { x.UserId, x.DrugId })
            .HasDatabaseName("UX_client_regular_drugs_user_id_drug_id_live")
            .IsUnique()
            .HasFilter("\"Status\" IN ('Scheduled', 'Active')");

        builder.HasIndex(x => new { x.UserId, x.ArchivedAtUtc })
            .HasDatabaseName("IX_client_regular_drugs_user_id_archived_at_utc")
            .HasFilter("\"Status\" = 'Expired'");

        builder.HasIndex(x => new { x.Status, x.EndDate })
            .HasDatabaseName("IX_client_regular_drugs_status_end_date_live")
            .HasFilter("\"Status\" IN ('Scheduled', 'Active')");
    }
}
