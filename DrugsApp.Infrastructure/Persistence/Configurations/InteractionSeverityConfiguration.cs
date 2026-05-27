using DrugsApp.Domain.InteractionFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class InteractionSeverityConfiguration : BaseEntityConfiguration<InteractionSeverity>
{
    public override void Configure(EntityTypeBuilder<InteractionSeverity> builder)
    {
        base.Configure(builder);

        builder.ToTable("InteractionSeverities");

        builder.Property(severity => severity.SeverityNumber)
            .IsRequired();

        builder.Property(severity => severity.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(severity => severity.Description)
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasIndex(severity => severity.SeverityNumber, "Index_InteractionSeverities_SeverityNumber")
            .IsUnique();

        builder.HasIndex(severity => severity.Name, "Index_InteractionSeverities_Name")
            .IsUnique();

        builder.HasData(
            new
            {
                Id = new Guid("11111111-1111-1111-1111-111111110111"),
                CreatedAtUtc = DateTime.UnixEpoch,
                UpdatedAtUtc = (DateTime?)null,
                SeverityNumber = (short)1,
                Name = "Major",
                Description = (string?)null
            },
            new
            {
                Id = new Guid("22222222-2222-2222-2222-222222221222"),
                CreatedAtUtc = DateTime.UnixEpoch,
                UpdatedAtUtc = (DateTime?)null,
                SeverityNumber = (short)2,
                Name = "Moderate",
                Description = (string?)null
            },
            new
            {
                Id = new Guid("33333333-3333-3333-3333-333333332333"),
                CreatedAtUtc = DateTime.UnixEpoch,
                UpdatedAtUtc = (DateTime?)null,
                SeverityNumber = (short)3,
                Name = "Minor",
                Description = (string?)null
            },
            new
            {
                Id = new Guid("44444444-4444-4444-4444-444444443444"),
                CreatedAtUtc = DateTime.UnixEpoch,
                UpdatedAtUtc = (DateTime?)null,
                SeverityNumber = (short)4,
                Name = "Not applicable",
                Description = (string?)null
            });
    }
}
