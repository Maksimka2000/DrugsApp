using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class InteractionExpandedConfiguration : BaseEntityConfiguration<InteractionExpanded>
{
    public override void Configure(EntityTypeBuilder<InteractionExpanded> builder)
    {
        base.Configure(builder);

        builder.ToTable("InteractionExpanded");

        builder.Property(row => row.InteractionId)
            .IsRequired();

        builder.Property(row => row.PartyTypeA)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(row => row.PartyIdA)
            .IsRequired();

        builder.Property(row => row.PartyTypeB)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(row => row.PartyIdB)
            .IsRequired();

        builder.HasOne(row => row.Interaction)
            .WithMany(interaction => interaction.ExpandedRows)
            .HasForeignKey(row => row.InteractionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(
            row => new { row.PartyTypeA, row.PartyIdA },
            "Index_InteractionExpanded_PartyA");

        builder.HasIndex(
            row => new { row.PartyTypeB, row.PartyIdB },
            "Index_InteractionExpanded_PartyB");

        builder.HasIndex(
                row => new
                {
                    row.InteractionId,
                    row.PartyTypeA,
                    row.PartyIdA,
                    row.PartyTypeB,
                    row.PartyIdB
                },
                "UX_InteractionExpanded_UniquePair")
            .IsUnique();
    }
}
