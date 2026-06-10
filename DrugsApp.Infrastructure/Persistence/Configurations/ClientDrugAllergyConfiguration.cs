using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientDrugAllergyConfiguration : BaseEntityConfiguration<ClientDrugAllergy>
{
    public override void Configure(EntityTypeBuilder<ClientDrugAllergy> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClientDrugAllergies");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.CustomDrugName)
            .HasMaxLength(256);

        builder.Property(x => x.CustomDrugNameNormalized)
            .HasMaxLength(256);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Drug)
            .WithMany()
            .HasForeignKey(x => x.DrugId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId, "IX_ClientDrugAllergies_UserId");

        builder.HasIndex(
                x => new { x.UserId, x.DrugId },
                "IX_ClientDrugAllergies_UserId_DrugId_Database")
            .HasFilter("\"DrugId\" IS NOT NULL")
            .IsUnique();

        builder.HasIndex(
                x => new { x.UserId, x.CustomDrugNameNormalized },
                "IX_ClientDrugAllergies_UserId_CustomDrugNameNormalized_Custom")
            .HasFilter("\"CustomDrugNameNormalized\" IS NOT NULL")
            .IsUnique();

        builder.HasIndex(
            x => new { x.UserId, x.Source, x.Id },
            "IX_ClientDrugAllergies_UserId_Source_Id");
    }
}
