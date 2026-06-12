using DrugsApp.Domain.AllergiesFeature.Entities;
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

        builder.Property(x => x.DrugFormId)
            .IsRequired(false);

        builder.Property(x => x.CustomDrugName)
            .HasMaxLength(300);

        builder.Property(x => x.CustomDrugNameNormalized)
            .HasMaxLength(300);

        builder.Property(x => x.CustomActiveSubstances)
            .HasMaxLength(300);

        builder.Property(x => x.CustomFormulationName)
            .HasMaxLength(300);

        builder.Property(x => x.CustomDosage)
            .HasMaxLength(200);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.HasOne(x => x.DrugForm)
            .WithMany()
            .HasForeignKey(x => x.DrugFormId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId, "IX_ClientDrugAllergies_UserId");

        builder.HasIndex(
                x => new { x.UserId, x.DrugFormId },
                "IX_ClientDrugAllergies_UserId_DrugFormId_Database")
            .HasFilter("\"DrugFormId\" IS NOT NULL")
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
