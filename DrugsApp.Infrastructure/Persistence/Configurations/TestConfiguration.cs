using DrugsApp.Domain.TestFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class TestConfiguration : BaseEntityConfiguration<Test>
{
    public override void Configure(EntityTypeBuilder<Test> builder)
    {
        base.Configure(builder);

        builder.ToTable("Tests");

        builder.Property(test => test.Attribute1)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(test => test.Attribute2)
            .IsRequired(false);

        builder.Property(test => test.Attribute3)
            .HasPrecision(10, 2)
            .IsRequired(false);

        builder.Property(test => test.Attribute4)
            .IsRequired()
            .HasDefaultValue(false);
    }
}

