using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class InteractionPartyConfiguration : BaseEntityConfiguration<InteractionParty>
{
    public override void Configure(EntityTypeBuilder<InteractionParty> builder)
    {
        base.Configure(builder);

        builder.ToTable(
            "InteractionParties",
            tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_InteractionParties_ExactlyOneFk",
                    "num_nonnulls(\"ActiveSubstanceId\",\"ActiveSubstanceGroupId\",\"FoodId\",\"FoodGroupId\",\"SupplementId\",\"SupplementGroupId\") = 1"));

        builder.Property(party => party.PartyType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(party => party.ActiveSubstanceId)
            .IsRequired(false);

        builder.Property(party => party.ActiveSubstanceGroupId)
            .IsRequired(false);

        builder.Property(party => party.FoodId)
            .IsRequired(false);

        builder.Property(party => party.FoodGroupId)
            .IsRequired(false);

        builder.Property(party => party.SupplementId)
            .IsRequired(false);

        builder.Property(party => party.SupplementGroupId)
            .IsRequired(false);

        builder.HasOne(party => party.Interaction)
            .WithMany(interaction => interaction.Parties)
            .HasForeignKey(party => party.InteractionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(party => party.InteractionId, "Index_InteractionParties_InteractionId");
        builder.HasIndex(party => party.ActiveSubstanceId, "Index_InteractionParties_ActiveSubstanceId");
        builder.HasIndex(party => party.ActiveSubstanceGroupId, "Index_InteractionParties_ActiveSubstanceGroupId");
        builder.HasIndex(party => party.FoodId, "Index_InteractionParties_FoodId");
        builder.HasIndex(party => party.FoodGroupId, "Index_InteractionParties_FoodGroupId");
        builder.HasIndex(party => party.SupplementId, "Index_InteractionParties_SupplementId");
        builder.HasIndex(party => party.SupplementGroupId, "Index_InteractionParties_SupplementGroupId");

        builder.HasIndex(
                party => new { party.InteractionId, party.ActiveSubstanceId },
                "UX_InteractionParties_Substance")
            .IsUnique()
            .HasFilter("\"PartyType\" = 'Substance'");

        builder.HasIndex(
                party => new { party.InteractionId, party.ActiveSubstanceGroupId },
                "UX_InteractionParties_SubstanceGroup")
            .IsUnique()
            .HasFilter("\"PartyType\" = 'SubstanceGroup'");

        builder.HasIndex(
                party => new { party.InteractionId, party.FoodId },
                "UX_InteractionParties_Food")
            .IsUnique()
            .HasFilter("\"PartyType\" = 'Food'");

        builder.HasIndex(
                party => new { party.InteractionId, party.FoodGroupId },
                "UX_InteractionParties_FoodGroup")
            .IsUnique()
            .HasFilter("\"PartyType\" = 'FoodGroup'");

        builder.HasIndex(
                party => new { party.InteractionId, party.SupplementId },
                "UX_InteractionParties_Supplement")
            .IsUnique()
            .HasFilter("\"PartyType\" = 'Supplement'");

        builder.HasIndex(
                party => new { party.InteractionId, party.SupplementGroupId },
                "UX_InteractionParties_SupplementGroup")
            .IsUnique()
            .HasFilter("\"PartyType\" = 'SupplementGroup'");
    }
}
