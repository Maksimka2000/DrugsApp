using DrugsApp.Domain.SupplementFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class SupplementGroupConfiguration : BaseEntityConfiguration<SupplementGroup>
{
    public override void Configure(EntityTypeBuilder<SupplementGroup> builder)
    {
        base.Configure(builder);

        builder.ToTable("SupplementGroups");

        builder.Property(group => group.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(group => group.Name)
            .IsUnique();

        builder
            .HasMany(group => group.Supplements)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "SupplementGroupSupplements",
                l => l.HasOne<Supplement>()
                    .WithMany()
                    .HasForeignKey("SupplementId")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<SupplementGroup>()
                    .WithMany()
                    .HasForeignKey("SupplementGroupId")
                    .OnDelete(DeleteBehavior.Cascade),
                joinEntity =>
                {
                    joinEntity.HasKey("SupplementGroupId", "SupplementId");
                    joinEntity.HasIndex("SupplementId", "SupplementGroupId");
                    joinEntity.ToTable("SupplementGroupSupplements");
                });
    }
}
