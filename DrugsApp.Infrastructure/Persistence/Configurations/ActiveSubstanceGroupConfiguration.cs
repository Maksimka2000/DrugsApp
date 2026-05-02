using System.Collections.Generic;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ActiveSubstanceGroupConfiguration : BaseEntityConfiguration<ActiveSubstanceGroup>
{
    public override void Configure(EntityTypeBuilder<ActiveSubstanceGroup> builder)
    {
        base.Configure(builder);

        builder.ToTable("ActiveSubstanceGroups");

        builder.Property(group => group.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(group => group.Name, "Index_ActiveSubstanceGroups_Name")
            .IsUnique();

        builder.HasIndex(group => group.Name, "Index_ActiveSubstanceGroups_Name_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.HasMany(group => group.ActiveSubstances)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ActiveSubstanceGroupMembers",
                right => right
                    .HasOne<ActiveSubstance>()
                    .WithMany()
                    .HasForeignKey("ActiveSubstanceId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<ActiveSubstanceGroup>()
                    .WithMany()
                    .HasForeignKey("ActiveSubstanceGroupId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("ActiveSubstanceGroupMembers");
                    join.HasKey("ActiveSubstanceGroupId", "ActiveSubstanceId");
                    join.HasIndex("ActiveSubstanceId", "ActiveSubstanceGroupId");
                });
    }
}
