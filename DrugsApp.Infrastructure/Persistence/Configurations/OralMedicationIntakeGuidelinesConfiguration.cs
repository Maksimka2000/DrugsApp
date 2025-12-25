using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class OralMedicationIntakeGuidelinesConfiguration : BaseEntityConfiguration<OralMedicationIntakeGuidelines>
{
    public override void Configure(EntityTypeBuilder<OralMedicationIntakeGuidelines> builder)
    {
        base.Configure(builder);

        builder.ToTable("OralMedicationIntakeGuidelines");

        builder.Property(g => g.ActiveSubstanceId)
            .IsRequired();

        builder.HasIndex(g => g.ActiveSubstanceId)
            .IsUnique();

        builder.Property(g => g.FoodIntakeType)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(g => g.WaitBeforeMinutes)
            .IsRequired(false);

        builder.Property(g => g.WaitAfterMinutes)
            .IsRequired(false);

        builder.Property(g => g.MealNotes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(g => g.AvoidFoodDrug)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(g => g.AvoidFoodTherapy)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(g => g.LiquidNotes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(g => g.LiquidInstructions)
            .IsRequired()
            .IsRequired(false);

        builder.Property(g => g.RecommendedMorning)
            .IsRequired()
            .IsRequired(false);

        builder.Property(g => g.RecommendedEvening)
            .IsRequired()
            .IsRequired(false);
    }
}


