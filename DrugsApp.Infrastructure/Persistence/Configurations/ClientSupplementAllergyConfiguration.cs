using DrugsApp.Domain.AllergiesFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientSupplementAllergyConfiguration : BaseEntityConfiguration<ClientSupplementAllergy>
{
    public override void Configure(EntityTypeBuilder<ClientSupplementAllergy> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClientSupplementAllergies");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.CustomSupplementName)
            .HasMaxLength(256);

        builder.Property(x => x.CustomSupplementNameNormalized)
            .HasMaxLength(256);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Supplement)
            .WithMany()
            .HasForeignKey(x => x.SupplementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId, "IX_ClientSupplementAllergies_UserId");

        builder.HasIndex(
                x => new { x.UserId, x.SupplementId },
                "IX_ClientSupplementAllergies_UserId_SupplementId_Database")
            .HasFilter("\"SupplementId\" IS NOT NULL")
            .IsUnique();

        builder.HasIndex(
                x => new { x.UserId, x.CustomSupplementNameNormalized },
                "IX_ClientSupplementAllergies_UserId_CustomSupplementNameNormalized_Custom")
            .HasFilter("\"CustomSupplementNameNormalized\" IS NOT NULL")
            .IsUnique();
    }
}
