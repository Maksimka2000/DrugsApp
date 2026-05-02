using System.Collections.Generic;
using DrugsApp.Domain.FoodFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class FoodConfiguration : BaseEntityConfiguration<Food>
{
    public override void Configure(EntityTypeBuilder<Food> builder)
    {
        base.Configure(builder);

        builder.ToTable("Foods");

        builder.Property(food => food.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property<List<string>>("_aliases")
            .HasColumnName("Aliases")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(food => food.SearchText)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(food => food.Name, "Index_Foods_Name")
            .IsUnique();

        builder.HasIndex(food => food.SearchText, "Index_Foods_SearchText_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");
    }
}
