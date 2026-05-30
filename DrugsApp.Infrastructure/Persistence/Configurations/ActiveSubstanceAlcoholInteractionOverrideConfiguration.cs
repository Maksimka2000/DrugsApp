using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ActiveSubstanceAlcoholInteractionOverrideConfiguration
    : BaseEntityConfiguration<ActiveSubstanceAlcoholInteractionOverride>
{
    public override void Configure(EntityTypeBuilder<ActiveSubstanceAlcoholInteractionOverride> builder)
    {
        base.Configure(builder);

        builder.ToTable("ActiveSubstanceAlcoholInteractionOverrides");

        builder.Property(o => o.AlcoholSpecialText)
            .IsRequired();

        builder.HasIndex(
                o => o.ActiveSubstanceId,
                "Index_ActiveSubstanceAlcoholInteractionOverrides_ActiveSubstanceId")
            .IsUnique();

        builder.HasOne(o => o.ActiveSubstance)
            .WithOne(a => a.AlcoholInteractionOverride)
            .HasForeignKey<ActiveSubstanceAlcoholInteractionOverride>(o => o.ActiveSubstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

