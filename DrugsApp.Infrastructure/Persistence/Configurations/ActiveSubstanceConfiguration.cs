using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ActiveSubstanceConfiguration : BaseEntityConfiguration<ActiveSubstance>
{
    public override void Configure(EntityTypeBuilder<ActiveSubstance> builder)
    {
        base.Configure(builder);

        builder.ToTable("ActiveSubstances");

        builder.Property(a => a.ActiveSubstanceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(a => a.ActiveSubstanceName, "Index_ActiveSubstances_ActiveSubstanceName")
            .IsUnique();

        builder.HasIndex(a => a.ActiveSubstanceName, "Index_ActiveSubstances_ActiveSubstanceName_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.HasOne(a => a.OralMedicationIntakeGuidelines)
            .WithOne(g => g.ActiveSubstance)
            .HasForeignKey<OralMedicationIntakeGuidelines>(g => g.ActiveSubstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


