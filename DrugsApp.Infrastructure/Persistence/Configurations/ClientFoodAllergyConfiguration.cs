using DrugsApp.Domain.AllergiesFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientFoodAllergyConfiguration : BaseEntityConfiguration<ClientFoodAllergy>
{
    public override void Configure(EntityTypeBuilder<ClientFoodAllergy> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClientFoodAllergies");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.CustomFoodName)
            .HasMaxLength(256);

        builder.Property(x => x.CustomFoodNameNormalized)
            .HasMaxLength(256);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Food)
            .WithMany()
            .HasForeignKey(x => x.FoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId, "IX_ClientFoodAllergies_UserId");

        builder.HasIndex(
                x => new { x.UserId, x.FoodId },
                "IX_ClientFoodAllergies_UserId_FoodId_Database")
            .HasFilter("\"FoodId\" IS NOT NULL")
            .IsUnique();

        builder.HasIndex(
                x => new { x.UserId, x.CustomFoodNameNormalized },
                "IX_ClientFoodAllergies_UserId_CustomFoodNameNormalized_Custom")
            .HasFilter("\"CustomFoodNameNormalized\" IS NOT NULL")
            .IsUnique();
    }
}
