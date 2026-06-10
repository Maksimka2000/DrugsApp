using DrugsApp.Domain.AllergiesFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientOtherAllergyConfiguration : BaseEntityConfiguration<ClientOtherAllergy>
{
    public override void Configure(EntityTypeBuilder<ClientOtherAllergy> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClientOtherAllergies");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.AllergyName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.AllergyNameNormalized)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.UserId, "IX_ClientOtherAllergies_UserId");

        builder.HasIndex(
                x => new { x.UserId, x.AllergyNameNormalized },
                "IX_ClientOtherAllergies_UserId_AllergyNameNormalized")
            .IsUnique();
    }
}
