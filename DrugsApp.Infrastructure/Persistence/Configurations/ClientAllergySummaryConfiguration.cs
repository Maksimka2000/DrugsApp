using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientAllergySummaryConfiguration : BaseEntityConfiguration<ClientAllergySummary>
{
    public override void Configure(EntityTypeBuilder<ClientAllergySummary> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClientAllergySummaries");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.SevereReactionStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.PollenDustMitesAllergyStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.InsectVenomAllergyStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.LatexAllergyStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(x => x.UserId, "IX_ClientAllergySummaries_UserId")
            .IsUnique();
    }
}
