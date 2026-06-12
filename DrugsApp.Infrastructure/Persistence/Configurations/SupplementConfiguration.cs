using System.Collections.Generic;
using DrugsApp.Domain.SupplementFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class SupplementConfiguration : BaseEntityConfiguration<Supplement>
{
    public override void Configure(EntityTypeBuilder<Supplement> builder)
    {
        base.Configure(builder);

        builder.ToTable("Supplements");

        builder.Property(supplement => supplement.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property<List<string>>("_aliases")
            .HasColumnName("Aliases")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(supplement => supplement.SearchText)
            .IsRequired()
            .HasColumnType("text");

        builder.HasIndex(supplement => supplement.Name)
            .IsUnique();

        builder.HasIndex(
            supplement => supplement.SearchText,
            name: "IX_Supplements_SearchText_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");
    }
}
