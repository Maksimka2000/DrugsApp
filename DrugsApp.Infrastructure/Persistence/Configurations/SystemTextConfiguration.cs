using DrugsApp.Domain.SystemTextFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class SystemTextConfiguration : BaseEntityConfiguration<SystemText>
{
    public override void Configure(EntityTypeBuilder<SystemText> builder)
    {
        base.Configure(builder);

        builder.ToTable("SystemTexts");

        builder.Property(systemText => systemText.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(systemText => systemText.Key)
            .IsUnique();

        builder.Property(systemText => systemText.Value)
            .IsRequired();

        builder.HasData(new
        {
            Id = Guid.Parse("f6c3b9ba-0c43-4f6a-9f42-1c6d7f2b6d1f"),
            Key = "activeSubstance.alcohol.defaultText",
            Value = "Default alcohol interaction text.",
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = (DateTime?)null
        });
    }
}
