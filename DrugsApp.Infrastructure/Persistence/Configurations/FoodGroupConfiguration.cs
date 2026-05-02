using System.Collections.Generic;
using DrugsApp.Domain.FoodFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class FoodGroupConfiguration : BaseEntityConfiguration<FoodGroup>
{
    public override void Configure(EntityTypeBuilder<FoodGroup> builder)
    {
        base.Configure(builder);

        builder.ToTable("FoodGroups");

        builder.Property(group => group.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(group => group.Name, "Index_FoodGroups_Name")
            .IsUnique();

        builder.HasIndex(group => group.Name, "Index_FoodGroups_Name_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.HasMany(group => group.Foods)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "FoodGroupFoods",
                right => right
                    .HasOne<Food>()
                    .WithMany()
                    .HasForeignKey("FoodId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<FoodGroup>()
                    .WithMany()
                    .HasForeignKey("FoodGroupId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("FoodGroupFoods");
                    join.HasKey("FoodGroupId", "FoodId");
                    join.HasIndex("FoodId", "FoodGroupId");
                });
    }
}
