using DrugsApp.Domain.InteractionFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class InteractionConfiguration : BaseEntityConfiguration<Interaction>
{
    public override void Configure(EntityTypeBuilder<Interaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("Interactions");

        builder.Property(interaction => interaction.InteractionSeverityId)
            .IsRequired();

        builder.Property(interaction => interaction.DefaultText)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(interaction => interaction.StomachText)
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasOne(interaction => interaction.InteractionSeverity)
            .WithMany()
            .HasForeignKey(interaction => interaction.InteractionSeverityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(interaction => interaction.Parties)
            .WithOne(party => party.Interaction)
            .HasForeignKey(party => party.InteractionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(interaction => interaction.ExpandedRows)
            .WithOne(expanded => expanded.Interaction)
            .HasForeignKey(expanded => expanded.InteractionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(interaction => interaction.InteractionSeverityId, "Index_Interactions_InteractionSeverityId");
        builder.HasIndex(interaction => interaction.CreatedAtUtc, "Index_Interactions_CreatedAtUtc");
        builder.HasIndex(interaction => interaction.UpdatedAtUtc, "Index_Interactions_UpdatedAtUtc");
    }
}
